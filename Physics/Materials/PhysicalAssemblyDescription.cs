namespace TheSingularityWorkshop.Physics.Materials;

/// <summary>
/// An inspectable assembly declaration contributed by a source bundle.
/// Part relationships are explicit; the declaration does not infer geometry or physical properties.
/// </summary>
public sealed record PhysicalAssemblyDescription(
    string AssemblyId,
    string SourceBundleId,
    string SourceFieldId,
    IReadOnlyList<PhysicalPartDescription> Parts)
{
    /// <summary>Validates identities, provenance, parent references, and acyclic part relationships.</summary>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(AssemblyId) ||
            string.IsNullOrWhiteSpace(SourceBundleId) ||
            string.IsNullOrWhiteSpace(SourceFieldId) ||
            Parts is null || Parts.Count == 0)
            throw new ArgumentException("An assembly requires an identity, source provenance, and at least one part.", nameof(PhysicalAssemblyDescription));

        if (Parts.Any(part => part is null || !part.IsValid))
            throw new ArgumentException("Every part requires identity and source provenance.", nameof(PhysicalAssemblyDescription));

        var byId = new Dictionary<string, PhysicalPartDescription>(StringComparer.Ordinal);
        foreach (var part in Parts)
            if (!byId.TryAdd(part.PartId, part))
                throw new ArgumentException($"Duplicate part identity '{part.PartId}' in assembly '{AssemblyId}'.", nameof(PhysicalAssemblyDescription));

        foreach (var part in Parts)
            if (part.ParentPartId is not null &&
                (!byId.ContainsKey(part.ParentPartId) || string.Equals(part.PartId, part.ParentPartId, StringComparison.Ordinal)))
                throw new ArgumentException($"Part '{part.PartId}' references a missing or self parent.", nameof(PhysicalAssemblyDescription));

        var completed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var part in Parts)
        {
            var path = new HashSet<string>(StringComparer.Ordinal);
            var current = part;
            while (current.ParentPartId is not null && !completed.Contains(current.PartId))
            {
                if (!path.Add(current.PartId))
                    throw new ArgumentException($"Part relationships in assembly '{AssemblyId}' contain a cycle.", nameof(PhysicalAssemblyDescription));
                current = byId[current.ParentPartId];
            }

            completed.UnionWith(path);
        }
    }
}

/// <summary>A part and its optional parent part, as explicitly declared by a source bundle.</summary>
public sealed record PhysicalPartDescription(
    string PartId,
    string? ParentPartId,
    string SourceBundleId,
    string SourceFieldId)
{
    /// <summary>Whether part identity and source provenance are present.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(PartId) &&
        (ParentPartId is null || !string.IsNullOrWhiteSpace(ParentPartId)) &&
        !string.IsNullOrWhiteSpace(SourceBundleId) &&
        !string.IsNullOrWhiteSpace(SourceFieldId);
}

/// <summary>Material identity outcomes for every part in a declared assembly.</summary>
public sealed record PhysicalAssemblyMaterialResolution(
    string AssemblyId,
    IReadOnlyList<MaterialIdentityResolution> Parts);
