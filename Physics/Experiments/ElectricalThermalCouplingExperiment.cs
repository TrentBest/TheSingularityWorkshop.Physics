using TheSingularityWorkshop.Physics.Electricity;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// A bounded coupling experiment that transfers a declared fraction of ideal resistive-load
/// electrical energy into a lumped thermal body over a fixed interval.
/// </summary>
/// <remarks>
/// This is a numerical integration fixture, not a model of a real appliance. The absorbed
/// fraction is an explicit experiment input; losses, resistance variation, thermal exchange
/// with surroundings, phase change, and spatial temperature gradients are not modeled.
/// </remarks>
public static class ElectricalThermalCouplingExperiment
{
    /// <summary>Runs one fixed-interval electrical-to-thermal energy transfer.</summary>
    /// <param name="initialThermalState">Valid lumped thermal state.</param>
    /// <param name="busVoltageVolts">Positive ideal DC bus voltage in volts.</param>
    /// <param name="resistanceOhms">Positive constant load resistance in ohms.</param>
    /// <param name="generationCapacityWatts">Declared DC bus generation capacity in watts.</param>
    /// <param name="durationSeconds">Positive finite duration in seconds.</param>
    /// <param name="absorbedPowerFraction">Declared fraction of electrical energy transferred to the body, from zero to one.</param>
    /// <returns>The electrical observation, energy accounting, and resulting thermal state.</returns>
    public static ElectricalThermalCouplingResult Run(
        LumpedThermalState initialThermalState,
        double busVoltageVolts,
        double resistanceOhms,
        double generationCapacityWatts,
        double durationSeconds,
        double absorbedPowerFraction)
    {
        if (!initialThermalState.IsValid)
            throw new ArgumentException("The initial thermal state must be valid.", nameof(initialThermalState));
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be positive and finite.");
        if (!double.IsFinite(absorbedPowerFraction) || absorbedPowerFraction < 0d || absorbedPowerFraction > 1d)
            throw new ArgumentOutOfRangeException(nameof(absorbedPowerFraction), "Absorbed fraction must be finite and within [0, 1].");

        var load = new ResistiveElectricalLoad("coupled-load", resistanceOhms);
        var electrical = ElectricalGrid.Observe(busVoltageVolts, generationCapacityWatts, [load]);
        var electricalEnergy = electrical.DemandWatts * durationSeconds;
        if (!double.IsFinite(electricalEnergy))
            throw new OverflowException("Electrical energy over the requested interval is not finite.");

        var transferredEnergy = electricalEnergy * absorbedPowerFraction;
        if (!double.IsFinite(transferredEnergy))
            throw new OverflowException("Transferred thermal energy is not finite.");

        var finalThermalState = initialThermalState.ApplyNetEnergyTransfer(transferredEnergy);
        return new ElectricalThermalCouplingResult(
            electrical,
            durationSeconds,
            absorbedPowerFraction,
            electricalEnergy,
            transferredEnergy,
            electricalEnergy - transferredEnergy,
            initialThermalState,
            finalThermalState);
    }
}

/// <summary>Immutable observations from one electrical-to-thermal coupling experiment.</summary>
public sealed record ElectricalThermalCouplingResult(
    ElectricalGridObservation ElectricalObservation,
    double DurationSeconds,
    double AbsorbedPowerFraction,
    double ElectricalEnergyJoules,
    double ThermalEnergyTransferJoules,
    double EnergyNotTransferredToThermalBodyJoules,
    LumpedThermalState InitialThermalState,
    LumpedThermalState FinalThermalState);
