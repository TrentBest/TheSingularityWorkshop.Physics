namespace TheSingularityWorkshop.Physics.Combustion;

using TheSingularityWorkshop.Physics.Materials;

/// <summary>
/// Energy ledger for a bounded chemical-energy release event. This is bookkeeping, not a
/// blast-pressure, impulse, detonation, or structural-failure solver.
/// </summary>
public sealed record CombustionEnergyReleaseBudget(
    string MaterialId,
    double InitialFuelMassKilograms,
    double ReactedFuelMassKilograms,
    double SpecificChemicalEnergyJoulesPerKilogram,
    double ChemicalEnergyAvailableJoules,
    double ChemicalEnergyReleasedJoules,
    double MechanicalEnergyJoules,
    double ThermalEnergyJoules,
    double RadiantEnergyJoules,
    double UnallocatedReleasedEnergyJoules,
    double RemainingChemicalEnergyJoules);

/// <summary>
/// Computes an auditable upper-bound chemical-energy ledger from declared fuel energy content
/// and the mass actually reacted. Energy partition fractions are explicit inputs, not inferred
/// explosion physics. Fractions left unassigned remain visible as unallocated energy.
/// </summary>
public static class CombustionEnergyReleaseCalculator
{
    /// <summary>
    /// Calculates stored and released chemical energy without mutating fuel state or predicting
    /// pressure, blast impulse, fragment motion, or structural damage.
    /// </summary>
    public static CombustionEnergyReleaseBudget Calculate(
        string materialId,
        double initialFuelMassKilograms,
        double reactedFuelMassKilograms,
        double specificChemicalEnergyJoulesPerKilogram,
        MaterialPropertyDefinition energyEvidence,
        double mechanicalEnergyFraction,
        double thermalEnergyFraction,
        double radiantEnergyFraction)
    {
        if (string.IsNullOrWhiteSpace(materialId))
            throw new ArgumentException("Material identity is required.", nameof(materialId));
        ArgumentNullException.ThrowIfNull(energyEvidence);
        energyEvidence.EnsureValid();

        if (!string.Equals(energyEvidence.MaterialId, materialId, StringComparison.Ordinal) ||
            !string.Equals(energyEvidence.PropertyId, "specific-chemical-energy", StringComparison.Ordinal) ||
            !string.Equals(energyEvidence.Unit, "J/kg", StringComparison.Ordinal) ||
            !string.Equals(energyEvidence.ValidityQuantity, "temperature", StringComparison.Ordinal) ||
            !string.Equals(energyEvidence.ValidityUnit, "K", StringComparison.Ordinal))
            throw new ArgumentException(
                "Energy evidence must describe this material's specific-chemical-energy in J/kg with temperature applicability metadata.",
                nameof(energyEvidence));

        if (!double.IsFinite(specificChemicalEnergyJoulesPerKilogram) ||
            specificChemicalEnergyJoulesPerKilogram <= 0d ||
            !double.IsFinite(energyEvidence.Value) ||
            energyEvidence.Value <= 0d ||
            !NearlyEqual(specificChemicalEnergyJoulesPerKilogram, energyEvidence.Value))
            throw new ArgumentOutOfRangeException(
                nameof(specificChemicalEnergyJoulesPerKilogram),
                "The supplied positive specific energy must match the source-attributed material property.");

        if (!double.IsFinite(initialFuelMassKilograms) || initialFuelMassKilograms < 0d)
            throw new ArgumentOutOfRangeException(nameof(initialFuelMassKilograms));
        if (!double.IsFinite(reactedFuelMassKilograms) ||
            reactedFuelMassKilograms < 0d ||
            reactedFuelMassKilograms > initialFuelMassKilograms)
            throw new ArgumentOutOfRangeException(
                nameof(reactedFuelMassKilograms),
                "Reacted fuel mass must be between zero and the initial fuel mass.");

        ValidateFraction(mechanicalEnergyFraction, nameof(mechanicalEnergyFraction));
        ValidateFraction(thermalEnergyFraction, nameof(thermalEnergyFraction));
        ValidateFraction(radiantEnergyFraction, nameof(radiantEnergyFraction));
        var assignedFraction = mechanicalEnergyFraction + thermalEnergyFraction + radiantEnergyFraction;
        if (!double.IsFinite(assignedFraction) || assignedFraction > 1d)
            throw new ArgumentException("Energy partition fractions must sum to no more than one.");

        var available = initialFuelMassKilograms * specificChemicalEnergyJoulesPerKilogram;
        var released = reactedFuelMassKilograms * specificChemicalEnergyJoulesPerKilogram;
        var mechanical = released * mechanicalEnergyFraction;
        var thermal = released * thermalEnergyFraction;
        var radiant = released * radiantEnergyFraction;
        var unallocated = released - mechanical - thermal - radiant;
        var remaining = available - released;
        if (new[] { available, released, mechanical, thermal, radiant, unallocated, remaining }
            .Any(x => !double.IsFinite(x) || x < -Math.Max(1d, available) * 1e-12d))
            throw new ArgumentOutOfRangeException(nameof(initialFuelMassKilograms), "Energy accounting overflowed or produced a negative ledger entry.");

        return new CombustionEnergyReleaseBudget(
            materialId, initialFuelMassKilograms, reactedFuelMassKilograms,
            specificChemicalEnergyJoulesPerKilogram, available, released,
            mechanical, thermal, radiant, Math.Max(0d, unallocated), Math.Max(0d, remaining));
    }

    private static void ValidateFraction(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d || value > 1d)
            throw new ArgumentOutOfRangeException(parameterName, "Energy partition fractions must be finite and in [0, 1].");
    }

    private static bool NearlyEqual(double first, double second) =>
        Math.Abs(first - second) <= Math.Max(Math.Abs(first), Math.Abs(second)) * 1e-12d;
}
