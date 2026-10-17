using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Materials;

[TestFixture]
public sealed class MaterialPropertyDefinitionTests
{
    [Test]
    public void IsValid_AcceptsAPropertyWithExplicitUnitsRangeConditionsAndProvenance()
    {
        var property = CreateProperty();

        Assert.That(property.IsValid, Is.True);
        Assert.DoesNotThrow(property.EnsureValid);
    }

    [Test]
    public void IsValid_RejectsAnInvertedValidityConditionRange()
    {
        var property = CreateProperty() with { ValidMinimum = 373.15d, ValidMaximum = 273.15d };

        Assert.That(property.IsValid, Is.False);
        Assert.Throws<ArgumentException>(property.EnsureValid);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void IsValid_RejectsMissingSourceIdentity(string sourceId)
    {
        var property = CreateProperty() with { SourceId = sourceId };

        Assert.That(property.IsValid, Is.False);
    }

    [Test]
    public void IsValid_RejectsNegativeOrNonFiniteUncertainty()
    {
        Assert.That((CreateProperty() with { StandardUncertainty = -0.1d }).IsValid, Is.False);
        Assert.That((CreateProperty() with { StandardUncertainty = double.NaN }).IsValid, Is.False);
        Assert.That((CreateProperty() with { StandardUncertainty = double.PositiveInfinity }).IsValid, Is.False);
    }

    [Test]
    public void IsValid_RejectsUnknownEvidenceStatus()
    {
        var property = CreateProperty() with { EvidenceStatus = (PropertyEvidenceStatus)999 };

        Assert.That(property.IsValid, Is.False);
    }

    private static MaterialPropertyDefinition CreateProperty() => new(
        MaterialId: "reference-thermal-body",
        PropertyId: "specific-heat-capacity",
        Value: 500d,
        Unit: "J/(kg*K)",
        EvidenceStatus: PropertyEvidenceStatus.Illustrative,
        SourceId: "workshop.electrical-thermal-fixture",
        SourceVersion: "1",
        ValidityQuantity: "temperature",
        ValidityUnit: "K",
        ValidMinimum: 273.15d,
        ValidMaximum: 373.15d,
        ValidConditions: "Illustrative constant value over the example temperature interval.",
        StandardUncertainty: null);
}
