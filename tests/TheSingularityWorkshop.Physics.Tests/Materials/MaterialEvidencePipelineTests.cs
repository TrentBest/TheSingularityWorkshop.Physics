using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Materials;

[TestFixture]
public sealed class MaterialEvidencePipelineTests
{
    [Test]
    public void ArrowPipeline_ResolvesPartsMapsOnlyKnownPropertiesAndChecksDeclaredValidity()
    {
        var assembly = new PhysicalAssemblyDescription("arrow", "archery-bundle", "assembly.parts", [
            new PhysicalPartDescription("arrow.tip", null, "archery-bundle", "tip"),
            new PhysicalPartDescription("arrow.shaft", null, "wood-parts", "shaft"),
            new PhysicalPartDescription("arrow.fletching", "arrow.shaft", "archery-bundle", "fletching")
        ]);
        var arbitrator = new MaterialIdentityArbitrator(["material:steel", "material:wood"]);
        var evidence = new[]
        {
            new MaterialReferenceEvidence("arrow.tip", "material:steel", null,
                MaterialReferenceEvidenceKind.ExplicitReference, "archery-bundle", "tip.material"),
            new MaterialReferenceEvidence("arrow.shaft", "material:wood", null,
                MaterialReferenceEvidenceKind.StructuredDeclaration, "wood-parts", "shaft.material")
        };
        var catalog = new[]
        {
            Property("material:steel", "density", 7850d, "kg/m^3", "steel-source"),
            Property("material:steel", "specific-heat-capacity", 490d, "J/(kg*K)", "steel-source"),
            Property("material:wood", "density", 600d, "kg/m^3", "wood-fixture")
        };

        var identities = arbitrator.Arbitrate(assembly, evidence);
        var mapped = PhysicalAssemblyMaterialPropertyMapper.Map(identities, catalog);

        var tip = mapped.Single(part => part.PartId == "arrow.tip");
        Assert.That(tip.Properties.Select(property => property.PropertyId),
            Is.EqualTo(new[] { "density", "specific-heat-capacity" }));
        Assert.That(tip.Properties.All(property => property.MaterialId == "material:steel"), Is.True);

        var shaft = mapped.Single(part => part.PartId == "arrow.shaft");
        Assert.That(shaft.Properties.Select(property => property.PropertyId), Is.EqualTo(new[] { "density" }));
        Assert.That(shaft.Properties.Single().MaterialId, Is.EqualTo("material:wood"));

        var fletching = mapped.Single(part => part.PartId == "arrow.fletching");
        Assert.That(fletching.IdentityStatus, Is.EqualTo(MaterialIdentityResolutionStatus.Unresolved));
        Assert.That(fletching.MaterialId, Is.Null);
        Assert.That(fletching.Properties, Is.Empty);

        var temperatureAssessment = MaterialPropertyRangeEvaluator.Assess(
            tip.Properties.Single(property => property.PropertyId == "specific-heat-capacity"),
            "temperature", "K", 300d);
        Assert.That(temperatureAssessment.Status, Is.EqualTo(MaterialPropertyRangeStatus.WithinDeclaredRange));
        Assert.That(temperatureAssessment.EvidenceStatus, Is.EqualTo(PropertyEvidenceStatus.Sourced));
        Assert.That(temperatureAssessment.SourceId, Is.EqualTo("steel-source"));

        var incomparableAssessment = MaterialPropertyRangeEvaluator.Assess(
            tip.Properties.Single(property => property.PropertyId == "specific-heat-capacity"),
            "temperature", "C", 26.85d);
        Assert.That(incomparableAssessment.Status, Is.EqualTo(MaterialPropertyRangeStatus.Incomparable));
    }

    private static MaterialPropertyDefinition Property(
        string materialId, string propertyId, double value, string unit, string sourceId) =>
        new(materialId, propertyId, value, unit, PropertyEvidenceStatus.Sourced,
            sourceId, "fixture-v1", "temperature", "K", 250d, 400d,
            "Test fixture only; range is illustrative and not an empirical material claim.");
}
