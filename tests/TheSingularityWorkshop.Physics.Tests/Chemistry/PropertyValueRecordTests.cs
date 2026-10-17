using NUnit.Framework;
using TheSingularityWorkshop.Physics.Chemistry;

namespace TheSingularityWorkshop.Physics.Tests.Chemistry;

[TestFixture]
public sealed class PropertyValueRecordTests
{
    private static PropertyProvenance Provenance(
        PropertyEvidenceClass evidence = PropertyEvidenceClass.Measured) =>
        new("fixture-dataset", "1.0", "fixture:record-1", "test-only", evidence);

    [Test]
    public void KnownPropertyRequiresFiniteOrderedBoundsAndExplicitUnit()
    {
        var property = new PropertyValueRecord(
            "material:copper",
            "thermal.conductivity",
            PropertyValueStatus.Known,
            390d,
            401d,
            "W/(m·K)",
            new PropertyConditions(TemperatureKelvin: 293.15d, Phase: "solid"),
            2d,
            Provenance());

        Assert.That(property.IsValid, Is.True);
        Assert.DoesNotThrow(property.EnsureValid);
    }

    [TestCase(PropertyValueStatus.Unknown)]
    [TestCase(PropertyValueStatus.NotApplicable)]
    [TestCase(PropertyValueStatus.NotReportedBySource)]
    [TestCase(PropertyValueStatus.OmittedFromProfile)]
    [TestCase(PropertyValueStatus.OutOfModelRange)]
    public void UnavailablePropertyStatesDoNotRequireInventedZero(PropertyValueStatus status)
    {
        var property = new PropertyValueRecord(
            "element:6",
            "nuclear.half-life",
            status,
            null,
            null,
            "s",
            new PropertyConditions(),
            null,
            Provenance(PropertyEvidenceClass.CriticallyEvaluated));

        Assert.That(property.IsValid, Is.True);
    }

    [Test]
    public void UnavailableStatusCannotCarryNumericValues()
    {
        var property = new PropertyValueRecord(
            "material:copper",
            "thermal.conductivity",
            PropertyValueStatus.Unknown,
            0d,
            0d,
            "W/(m·K)",
            new PropertyConditions(),
            null,
            Provenance());

        Assert.That(property.IsValid, Is.False);
        Assert.Throws<ArgumentException>(property.EnsureValid);
    }

    [Test]
    public void KnownStatusRequiresBothBounds()
    {
        var property = new PropertyValueRecord(
            "material:copper",
            "thermal.conductivity",
            PropertyValueStatus.Known,
            400d,
            null,
            "W/(m·K)",
            new PropertyConditions(),
            null,
            Provenance());

        Assert.That(property.IsValid, Is.False);
    }

    [Test]
    public void ReversedBoundsAreRejected()
    {
        var property = new PropertyValueRecord(
            "material:copper",
            "thermal.conductivity",
            PropertyValueStatus.Known,
            401d,
            390d,
            "W/(m·K)",
            new PropertyConditions(),
            null,
            Provenance());

        Assert.That(property.IsValid, Is.False);
    }

    [Test]
    public void NonFiniteValuesAndInvalidConditionsAreRejected()
    {
        var nonFinite = new PropertyValueRecord(
            "material:copper",
            "thermal.conductivity",
            PropertyValueStatus.Known,
            double.NaN,
            double.NaN,
            "W/(m·K)",
            new PropertyConditions(),
            null,
            Provenance());

        var invalidConditions = nonFinite with
        {
            MinimumValue = 1d,
            MaximumValue = 1d,
            Conditions = new PropertyConditions(TemperatureKelvin: -1d)
        };

        Assert.That(nonFinite.IsValid, Is.False);
        Assert.That(invalidConditions.IsValid, Is.False);
    }

    [Test]
    public void MissingProvenanceIsRejected()
    {
        var property = new PropertyValueRecord(
            "material:copper",
            "density",
            PropertyValueStatus.Known,
            8900d,
            8900d,
            "kg/m³",
            new PropertyConditions(),
            null,
            new PropertyProvenance("", "1", "fixture:record", "test-only", PropertyEvidenceClass.Measured));

        Assert.That(property.IsValid, Is.False);
    }
}
