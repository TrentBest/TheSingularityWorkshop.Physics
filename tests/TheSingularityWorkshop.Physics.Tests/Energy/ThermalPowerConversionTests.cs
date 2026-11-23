using NUnit.Framework;
using TheSingularityWorkshop.Physics.Energy;

namespace TheSingularityWorkshop.Physics.Tests.Energy;

[TestFixture]
public sealed class ThermalPowerConversionTests
{
    [Test]
    public void Observe_AccountsForHeatRejectionShaftWorkAndElectricalOutput()
    {
        var result = ThermalPowerConversion.Observe(
            heatInputJoules: 1_000_000d,
            hotReservoirTemperatureKelvin: 600d,
            coldReservoirTemperatureKelvin: 300d,
            carnotLimitFraction: 0.5d,
            generatorEfficiencyFraction: 0.9d);

        Assert.That(result.CarnotEfficiencyUpperBound, Is.EqualTo(0.5d));
        Assert.That(result.ThermalCycleEfficiencyFraction, Is.EqualTo(0.25d));
        Assert.That(result.ShaftWorkJoules, Is.EqualTo(250_000d));
        Assert.That(result.RejectedThermalEnergyJoules, Is.EqualTo(750_000d));
        Assert.That(result.ElectricalEnergyJoules, Is.EqualTo(225_000d));
        Assert.That(result.EnergyNotDeliveredAsElectricalOutputJoules, Is.EqualTo(25_000d));
        Assert.That(
            result.RejectedThermalEnergyJoules + result.ShaftWorkJoules,
            Is.EqualTo(result.HeatInputJoules));
        Assert.That(
            result.ElectricalEnergyJoules + result.EnergyNotDeliveredAsElectricalOutputJoules,
            Is.EqualTo(result.ShaftWorkJoules));
    }

    [Test]
    public void Observe_NeverExceedsCarnotLimit()
    {
        var result = ThermalPowerConversion.Observe(10_000d, 900d, 300d, 1d, 1d);

        Assert.That(result.ThermalCycleEfficiencyFraction,
            Is.LessThanOrEqualTo(result.CarnotEfficiencyUpperBound));
        Assert.That(result.ElectricalEnergyJoules,
            Is.LessThanOrEqualTo(result.HeatInputJoules * result.CarnotEfficiencyUpperBound));
    }

    [Test]
    public void Observe_HandlesZeroAndPerfectDeclaredStageEfficiencies()
    {
        var zero = ThermalPowerConversion.Observe(1_000d, 500d, 250d, 0d, 1d);
        var perfectGenerator = ThermalPowerConversion.Observe(1_000d, 500d, 250d, 1d, 1d);
        var zeroGenerator = ThermalPowerConversion.Observe(1_000d, 500d, 250d, 1d, 0d);

        Assert.That(zero.ShaftWorkJoules, Is.Zero);
        Assert.That(zero.RejectedThermalEnergyJoules, Is.EqualTo(1_000d));
        Assert.That(perfectGenerator.ElectricalEnergyJoules,
            Is.EqualTo(perfectGenerator.ShaftWorkJoules));
        Assert.That(zeroGenerator.ElectricalEnergyJoules, Is.Zero);
        Assert.That(zeroGenerator.EnergyNotDeliveredAsElectricalOutputJoules,
            Is.EqualTo(zeroGenerator.ShaftWorkJoules));
    }

    [Test]
    public void Observe_IsDeterministicForIdenticalInputs()
    {
        var first = ThermalPowerConversion.Observe(2_500_000d, 800d, 290d, 0.42d, 0.96d);
        var second = ThermalPowerConversion.Observe(2_500_000d, 800d, 290d, 0.42d, 0.96d);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Observe_RejectsInvalidInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThermalPowerConversion.Observe(-1d, 600d, 300d, 0.5d, 0.9d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThermalPowerConversion.Observe(1d, 0d, 0d, 0.5d, 0.9d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThermalPowerConversion.Observe(1d, 300d, 300d, 0.5d, 0.9d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThermalPowerConversion.Observe(1d, 600d, 300d, 1.1d, 0.9d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ThermalPowerConversion.Observe(1d, 600d, 300d, 0.5d, -0.1d));
    }
}
