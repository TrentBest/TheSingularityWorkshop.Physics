using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class VolcanicTerrain2DTests
{
    [Test]
    public void DepositEruption_RaisesTerrainAndConservesRequestedVolume()
    {
        const int width = 21;
        const int height = 17;
        var initial = Enumerable.Repeat(0d, width * height).ToArray();
        var terrain = new VolcanicTerrain2D(width, height, 2d, 3d, initial);

        var maximumRise = terrain.DepositEruption(10, 8, 12d, 900d);

        Assert.That(maximumRise, Is.GreaterThan(0d));
        Assert.That(terrain.SampleElevation(10, 8), Is.EqualTo(maximumRise).Within(1e-12));
        var depositedVolume = terrain.SnapshotElevation().Sum(elevation => elevation * 2d * 3d);
        Assert.That(depositedVolume, Is.EqualTo(900d).Within(1e-10));
        Assert.That(terrain.NetDepositedVolume, Is.EqualTo(900d));
    }

    [Test]
    public void DepositEruption_IsDeterministicAndLocalizedToFootprint()
    {
        var initial = Enumerable.Repeat(-4d, 15 * 15).ToArray();
        var first = new VolcanicTerrain2D(15, 15, 1d, 1d, initial);
        var second = new VolcanicTerrain2D(15, 15, 1d, 1d, initial);

        first.DepositEruption(7, 7, 3d, 50d);
        second.DepositEruption(7, 7, 3d, 50d);

        Assert.That(first.SnapshotElevation(), Is.EqualTo(second.SnapshotElevation()));
        Assert.That(first.SampleElevation(0, 0), Is.EqualTo(-4d));
        Assert.That(first.SampleElevation(7, 7), Is.GreaterThan(-4d));
    }

    [Test]
    public void DepositEruption_AccumulatesVolumeAcrossEvents()
    {
        var terrain = new VolcanicTerrain2D(9, 9, 1d, 1d, new double[81]);

        terrain.DepositEruption(3, 3, 2d, 12d);
        terrain.DepositEruption(5, 5, 2d, 18d);

        var volume = terrain.SnapshotElevation().Sum();
        Assert.That(volume, Is.EqualTo(30d).Within(1e-12));
        Assert.That(terrain.NetDepositedVolume, Is.EqualTo(30d));
    }

    [Test]
    public void DepositEruption_InvalidRequestDoesNotChangeTerrain()
    {
        var terrain = new VolcanicTerrain2D(5, 5, 1d, 1d, new double[25]);
        var before = terrain.SnapshotElevation();

        Assert.Throws<ArgumentOutOfRangeException>(() => terrain.DepositEruption(2, 2, 0d, 10d));
        Assert.Throws<ArgumentOutOfRangeException>(() => terrain.DepositEruption(2, 2, 1d, double.NaN));
        Assert.That(terrain.SnapshotElevation(), Is.EqualTo(before));
        Assert.That(terrain.NetDepositedVolume, Is.EqualTo(0d));
    }

    [Test]
    public void Snapshot_IsDefensiveCopy()
    {
        var terrain = new VolcanicTerrain2D(3, 3, 1d, 1d, new double[9]);
        var snapshot = terrain.SnapshotElevation();
        snapshot[4] = 100d;

        Assert.That(terrain.SampleElevation(1, 1), Is.EqualTo(0d));
    }
}
