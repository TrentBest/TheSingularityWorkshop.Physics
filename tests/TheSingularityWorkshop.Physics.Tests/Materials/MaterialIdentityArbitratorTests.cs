using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Materials;

[TestFixture]
public sealed class MaterialIdentityArbitratorTests
{
    private static readonly IReadOnlyDictionary<string, string> Aliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["steel"] = "material:steel",
            ["wood"] = "material:wood"
        };

    private static MaterialIdentityArbitrator CreateArbitrator() =>
        new(["material:steel", "material:wood"], Aliases);

    [Test]
    public void Arbitrate_AssemblyKeepsMaterialIdentityIndependentPerPartAndIncludesMissingEvidence()
    {
        var assembly = new PhysicalAssemblyDescription("arrow", "weapon", "assembly.parts", [
            new PhysicalPartDescription("arrow.tip", null, "weapon", "tip"),
            new PhysicalPartDescription("arrow.shaft", null, "parts", "shaft"),
            new PhysicalPartDescription("arrow.fletching", "arrow.shaft", "parts", "fletching")
        ]);

        var result = CreateArbitrator().Arbitrate(assembly, [
            Evidence("arrow.tip", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "weapon", "tip.material"),
            Evidence("arrow.shaft", "material:wood", MaterialReferenceEvidenceKind.StructuredDeclaration, "parts", "shaft.material")
        ]);

        Assert.That(result.AssemblyId, Is.EqualTo("arrow"));
        Assert.That(result.Parts.Select(part => part.PartId),
            Is.EqualTo(new[] { "arrow.fletching", "arrow.shaft", "arrow.tip" }));
        Assert.That(Find(result.Parts, "arrow.tip").MaterialId, Is.EqualTo("material:steel"));
        Assert.That(Find(result.Parts, "arrow.shaft").MaterialId, Is.EqualTo("material:wood"));
        Assert.That(Find(result.Parts, "arrow.fletching").Status, Is.EqualTo(MaterialIdentityResolutionStatus.Unresolved));
        Assert.That(Find(result.Parts, "arrow.fletching").MaterialId, Is.Null);
    }

    [Test]
    public void Arbitrate_AssemblyRejectsEvidenceForUndeclaredPart()
    {
        var assembly = new PhysicalAssemblyDescription("arrow", "weapon", "assembly.parts", [
            new PhysicalPartDescription("arrow.tip", null, "weapon", "tip")
        ]);

        Assert.Throws<ArgumentException>(() => CreateArbitrator().Arbitrate(assembly, [
            Evidence("arrow", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "weapon", "material")
        ]));
    }

    [Test]
    public void PhysicalAssemblyDescription_RejectsCyclicPartRelationships()
    {
        var assembly = new PhysicalAssemblyDescription("arrow", "weapon", "assembly.parts", [
            new PhysicalPartDescription("tip", "shaft", "weapon", "tip.parent"),
            new PhysicalPartDescription("shaft", "tip", "weapon", "shaft.parent")
        ]);

        Assert.Throws<ArgumentException>(() => assembly.EnsureValid());
    }

    [Test]
    public void Arbitrate_ResolvesTipIndependentlyFromShaftAndFletching()
    {
        var result = CreateArbitrator().Arbitrate([
            Evidence("arrow.tip", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "weapon", "tip.material"),
            Evidence("arrow.shaft", "material:wood", MaterialReferenceEvidenceKind.StructuredDeclaration, "parts", "shaft.material"),
            Evidence("arrow.fletching", null, MaterialReferenceEvidenceKind.DescriptiveText, "parts", "fletching.description", "feathered construction")
        ]);

        Assert.That(result.Select(x => x.PartId), Is.EqualTo(new[] { "arrow.fletching", "arrow.shaft", "arrow.tip" }));
        Assert.That(Find(result, "arrow.tip").MaterialId, Is.EqualTo("material:steel"));
        Assert.That(Find(result, "arrow.shaft").MaterialId, Is.EqualTo("material:wood"));
        Assert.That(Find(result, "arrow.fletching").Status, Is.EqualTo(MaterialIdentityResolutionStatus.Unresolved));
        Assert.That(Find(result, "arrow.fletching").MaterialId, Is.Null);
    }

    [Test]
    public void Arbitrate_ExplicitReferenceOutranksContradictoryDescriptiveText()
    {
        var result = CreateArbitrator().Arbitrate([
            Evidence("tip", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "weapon", "materialRef"),
            Evidence("tip", null, MaterialReferenceEvidenceKind.DescriptiveText, "notes", "description", "wood")
        ]);

        Assert.That(Find(result, "tip").Status, Is.EqualTo(MaterialIdentityResolutionStatus.Resolved));
        Assert.That(Find(result, "tip").MaterialId, Is.EqualTo("material:steel"));
    }

    [Test]
    public void Arbitrate_ConflictingExplicitReferencesRemainAmbiguous()
    {
        var evidence = new[]
        {
            Evidence("tip", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "a", "material"),
            Evidence("tip", "material:wood", MaterialReferenceEvidenceKind.ExplicitReference, "b", "material")
        };

        var result = CreateArbitrator().Arbitrate(evidence).Single();

        Assert.That(result.Status, Is.EqualTo(MaterialIdentityResolutionStatus.Ambiguous));
        Assert.That(result.MaterialId, Is.Null);
        Assert.That(result.Evidence.Select(x => x.SourceBundleId), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void Arbitrate_IsIndependentOfInputEnumerationOrder()
    {
        var evidence = new[]
        {
            Evidence("tip", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "weapon", "tip.material"),
            Evidence("shaft", "material:wood", MaterialReferenceEvidenceKind.StructuredDeclaration, "parts", "shaft.material")
        };

        var forward = CreateArbitrator().Arbitrate(evidence);
        var reverse = CreateArbitrator().Arbitrate(evidence.Reverse());

        Assert.That(Project(reverse), Is.EqualTo(Project(forward)));
    }

    [Test]
    public void Arbitrate_UnknownMaterialAndMissingPropertiesAreNotInvented()
    {
        var result = CreateArbitrator().Arbitrate([
            Evidence("part", "material:unregistered-alloy", MaterialReferenceEvidenceKind.ExplicitReference, "bundle", "material")
        ]).Single();

        Assert.That(result.Status, Is.EqualTo(MaterialIdentityResolutionStatus.Unresolved));
        Assert.That(result.MaterialId, Is.Null);
        Assert.That(result.Explanation, Does.Contain("properties remain unspecified"));
    }

    [Test]
    public void Arbitrate_UnknownExplicitReferenceCannotBeOverriddenByWeakerKnownDeclaration()
    {
        var result = CreateArbitrator().Arbitrate([
            Evidence("part", "material:unregistered-alloy", MaterialReferenceEvidenceKind.ExplicitReference, "primary", "material"),
            Evidence("part", "material:wood", MaterialReferenceEvidenceKind.StructuredDeclaration, "secondary", "material")
        ]).Single();

        Assert.That(result.Status, Is.EqualTo(MaterialIdentityResolutionStatus.Unresolved));
        Assert.That(result.MaterialId, Is.Null);
        Assert.That(result.Evidence.Select(item => item.SourceBundleId), Is.EqualTo(new[] { "primary" }));
    }

    [Test]
    public void Arbitrate_UnknownAndKnownExplicitReferencesRemainAmbiguous()
    {
        var result = CreateArbitrator().Arbitrate([
            Evidence("part", "material:unregistered-alloy", MaterialReferenceEvidenceKind.ExplicitReference, "a", "material"),
            Evidence("part", "material:steel", MaterialReferenceEvidenceKind.ExplicitReference, "b", "material")
        ]).Single();

        Assert.That(result.Status, Is.EqualTo(MaterialIdentityResolutionStatus.Ambiguous));
        Assert.That(result.MaterialId, Is.Null);
        Assert.That(result.Evidence.Select(item => item.SourceBundleId), Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public void Arbitrate_KnownAliasResolvesIdentityAndRetainsProvenance()
    {
        var result = CreateArbitrator().Arbitrate([
            Evidence("tip", null, MaterialReferenceEvidenceKind.DescriptiveText, "weapon", "tip.description", "  steel ")
        ]).Single();

        Assert.That(result.Status, Is.EqualTo(MaterialIdentityResolutionStatus.Resolved));
        Assert.That(result.MaterialId, Is.EqualTo("material:steel"));
        Assert.That(result.Evidence.Single().SourceFieldId, Is.EqualTo("tip.description"));
    }

    private static string Project(IEnumerable<MaterialIdentityResolution> results) =>
        System.Text.Json.JsonSerializer.Serialize(results.Select(result => new
        {
            result.PartId,
            result.Status,
            result.MaterialId,
            result.Explanation,
            Evidence = result.Evidence.Select(item => new
            {
                item.PartId,
                item.MaterialId,
                item.ObservedTerm,
                item.Kind,
                item.SourceBundleId,
                item.SourceFieldId
            }).ToArray()
        }).ToArray());

    private static MaterialIdentityResolution Find(
        IReadOnlyList<MaterialIdentityResolution> results, string partId) =>
        results.Single(result => result.PartId == partId);

    private static MaterialReferenceEvidence Evidence(
        string partId, string? materialId, MaterialReferenceEvidenceKind kind,
        string bundle, string field, string? observedTerm = null) =>
        new(partId, materialId, observedTerm, kind, bundle, field);
}
