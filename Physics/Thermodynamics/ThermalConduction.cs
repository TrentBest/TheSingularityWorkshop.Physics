using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Thermodynamics;

/// <summary>
/// Transfers sensible heat between two lumped bodies using one declared thermal-conductivity value.
/// </summary>
/// <remarks>
/// Uses Q = k A (T1 - T2) Δt / L over one explicit interval. The interval is rejected if this
/// update would reverse the bodies' temperature ordering. This is a bounded conduction primitive,
/// not a spatial heat equation, contact-resistance model, or temperature-dependent material solver.
/// </remarks>
public static class ThermalConduction
{
    /// <summary>Calculates one energy-conserving conduction transfer between two lumped thermal bodies.</summary>
    /// <param name="first">First valid thermal body.</param>
    /// <param name="second">Second valid thermal body.</param>
    /// <param name="thermalConductivity">Declared conductivity in W/(m*K), with temperature validity conditions.</param>
    /// <param name="contactAreaSquareMeters">Effective conducting area in square metres.</param>
    /// <param name="separationMeters">Conducting path length in metres.</param>
    /// <param name="durationSeconds">Positive explicit interval in seconds.</param>
    /// <returns>Transfer amount and both resulting thermal states.</returns>
    public static ThermalConductionResult Transfer(
        LumpedThermalState first,
        LumpedThermalState second,
        MaterialPropertyDefinition thermalConductivity,
        double contactAreaSquareMeters,
        double separationMeters,
        double durationSeconds)
    {
        if (!first.IsValid)
            throw new ArgumentException("The first thermal state must be valid.", nameof(first));
        if (!second.IsValid)
            throw new ArgumentException("The second thermal state must be valid.", nameof(second));
        ArgumentNullException.ThrowIfNull(thermalConductivity);
        thermalConductivity.EnsureValid();

        if (!string.Equals(thermalConductivity.PropertyId, "thermal-conductivity", StringComparison.Ordinal))
            throw new ArgumentException("PropertyId must be 'thermal-conductivity'.", nameof(thermalConductivity));
        if (!string.Equals(thermalConductivity.Unit, "W/(m*K)", StringComparison.Ordinal))
            throw new ArgumentException("Thermal conductivity unit must be W/(m*K).", nameof(thermalConductivity));
        if (thermalConductivity.Value <= 0d)
            throw new ArgumentOutOfRangeException(nameof(thermalConductivity), "Thermal conductivity must be positive.");
        if (!string.Equals(thermalConductivity.ValidityQuantity, "temperature", StringComparison.Ordinal)
            || !string.Equals(thermalConductivity.ValidityUnit, "K", StringComparison.Ordinal))
            throw new ArgumentException("Thermal conductivity validity conditions must be expressed as temperature in kelvin.", nameof(thermalConductivity));
        if (first.TemperatureKelvin < thermalConductivity.ValidMinimum
            || first.TemperatureKelvin > thermalConductivity.ValidMaximum
            || second.TemperatureKelvin < thermalConductivity.ValidMinimum
            || second.TemperatureKelvin > thermalConductivity.ValidMaximum)
            throw new ArgumentOutOfRangeException(nameof(thermalConductivity), "Both body temperatures must be inside the property's declared validity range.");
        if (!double.IsFinite(contactAreaSquareMeters) || contactAreaSquareMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(contactAreaSquareMeters), "Conducting area must be finite and positive.");
        if (!double.IsFinite(separationMeters) || separationMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(separationMeters), "Conducting path length must be finite and positive.");
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be finite and positive.");

        var conductance = thermalConductivity.Value * contactAreaSquareMeters / separationMeters;
        if (!double.IsFinite(conductance) || conductance <= 0d)
            throw new OverflowException("Thermal conductance is outside finite positive double range.");

        var temperatureDifference = first.TemperatureKelvin - second.TemperatureKelvin;
        var transferredEnergy = conductance * temperatureDifference * durationSeconds;
        if (!double.IsFinite(transferredEnergy))
            throw new OverflowException("Thermal energy transfer is outside finite double range.");

        var firstHeatCapacity = first.MassKilograms * first.SpecificHeatCapacityJoulesPerKilogramKelvin;
        var secondHeatCapacity = second.MassKilograms * second.SpecificHeatCapacityJoulesPerKilogramKelvin;
        var equilibriumTransfer = Math.Abs(temperatureDifference)
            / (1d / firstHeatCapacity + 1d / secondHeatCapacity);
        var tolerance = 1e-12d * Math.Max(1d, equilibriumTransfer);
        if (Math.Abs(transferredEnergy) > equilibriumTransfer + tolerance)
            throw new InvalidOperationException("The interval would reverse the bodies' temperature ordering; reduce duration.");

        var nextFirst = first.ApplyNetEnergyTransfer(-transferredEnergy);
        var nextSecond = second.ApplyNetEnergyTransfer(transferredEnergy);
        return new ThermalConductionResult(
            conductance,
            durationSeconds,
            transferredEnergy,
            first,
            second,
            nextFirst,
            nextSecond);
    }
}

/// <summary>Immutable result of one lumped-body conduction transfer.</summary>
/// <param name="ThermalConductanceWattsPerKelvin">Conductance kA/L in W/K.</param>
/// <param name="DurationSeconds">Explicit integration interval in seconds.</param>
/// <param name="EnergyTransferredFromFirstToSecondJoules">Signed energy; positive means first to second.</param>
/// <param name="InitialFirst">First body before transfer.</param>
/// <param name="InitialSecond">Second body before transfer.</param>
/// <param name="FinalFirst">First body after transfer.</param>
/// <param name="FinalSecond">Second body after transfer.</param>
public sealed record ThermalConductionResult(
    double ThermalConductanceWattsPerKelvin,
    double DurationSeconds,
    double EnergyTransferredFromFirstToSecondJoules,
    LumpedThermalState InitialFirst,
    LumpedThermalState InitialSecond,
    LumpedThermalState FinalFirst,
    LumpedThermalState FinalSecond);
