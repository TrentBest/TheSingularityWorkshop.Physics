using NUnit.Framework;
using TheSingularityWorkshop.Physics.Hydrodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ShallowWaterBasin2DTests
{
    [Test]
    public void Advance_UniformStillWaterRemainsAtRest()
    {
        var basin = new ShallowWaterBasin2D(8, 6, 1d, 1d, 0.01d, Enumerable.Repeat(2d, 48).ToArray());
        var initialVolume = basin.WaterVolume;

        for (var i = 0; i < 100; i++)
            basin.Advance();

        Assert.That(basin.SnapshotDepth(), Is.All.EqualTo(2d));
        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-12));
        Assert.That(basin.SampleVelocityX(3, 3), Is.EqualTo(0d));
        Assert.That(basin.SampleVelocityY(3, 3), Is.EqualTo(0d));
    }

    [Test]
    public void Advance_DepthDisturbanceEvolvesAndConservesClosedBasinVolume()
    {
        const int width = 25;
        const int height = 17;
        var depth = Enumerable.Repeat(1d, width * height).ToArray();
        depth[8 * width + 12] = 1.1d;
        var basin = new ShallowWaterBasin2D(width, height, 1d, 1d, 0.01d, depth);
        var initialVolume = basin.WaterVolume;

        for (var i = 0; i < 50; i++)
            basin.Advance();

        Assert.That(basin.WaterVolume, Is.EqualTo(initialVolume).Within(1e-10));
        Assert.That(basin.SnapshotDepth(), Is.Not.EqualTo(depth));
        Assert.That(basin.SnapshotDepth().All(value => double.IsFinite(value) && value > 0d), Is.True);
        Assert.That(basin.StepCount, Is.EqualTo(50));
    }

    [Test]
    public void Constructor_RejectsUnsafeCflBound()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ShallowWaterBasin2D(4, 4, 1d, 1d, 0.2d, Enumerable.Repeat(10d, 16).ToArray()));
    }

    [Test]
    public void Snapshot_IsDefensiveCopy()
    {
        var basin = new ShallowWaterBasin2D(2, 2, 1d, 1d, 0.01d, new[] { 1d, 1d, 1d, 1d });
        var snapshot = basin.SnapshotDepth();
        snapshot[0] = 100d;

        Assert.That(basin.SampleDepth(0, 0), Is.EqualTo(1d));
    }
}
