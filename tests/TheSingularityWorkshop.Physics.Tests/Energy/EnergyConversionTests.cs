using NUnit.Framework;
using TheSingularityWorkshop.Physics.Energy;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class EnergyConversionTests
{
    [Test]
    public void Observe_AccountsForUsefulOutputAndEnergyNotDeliveredAsUsefulOutput()
    {
        var result = EnergyConversion.Observe(
            inputPowerWatts: 1_000d,
            efficiencyFraction: 0.4d,
            durationSeconds: 30d);

        Assert.That(result.UsefulOutputPowerWatts, Is.EqualTo(400d));
        Assert.That(result.PowerNotDeliveredAsUsefulOutputWatts, Is.EqualTo(600d));
        Assert.That(result.InputEnergyJoules, Is.EqualTo(30_000d));
        Assert.That(result.UsefulOutputEnergyJoules, Is.EqualTo(12_000d));
        Assert.That(result.EnergyNotDeliveredAsUsefulOutputJoules, Is.EqualTo(18_000d));
        Assert.That(
            result.UsefulOutputEnergyJoules + result.EnergyNotDeliveredAsUsefulOutputJoules,
            Is.EqualTo(result.InputEnergyJoules));
    }

    [TestCase(0d, 0d, 1d)]
    [TestCase(500d, 0d, 2d)]
    [TestCase(500d, 1d, 2d)]
    public void Observe_HandlesEfficiencyBoundaries(double inputPower, double efficiency, double duration)
    {
        var result = EnergyConversion.Observe(inputPower, efficiency, duration);

        Assert.That(result.UsefulOutputPowerWatts, Is.EqualTo(inputPower * efficiency));
        Assert.That(result.PowerNotDeliveredAsUsefulOutputWatts,
            Is.EqualTo(inputPower - inputPower * efficiency));
    }

    [Test]
    public void Observe_IsDeterministicForIdenticalInputs()
    {
        var first = EnergyConversion.Observe(12_500d, 0.37d, 60d);
        var second = EnergyConversion.Observe(12_500d, 0.37d, 60d);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Observe_RejectsInvalidInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConversion.Observe(-1d, 0.5d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConversion.Observe(double.NaN, 0.5d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConversion.Observe(1d, -0.1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConversion.Observe(1d, 1.1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => EnergyConversion.Observe(1d, 0.5d, 0d));
    }

    [Test]
    public void Observe_RejectsOverflowingEnergy()
    {
        Assert.Throws<OverflowException>(() =>
            EnergyConversion.Observe(double.MaxValue, 1d, 2d));
    }
}
