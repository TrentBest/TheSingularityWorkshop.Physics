namespace TheSingularityWorkshop.Physics.Thermodynamics;

/// <summary>
/// A lumped thermal body's state under a constant-specific-heat approximation.
/// Temperature is in kelvin, mass in kilograms, and specific heat capacity in joules
/// per kilogram-kelvin. This model tracks sensible energy relative to a declared
/// reference temperature; it does not model phase change, spatial gradients, or
/// temperature-dependent material properties.
/// </summary>
/// <param name="MassKilograms">Positive body mass in kilograms.</param>
/// <param name="TemperatureKelvin">Absolute temperature in kelvin; must be non-negative.</param>
/// <param name="SpecificHeatCapacityJoulesPerKilogramKelvin">
/// Positive constant specific heat capacity in J/(kg·K), valid only over the caller's declared range.
/// </param>
/// <param name="ReferenceTemperatureKelvin">
/// Reference temperature in kelvin for the reported sensible energy; must be non-negative.
/// </param>
public readonly record struct LumpedThermalState(
    double MassKilograms,
    double TemperatureKelvin,
    double SpecificHeatCapacityJoulesPerKilogramKelvin,
    double ReferenceTemperatureKelvin)
{
    /// <summary>Returns true when the state and all thermal properties are physically admissible and finite.</summary>
    public bool IsValid =>
        double.IsFinite(MassKilograms) && MassKilograms > 0d &&
        double.IsFinite(TemperatureKelvin) && TemperatureKelvin >= 0d &&
        double.IsFinite(SpecificHeatCapacityJoulesPerKilogramKelvin) &&
        SpecificHeatCapacityJoulesPerKilogramKelvin > 0d &&
        double.IsFinite(ReferenceTemperatureKelvin) && ReferenceTemperatureKelvin >= 0d &&
        double.IsFinite(MassKilograms * SpecificHeatCapacityJoulesPerKilogramKelvin) &&
        MassKilograms * SpecificHeatCapacityJoulesPerKilogramKelvin > 0d &&
        double.IsFinite(SensibleEnergyRelativeToReferenceJoules);

    /// <summary>
    /// Sensible energy relative to the reference temperature, in joules:
    /// mass × constant specific heat capacity × (temperature − reference temperature).
    /// This is a model-relative energy difference, not a claim of absolute internal energy.
    /// </summary>
    public double SensibleEnergyRelativeToReferenceJoules =>
        MassKilograms *
        SpecificHeatCapacityJoulesPerKilogramKelvin *
        (TemperatureKelvin - ReferenceTemperatureKelvin);

    /// <summary>
    /// Applies a net energy transfer to this lumped body and returns its new state.
    /// Positive joules enter the body (heating); negative joules leave it (cooling).
    /// Assumes constant specific heat capacity and no phase transition.
    /// </summary>
    /// <param name="NetEnergyTransferJoules">Net energy transferred into the body, in joules.</param>
    /// <exception cref="ArgumentException">The state or energy transfer is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The transfer would produce a temperature below absolute zero.</exception>
    /// <exception cref="OverflowException">The resulting energy or temperature is outside finite double range.</exception>
    public LumpedThermalState ApplyNetEnergyTransfer(double NetEnergyTransferJoules)
    {
        if (!IsValid)
        {
            throw new ArgumentException("The thermal state and its properties must be valid.", nameof(LumpedThermalState));
        }

        if (!double.IsFinite(NetEnergyTransferJoules))
        {
            throw new ArgumentException("The net energy transfer must be finite.", nameof(NetEnergyTransferJoules));
        }

        var resultingEnergy = SensibleEnergyRelativeToReferenceJoules + NetEnergyTransferJoules;
        if (!double.IsFinite(resultingEnergy))
        {
            throw new OverflowException("The resulting sensible energy is outside finite double range.");
        }

        var resultingTemperature =
            ReferenceTemperatureKelvin +
            resultingEnergy / (MassKilograms * SpecificHeatCapacityJoulesPerKilogramKelvin);

        if (!double.IsFinite(resultingTemperature))
        {
            throw new OverflowException("The resulting temperature is outside finite double range.");
        }

        if (resultingTemperature < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(NetEnergyTransferJoules),
                "The energy transfer would lower the body below absolute zero.");
        }

        return this with { TemperatureKelvin = resultingTemperature };
    }
}
