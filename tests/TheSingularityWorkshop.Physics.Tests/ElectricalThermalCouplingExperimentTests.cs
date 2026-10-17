using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ElectricalThermalCouplingExperimentTests
{
    [Test]
    public void Run_TransfersDeclaredFractionOfElectricalEnergyAndClosesAccounting()
    {
        var initial = CreateThermalState(300d);

        var result = ElectricalThermalCouplingExperiment.Run(
            initial, busVoltageVolts: 12d, resistanceOhms: 6d,
            generationCapacityWatts: 100d, durationSeconds: 10d,
            absorbedPowerFraction: 0.5d);

        // 12 V / 6 ohm = 2 A; P = V I = 24 W; E = P dt = 240 J.
        Assert.That(result.ElectricalObservation.TotalCurrentAmperes, Is.EqualTo(2d).Within(1e-12));
        Assert.That(result.ElectricalObservation.DemandWatts, Is.EqualTo(24d).Within(1e-12));
        Assert.That(result.ElectricalEnergyJoules, Is.EqualTo(240d).Within(1e-12));
        Assert.That(result.ThermalEnergyTransferJoules, Is.EqualTo(120d).Within(1e-12));
        Assert.That(result.EnergyNotTransferredToThermalBodyJoules, Is.EqualTo(120d).Within(1e-12));
        Assert.That(
            result.ThermalEnergyTransferJoules + result.EnergyNotTransferredToThermalBodyJoules,
            Is.EqualTo(result.ElectricalEnergyJoules).Within(1e-12));
        Assert.That(result.FinalThermalState.TemperatureKelvin, Is.EqualTo(300.12d).Within(1e-10));
        Assert.That(result.InitialThermalState, Is.EqualTo(initial));
    }

    [Test]
    public void Run_ZeroAbsorbedFractionLeavesThermalBodyUnchanged()
    {
        var initial = CreateThermalState(300d);

        var result = ElectricalThermalCouplingExperiment.Run(
            initial, 12d, 6d, 100d, 10d, absorbedPowerFraction: 0d);

        Assert.That(result.ThermalEnergyTransferJoules, Is.Zero);
        Assert.That(result.FinalThermalState, Is.EqualTo(initial));
        Assert.That(result.EnergyNotTransferredToThermalBodyJoules, Is.EqualTo(240d).Within(1e-12));
    }

    [Test]
    public void Run_IsDeterministicForIdenticalInputs()
    {
        var initial = CreateThermalState(300d);

        var first = ElectricalThermalCouplingExperiment.Run(initial, 12d, 6d, 100d, 10d, 0.25d);
        var second = ElectricalThermalCouplingExperiment.Run(initial, 12d, 6d, 100d, 10d, 0.25d);

        Assert.That(second.ElectricalEnergyJoules, Is.EqualTo(first.ElectricalEnergyJoules));
        Assert.That(second.ThermalEnergyTransferJoules, Is.EqualTo(first.ThermalEnergyTransferJoules));
        Assert.That(second.FinalThermalState, Is.EqualTo(first.FinalThermalState));
        Assert.That(second.ElectricalObservation.Loads.Select(load => load.PowerWatts),
            Is.EqualTo(first.ElectricalObservation.Loads.Select(load => load.PowerWatts)));
    }

    [Test]
    public void Run_RejectsInvalidDurationAndAbsorbedFraction()
    {
        var initial = CreateThermalState(300d);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ElectricalThermalCouplingExperiment.Run(initial, 12d, 6d, 100d, 0d, 0.5d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ElectricalThermalCouplingExperiment.Run(initial, 12d, 6d, 100d, 10d, -0.1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ElectricalThermalCouplingExperiment.Run(initial, 12d, 6d, 100d, 10d, 1.1d));
        Assert.Throws<ArgumentException>(() =>
            ElectricalThermalCouplingExperiment.Run(default, 12d, 6d, 100d, 10d, 0.5d));
    }

    [Test]
    public void Run_RejectsOverloadAndInvalidElectricalInputsThroughGridContract()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ElectricalThermalCouplingExperiment.Run(CreateThermalState(300d), 0d, 6d, 100d, 10d, 0.5d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ElectricalThermalCouplingExperiment.Run(CreateThermalState(300d), 12d, 0d, 100d, 10d, 0.5d));

        var overloaded = ElectricalThermalCouplingExperiment.Run(
            CreateThermalState(300d), 12d, 6d, 1d, 10d, 0.5d);
        Assert.That(overloaded.ElectricalObservation.IsOverloaded, Is.True);
    }

    private static LumpedThermalState CreateThermalState(double temperatureKelvin) =>
        new(2d, temperatureKelvin, 500d, 273.15d);
}
