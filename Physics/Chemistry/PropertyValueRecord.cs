namespace TheSingularityWorkshop.Physics.Chemistry;

/// <summary>
/// Describes whether a property value is available for the declared subject and scope.
/// Missing values must not be represented as zero.
/// </summary>
public enum PropertyValueStatus
{
    Known = 0,
    Unknown = 1,
    NotApplicable = 2,
    NotReportedBySource = 3,
    OmittedFromProfile = 4,
    OutOfModelRange = 5
}

/// <summary>
/// Declares the epistemic category of a sourced or derived property value.
/// </summary>
public enum PropertyEvidenceClass
{
    Measured = 0,
    CriticallyEvaluated = 1,
    Calculated = 2,
    Estimated = 3,
    Inferred = 4,
    Illustrative = 5
}

/// <summary>
/// Conditions and scope needed to interpret a property fact.
/// This intentionally stores declared context; it does not perform condition matching.
/// </summary>
/// <param name="TemperatureKelvin">Temperature in kelvin, when the source declares a single temperature.</param>
/// <param name="PressurePascals">Pressure in pascals, when the source declares a single pressure.</param>
/// <param name="Phase">Declared phase or phase convention, if applicable.</param>
/// <param name="Composition">Stable composition/material descriptor or identifier, if applicable.</param>
/// <param name="Method">Measurement or calculation method, if supplied.</param>
/// <param name="Notes">Additional source conditions that do not yet have a dedicated typed field.</param>
public sealed record PropertyConditions(
    double? TemperatureKelvin = null,
    double? PressurePascals = null,
    string? Phase = null,
    string? Composition = null,
    string? Method = null,
    string? Notes = null)
{
    /// <summary>Whether numeric condition fields are finite and physically within their basic domains.</summary>
    public bool IsValid =>
        (TemperatureKelvin is null ||
            (double.IsFinite(TemperatureKelvin.Value) && TemperatureKelvin.Value >= 0d)) &&
        (PressurePascals is null ||
            (double.IsFinite(PressurePascals.Value) && PressurePascals.Value >= 0d));
}

/// <summary>
/// Provenance for one property fact. Licensing and citation apply to the actual source record,
/// not merely to the element or material that references it.
/// </summary>
public sealed record PropertyProvenance(
    string SourceId,
    string SourceVersion,
    string SourceReference,
    string LicenseId,
    PropertyEvidenceClass EvidenceClass)
{
    /// <summary>Whether required provenance fields and evidence classification are present.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(SourceId) &&
        !string.IsNullOrWhiteSpace(SourceVersion) &&
        !string.IsNullOrWhiteSpace(SourceReference) &&
        !string.IsNullOrWhiteSpace(LicenseId) &&
        Enum.IsDefined(EvidenceClass);
}

/// <summary>
/// A sourced, condition-qualified property fact or an explicit statement about its availability.
/// Values and uncertainty use the declared unit. This is a data contract, not a property solver.
/// </summary>
/// <param name="SubjectId">Stable identity of the element, nuclide, material, or other declared subject.</param>
/// <param name="PropertyId">Stable property identifier, such as a namespaced schema key.</param>
/// <param name="Status">Whether a value is available and applicable.</param>
/// <param name="MinimumValue">Lower bound; equal to maximum for a point value.</param>
/// <param name="MaximumValue">Upper bound; equal to minimum for a point value.</param>
/// <param name="Unit">Explicit unit identifier; never inferred from the property name.</param>
/// <param name="Conditions">Conditions and scope under which this fact applies.</param>
/// <param name="Uncertainty">Optional non-negative uncertainty expressed in the same unit.</param>
/// <param name="Provenance">Source, version, reference, license and evidence class.</param>
public sealed record PropertyValueRecord(
    string SubjectId,
    string PropertyId,
    PropertyValueStatus Status,
    double? MinimumValue,
    double? MaximumValue,
    string Unit,
    PropertyConditions Conditions,
    double? Uncertainty,
    PropertyProvenance Provenance)
{
    /// <summary>Whether identity, status/value consistency, units, conditions and provenance are valid.</summary>
    public bool IsValid
    {
        get
        {
            var statusIsDefined = Enum.IsDefined(Status);
            var valueBoundsAreValid =
                (MinimumValue is null || double.IsFinite(MinimumValue.Value)) &&
                (MaximumValue is null || double.IsFinite(MaximumValue.Value)) &&
                (MinimumValue is null || MaximumValue is null ||
                    MinimumValue.Value <= MaximumValue.Value);
            var hasValue = MinimumValue is not null && MaximumValue is not null;
            var statusMatchesValue = Status == PropertyValueStatus.Known
                ? hasValue
                : MinimumValue is null && MaximumValue is null;

            return !string.IsNullOrWhiteSpace(SubjectId) &&
                !string.IsNullOrWhiteSpace(PropertyId) &&
                statusIsDefined &&
                statusMatchesValue &&
                valueBoundsAreValid &&
                !string.IsNullOrWhiteSpace(Unit) &&
                Conditions is not null && Conditions.IsValid &&
                (Uncertainty is null ||
                    (double.IsFinite(Uncertainty.Value) && Uncertainty.Value >= 0d)) &&
                Provenance is not null && Provenance.IsValid;
        }
    }

    /// <summary>Throws when this fact is internally inconsistent or lacks required provenance.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException(
                "Property fact status, bounds, unit, conditions, and provenance must be valid.",
                nameof(PropertyValueRecord));
    }
}
