namespace TheSingularityWorkshop.Physics.Materials;

/// <summary>Evidence class for a declared material-property value.</summary>
public enum PropertyEvidenceStatus
{
    /// <summary>Captured from a measurement with supporting measurement provenance.</summary>
    Measured,
    /// <summary>Transcribed from an identified external source.</summary>
    Sourced,
    /// <summary>Inferred or approximated by a stated method.</summary>
    Estimated,
    /// <summary>Chosen for an example or test, not asserted as an empirical material constant.</summary>
    Illustrative
}

/// <summary>
/// A material property with explicit units, validity conditions, and provenance.
/// This is metadata for a declared value, not a constitutive law or a material solver.
/// </summary>
/// <param name="MaterialId">Stable identity for the material or reference material.</param>
/// <param name="PropertyId">Stable identity for the property, such as specific-heat-capacity.</param>
/// <param name="Value">Declared numeric property value in the stated unit.</param>
/// <param name="Unit">Unit symbol or unambiguous unit expression.</param>
/// <param name="EvidenceStatus">How the value should be interpreted epistemically.</param>
/// <param name="SourceId">Source identifier, or a stable fixture identifier for illustrative values.</param>
/// <param name="SourceVersion">Version or revision of the source/fixture.</param>
/// <param name="ValidMinimum">Inclusive lower bound of the property's validity range.</param>
/// <param name="ValidMaximum">Inclusive upper bound of the property's validity range.</param>
/// <param name="ValidConditions">Conditions under which the value/range is intended to apply.</param>
/// <param name="StandardUncertainty">Optional standard uncertainty in the same unit as Value.</param>
public sealed record MaterialPropertyDefinition(
    string MaterialId,
    string PropertyId,
    double Value,
    string Unit,
    PropertyEvidenceStatus EvidenceStatus,
    string SourceId,
    string SourceVersion,
    double ValidMinimum,
    double ValidMaximum,
    string ValidConditions,
    double? StandardUncertainty = null)
{
    /// <summary>Returns whether identity, value, provenance, range, and uncertainty are internally consistent.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(MaterialId) &&
        !string.IsNullOrWhiteSpace(PropertyId) &&
        double.IsFinite(Value) &&
        !string.IsNullOrWhiteSpace(Unit) &&
        Enum.IsDefined(EvidenceStatus) &&
        !string.IsNullOrWhiteSpace(SourceId) &&
        !string.IsNullOrWhiteSpace(SourceVersion) &&
        double.IsFinite(ValidMinimum) &&
        double.IsFinite(ValidMaximum) &&
        ValidMinimum <= ValidMaximum &&
        Value >= ValidMinimum &&
        Value <= ValidMaximum &&
        !string.IsNullOrWhiteSpace(ValidConditions) &&
        (StandardUncertainty is null ||
            (double.IsFinite(StandardUncertainty.Value) && StandardUncertainty.Value >= 0d));

    /// <summary>Throws when this property definition is not internally consistent.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Material property identity, value, unit, provenance, validity range, conditions, or uncertainty is invalid.", nameof(MaterialPropertyDefinition));
    }
}
