using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceReactionInventoryStepperTests
{
    private static SurfaceReactionProfile MethaneReaction() =>
        new SurfaceReactionProfileTestsFixture().Create();

    [Test]
    public void ConsumesReactantsAndPersistsProductsWithoutMutatingInitialInventory()
    {
        var initial = new SurfaceReactionInventory(
            "cell-7",
            new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["methane"] = 0.016043d,
                ["oxygen"] = 0.063996d,
                ["inert-marker"] = 0.25d
            });

        var result = SurfaceReactionInventoryStepper.Apply(MethaneReaction(), initial, 1d);

        Assert.That(result.ReactionExtent.ActualExtentMoles, Is.EqualTo(1d).Within(1e-12));
        Assert.That(result.FinalInventory.CellId, Is.EqualTo("cell-7"));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["methane"], Is.EqualTo(0d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["oxygen"], Is.EqualTo(0d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["carbon-dioxide"], Is.EqualTo(0.044009d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["water-vapor"], Is.EqualTo(0.03603d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["inert-marker"], Is.EqualTo(0.25d).Within(1e-12));
        Assert.That(initial.SpeciesMassKilograms["methane"], Is.EqualTo(0.016043d).Within(1e-12));
        Assert.That(initial.SpeciesMassKilograms.ContainsKey("carbon-dioxide"), Is.False);
    }

    [Test]
    public void CapsReactionAtAvailableInventoryAndAccumulatesExistingProducts()
    {
        var initial = new SurfaceReactionInventory(
            "cell-2",
            new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["methane"] = 0.032086d,
                ["oxygen"] = 0.031998d,
                ["carbon-dioxide"] = 0.01d
            });

        var result = SurfaceReactionInventoryStepper.Apply(MethaneReaction(), initial, 2d);

        Assert.That(result.ReactionExtent.ActualExtentMoles, Is.EqualTo(0.5d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["carbon-dioxide"], Is.EqualTo(0.0320045d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["methane"], Is.EqualTo(0.0240645d).Within(1e-12));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["oxygen"], Is.EqualTo(0d).Within(1e-12));
    }

    [Test]
    public void ZeroAvailableReactantsPreservesInventoryAndCreatesNoProducts()
    {
        var initial = new SurfaceReactionInventory(
            "cell-3",
            new Dictionary<string, double> { ["inert-marker"] = 0.5d });

        var result = SurfaceReactionInventoryStepper.Apply(MethaneReaction(), initial, 1d);

        Assert.That(result.ReactionExtent.ActualExtentMoles, Is.Zero);
        Assert.That(result.FinalInventory.SpeciesMassKilograms["inert-marker"], Is.EqualTo(0.5d));
        Assert.That(result.FinalInventory.SpeciesMassKilograms["methane"], Is.Zero);
        Assert.That(result.FinalInventory.SpeciesMassKilograms["oxygen"], Is.Zero);
        Assert.That(result.FinalInventory.SpeciesMassKilograms["carbon-dioxide"], Is.Zero);
        Assert.That(result.ReactionExtent.HeatReleasedJoules, Is.Zero);
    }

    [Test]
    public void CopiesInputAndRejectsInvalidInventory()
    {
        var source = new Dictionary<string, double> { ["methane"] = 0.016043d };
        var inventory = new SurfaceReactionInventory("cell-4", source);
        source["methane"] = 99d;

        Assert.That(inventory.SpeciesMassKilograms["methane"], Is.EqualTo(0.016043d).Within(1e-12));
        Assert.Throws<ArgumentException>(() =>
            new SurfaceReactionInventory("cell-4", new Dictionary<string, double> { ["oxygen"] = double.NaN }));
        Assert.Throws<ArgumentException>(() =>
            new SurfaceReactionInventory("", new Dictionary<string, double>()));
    }

    [Test]
    public void RejectsInvalidRequestAndProfile()
    {
        var inventory = new SurfaceReactionInventory("cell-5", new Dictionary<string, double>());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SurfaceReactionInventoryStepper.Apply(MethaneReaction(), inventory, double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() =>
            SurfaceReactionInventoryStepper.Apply(
                MethaneReaction() with { HeatReleaseJoulesPerMoleOfReaction = -1d },
                inventory,
                1d));
    }
}
