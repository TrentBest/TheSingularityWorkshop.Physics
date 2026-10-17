using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Materials;

[TestFixture]
public sealed class MaterialModelCapabilityMatcherTests
{
    [Test]
    public void Assess_ReportsReadyWhenAllRequiredPropertiesAndUnitsExist()
    {
        var part = Part("sword.blade", "material:steel",
            Property("material:steel", "density", 7850d, "kg/m^3"),
            Property("material:steel", "yield-strength", 250e6, "Pa"));
        var capability = Capability(
            new MaterialPropertyRequirement("yield-strength", "Pa"),
            new MaterialPropertyRequirement("density", "kg/m^3"));

        var result = MaterialModelCapabilityMatcher.Assess(part, capability);

        Assert.That(result.Status, Is.EqualTo(MaterialModelCapabilityStatus.Ready));
        Assert.That(result.SatisfiedPropertyIds, Is.EqualTo(new[] { "density", "yield-strength" }));
        Assert.That(result.MissingPropertyIds, Is.Empty);
        Assert.That(result.IncompatiblePropertyIds, Is.Empty);
        Assert.That(result.SourceId, Is.EqualTo("mechanics-bundle"));
        Assert.That(result.SourceVersion, Is.EqualTo("2"));
    }

    [Test]
    public void Assess_ReportsMissingRequirementsWithoutInventingEvidence()
    {
        var part = Part("sword.blade", "material:steel",
            Property("material:steel", "density", 7850d, "kg/m^3"));
        var capability = Capability(
            new MaterialPropertyRequirement("density", "kg/m^3"),
            new MaterialPropertyRequirement("fracture-toughness", "MPa*sqrt(m)"));

        var result = MaterialModelCapabilityMatcher.Assess(part, capability);

        Assert.That(result.Status, Is.EqualTo(MaterialModelCapabilityStatus.MissingRequirements));
        Assert.That(result.SatisfiedPropertyIds, Is.EqualTo(new[] { "density" }));
        Assert.That(result.MissingPropertyIds, Is.EqualTo(new[] { "fracture-toughness" }));
    }

    [Test]
    public void Assess_ReportsUnitMismatchAsIncompatibleInsteadOfConverting()
    {
        var part = Part("sword.guard", "material:copper",
            Property("material:copper", "density", 8960d, "kg/m^3"));
        var capability = Capability(new MaterialPropertyRequirement("density", "g/cm^3"));

        var result = MaterialModelCapabilityMatcher.Assess(part, capability);

        Assert.That(result.Status, Is.EqualTo(MaterialModelCapabilityStatus.IncompatibleRequirements));
        Assert.That(result.IncompatiblePropertyIds, Is.EqualTo(new[] { "density" }));
    }

    [Test]
    public void Assess_UnresolvedIdentityCannotSatisfyMaterialRequirements()
    {
        var part = new PhysicalPartMaterialPropertyMap(
            "sword.grip", MaterialIdentityResolutionStatus.Unresolved, null, []);
        var result = MaterialModelCapabilityMatcher.Assess(
            part, Capability(new MaterialPropertyRequirement("density", "kg/m^3")));

        Assert.That(result.Status, Is.EqualTo(MaterialModelCapabilityStatus.MissingRequirements));
        Assert.That(result.MaterialId, Is.Null);
        Assert.That(result.MissingPropertyIds, Is.EqualTo(new[] { "density" }));
    }

    [Test]
    public void Assess_ReportsBothMissingAndIncompatibleRequirements()
    {
        var part = Part("sword.guard", "material:copper",
            Property("material:copper", "density", 8960d, "kg/m^3"));
        var capability = Capability(
            new MaterialPropertyRequirement("density", "g/cm^3"),
            new MaterialPropertyRequirement("elastic-modulus", "Pa"));

        var result = MaterialModelCapabilityMatcher.Assess(part, capability);

        Assert.That(result.Status, Is.EqualTo(MaterialModelCapabilityStatus.MissingAndIncompatibleRequirements));
        Assert.That(result.IncompatiblePropertyIds, Is.EqualTo(new[] { "density" }));
        Assert.That(result.MissingPropertyIds, Is.EqualTo(new[] { "elastic-modulus" }));
    }

    [Test]
    public void Assess_RejectsDuplicateRequirements()
    {
        var capability = Capability(
            new MaterialPropertyRequirement("density"),
            new MaterialPropertyRequirement("density"));

        Assert.Throws<ArgumentException>(() => MaterialModelCapabilityMatcher.Assess(
            Part("blade", "material:steel", Property("material:steel", "density", 7850d, "kg/m^3")),
            capability));
    }

    private static PhysicalPartMaterialPropertyMap Part(
        string partId, string materialId, params MaterialPropertyDefinition[] properties) =>
        new(partId, MaterialIdentityResolutionStatus.Resolved, materialId, properties);

    private static MaterialModelCapability Capability(params MaterialPropertyRequirement[] requirements) =>
        new("model:structural-yield-check", requirements, "mechanics-bundle", "2");

    private static MaterialPropertyDefinition Property(
        string materialId, string propertyId, double value, string unit) =>
        new(materialId, propertyId, value, unit, PropertyEvidenceStatus.Illustrative,
            "fixture-" + materialId, "1", "temperature", "K", 250d, 400d,
            "Illustrative test fixture only; not an empirical material claim.");
}
