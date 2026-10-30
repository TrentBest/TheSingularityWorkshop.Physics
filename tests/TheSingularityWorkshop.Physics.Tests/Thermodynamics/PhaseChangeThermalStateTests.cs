using NUnit.Framework;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Tests.Thermodynamics;

[TestFixture]
public sealed class PhaseChangeThermalStateTests
{
    [Test]
    public void ApplyNetEnergyTransfer_HeatsSolidBeforeReachingTransition()
    {
        var initial = CreateState(temperatureKelvin: 270d, liquidFraction: 0d);

        var result = initial.ApplyNetEnergyTransfer(10_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(275d).Within(1e-12));
        Assert.That(result.LiquidFraction, Is.EqualTo(0d));
        Assert.That(result.EnthalpyRelativeToTransitionJoules, Is.EqualTo(-10_000d).Within(1e-9));
    }

    [Test]
    public void ApplyNetEnergyTransfer_MeltsAtConstantTemperatureAcrossLatentHeatPlateau()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 0d);

        var result = initial.ApplyNetEnergyTransfer(50_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(280d));
        Assert.That(result.LiquidFraction, Is.EqualTo(0.5d).Within(1e-12));
        Assert.That(result.EnthalpyRelativeToTransitionJoules, Is.EqualTo(50_000d).Within(1e-9));
    }

    [Test]
    public void ApplyNetEnergyTransfer_HeatsLiquidAfterMeltingCompletes()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 1d);

        var result = initial.ApplyNetEnergyTransfer(20_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(290d).Within(1e-12));
        Assert.That(result.LiquidFraction, Is.EqualTo(1d));
    }

    [Test]
    public void ApplyNetEnergyTransfer_CoolingLiquidFreezesAtConstantTemperature()
    {
        var initial = CreateState(temperatureKelvin: 290d, liquidFraction: 1d);

        var result = initial.ApplyNetEnergyTransfer(-90_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(280d));
        Assert.That(result.LiquidFraction, Is.EqualTo(0.5d).Within(1e-12));
    }

    [Test]
    public void ApplyNetEnergyTransfer_FreezesThenCoolsSolid()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 0.5d);

        var result = initial.ApplyNetEnergyTransfer(-70_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(270d).Within(1e-12));
        Assert.That(result.LiquidFraction, Is.EqualTo(0d));
    }

    [Test]
    public void ApplyNetEnergyTransfer_ConservesDeclaredEnthalpyAcrossRoundTrip()
    {
        var initial = CreateState(temperatureKelvin: 260d, liquidFraction: 0d);
        var heated = initial.ApplyNetEnergyTransfer(150_000d);
        var returned = heated.ApplyNetEnergyTransfer(-150_000d);

        Assert.That(returned.TemperatureKelvin, Is.EqualTo(initial.TemperatureKelvin).Within(1e-12));
        Assert.That(returned.LiquidFraction, Is.EqualTo(initial.LiquidFraction));
        Assert.That(returned.EnthalpyRelativeToTransitionJoules,
            Is.EqualTo(initial.EnthalpyRelativeToTransitionJoules).Within(1e-9));
    }

    [Test]
    public void ApplyNetEnergyTransfer_RejectsCoolingBelowAbsoluteZero()
    {
        var state = CreateState(temperatureKelvin: 1d, liquidFraction: 0d);

        Assert.Throws<ArgumentOutOfRangeException>(() => state.ApplyNetEnergyTransfer(-1_000_000d));
    }

    [Test]
    public void ApplyNetEnergyTransfer_RejectsNonFiniteTransferAndInvalidState()
    {
        var state = CreateState(temperatureKelvin: 280d, liquidFraction: 0d);

        Assert.Throws<ArgumentException>(() => state.ApplyNetEnergyTransfer(double.NaN));
        Assert.Throws<ArgumentException>(() => state.ApplyNetEnergyTransfer(double.PositiveInfinity));
        Assert.That((state with { LiquidFraction = 1.1d }).IsValid, Is.False);
        Assert.That((state with { TemperatureKelvin = 279d, LiquidFraction = 0.2d }).IsValid, Is.False);
    }

    [Test]
    public void ApplyNetEnergyTransfer_ZeroTransferPreservesState()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 0.25d);

        var result = initial.ApplyNetEnergyTransfer(0d);

        Assert.That(result, Is.EqualTo(initial));
    }

    [Test]
    public void ApplyNetEnergyTransfer_ExactLatentEnergyBoundaryCompletesMelting()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 0.5d);

        var result = initial.ApplyNetEnergyTransfer(50_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(280d));
        Assert.That(result.LiquidFraction, Is.EqualTo(1d));
        Assert.That(result.EnthalpyRelativeToTransitionJoules, Is.EqualTo(100_000d).Within(1e-9));
    }

    [Test]
    public void ApplyNetEnergyTransfer_ExactLatentEnergyBoundaryCompletesFreezing()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 0.5d);

        var result = initial.ApplyNetEnergyTransfer(-50_000d);

        Assert.That(result.TemperatureKelvin, Is.EqualTo(280d));
        Assert.That(result.LiquidFraction, Is.EqualTo(0d));
        Assert.That(result.EnthalpyRelativeToTransitionJoules, Is.EqualTo(0d));
    }

    [Test]
    public void ApplyNetEnergyTransfer_RejectsFiniteTransferThatOverflowsEnergy()
    {
        var initial = CreateState(temperatureKelvin: 280d, liquidFraction: 1d);

        Assert.Throws<OverflowException>(() => initial.ApplyNetEnergyTransfer(double.MaxValue));
    }

    private static PhaseChangeThermalState CreateState(double temperatureKelvin, double liquidFraction) =>
        new(
            MassKilograms: 2d,
            TemperatureKelvin: temperatureKelvin,
            SolidSpecificHeatCapacityJoulesPerKilogramKelvin: 1_000d,
            LiquidSpecificHeatCapacityJoulesPerKilogramKelvin: 2_000d,
            TransitionTemperatureKelvin: 280d,
            SpecificLatentHeatJoulesPerKilogram: 50_000d,
            LiquidFraction: liquidFraction);
}
