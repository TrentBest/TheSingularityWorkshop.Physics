namespace TheSingularityWorkshop.Physics.Materials;

/// <summary>Strength of the evidence linking an assembly part to a material identity.</summary>
public enum MaterialReferenceEvidenceKind
{
    /// <summary>A source explicitly names a stable material identifier.</summary>
    ExplicitReference,
    /// <summary>A structured source field declares a material identity.</summary>
    StructuredDeclaration,
    /// <summary>A known, configured alias identifies a material.</summary>
    KnownAlias,
    /// <summary>A descriptive string mentions a possible material.</summary>
    DescriptiveText
}

/// <summary>Outcome of resolving the material identity for one part.</summary>
public enum MaterialIdentityResolutionStatus
{
    /// <summary>One material identity is supported by the strongest available evidence.</summary>
    Resolved,
    /// <summary>Strongest available evidence identifies multiple materials.</summary>
    Ambiguous,
    /// <summary>No available evidence identifies a known material.</summary>
    Unresolved
}

/// <summary>
/// Evidence that a specific assembly part may be made from a material.
/// This describes identity only; it does not supply material properties.
/// </summary>
public sealed record MaterialReferenceEvidence(
    string PartId,
    string? MaterialId,
    string? ObservedTerm,
    MaterialReferenceEvidenceKind Kind,
    string SourceBundleId,
    string SourceFieldId)
{
    /// <summary>Whether required identity and provenance fields are present.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(PartId) &&
        Enum.IsDefined(Kind) &&
        !string.IsNullOrWhiteSpace(SourceBundleId) &&
        !string.IsNullOrWhiteSpace(SourceFieldId) &&
        (Kind == MaterialReferenceEvidenceKind.DescriptiveText
            ? !string.IsNullOrWhiteSpace(ObservedTerm)
            : !string.IsNullOrWhiteSpace(MaterialId));
}

/// <summary>A resolved or explicitly unresolved material identity and its supporting evidence.</summary>
public sealed record MaterialIdentityResolution(
    string PartId,
    MaterialIdentityResolutionStatus Status,
    string? MaterialId,
    IReadOnlyList<MaterialReferenceEvidence> Evidence,
    string Explanation);

/// <summary>
/// Deterministically resolves material identity for independently described parts.
/// Explicit references outrank structured declarations, which outrank known aliases and descriptive text.
/// Conflicting candidates at the strongest applicable level remain ambiguous.
/// </summary>
public sealed class MaterialIdentityArbitrator
{
    private readonly HashSet<string> _knownMaterialIds;
    private readonly Dictionary<string, string[]> _aliases;

    /// <param name="knownMaterialIds">Stable material IDs that the caller has made available.</param>
    /// <param name="aliases">Optional normalized term-to-material-ID aliases. Alias targets must be known IDs.</param>
    public MaterialIdentityArbitrator(
        IEnumerable<string> knownMaterialIds,
        IReadOnlyDictionary<string, string>? aliases = null)
    {
        ArgumentNullException.ThrowIfNull(knownMaterialIds);
        _knownMaterialIds = new HashSet<string>(
            knownMaterialIds.Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.Ordinal);

        _aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (aliases is null)
            return;

        foreach (var pair in aliases)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) ||
                string.IsNullOrWhiteSpace(pair.Value) ||
                !_knownMaterialIds.Contains(pair.Value))
                throw new ArgumentException("Alias terms and their known material targets must be non-empty and valid.", nameof(aliases));

            var key = Normalize(pair.Key);
            _aliases[key] = _aliases.TryGetValue(key, out var existing)
                ? existing.Append(pair.Value).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray()
                : [pair.Value];
        }
    }

    /// <summary>Resolves all supplied evidence into one deterministic result per part.</summary>
    public IReadOnlyList<MaterialIdentityResolution> Arbitrate(IEnumerable<MaterialReferenceEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        var all = evidence.ToArray();
        if (all.Any(item => item is null || !item.IsValid))
            throw new ArgumentException("Evidence must contain valid part identity and source provenance.", nameof(evidence));

        return all.GroupBy(item => item.PartId, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => ResolvePart(group.Key, group.ToArray()))
            .ToArray();
    }

    /// <summary>
    /// Resolves material identity for every declared assembly part, including parts with no material evidence.
    /// Evidence for parts outside the declared assembly is rejected instead of silently attaching it elsewhere.
    /// </summary>
    public PhysicalAssemblyMaterialResolution Arbitrate(
        PhysicalAssemblyDescription assembly,
        IEnumerable<MaterialReferenceEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(evidence);
        assembly.EnsureValid();

        var all = evidence.ToArray();
        if (all.Any(item => item is null || !item.IsValid))
            throw new ArgumentException("Evidence must contain valid part identity and source provenance.", nameof(evidence));

        var partIds = new HashSet<string>(assembly.Parts.Select(part => part.PartId), StringComparer.Ordinal);
        var outsideEvidence = all.FirstOrDefault(item => !partIds.Contains(item.PartId));
        if (outsideEvidence is not null)
            throw new ArgumentException(
                $"Material evidence references part '{outsideEvidence.PartId}', which is not declared in assembly '{assembly.AssemblyId}'.",
                nameof(evidence));

        var byPart = all.GroupBy(item => item.PartId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var results = assembly.Parts
            .OrderBy(part => part.PartId, StringComparer.Ordinal)
            .Select(part => ResolvePart(part.PartId,
                byPart.TryGetValue(part.PartId, out var partEvidence) ? partEvidence : []))
            .ToArray();

        return new PhysicalAssemblyMaterialResolution(assembly.AssemblyId, results);
    }

    private MaterialIdentityResolution ResolvePart(string partId, MaterialReferenceEvidence[] evidence)
    {
        var eligible = new List<(MaterialReferenceEvidence Evidence, string MaterialId, int Rank)>();
        foreach (var item in evidence)
        {
            if (item.Kind == MaterialReferenceEvidenceKind.DescriptiveText)
            {
                var term = Normalize(item.ObservedTerm!);
                if (_aliases.TryGetValue(term, out var targets))
                    eligible.AddRange(targets.Select(target => (item, target, 1)));
                continue;
            }

            if (_knownMaterialIds.Contains(item.MaterialId!))
            {
                var rank = item.Kind switch
                {
                    MaterialReferenceEvidenceKind.ExplicitReference => 3,
                    MaterialReferenceEvidenceKind.StructuredDeclaration => 2,
                    MaterialReferenceEvidenceKind.KnownAlias => 1,
                    _ => 0
                };
                if (rank > 0)
                    eligible.Add((item, item.MaterialId!, rank));
            }
        }

        var unknownReferences = evidence
            .Where(item => item.Kind != MaterialReferenceEvidenceKind.DescriptiveText &&
                !_knownMaterialIds.Contains(item.MaterialId!))
            .Select(item => (Evidence: item, Rank: Rank(item.Kind)))
            .Where(candidate => candidate.Rank > 0)
            .ToArray();

        var strongestKnownRank = eligible.Count == 0 ? 0 : eligible.Max(candidate => candidate.Rank);
        var strongestUnknownRank = unknownReferences.Length == 0 ? 0 : unknownReferences.Max(candidate => candidate.Rank);

        if (strongestUnknownRank > strongestKnownRank)
        {
            var unresolvedEvidence = unknownReferences
                .Where(candidate => candidate.Rank == strongestUnknownRank)
                .Select(candidate => candidate.Evidence);
            return new(partId, MaterialIdentityResolutionStatus.Unresolved, null,
                SortEvidence(unresolvedEvidence),
                "The strongest material reference is not present in the known material catalog; weaker evidence cannot override it, and physical properties remain unspecified.");
        }

        if (eligible.Count == 0)
            return new(partId, MaterialIdentityResolutionStatus.Unresolved, null,
                SortEvidence(evidence), "No evidence identifies a known material; physical properties remain unspecified.");

        var strongestRank = eligible.Max(candidate => candidate.Rank);
        var strongest = eligible.Where(candidate => candidate.Rank == strongestRank)
            .OrderBy(candidate => candidate.MaterialId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Evidence.SourceBundleId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Evidence.SourceFieldId, StringComparer.Ordinal)
            .ToArray();

        if (strongestUnknownRank == strongestRank)
        {
            var contestedEvidence = strongest.Select(candidate => candidate.Evidence)
                .Concat(unknownReferences.Where(candidate => candidate.Rank == strongestRank)
                    .Select(candidate => candidate.Evidence));
            return new(partId, MaterialIdentityResolutionStatus.Ambiguous, null,
                SortEvidence(contestedEvidence),
                "The strongest evidence includes material references absent from the known catalog; no material identity was selected.");
        }
        var materials = strongest.Select(candidate => candidate.MaterialId)
            .Distinct(StringComparer.Ordinal).ToArray();
        var provenance = SortEvidence(strongest.Select(candidate => candidate.Evidence));

        if (materials.Length != 1)
            return new(partId, MaterialIdentityResolutionStatus.Ambiguous, null, provenance,
                "The strongest available evidence conflicts; no material identity was selected.");

        return new(partId, MaterialIdentityResolutionStatus.Resolved, materials[0], provenance,
            strongestRank == 3
                ? "Resolved from an explicit material reference."
                : strongestRank == 2
                    ? "Resolved from a structured material declaration."
                    : "Resolved from a known material alias; descriptive evidence is not treated as a physical property.");
    }

    private static int Rank(MaterialReferenceEvidenceKind kind) => kind switch
    {
        MaterialReferenceEvidenceKind.ExplicitReference => 3,
        MaterialReferenceEvidenceKind.StructuredDeclaration => 2,
        MaterialReferenceEvidenceKind.KnownAlias => 1,
        _ => 0
    };

    private static IReadOnlyList<MaterialReferenceEvidence> SortEvidence(IEnumerable<MaterialReferenceEvidence> evidence) =>
        evidence.OrderBy(item => item.SourceBundleId, StringComparer.Ordinal)
            .ThenBy(item => item.SourceFieldId, StringComparer.Ordinal)
            .ThenBy(item => item.Kind)
            .ThenBy(item => item.MaterialId, StringComparer.Ordinal)
            .ThenBy(item => item.ObservedTerm, StringComparer.Ordinal)
            .ToArray();

    private static string Normalize(string term) =>
        string.Join(' ', term.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
