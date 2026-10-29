namespace TheSingularityWorkshop.Physics.Combustion;

using System.Collections.ObjectModel;

/// <summary>
/// Immutable per-cell chemical species inventory. Masses are in kilograms and keyed by stable
/// species identity; the inventory does not assign chemistry or infer species definitions.
/// </summary>
public sealed class SurfaceReactionInventory
{
    private readonly IReadOnlyDictionary<string, double> _speciesMassKilograms;

    /// <summary>Stable identity of the surface cell that owns this inventory.</summary>
    public string CellId { get; }

    /// <summary>Species mass in kilograms, keyed by stable species ID.</summary>
    public IReadOnlyDictionary<string, double> SpeciesMassKilograms => _speciesMassKilograms;

    /// <summary>Whether the cell identity and every species mass are valid.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(CellId) &&
        _speciesMassKilograms.All(x =>
            !string.IsNullOrWhiteSpace(x.Key) &&
            double.IsFinite(x.Value) &&
            x.Value >= 0d);

    /// <summary>Creates a copied inventory so caller mutations cannot change this state.</summary>
    public SurfaceReactionInventory(
        string cellId,
        IReadOnlyDictionary<string, double> speciesMassKilograms)
    {
        ArgumentNullException.ThrowIfNull(speciesMassKilograms);
        CellId = cellId;
        var copy = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var item in speciesMassKilograms)
        {
            if (string.IsNullOrWhiteSpace(item.Key) ||
                !double.IsFinite(item.Value) ||
                item.Value < 0d)
                throw new ArgumentException(
                    "Species IDs must be non-empty and masses must be finite and non-negative.",
                    nameof(speciesMassKilograms));
            copy.Add(item.Key, item.Value);
        }

        _speciesMassKilograms = new ReadOnlyDictionary<string, double>(copy);
        EnsureValid();
    }

    /// <summary>Throws when the inventory does not contain a valid cell identity and masses.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Reaction inventory must have a cell ID and valid species masses.", nameof(SurfaceReactionInventory));
    }
}

/// <summary>
/// Immutable result of applying one bounded reaction to a cell inventory.
/// Heat is reported but not applied to a thermal state by this chemical-inventory operation.
/// </summary>
public sealed record SurfaceReactionInventoryTransitionResult(
    SurfaceReactionInventory InitialInventory,
    SurfaceReactionInventory FinalInventory,
    SurfaceReactionExtentResult ReactionExtent);

/// <summary>
/// Applies a validated single-reaction extent to an immutable per-cell species inventory.
/// This is not a kinetics solver and is not yet coupled to SurfaceCombustionStepper.
/// </summary>
public static class SurfaceReactionInventoryStepper
{
    /// <summary>
    /// Calculates a bounded extent, subtracts consumed reactants, and accumulates generated
    /// products into a new inventory. The input inventory is never mutated.
    /// </summary>
    public static SurfaceReactionInventoryTransitionResult Apply(
        SurfaceReactionProfile profile,
        SurfaceReactionInventory inventory,
        double requestedExtentMoles)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(inventory);
        profile.EnsureValid();
        inventory.EnsureValid();

        var availableReactants = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var reactant in profile.Reactants)
            availableReactants.Add(
                reactant.Species.SpeciesId,
                inventory.SpeciesMassKilograms.GetValueOrDefault(reactant.Species.SpeciesId));

        var extent = SurfaceReactionExtentCalculator.Calculate(profile, availableReactants, requestedExtentMoles);
        var nextMasses = new Dictionary<string, double>(
            inventory.SpeciesMassKilograms, StringComparer.Ordinal);

        foreach (var consumed in extent.ReactantMassConsumedKilograms)
        {
            var remaining = nextMasses.GetValueOrDefault(consumed.Key) - consumed.Value;
            if (!double.IsFinite(remaining))
                throw new ArgumentOutOfRangeException(nameof(requestedExtentMoles), "Reactant accounting produced a non-finite inventory.");
            nextMasses[consumed.Key] = Math.Max(0d, remaining);
        }

        foreach (var generated in extent.ProductMassGeneratedKilograms)
        {
            var accumulated = nextMasses.GetValueOrDefault(generated.Key) + generated.Value;
            if (!double.IsFinite(accumulated))
                throw new ArgumentOutOfRangeException(nameof(requestedExtentMoles), "Product accounting produced a non-finite inventory.");
            nextMasses[generated.Key] = accumulated;
        }

        var finalInventory = new SurfaceReactionInventory(inventory.CellId, nextMasses);
        return new SurfaceReactionInventoryTransitionResult(inventory, finalInventory, extent);
    }
}
