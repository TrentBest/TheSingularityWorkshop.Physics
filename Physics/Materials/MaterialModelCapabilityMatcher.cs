namespace TheSingularityWorkshop.Physics.Materials;

/// <summary>A material-property prerequisite declared by a physical model.</summary>
/// <param name="PropertyId">Stable property identity required by the model.</param>
/// <param name="RequiredUnit">Optional exact unit expression required by the model.</param>
public sealed record MaterialPropertyRequirement(string PropertyId, string? RequiredUnit = null)
{
    /// <summary>Whether the requirement has a stable identity and, when supplied, a unit.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(PropertyId) &&
        (RequiredUnit is null || !string.IsNullOrWhiteSpace(RequiredUnit));
}

/// <summary>
/// The material-property prerequisites declared by a model capability.
/// This is a discovery contract, not a constitutive law or solver implementation.
/// </summary>
/// <param name="ModelId">Stable model identity.</param>
/// <param name="Requirements">Required material properties and optional exact units.</param>
/// <param name="SourceId">Provider or bundle contributing this capability.</param>
/// <param name="SourceVersion">Version of the provider or bundle.</param>
public sealed record MaterialModelCapability(
    string ModelId,
    IReadOnlyList<MaterialPropertyRequirement> Requirements,
    string SourceId,
    string SourceVersion)
{
    /// <summary>Validates the model identity, provenance, and unique property prerequisites.</summary>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(ModelId) ||
            string.IsNullOrWhiteSpace(SourceId) ||
            string.IsNullOrWhiteSpace(SourceVersion) ||
            Requirements is null ||
            Requirements.Any(requirement => requirement is null || !requirement.IsValid))
            throw new ArgumentException("A model capability requires identity, provenance, and valid property requirements.", nameof(MaterialModelCapability));

        if (Requirements.Select(requirement => requirement.PropertyId)
            .Distinct(StringComparer.Ordinal).Count() != Requirements.Count)
            throw new ArgumentException("A model capability cannot declare the same property requirement more than once.", nameof(MaterialModelCapability));
    }
}

/// <summary>Outcome of checking a model's declared property prerequisites against one part.</summary>
public enum MaterialModelCapabilityStatus
{
    /// <summary>Every declared property prerequisite has compatible evidence.</summary>
    Ready,
    /// <summary>At least one required property has no evidence for this material.</summary>
    MissingRequirements,
    /// <summary>Properties exist, but none for at least one requirement use its required unit.</summary>
    IncompatibleRequirements,
    /// <summary>Some requirements are missing and others have incompatible units.</summary>
    MissingAndIncompatibleRequirements
}

/// <summary>
/// A deterministic, inspectable material-to-model capability assessment.
/// It reports catalog evidence only; validity conditions, evidence quality, and solver suitability
/// must be assessed separately before a model is executed.
/// </summary>
public sealed record MaterialModelCapabilityAssessment(
    string PartId,
    string? MaterialId,
    string ModelId,
    string SourceId,
    string SourceVersion,
    MaterialModelCapabilityStatus Status,
    IReadOnlyList<string> SatisfiedPropertyIds,
    IReadOnlyList<string> MissingPropertyIds,
    IReadOnlyList<string> IncompatiblePropertyIds);

/// <summary>
/// Checks model property prerequisites against the catalog evidence mapped to a single assembly part.
/// It does not infer material identity, convert units, interpret free-text conditions, or choose a solver.
/// </summary>
public static class MaterialModelCapabilityMatcher
{
    /// <summary>Assesses whether the mapped evidence contains each model-required property and exact unit.</summary>
    public static MaterialModelCapabilityAssessment Assess(
        PhysicalPartMaterialPropertyMap part,
        MaterialModelCapability capability)
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(capability);
        capability.EnsureValid();

        if (string.IsNullOrWhiteSpace(part.PartId) ||
            !Enum.IsDefined(part.IdentityStatus) ||
            (part.IdentityStatus == MaterialIdentityResolutionStatus.Resolved
                ? string.IsNullOrWhiteSpace(part.MaterialId)
                : part.MaterialId is not null) ||
            part.Properties is null ||
            part.Properties.Any(property => property is null || !property.IsValid ||
                !string.Equals(property.MaterialId, part.MaterialId, StringComparison.Ordinal)))
            throw new ArgumentException("Part property mapping must have a valid identity and only valid properties for its resolved material.", nameof(part));

        var satisfied = new List<string>();
        var missing = new List<string>();
        var incompatible = new List<string>();

        foreach (var requirement in capability.Requirements.OrderBy(item => item.PropertyId, StringComparer.Ordinal))
        {
            var candidates = part.IdentityStatus == MaterialIdentityResolutionStatus.Resolved
                ? part.Properties.Where(property =>
                    string.Equals(property.PropertyId, requirement.PropertyId, StringComparison.Ordinal)).ToArray()
                : Array.Empty<MaterialPropertyDefinition>();

            if (candidates.Length == 0)
            {
                missing.Add(requirement.PropertyId);
                continue;
            }

            if (requirement.RequiredUnit is not null &&
                !candidates.Any(property => string.Equals(
                    property.Unit, requirement.RequiredUnit, StringComparison.Ordinal)))
            {
                incompatible.Add(requirement.PropertyId);
                continue;
            }

            satisfied.Add(requirement.PropertyId);
        }

        var status = missing.Count > 0
            ? incompatible.Count > 0
                ? MaterialModelCapabilityStatus.MissingAndIncompatibleRequirements
                : MaterialModelCapabilityStatus.MissingRequirements
            : incompatible.Count > 0
                ? MaterialModelCapabilityStatus.IncompatibleRequirements
                : MaterialModelCapabilityStatus.Ready;

        return new MaterialModelCapabilityAssessment(
            part.PartId,
            part.MaterialId,
            capability.ModelId,
            capability.SourceId,
            capability.SourceVersion,
            status,
            satisfied,
            missing,
            incompatible);
    }
}
