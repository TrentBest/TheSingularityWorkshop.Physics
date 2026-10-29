using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceReactionProfileTests
{
    private const string Source = "illustrative-methane-combustion-fixture";

    private static SurfaceReactionSpeciesDefinition Species(
        string id, double molarMass, params (string Element, int Count)[] atoms) =>
        new(id, molarMass, atoms.ToDictionary(x => x.Element, x => x.Count, StringComparer.Ordinal),
            PropertyEvidenceStatus.Illustrative, Source, "1");

    private static SurfaceReactionProfile MethaneCombustion(
        SurfaceReactionSpeciesDefinition? oxygen = null,
        double heatRelease = 802300d) 
    {
        var methane = Species("methane", 0.016043d, ("C", 1), ("H", 4));
        oxygen ??= Species("oxygen", 0.031998d, ("O", 2));
        var carbonDioxide = Species("carbon-dioxide", 0.044009d, ("C", 1), ("O", 2));
        var water = Species("water-vapor", 0.018015d, ("H", 2), ("O", 1));
        return new SurfaceReactionProfile(
            "methane-combustion-illustrative",
            [new(methane, 1), new(oxygen, 2)],
            [new(carbonDioxide, 1), new(water, 2)],
            heatRelease, Source, "1", "Illustrative reference fixture; not a validated fuel model.");
    }

    [Test]
    public void AcceptsBalancedMethaneCombustionEquation()
    {
        var reaction = MethaneCombustion();

        Assert.That(reaction.IsValid, Is.True);
        Assert.DoesNotThrow(reaction.EnsureValid);
    }

    [Test]
    public void RejectsEquationWithUnbalancedElementalComposition()
    {
        var reaction = MethaneCombustion() with
        {
            Products =
            [
                new(Species("carbon-dioxide", 0.044009d, ("C", 1), ("O", 2)), 1),
                new(Species("water-vapor", 0.018015d, ("H", 2), ("O", 1)), 1)
            ]
        };

        Assert.That(reaction.IsValid, Is.False);
        Assert.Throws<ArgumentException>(reaction.EnsureValid);
    }

    [Test]
    public void RejectsElementallyBalancedEquationWithInconsistentMolarMass()
    {
        var wrongOxygenMass = Species("oxygen", 0.020d, ("O", 2));
        var reaction = MethaneCombustion(wrongOxygenMass);

        Assert.That(reaction.IsValid, Is.False);
    }

    [Test]
    public void RejectsConflictingDefinitionsForSpeciesIdSharedAcrossBothSides()
    {
        var reaction = MethaneCombustion() with
        {
            Products =
            [
                new(Species("methane", 0.044009d, ("C", 1), ("O", 2)), 1),
                new(Species("water-vapor", 0.018015d, ("H", 2), ("O", 1)), 2)
            ]
        };

        Assert.That(reaction.IsValid, Is.False);
    }

    [Test]
    public void RejectsDuplicateSpeciesTermsOnOneSide()
    {
        var reaction = MethaneCombustion() with
        {
            Reactants =
            [
                new(Species("methane", 0.016043d, ("C", 1), ("H", 4)), 1),
                new(Species("methane", 0.016043d, ("C", 1), ("H", 4)), 1),
                new(Species("oxygen", 0.031998d, ("O", 2)), 2)
            ]
        };

        Assert.That(reaction.IsValid, Is.False);
    }

    [Test]
    public void RejectsInvalidCoefficientsAndNegativeHeatRelease()
    {
        var reaction = MethaneCombustion() with
        {
            Reactants = [new(Species("methane", 0.016043d, ("C", 1), ("H", 4)), 0)]
        };
        Assert.That(reaction.IsValid, Is.False);
        Assert.That((MethaneCombustion(heatRelease: -1d)).IsValid, Is.False);
    }

    [Test]
    public void RejectsSpeciesWithoutCompositionOrProvenance()
    {
        var invalid = new SurfaceReactionSpeciesDefinition(
            "unknown", 0.02d, new Dictionary<string, int>(), PropertyEvidenceStatus.Illustrative, "", "1");
        Assert.That(invalid.IsValid, Is.False);
    }
}
