using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TemperatureDependentHeatCapacityTests
{
    [Test]
    public void GetSpecificHeatCapacity_InterpolatesWithinSegment()
    {
        var curve = CreateCurve();

        Assert.That(curve.GetSpecificHeatCapacity(300d), Is.EqualTo(1_500d).Within(1e-12));
    }

    [Test]
    public void GetSpecificEnthalpyChange_IntegratesLinearCapacityExactly()
    {
        var curve = CreateCurve();

        // Integral of a linear 1000 -> 2000 J/(kg K) curve over 200 K is 300,000 J/kg.
        Assert.That(curve.GetSpecificEnthalpyChangeJoulesPerKilogram(200d, 400d),
            Is.EqualTo(300_000d).Within(1e-8));
        Assert.That(curve.GetSpecificEnthalpyChangeJoulesPerKilogram(400d, 200d),
            Is.EqualTo(-300_000d).Within(1e-8));
    }

    [Test]
    public void GetSpecificEnthalpyChange_IntegratesAcrossSegmentBoundary()
    {
        var curve = new TemperatureDependentHeatCapacity(
            "test-material",
            [
                new HeatCapacitySample(100d, 500d),
                new HeatCapacitySample(200d, 1_000d),
                new HeatCapacitySample(300d, 1_000d)
            ],
            PropertyEvidenceStatus.Illustrative,
            "unit-test-fixture",
            "1",
            "Illustrative values only.");

        Assert.That(curve.GetSpecificEnthalpyChangeJoulesPerKilogram(150d, 250d),
            Is.EqualTo(93_750d).Within(1e-8));
    }

    [Test]
    public void ApplyNetEnergyTransfer_InvertsIntegratedEnthalpyAndConservesEnergy()
    {
        var curve = CreateCurve();

        var finalTemperature = curve.ApplyNetEnergyTransfer(
            massKilograms: 2d,
            initialTemperatureKelvin: 200d,
            netEnergyTransferJoules: 600_000d);

        Assert.That(finalTemperature, Is.EqualTo(400d).Within(1e-10));
        var recoveredEnergy = 2d * curve.GetSpecificEnthalpyChangeJoulesPerKilogram(200d, finalTemperature);
        Assert.That(recoveredEnergy, Is.EqualTo(600_000d).Within(1e-6));
    }

    [Test]
    public void ApplyNetEnergyTransfer_UsesIntegratedCurveRatherThanInitialPointCapacity()
    {
        var curve = CreateCurve();

        var finalTemperature = curve.ApplyNetEnergyTransfer(1d, 200d, 125_000d);

        Assert.That(finalTemperature, Is.EqualTo(300d).Within(1e-10));
    }

    [Test]
    public void ApplyNetEnergyTransfer_RejectsEnergyOutsideDeclaredRange()
    {
        var curve = CreateCurve();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.ApplyNetEnergyTransfer(1d, 200d, 300_001d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.ApplyNetEnergyTransfer(1d, 200d, -1d));
    }

    [Test]
    public void GetSpecificHeatCapacity_RejectsExtrapolation()
    {
        var curve = CreateCurve();

        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetSpecificHeatCapacity(199.9d));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetSpecificHeatCapacity(400.1d));
    }

    [Test]
    public void Constructor_RejectsUnorderedOrNonPositiveSamples()
    {
        Assert.Throws<ArgumentException>(() => new TemperatureDependentHeatCapacity(
            "material",
            [new HeatCapacitySample(200d, 1_000d), new HeatCapacitySample(200d, 2_000d)],
            PropertyEvidenceStatus.Illustrative, "fixture", "1", "test"));
        Assert.Throws<ArgumentException>(() => new TemperatureDependentHeatCapacity(
            "material",
            [new HeatCapacitySample(200d, 1_000d), new HeatCapacitySample(300d, 0d)],
            PropertyEvidenceStatus.Illustrative, "fixture", "1", "test"));
    }

    [Test]
    public void Constructor_RejectsInsufficientEvidenceIdentity()
    {
        Assert.Throws<ArgumentException>(() => new TemperatureDependentHeatCapacity(
            "material",
            [new HeatCapacitySample(200d, 1_000d), new HeatCapacitySample(300d, 1_500d)],
            PropertyEvidenceStatus.Illustrative, "", "1", "test"));
    }

    private static TemperatureDependentHeatCapacity CreateCurve() =>
        new(
            "illustrative-linear-material",
            [
                new HeatCapacitySample(200d, 1_000d),
                new HeatCapacitySample(400d, 2_000d)
            ],
            PropertyEvidenceStatus.Illustrative,
            "unit-test-fixture",
            "1",
            "Synthetic linear curve; not empirical material data.");
}
