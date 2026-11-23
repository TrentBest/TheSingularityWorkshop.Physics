using NUnit.Framework;
using TheSingularityWorkshop.Physics.Energy;

namespace TheSingularityWorkshop.Physics.Tests.Energy;

[TestFixture]
public sealed class EnergyStorageStateTests
{
    [Test]
    public void Charge_AccountsForEfficiencyAndStoredEnergy()
    {
        var initial = CreateState(storedEnergy: 0d);
        var result = initial.Charge(requestedInputPowerWatts: 1_000d, durationSeconds: 10d);

        Assert.That(result.RequestedInputEnergyJoules, Is.EqualTo(10_000d));
        Assert.That(result.AcceptedInputEnergyJoules, Is.EqualTo(10_000d));
        Assert.That(result.StoredEnergyIncreaseJoules, Is.EqualTo(9_000d));
        Assert.That(result.InputEnergyNotStoredJoules, Is.EqualTo(1_000d));
        Assert.That(result.InputEnergyNotAcceptedJoules, Is.Zero);
        Assert.That(result.StoredEnergyIncreaseJoules + result.InputEnergyNotStoredJoules,
            Is.EqualTo(result.AcceptedInputEnergyJoules));
        Assert.That(result.AcceptedInputEnergyJoules + result.InputEnergyNotAcceptedJoules,
            Is.EqualTo(result.RequestedInputEnergyJoules));
        Assert.That(result.FinalState.StoredEnergyJoules, Is.EqualTo(9_000d));
        Assert.That(result.FinalState.StateOfChargeFraction, Is.EqualTo(0.09d));
        Assert.That(initial.StoredEnergyJoules, Is.Zero);
    }

    [Test]
    public void Charge_RespectsMaximumChargePower()
    {
        var result = CreateState(storedEnergy: 0d).Charge(
            requestedInputPowerWatts: 2_000d,
            durationSeconds: 10d);

        Assert.That(result.RequestedInputEnergyJoules, Is.EqualTo(20_000d));
        Assert.That(result.AcceptedInputEnergyJoules, Is.EqualTo(5_000d));
        Assert.That(result.StoredEnergyIncreaseJoules, Is.EqualTo(4_500d));
        Assert.That(result.InputEnergyNotStoredJoules, Is.EqualTo(500d));
        Assert.That(result.InputEnergyNotAcceptedJoules, Is.EqualTo(15_000d));
    }

    [Test]
    public void Charge_StopsAtCapacityWithoutOverfilling()
    {
        var result = CreateState(storedEnergy: 98_000d).Charge(
            requestedInputPowerWatts: 1_000d,
            durationSeconds: 10d);

        Assert.That(result.StoredEnergyIncreaseJoules, Is.EqualTo(2_000d));
        Assert.That(result.AcceptedInputEnergyJoules, Is.EqualTo(2_222.222222222222d).Within(1e-9));
        Assert.That(result.FinalState.StoredEnergyJoules, Is.EqualTo(100_000d));
        Assert.That(result.InputEnergyNotAcceptedJoules, Is.EqualTo(7_777.777777777778d).Within(1e-9));
    }

    [Test]
    public void Discharge_AccountsForEfficiencyAndStoredEnergyRemoval()
    {
        var result = CreateState(storedEnergy: 10_000d).Discharge(
            requestedOutputPowerWatts: 1_000d,
            durationSeconds: 5d);

        Assert.That(result.RequestedOutputEnergyJoules, Is.EqualTo(5_000d));
        Assert.That(result.DeliveredOutputEnergyJoules, Is.EqualTo(5_000d));
        Assert.That(result.StoredEnergyRemovedJoules, Is.EqualTo(5_555.555555555556d).Within(1e-9));
        Assert.That(result.StoredEnergyNotDeliveredAsOutputJoules, Is.EqualTo(555.555555555556d).Within(1e-9));
        Assert.That(result.FinalState.StoredEnergyJoules, Is.EqualTo(4_444.444444444444d).Within(1e-9));
    }

    [Test]
    public void Discharge_RespectsMaximumPowerAndAvailableInventory()
    {
        var powerLimited = CreateState(storedEnergy: 50_000d, maximumDischargePower: 250d).Discharge(2_000d, 10d);
        var inventoryLimited = CreateState(storedEnergy: 1_000d).Discharge(1_000d, 10d);

        Assert.That(powerLimited.DeliveredOutputEnergyJoules, Is.EqualTo(2_500d));
        Assert.That(powerLimited.OutputEnergyNotDeliveredJoules, Is.EqualTo(17_500d));
        Assert.That(inventoryLimited.DeliveredOutputEnergyJoules, Is.EqualTo(900d));
        Assert.That(inventoryLimited.StoredEnergyRemovedJoules, Is.EqualTo(1_000d));
        Assert.That(inventoryLimited.FinalState.StoredEnergyJoules, Is.Zero);
    }

    [Test]
    public void ChargeAndDischarge_AreDeterministicAndReturnNewStates()
    {
        var initial = CreateState(storedEnergy: 25_000d);
        var first = initial.Charge(250d, 20d);
        var second = initial.Charge(250d, 20d);

        Assert.That(second, Is.EqualTo(first));
        Assert.That(initial.StoredEnergyJoules, Is.EqualTo(25_000d));
    }

    [Test]
    public void TransferMethods_RejectInvalidStateAndInputs()
    {
        var state = CreateState(storedEnergy: 10_000d);

        Assert.Throws<ArgumentException>(() => (state with { StoredEnergyJoules = 101_000d }).Charge(1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Charge(-1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Charge(1d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Discharge(double.NaN, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Discharge(1d, double.PositiveInfinity));
    }

    [Test]
    public void TransferMethods_RejectOverflowingRequestedEnergy()
    {
        var state = CreateState(storedEnergy: 10_000d);

        Assert.Throws<OverflowException>(() => state.Charge(double.MaxValue, 2d));
        Assert.Throws<OverflowException>(() => state.Discharge(double.MaxValue, 2d));
    }

    private static EnergyStorageState CreateState(double storedEnergy, double maximumDischargePower = 2_000d) =>
        new(
            StoredEnergyJoules: storedEnergy,
            CapacityJoules: 100_000d,
            MaximumChargePowerWatts: 500d,
            MaximumDischargePowerWatts: maximumDischargePower,
            ChargeEfficiencyFraction: 0.9d,
            DischargeEfficiencyFraction: 0.9d);
}
