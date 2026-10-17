using NUnit.Framework;
using TheSingularityWorkshop.Physics.Hydrodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class RainfallRateField2DTests
{
    [Test]
    public void CalculateDepthIncrement_ConvertsHourlyRainfallToMetres()
    {
        // 10 mm/hour = 0.01 m over one hour.
        var field = new RainfallRateField2D(2, 1, new[] { 0.01d / 3600d, 0d });

        Assert.That(field.CalculateDepthIncrement(3600d), Is.EqualTo(new[] { 0.01d, 0d }));
    }

    [Test]
    public void CalculateDepthIncrement_IsLinearInElapsedTimeAndCellRate()
    {
        var field = new RainfallRateField2D(2, 2, new[] { 0.001d, 0.002d, 0.003d, 0.004d });

        Assert.That(field.CalculateDepthIncrement(2d), Is.EqualTo(new[] { 0.002d, 0.004d, 0.006d, 0.008d }));
    }

    [Test]
    public void Constructor_RejectsMismatchedGridAndInvalidRates()
    {
        Assert.Throws<ArgumentException>(() => new RainfallRateField2D(2, 2, new[] { 0d, 0d, 0d }));
        Assert.Throws<ArgumentException>(() => new RainfallRateField2D(1, 1, new[] { -0.1d }));
        Assert.Throws<ArgumentException>(() => new RainfallRateField2D(1, 1, new[] { double.NaN }));
        Assert.Throws<ArgumentException>(() => new RainfallRateField2D(1, 1, new[] { double.PositiveInfinity }));
    }

    [Test]
    public void CalculateDepthIncrement_RejectsInvalidDuration()
    {
        var field = new RainfallRateField2D(1, 1, new[] { 0.001d });

        Assert.Throws<ArgumentOutOfRangeException>(() => field.CalculateDepthIncrement(-1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => field.CalculateDepthIncrement(double.NaN));
    }

    [Test]
    public void SnapshotRates_IsDefensiveCopy()
    {
        var field = new RainfallRateField2D(1, 1, new[] { 0.001d });
        var snapshot = field.SnapshotRates();
        snapshot[0] = 99d;

        Assert.That(field.SnapshotRates()[0], Is.EqualTo(0.001d));
    }
}
