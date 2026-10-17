namespace TheSingularityWorkshop.Physics.Materials;

/// <summary>
/// Material-property evidence available to one declared physical assembly part.
/// Properties are catalog entries, not automatically selected constitutive laws or solver inputs.
/// </summary>
public sealed record PhysicalPartMaterialPropertyMap(
    string PartId,
    MaterialIdentityResolutionStatus IdentityStatus,
    string? MaterialId,
    IReadOnlyList<MaterialPropertyDefinition> Properties);

/// <summary>
/// Maps resolved material identities in an assembly to matching catalogued property evidence.
/// It deliberately does not infer identity, combine conflicting property values, or select a solver.
/// </summary>
public static class PhysicalAssemblyMaterialPropertyMapper
{
    /// <summary>
    /// Finds all property definitions for each resolved part's exact material ID.
    /// Unresolved and ambiguous identities receive no properties; unrelated catalog entries are ignored.
    /// </summary>
    public static IReadOnlyList<PhysicalPartMaterialPropertyMap> Map(
        PhysicalAssemblyMaterialResolution assemblyResolution,
        IEnumerable<MaterialPropertyDefinition> propertyCatalog)
    {
        ArgumentNullException.ThrowIfNull(assemblyResolution);
        ArgumentNullException.ThrowIfNull(propertyCatalog);

        if (string.IsNullOrWhiteSpace(assemblyResolution.AssemblyId) || assemblyResolution.Parts is null)
            throw new ArgumentException("An assembly material resolution requires an identity and part results.", nameof(assemblyResolution));

        var properties = propertyCatalog.ToArray();
        if (properties.Any(property => property is null || !property.IsValid))
            throw new ArgumentException("Every catalog property must have valid identity, units, validity conditions, and provenance.", nameof(propertyCatalog));

        var byMaterial = properties
            .GroupBy(property => property.MaterialId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<MaterialPropertyDefinition>)group
                    .OrderBy(property => property.PropertyId, StringComparer.Ordinal)
                    .ThenBy(property => property.SourceId, StringComparer.Ordinal)
                    .ThenBy(property => property.SourceVersion, StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        var partIds = new HashSet<string>(StringComparer.Ordinal);
        var results = new List<PhysicalPartMaterialPropertyMap>(assemblyResolution.Parts.Count);
        foreach (var part in assemblyResolution.Parts.OrderBy(part => part.PartId, StringComparer.Ordinal))
        {
            if (part is null || string.IsNullOrWhiteSpace(part.PartId) ||
                !Enum.IsDefined(part.Status) || !partIds.Add(part.PartId))
                throw new ArgumentException("Assembly material results must contain unique, valid part identities and statuses.", nameof(assemblyResolution));

            var isResolved = part.Status == MaterialIdentityResolutionStatus.Resolved &&
                !string.IsNullOrWhiteSpace(part.MaterialId);
            var matched = isResolved && byMaterial.TryGetValue(part.MaterialId!, out var available)
                ? available
                : Array.Empty<MaterialPropertyDefinition>();

            results.Add(new PhysicalPartMaterialPropertyMap(
                part.PartId,
                part.Status,
                isResolved ? part.MaterialId : null,
                matched));
        }

        return results;
    }
}
