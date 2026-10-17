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
            "material:test-sample",
            "test.property",
            PropertyValueStatus.Known,
            10d,
            12d,
            "test-unit",
            new PropertyConditions(TemperatureKelvin: 293.15d, Phase: "solid"),
            1d,
            Provenance(PropertyEvidenceClass.Illustrative));

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
            "material:test-sample",
            "test.property",
            PropertyValueStatus.Unknown,
            0d,
            0d,
            "test-unit",
            new PropertyConditions(),
            null,
            Provenance(PropertyEvidenceClass.Illustrative));

        Assert.That(property.IsValid, Is.False);
        Assert.Throws<ArgumentException>(property.EnsureValid);
    }

    [Test]
    public void KnownStatusRequiresBothBounds()
    {
        var property = new PropertyValueRecord(
            "material:test-sample",
            "test.property",
            PropertyValueStatus.Known,
            400d,
            null,
            "test-unit",
            new PropertyConditions(),
            null,
            Provenance(PropertyEvidenceClass.Illustrative));

        Assert.That(property.IsValid, Is.False);
    }

    [Test]
    public void ReversedBoundsAreRejected()
    {
        var property = new PropertyValueRecord(
            "material:test-sample",
            "test.property",
            PropertyValueStatus.Known,
            12d,
            10d,
            "test-unit",
            new PropertyConditions(),
            null,
            Provenance(PropertyEvidenceClass.Illustrative));

        Assert.That(property.IsValid, Is.False);
    }

    [Test]
    public void NonFiniteValuesAndInvalidConditionsAreRejected()
    {
        var nonFinite = new PropertyValueRecord(
            "material:test-sample",
            "test.property",
            PropertyValueStatus.Known,
            double.NaN,
            double.NaN,
            "test-unit",
            new PropertyConditions(),
            null,
            Provenance(PropertyEvidenceClass.Illustrative));

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
            "material:test-sample",
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
