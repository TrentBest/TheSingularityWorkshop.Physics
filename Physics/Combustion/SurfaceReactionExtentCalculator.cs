namespace TheSingularityWorkshop.Physics.Combustion;

/// <summary>
/// Result of applying one balanced reaction profile to a bounded available-reactant inventory.
/// Inventories are in kilograms and reaction extent is in moles of reaction.
/// </summary>
/// <param name="ReactionId">Stable identity of the applied reaction profile.</param>
/// <param name="RequestedExtentMoles">Requested reaction extent before reactant limits.</param>
/// <param name="ActualExtentMoles">Extent permitted by the available reactant inventory.</param>
/// <param name="LimitingReactantId">First declared reactant that limits the requested extent, or null when the request is not inventory-limited.</param>
/// <param name="ReactantMassConsumedKilograms">Mass consumed for each reactant species.</param>
/// <param name="ProductMassGeneratedKilograms">Mass generated for each product species.</param>
/// <param name="HeatReleasedJoules">Declared reaction heat for the actual extent.</param>
/// <param name="MassBalanceResidualKilograms">Total consumed reactant mass minus total generated product mass.</param>
public sealed record SurfaceReactionExtentResult(
    string ReactionId,
    double RequestedExtentMoles,
    double ActualExtentMoles,
    string? LimitingReactantId,
    IReadOnlyDictionary<string, double> ReactantMassConsumedKilograms,
    IReadOnlyDictionary<string, double> ProductMassGeneratedKilograms,
    double HeatReleasedJoules,
    double MassBalanceResidualKilograms);

/// <summary>
/// Computes a single reaction extent from a validated stoichiometric profile and available
/// reactant masses. This is inventory accounting, not a kinetics law or a time integrator.
/// </summary>
public static class SurfaceReactionExtentCalculator
{
    /// <summary>
    /// Applies at most the requested extent, capped by every available reactant.
    /// Missing reactant inventory entries mean zero available mass; unknown inventory keys are rejected.
    /// </summary>
    public static SurfaceReactionExtentResult Calculate(
        SurfaceReactionProfile profile,
        IReadOnlyDictionary<string, double> availableReactantMassKilograms,
        double requestedExtentMoles)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(availableReactantMassKilograms);
        profile.EnsureValid();

        if (!double.IsFinite(requestedExtentMoles) || requestedExtentMoles < 0d)
            throw new ArgumentOutOfRangeException(nameof(requestedExtentMoles), "Requested reaction extent must be finite and non-negative.");

        var reactantById = profile.Reactants.ToDictionary(x => x.Species.SpeciesId, StringComparer.Ordinal);
        foreach (var inventory in availableReactantMassKilograms)
        {
            if (string.IsNullOrWhiteSpace(inventory.Key) ||
                !reactantById.ContainsKey(inventory.Key) ||
                !double.IsFinite(inventory.Value) ||
                inventory.Value < 0d)
                throw new ArgumentException("Reactant inventory keys must identify declared reactants and masses must be finite and non-negative.", nameof(availableReactantMassKilograms));
        }

        var actualExtent = requestedExtentMoles;
        string? limitingReactantId = null;
        foreach (var term in profile.Reactants)
        {
            var availableMass = availableReactantMassKilograms.GetValueOrDefault(term.Species.SpeciesId);
            var availableExtent = availableMass / term.Species.MolarMassKilogramsPerMole / term.Coefficient;
            if (availableExtent < actualExtent)
            {
                actualExtent = availableExtent;
                limitingReactantId = term.Species.SpeciesId;
            }
        }

        var consumed = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var term in profile.Reactants)
            consumed.Add(term.Species.SpeciesId,
                actualExtent * term.Coefficient * term.Species.MolarMassKilogramsPerMole);

        var generated = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var term in profile.Products)
            generated.Add(term.Species.SpeciesId,
                actualExtent * term.Coefficient * term.Species.MolarMassKilogramsPerMole);

        var consumedTotal = consumed.Values.Sum();
        var generatedTotal = generated.Values.Sum();
        var heatReleased = actualExtent * profile.HeatReleaseJoulesPerMoleOfReaction;
        var massResidual = consumedTotal - generatedTotal;
        if (!double.IsFinite(actualExtent) ||
            consumed.Values.Any(x => !double.IsFinite(x)) ||
            generated.Values.Any(x => !double.IsFinite(x)) ||
            !double.IsFinite(heatReleased) ||
            !double.IsFinite(massResidual))
            throw new ArgumentOutOfRangeException(nameof(requestedExtentMoles), "The requested extent produces a non-finite accounting result.");

        return new SurfaceReactionExtentResult(
            profile.ReactionId,
            requestedExtentMoles,
            actualExtent,
            limitingReactantId,
            consumed,
            generated,
            heatReleased,
            massResidual);
    }
}
