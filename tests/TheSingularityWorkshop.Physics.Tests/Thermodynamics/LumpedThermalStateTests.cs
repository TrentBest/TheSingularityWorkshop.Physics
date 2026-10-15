using NUnit.Framework;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class LumpedThermalStateTests
{
    [Test]
    public void SensibleEnergy_IsMeasuredRelativeToDeclaredReferenceTemperature()
    {
        var state = new LumpedThermalState(
            MassKilograms: 2d,
            TemperatureKelvin: 300d,
            SpecificHeatCapacityJoulesPerKilogramKelvin: 500d,
            ReferenceTemperatureKelvin: 280d);

        Assert.That(state.IsValid, Is.True);
        Assert.That(state.SensibleEnergyRelativeToReferenceJoules, Is.EqualTo(20_000d));
    }

    [Test]
    public void ApplyNetEnergyTransfer_HeatingRaisesTemperatureByConstantHeatCapacityRelation()
    {
        var state = CreateState(temperatureKelvin: 300d);

        var heated = state.ApplyNetEnergyTransfer(10_000d);

        Assert.That(heated.TemperatureKelvin, Is.EqualTo(310d).Within(1e-12));
        Assert.That(heated.MassKilograms, Is.EqualTo(state.MassKilograms));
        Assert.That(heated.SpecificHeatCapacityJoulesPerKilogramKelvin,
            Is.EqualTo(state.SpecificHeatCapacityJoulesPerKilogramKelvin));
        Assert.That(heated.ReferenceTemperatureKelvin, Is.EqualTo(state.ReferenceTemperatureKelvin));
    }

    [Test]
    public void ApplyNetEnergyTransfer_CoolingLowersTemperature()
    {
        var state = CreateState(temperatureKelvin: 300d);

        var cooled = state.ApplyNetEnergyTransfer(-5_000d);

        Assert.That(cooled.TemperatureKelvin, Is.EqualTo(295d).Within(1e-12));
    }

    [Test]
    public void ApplyNetEnergyTransfer_ZeroEnergyLeavesStateUnchanged()
    {
        var state = CreateState(temperatureKelvin: 300d);

        Assert.That(state.ApplyNetEnergyTransfer(0d), Is.EqualTo(state));
    }

    [Test]
    public void ApplyNetEnergyTransfer_RejectsTransferBelowAbsoluteZero()
    {
        var state = CreateState(temperatureKelvin: 1d);

        Assert.Throws<ArgumentOutOfRangeException>(() => state.ApplyNetEnergyTransfer(-1_000d));
    }

    [TestCase(0d, 300d, 500d, 273.15d)]
    [TestCase(-1d, 300d, 500d, 273.15d)]
    [TestCase(1d, -1d, 500d, 273.15d)]
    [TestCase(1d, 300d, 0d, 273.15d)]
    [TestCase(1d, 300d, 500d, -1d)]
    [TestCase(double.NaN, 300d, 500d, 273.15d)]
    [TestCase(1d, double.PositiveInfinity, 500d, 273.15d)]
    public void IsValid_RejectsInvalidMassTemperatureOrThermalProperties(
        double mass,
        double temperature,
        double specificHeat,
        double referenceTemperature)
    {
        var state = new LumpedThermalState(mass, temperature, specificHeat, referenceTemperature);

        Assert.That(state.IsValid, Is.False);
        Assert.Throws<ArgumentException>(() => state.ApplyNetEnergyTransfer(1d));
    }

    [Test]
    public void ApplyNetEnergyTransfer_RejectsNonFiniteTransfer()
    {
        var state = CreateState(temperatureKelvin: 300d);

        Assert.Throws<ArgumentException>(() => state.ApplyNetEnergyTransfer(double.NaN));
        Assert.Throws<ArgumentException>(() => state.ApplyNetEnergyTransfer(double.PositiveInfinity));
    }

    [Test]
    public void IsValid_RejectsOverflowedSensibleEnergy()
    {
        var state = new LumpedThermalState(
            MassKilograms: double.MaxValue,
            TemperatureKelvin: double.MaxValue,
            SpecificHeatCapacityJoulesPerKilogramKelvin: double.MaxValue,
            ReferenceTemperatureKelvin: 0d);

        Assert.That(state.IsValid, Is.False);
        Assert.Throws<ArgumentException>(() => state.ApplyNetEnergyTransfer(0d));
    }

    private static LumpedThermalState CreateState(double temperatureKelvin) =>
        new(
            MassKilograms: 2d,
            TemperatureKelvin: temperatureKelvin,
            SpecificHeatCapacityJoulesPerKilogramKelvin: 500d,
            ReferenceTemperatureKelvin: 273.15d);
}
