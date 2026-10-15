using NUnit.Framework;
using TheSingularityWorkshop.Physics.Hydrodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class LinearWaveBasin1DTests
{
    [Test]
    public void Advance_PropagatesLocalizedDisturbanceDeterministically()
    {
        var initial = CreatePulse(41, 20);
        var first = new LinearWaveBasin1D(41, 1d, 2d, 0.25d, initial);
        var second = new LinearWaveBasin1D(41, 1d, 2d, 0.25d, initial);

        for (var i = 0; i < 8; i++)
        {
            first.Advance();
            second.Advance();
        }

        Assert.That(first.SimulationTime, Is.EqualTo(2d).Within(1e-12));
        Assert.That(first.SnapshotElevation(), Is.EqualTo(second.SnapshotElevation()));
        Assert.That(first.SnapshotElevation(), Is.Not.EqualTo(initial));
    }

    [Test]
    public void Advance_ClosedBasinBoundaryReflectsWithoutLeavingDomain()
    {
        var initial = CreatePulse(31, 2);
        var basin = new LinearWaveBasin1D(31, 1d, 1d, 0.5d, initial);

        for (var i = 0; i < 100; i++)
            basin.Advance();

        Assert.That(basin.SimulationTime, Is.EqualTo(50d).Within(1e-12));
        Assert.That(basin.SnapshotElevation().All(double.IsFinite), Is.True);
        Assert.That(basin.StepCount, Is.EqualTo(100));
    }

    [Test]
    public void Constructor_RejectsCourantNumberAboveOne()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LinearWaveBasin1D(5, 1d, 2d, 0.6d, new double[5]));
    }

    [Test]
    public void Constructor_RejectsNonFiniteInitialElevation()
    {
        Assert.Throws<ArgumentException>(() =>
            new LinearWaveBasin1D(3, 1d, 1d, 0.5d, new[] { 0d, double.NaN, 0d }));
    }

    [Test]
    public void Snapshot_IsDefensiveCopy()
    {
        var basin = new LinearWaveBasin1D(3, 1d, 1d, 0.5d, new[] { 0d, 1d, 0d });
        var snapshot = basin.SnapshotElevation();
        snapshot[1] = 999d;

        Assert.That(basin.SampleElevation(1), Is.EqualTo(1d));
    }

    private static double[] CreatePulse(int count, int center)
    {
        var values = new double[count];
        for (var i = 0; i < count; i++)
        {
            var distance = (i - center) / 2.5d;
            values[i] = Math.Exp(-distance * distance);
        }
        return values;
    }
}
