using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceReactionExtentCalculatorTests
{
    private static SurfaceReactionProfile MethaneReaction() =>
        new SurfaceReactionProfileTestsFixture().Create();

    [Test]
    public void ComputesExtentAndMassesForRequestedBalancedReaction()
    {
        var profile = MethaneReaction();
        var available = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["methane"] = 0.016043d,
            ["oxygen"] = 0.063996d
        };

        var result = SurfaceReactionExtentCalculator.Calculate(profile, available, 1d);

        Assert.That(result.ActualExtentMoles, Is.EqualTo(1d).Within(1e-12));
        Assert.That(result.LimitingReactantId, Is.Null);
        Assert.That(result.ReactantMassConsumedKilograms["methane"], Is.EqualTo(0.016043d).Within(1e-12));
        Assert.That(result.ReactantMassConsumedKilograms["oxygen"], Is.EqualTo(0.063996d).Within(1e-12));
        Assert.That(result.ProductMassGeneratedKilograms["carbon-dioxide"], Is.EqualTo(0.044009d).Within(1e-12));
        Assert.That(result.ProductMassGeneratedKilograms["water-vapor"], Is.EqualTo(0.03603d).Within(1e-12));
        Assert.That(result.HeatReleasedJoules, Is.EqualTo(802300d).Within(1e-9));
        Assert.That(result.MassBalanceResidualKilograms, Is.EqualTo(0d).Within(1e-12));
    }

    [Test]
    public void CapsExtentByTheMostRestrictiveReactantInventory()
    {
        var result = SurfaceReactionExtentCalculator.Calculate(
            MethaneReaction(),
            new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["methane"] = 0.032086d,
                ["oxygen"] = 0.031998d
            },
            2d);

        Assert.That(result.ActualExtentMoles, Is.EqualTo(0.5d).Within(1e-12));
        Assert.That(result.LimitingReactantId, Is.EqualTo("oxygen"));
        Assert.That(result.ReactantMassConsumedKilograms["methane"], Is.EqualTo(0.0080215d).Within(1e-12));
        Assert.That(result.HeatReleasedJoules, Is.EqualTo(401150d).Within(1e-9));
    }

    [Test]
    public void MissingReactantInventoryMeansNoReactionExtent()
    {
        var result = SurfaceReactionExtentCalculator.Calculate(
            MethaneReaction(),
            new Dictionary<string, double>(),
            1d);

        Assert.That(result.ActualExtentMoles, Is.Zero);
        Assert.That(result.ReactantMassConsumedKilograms.Values, Is.All.Zero);
        Assert.That(result.ProductMassGeneratedKilograms.Values, Is.All.Zero);
        Assert.That(result.HeatReleasedJoules, Is.Zero);
    }

    [Test]
    public void RejectsNegativeOrNonFiniteInventoryAndExtent()
    {
        var profile = MethaneReaction();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SurfaceReactionExtentCalculator.Calculate(profile, new Dictionary<string, double>(), -1d));
        Assert.Throws<ArgumentException>(() =>
            SurfaceReactionExtentCalculator.Calculate(profile,
                new Dictionary<string, double> { ["methane"] = double.NaN }, 1d));
        Assert.Throws<ArgumentException>(() =>
            SurfaceReactionExtentCalculator.Calculate(profile,
                new Dictionary<string, double> { ["unknown"] = 1d }, 1d));
    }
}

internal sealed class SurfaceReactionProfileTestsFixture
{
    public SurfaceReactionProfile Create()
    {
        const string source = "illustrative-methane-combustion-fixture";
        var methane = Species("methane", 0.016043d, ("C", 1), ("H", 4));
        var oxygen = Species("oxygen", 0.031998d, ("O", 2));
        var carbonDioxide = Species("carbon-dioxide", 0.044009d, ("C", 1), ("O", 2));
        var water = Species("water-vapor", 0.018015d, ("H", 2), ("O", 1));
        return new SurfaceReactionProfile(
            "methane-combustion-illustrative",
            [new(methane, 1), new(oxygen, 2)],
            [new(carbonDioxide, 1), new(water, 2)],
            802300d, source, "1", "Illustrative reference fixture; not a validated fuel model.");
    }

    private static SurfaceReactionSpeciesDefinition Species(
        string id, double molarMass, params (string Element, int Count)[] atoms) =>
        new(id, molarMass,
            atoms.ToDictionary(x => x.Element, x => x.Count, StringComparer.Ordinal),
            TheSingularityWorkshop.Physics.Materials.PropertyEvidenceStatus.Illustrative,
            "illustrative-methane-combustion-fixture", "1");
}
