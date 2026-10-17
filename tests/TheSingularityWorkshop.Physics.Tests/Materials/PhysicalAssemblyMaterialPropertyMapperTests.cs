using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Materials;

[TestFixture]
public sealed class PhysicalAssemblyMaterialPropertyMapperTests
{
    [Test]
    public void Map_AssociatesSteelAndCopperPropertyEvidenceWithTheirOwnAssemblyParts()
    {
        var resolution = new PhysicalAssemblyMaterialResolution("sword", [
            Resolved("sword.blade", "material:steel"),
            Resolved("sword.guard", "material:copper"),
            new MaterialIdentityResolution("sword.grip", MaterialIdentityResolutionStatus.Unresolved,
                null, [], "No material evidence.")
        ]);
        var catalog = new[]
        {
            Property("material:steel", "density", 7850d, "kg/m^3", "steel-fixture"),
            Property("material:steel", "specific-heat-capacity", 490d, "J/(kg*K)", "steel-fixture"),
            Property("material:copper", "density", 8960d, "kg/m^3", "copper-fixture"),
            Property("material:wood", "density", 600d, "kg/m^3", "wood-fixture")
        };

        var mapped = PhysicalAssemblyMaterialPropertyMapper.Map(resolution, catalog);

        var blade = mapped.Single(part => part.PartId == "sword.blade");
        Assert.That(blade.MaterialId, Is.EqualTo("material:steel"));
        Assert.That(blade.Properties.Select(property => property.PropertyId),
            Is.EqualTo(new[] { "density", "specific-heat-capacity" }));
        Assert.That(blade.Properties.All(property => property.MaterialId == "material:steel"), Is.True);

        var guard = mapped.Single(part => part.PartId == "sword.guard");
        Assert.That(guard.MaterialId, Is.EqualTo("material:copper"));
        Assert.That(guard.Properties.Select(property => property.PropertyId), Is.EqualTo(new[] { "density" }));
        Assert.That(guard.Properties.All(property => property.MaterialId == "material:copper"), Is.True);

        var grip = mapped.Single(part => part.PartId == "sword.grip");
        Assert.That(grip.IdentityStatus, Is.EqualTo(MaterialIdentityResolutionStatus.Unresolved));
        Assert.That(grip.MaterialId, Is.Null);
        Assert.That(grip.Properties, Is.Empty);
    }

    [Test]
    public void Map_DoesNotAttachPropertiesToAmbiguousIdentity()
    {
        var resolution = new PhysicalAssemblyMaterialResolution("sword", [
            new MaterialIdentityResolution("sword.blade", MaterialIdentityResolutionStatus.Ambiguous,
                null, [], "Conflicting explicit references.")
        ]);

        var mapped = PhysicalAssemblyMaterialPropertyMapper.Map(resolution, [
            Property("material:steel", "density", 7850d, "kg/m^3", "steel-fixture")
        ]);

        Assert.That(mapped.Single().MaterialId, Is.Null);
        Assert.That(mapped.Single().Properties, Is.Empty);
        Assert.That(mapped.Single().IdentityStatus, Is.EqualTo(MaterialIdentityResolutionStatus.Ambiguous));
    }

    [Test]
    public void Map_RejectsInvalidCatalogProperties()
    {
        var resolution = new PhysicalAssemblyMaterialResolution("sword", [
            Resolved("sword.blade", "material:steel")
        ]);
        var invalid = Property("material:steel", "density", double.NaN, "kg/m^3", "fixture");

        Assert.Throws<ArgumentException>(() =>
            PhysicalAssemblyMaterialPropertyMapper.Map(resolution, [invalid]));
    }

    [Test]
    public void Map_RejectsResolvedPartWithoutMaterialIdentity()
    {
        var invalid = new PhysicalAssemblyMaterialResolution("sword", [
            new MaterialIdentityResolution("sword.blade", MaterialIdentityResolutionStatus.Resolved,
                null, [], "Malformed result.")
        ]);

        Assert.Throws<ArgumentException>(() =>
            PhysicalAssemblyMaterialPropertyMapper.Map(invalid, Array.Empty<MaterialPropertyDefinition>()));
    }


    [Test]
    public void Map_RejectsNonResolvedPartThatClaimsMaterialIdentity()
    {
        var invalid = new PhysicalAssemblyMaterialResolution("sword", [
            new MaterialIdentityResolution("sword.blade", MaterialIdentityResolutionStatus.Ambiguous,
                "material:steel", [], "Malformed ambiguous result.")
        ]);

        Assert.Throws<ArgumentException>(() =>
            PhysicalAssemblyMaterialPropertyMapper.Map(invalid, Array.Empty<MaterialPropertyDefinition>()));
    }

    [Test]
    public void Map_RejectsDuplicatePartIdentities()
    {
        var invalid = new PhysicalAssemblyMaterialResolution("sword", [
            Resolved("sword.blade", "material:steel"),
            Resolved("sword.blade", "material:copper")
        ]);

        Assert.Throws<ArgumentException>(() =>
            PhysicalAssemblyMaterialPropertyMapper.Map(invalid, Array.Empty<MaterialPropertyDefinition>()));
    }

    private static MaterialIdentityResolution Resolved(string partId, string materialId) =>
        new(partId, MaterialIdentityResolutionStatus.Resolved, materialId, [],
            "Resolved by test fixture.");

    private static MaterialPropertyDefinition Property(
        string materialId, string propertyId, double value, string unit, string sourceId) =>
        new(materialId, propertyId, value, unit, PropertyEvidenceStatus.Illustrative,
            sourceId, "1", "temperature", "K", 250d, 400d,
            "Illustrative fixture only; not an empirical material claim.");
}
