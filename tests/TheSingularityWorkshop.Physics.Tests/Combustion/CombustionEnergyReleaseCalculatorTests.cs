using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class CombustionEnergyReleaseCalculatorTests
{
    private static MaterialPropertyDefinition EnergyEvidence(double value = 43_000_000d) =>
        new("illustrative-diesel", "specific-chemical-energy", value, "J/kg",
            PropertyEvidenceStatus.Illustrative, "illustrative-energy-fixture", "1",
            "temperature", "K", 250d, 500d,
            "Illustrative bookkeeping fixture only; not a validated fuel property.");

    [Test]
    public void SeparatesStoredEnergyReleasedEnergyAndMechanicalBudget()
    {
        var result = CombustionEnergyReleaseCalculator.Calculate(
            "illustrative-diesel", 100d, 10d, 43_000_000d, EnergyEvidence(),
            mechanicalEnergyFraction: 0.2d, thermalEnergyFraction: 0.5d, radiantEnergyFraction: 0.1d);

        Assert.That(result.ChemicalEnergyAvailableJoules, Is.EqualTo(4_300_000_000d));
        Assert.That(result.ChemicalEnergyReleasedJoules, Is.EqualTo(430_000_000d));
        Assert.That(result.MechanicalEnergyJoules, Is.EqualTo(86_000_000d));
        Assert.That(result.ThermalEnergyJoules, Is.EqualTo(215_000_000d));
        Assert.That(result.RadiantEnergyJoules, Is.EqualTo(43_000_000d));
        Assert.That(result.UnallocatedReleasedEnergyJoules, Is.EqualTo(86_000_000d));
        Assert.That(result.RemainingChemicalEnergyJoules, Is.EqualTo(3_870_000_000d));
    }

    [Test]
    public void ZeroReactedMassReleasesNoEnergy()
    {
        var result = CombustionEnergyReleaseCalculator.Calculate(
            "illustrative-diesel", 5d, 0d, 43_000_000d, EnergyEvidence(), 1d, 0d, 0d);

        Assert.That(result.ChemicalEnergyReleasedJoules, Is.Zero);
        Assert.That(result.MechanicalEnergyJoules, Is.Zero);
        Assert.That(result.RemainingChemicalEnergyJoules, Is.EqualTo(215_000_000d));
    }

    [Test]
    public void RejectsFractionsThatOverAllocateReleasedEnergy()
    {
        Assert.Throws<ArgumentException>(() => CombustionEnergyReleaseCalculator.Calculate(
            "illustrative-diesel", 1d, 1d, 43_000_000d, EnergyEvidence(), 0.6d, 0.3d, 0.2d));
    }

    [Test]
    public void RejectsReactedMassGreaterThanStoredFuel()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CombustionEnergyReleaseCalculator.Calculate(
            "illustrative-diesel", 1d, 1.1d, 43_000_000d, EnergyEvidence(), 0.2d, 0.5d, 0.1d));
    }

    [Test]
    public void RejectsEnergyThatDoesNotMatchMaterialEvidence()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CombustionEnergyReleaseCalculator.Calculate(
            "illustrative-diesel", 1d, 1d, 42_000_000d, EnergyEvidence(), 0.2d, 0.5d, 0.1d));
    }
}
