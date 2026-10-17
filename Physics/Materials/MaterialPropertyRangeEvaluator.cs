namespace TheSingularityWorkshop.Physics.Materials;

/// <summary>
/// Result of comparing a requested condition with a property's declared numeric validity range.
/// This does not interpret free-text conditions or perform unit conversion.
/// </summary>
public enum MaterialPropertyRangeStatus
{
    /// <summary>The quantity and unit match exactly and the value lies within the inclusive range.</summary>
    WithinDeclaredRange,
    /// <summary>The quantity and unit match exactly but the value lies outside the declared range.</summary>
    OutsideDeclaredRange,
    /// <summary>The requested quantity or unit does not exactly match the property's validity condition.</summary>
    Incomparable
}

/// <summary>Inspectable range-only assessment for one material-property record.</summary>
/// <param name="PropertyId">Stable property identity.</param>
/// <param name="MaterialId">Stable material identity.</param>
/// <param name="Status">Outcome of the exact quantity/unit and numeric-range comparison.</param>
/// <param name="ObservedQuantity">Quantity supplied for assessment.</param>
/// <param name="ObservedUnit">Unit supplied for assessment.</param>
/// <param name="ObservedValue">Numeric value supplied for assessment.</param>
/// <param name="Explanation">Reason for the outcome.</param>
public sealed record MaterialPropertyRangeAssessment(
    string PropertyId,
    string MaterialId,
    MaterialPropertyRangeStatus Status,
    string ObservedQuantity,
    string ObservedUnit,
    double ObservedValue,
    string Explanation);

/// <summary>
/// Checks a property's declared numeric validity range without guessing unit conversions or interpreting prose.
/// A passing result means only that this declared range check passed, not that every free-text condition or
/// constitutive-model assumption is satisfied.
/// </summary>
public static class MaterialPropertyRangeEvaluator
{
    /// <summary>Assesses one property record against a requested validity quantity, unit, and value.</summary>
    public static MaterialPropertyRangeAssessment Assess(
        MaterialPropertyDefinition property,
        string observedQuantity,
        string observedUnit,
        double observedValue)
    {
        ArgumentNullException.ThrowIfNull(property);
        property.EnsureValid();

        if (string.IsNullOrWhiteSpace(observedQuantity))
            throw new ArgumentException("Observed quantity must be specified.", nameof(observedQuantity));
        if (string.IsNullOrWhiteSpace(observedUnit))
            throw new ArgumentException("Observed unit must be specified.", nameof(observedUnit));
        if (!double.IsFinite(observedValue))
            throw new ArgumentOutOfRangeException(nameof(observedValue), "Observed value must be finite.");

        if (!string.Equals(property.ValidityQuantity, observedQuantity, StringComparison.Ordinal) ||
            !string.Equals(property.ValidityUnit, observedUnit, StringComparison.Ordinal))
        {
            return new MaterialPropertyRangeAssessment(
                property.PropertyId,
                property.MaterialId,
                MaterialPropertyRangeStatus.Incomparable,
                observedQuantity,
                observedUnit,
                observedValue,
                "Observed quantity and unit must exactly match the property's declared validity quantity and unit; no unit conversion or condition inference is performed.");
        }

        var withinRange = observedValue >= property.ValidMinimum && observedValue <= property.ValidMaximum;
        return new MaterialPropertyRangeAssessment(
            property.PropertyId,
            property.MaterialId,
            withinRange
                ? MaterialPropertyRangeStatus.WithinDeclaredRange
                : MaterialPropertyRangeStatus.OutsideDeclaredRange,
            observedQuantity,
            observedUnit,
            observedValue,
            withinRange
                ? "Observed value is within the inclusive declared numeric validity range. Other validity conditions still require separate evaluation."
                : "Observed value is outside the declared numeric validity range.");
    }
}
