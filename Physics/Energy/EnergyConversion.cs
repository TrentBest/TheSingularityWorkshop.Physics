namespace TheSingularityWorkshop.Physics.Energy;

/// <summary>
/// Calculates a bounded energy-conversion observation for a constant-power process.
/// </summary>
/// <remarks>
/// Efficiency is the declared fraction of input power delivered as useful output power
/// across the selected system boundary. The remaining energy is not automatically heat:
/// it may represent thermal rejection, mechanical loss, electrical loss, or another
/// unmodelled destination. A technology-specific model must describe that destination.
/// This calculator does not model dynamics, transient response, or an actual machine.
/// </remarks>
public static class EnergyConversion
{
    /// <summary>
    /// Observes constant input power converted at a declared efficiency over a fixed interval.
    /// </summary>
    /// <param name="inputPowerWatts">Non-negative input power in watts.</param>
    /// <param name="efficiencyFraction">Useful-output efficiency in the closed interval [0, 1].</param>
    /// <param name="durationSeconds">Positive duration in seconds.</param>
    /// <returns>Power and energy observations across the declared conversion boundary.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An input is outside its valid range.</exception>
    /// <exception cref="OverflowException">A calculated power or energy is not finite.</exception>
    public static EnergyConversionObservation Observe(
        double inputPowerWatts,
        double efficiencyFraction,
        double durationSeconds)
    {
        if (!double.IsFinite(inputPowerWatts) || inputPowerWatts < 0d)
            throw new ArgumentOutOfRangeException(nameof(inputPowerWatts), "Input power must be non-negative and finite.");
        if (!double.IsFinite(efficiencyFraction) || efficiencyFraction < 0d || efficiencyFraction > 1d)
            throw new ArgumentOutOfRangeException(nameof(efficiencyFraction), "Efficiency must be finite and within [0, 1].");
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be positive and finite.");

        var usefulOutputPower = inputPowerWatts * efficiencyFraction;
        var notUsefulOutputPower = inputPowerWatts - usefulOutputPower;
        var inputEnergy = inputPowerWatts * durationSeconds;
        var usefulOutputEnergy = usefulOutputPower * durationSeconds;
        var notUsefulOutputEnergy = inputEnergy - usefulOutputEnergy;

        if (!double.IsFinite(usefulOutputPower) ||
            !double.IsFinite(notUsefulOutputPower) ||
            !double.IsFinite(inputEnergy) ||
            !double.IsFinite(usefulOutputEnergy) ||
            !double.IsFinite(notUsefulOutputEnergy))
        {
            throw new OverflowException("The calculated energy-conversion observation is outside the finite double range.");
        }

        return new EnergyConversionObservation(
            inputPowerWatts,
            efficiencyFraction,
            durationSeconds,
            usefulOutputPower,
            notUsefulOutputPower,
            inputEnergy,
            usefulOutputEnergy,
            notUsefulOutputEnergy);
    }
}

/// <summary>
/// Immutable energy and power accounting across one declared conversion boundary.
/// </summary>
/// <param name="InputPowerWatts">Input power in watts.</param>
/// <param name="EfficiencyFraction">Declared useful-output efficiency in [0, 1].</param>
/// <param name="DurationSeconds">Observation duration in seconds.</param>
/// <param name="UsefulOutputPowerWatts">Useful output power in watts.</param>
/// <param name="PowerNotDeliveredAsUsefulOutputWatts">Input power not delivered as useful output, in watts.</param>
/// <param name="InputEnergyJoules">Input energy in joules.</param>
/// <param name="UsefulOutputEnergyJoules">Useful output energy in joules.</param>
/// <param name="EnergyNotDeliveredAsUsefulOutputJoules">Input energy not delivered as useful output, in joules.</param>
public sealed record EnergyConversionObservation(
    double InputPowerWatts,
    double EfficiencyFraction,
    double DurationSeconds,
    double UsefulOutputPowerWatts,
    double PowerNotDeliveredAsUsefulOutputWatts,
    double InputEnergyJoules,
    double UsefulOutputEnergyJoules,
    double EnergyNotDeliveredAsUsefulOutputJoules);
