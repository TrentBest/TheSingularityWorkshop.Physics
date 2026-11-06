using NUnit.Framework;
using TheSingularityWorkshop.Physics.Timing;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class OscillatorPhaseComparisonTests
{
    [Test]
    public void Compare_ReportsInPhaseOscillatorsWithoutInventingCoupling()
    {
        var first = new IdealMetronomeDefinition("first", 120d, 0d, 1, 0d);
        var second = new IdealMetronomeDefinition("second", 120d, 0d, 1, 0d);

        var result = OscillatorPhaseComparison.Compare(first, second, 0d, 600d);

        Assert.That(result.UnwrappedDifferenceStartRadians, Is.EqualTo(0d));
        Assert.That(result.UnwrappedDifferenceEndRadians, Is.EqualTo(0d).Within(1e-10));
        Assert.That(result.WrappedDifferenceEndRadians, Is.EqualTo(0d).Within(1e-10));
        Assert.That(result.FrequencyDifferenceHz, Is.EqualTo(0d));
        Assert.That(result.PhaseDriftRateRadiansPerSecond, Is.EqualTo(0d).Within(1e-12));
    }

    [Test]
    public void Compare_ReportsAntiPhaseUsingSignedWrapConvention()
    {
        var first = new IdealMetronomeDefinition("first", 60d, 0d, 1, 0d);
        var second = new IdealMetronomeDefinition("second", 60d, Math.PI, 1, 0d);

        var result = OscillatorPhaseComparison.Compare(first, second, 0d, 10d);

        Assert.That(result.WrappedDifferenceStartRadians, Is.EqualTo(-Math.PI));
        Assert.That(result.WrappedDifferenceEndRadians, Is.EqualTo(-Math.PI).Within(1e-12));
        Assert.That(result.UnwrappedDifferenceEndRadians, Is.EqualTo(Math.PI).Within(1e-12));
    }

    [Test]
    public void Compare_ReportsLinearPhaseDriftForDifferentFrequencies()
    {
        var first = new IdealMetronomeDefinition("first", 60d, 0d, 1, 0d);
        var second = new IdealMetronomeDefinition("second", 66d, 0d, 1, 0d);

        var result = OscillatorPhaseComparison.Compare(first, second, 0d, 10d);

        Assert.That(result.FrequencyDifferenceHz, Is.EqualTo(0.1d).Within(1e-12));
        Assert.That(result.UnwrappedDifferenceEndRadians,
            Is.EqualTo(2d * Math.PI).Within(1e-12));
        Assert.That(result.PhaseDriftRateRadiansPerSecond,
            Is.EqualTo(2d * Math.PI / 10d).Within(1e-12));
        Assert.That(result.WrappedDifferenceEndRadians, Is.EqualTo(0d).Within(1e-12));
    }

    [Test]
    public void Compare_DistinguishesRepeatingRationalRatioFromPhaseLocking()
    {
        var first = new IdealMetronomeDefinition("first", 60d, 0d, 1, 0d);
        var second = new IdealMetronomeDefinition("second", 90d, 0d, 1, 0d);

        var result = OscillatorPhaseComparison.Compare(first, second, 0d, 8d);

        Assert.That(result.FrequencyDifferenceHz, Is.EqualTo(0.5d).Within(1e-12));
        Assert.That(result.UnwrappedDifferenceEndRadians,
            Is.EqualTo(8d * Math.PI).Within(1e-11));
        Assert.That(result.WrappedDifferenceEndRadians, Is.EqualTo(0d).Within(1e-11));
        Assert.That(result.UnwrappedDifferenceEndRadians,
            Is.Not.EqualTo(result.UnwrappedDifferenceStartRadians));
    }

    [Test]
    public void Compare_UsesEachOscillatorLogicalStartAndInitialPhase()
    {
        var first = new IdealMetronomeDefinition("first", 60d, Math.PI / 2d, 1, 2d);
        var second = new IdealMetronomeDefinition("second", 60d, 0d, 1, 4d);

        var result = OscillatorPhaseComparison.Compare(first, second, 4d, 5d);

        Assert.That(result.FirstPhaseStartRadians, Is.EqualTo(9d * Math.PI / 2d).Within(1e-12));
        Assert.That(result.SecondPhaseStartRadians, Is.EqualTo(0d));
        Assert.That(result.UnwrappedDifferenceStartRadians,
            Is.EqualTo(-5d * Math.PI / 2d).Within(1e-12));
    }

    [Test]
    public void WrapSigned_UsesNegativePiInclusivePositivePiExclusiveRange()
    {
        Assert.That(OscillatorPhaseComparison.WrapSigned(Math.PI), Is.EqualTo(-Math.PI));
        Assert.That(OscillatorPhaseComparison.WrapSigned(-Math.PI), Is.EqualTo(-Math.PI));
        Assert.That(OscillatorPhaseComparison.WrapSigned(3d * Math.PI),
            Is.EqualTo(-Math.PI).Within(1e-12));
        Assert.That(OscillatorPhaseComparison.WrapSigned(2d * Math.PI),
            Is.EqualTo(0d).Within(1e-12));
    }

    [Test]
    public void Compare_RejectsInvalidObservationIntervalsAndNonFinitePhase()
    {
        var first = new IdealMetronomeDefinition("first", 60d, 0d, 1, 2d);
        var second = new IdealMetronomeDefinition("second", 60d, 0d, 1, 0d);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OscillatorPhaseComparison.Compare(first, second, 1d, 3d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OscillatorPhaseComparison.Compare(first, second, 2d, 2d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OscillatorPhaseComparison.Compare(first, second, 2d, double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OscillatorPhaseComparison.WrapSigned(double.NaN));
    }
}
