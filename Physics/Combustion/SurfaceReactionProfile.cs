namespace TheSingularityWorkshop.Physics.Combustion;

using TheSingularityWorkshop.Physics.Materials;

/// <summary>
/// Source-attributed molecular species data used to validate a declared reaction equation.
/// Atom counts are per molecule; molar mass is in kg/mol.
/// </summary>
public sealed record SurfaceReactionSpeciesDefinition(
    string SpeciesId,
    double MolarMassKilogramsPerMole,
    IReadOnlyDictionary<string, int> ElementAtomCounts,
    PropertyEvidenceStatus EvidenceStatus,
    string SourceId,
    string SourceVersion)
{
    /// <summary>Whether species identity, positive molar mass, composition, and provenance are valid.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(SpeciesId) &&
        double.IsFinite(MolarMassKilogramsPerMole) &&
        MolarMassKilogramsPerMole > 0d &&
        ElementAtomCounts is { Count: > 0 } &&
        ElementAtomCounts.All(x => IsElementSymbol(x.Key) && x.Value > 0) &&
        Enum.IsDefined(EvidenceStatus) &&
        !string.IsNullOrWhiteSpace(SourceId) &&
        !string.IsNullOrWhiteSpace(SourceVersion);

    /// <summary>Throws when species data are invalid.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Reaction species requires a positive finite molar mass, valid elemental composition, evidence status, and source attribution.", nameof(SurfaceReactionSpeciesDefinition));
    }

    private static bool IsElementSymbol(string? symbol) =>
        symbol is { Length: >= 1 and <= 2 } &&
        symbol[0] is >= 'A' and <= 'Z' &&
        (symbol.Length == 1 || symbol[1] is >= 'a' and <= 'z');
}

/// <summary>A positive integer stoichiometric coefficient for one declared species.</summary>
public sealed record SurfaceReactionStoichiometricTerm(
    SurfaceReactionSpeciesDefinition Species,
    int Coefficient)
{
    /// <summary>Whether the referenced species is valid and the coefficient is positive.</summary>
    public bool IsValid => Species is not null && Species.IsValid && Coefficient > 0;
}

/// <summary>
/// An explicit reaction equation with elemental and molar-mass balance validation.
/// This is a reaction-data contract, not a kinetics solver or a claim that a reaction occurs.
/// </summary>
/// <param name="ReactionId">Stable reaction identity.</param>
/// <param name="Reactants">Species and positive integer coefficients on the left side.</param>
/// <param name="Products">Species and positive integer coefficients on the right side.</param>
/// <param name="HeatReleaseJoulesPerMoleOfReaction">Declared non-negative heat release per mole of reaction extent, in J/mol.</param>
/// <param name="SourceId">Source or fixture identity for the reaction data.</param>
/// <param name="SourceVersion">Version of the source or fixture.</param>
/// <param name="ValidConditions">Declared conditions and applicability limits.</param>
/// <param name="RelativeMassBalanceTolerance">Relative tolerance for the independent molar-mass balance check.</param>
public sealed record SurfaceReactionProfile(
    string ReactionId,
    IReadOnlyList<SurfaceReactionStoichiometricTerm> Reactants,
    IReadOnlyList<SurfaceReactionStoichiometricTerm> Products,
    double HeatReleaseJoulesPerMoleOfReaction,
    string SourceId,
    string SourceVersion,
    string ValidConditions,
    double RelativeMassBalanceTolerance = 1e-6d)
{
    /// <summary>Whether the declared equation is structurally valid and balances atoms and molar mass.</summary>
    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ReactionId) ||
                Reactants is not { Count: > 0 } ||
                Products is not { Count: > 0 } ||
                !double.IsFinite(HeatReleaseJoulesPerMoleOfReaction) ||
                HeatReleaseJoulesPerMoleOfReaction < 0d ||
                string.IsNullOrWhiteSpace(SourceId) ||
                string.IsNullOrWhiteSpace(SourceVersion) ||
                string.IsNullOrWhiteSpace(ValidConditions) ||
                !double.IsFinite(RelativeMassBalanceTolerance) ||
                RelativeMassBalanceTolerance < 0d ||
                !TermsAreValidAndUnique(Reactants) ||
                !TermsAreValidAndUnique(Products))
                return false;

            var reactantAtoms = CountAtoms(Reactants);
            var productAtoms = CountAtoms(Products);
            if (reactantAtoms is null || productAtoms is null ||
                reactantAtoms.Count != productAtoms.Count ||
                reactantAtoms.Any(x => !productAtoms.TryGetValue(x.Key, out var count) || count != x.Value))
                return false;

            var reactantMass = Reactants.Sum(x => x.Coefficient * x.Species.MolarMassKilogramsPerMole);
            var productMass = Products.Sum(x => x.Coefficient * x.Species.MolarMassKilogramsPerMole);
            var scale = Math.Max(Math.Abs(reactantMass), Math.Abs(productMass));
            return double.IsFinite(reactantMass) &&
                double.IsFinite(productMass) &&
                scale > 0d &&
                Math.Abs(reactantMass - productMass) <= RelativeMassBalanceTolerance * scale;
        }
    }

    /// <summary>Throws unless the reaction equation balances elemental counts and molar mass.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Reaction profile must contain valid unique terms and balance both elemental composition and molar mass within tolerance.", nameof(SurfaceReactionProfile));
    }

    private static bool TermsAreValidAndUnique(IReadOnlyList<SurfaceReactionStoichiometricTerm> terms)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var term in terms)
        {
            if (term is null || !term.IsValid || !ids.Add(term.Species.SpeciesId))
                return false;
        }

        return true;
    }

    private static Dictionary<string, long>? CountAtoms(IReadOnlyList<SurfaceReactionStoichiometricTerm> terms)
    {
        var totals = new Dictionary<string, long>(StringComparer.Ordinal);
        try
        {
            foreach (var term in terms)
            foreach (var atom in term.Species.ElementAtomCounts)
            {
                var amount = checked((long)term.Coefficient * atom.Value);
                totals[atom.Key] = checked(totals.GetValueOrDefault(atom.Key) + amount);
            }
        }
        catch (OverflowException)
        {
            return null;
        }

        return totals;
    }
}
