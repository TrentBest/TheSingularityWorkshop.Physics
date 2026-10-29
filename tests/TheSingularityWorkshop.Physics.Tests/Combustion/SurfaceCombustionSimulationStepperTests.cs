using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceCombustionSimulationStepperTests
{
    private static SurfaceCombustionCellState Cell(string id, double fuel, double oxygen, double energy) =>
        new(id, "material:test", 0.01d, fuel, oxygen, energy, 0d, 0d, 1d);

    private static SurfaceCombustionCellParameters Parameters(double ignition = 305d) =>
        new(0.01d, 1d, 1000d, ignition, 10d, 300d);

    private static MaterialPropertyDefinition Conductivity() => new(
        "material:test", "thermal-conductivity", 1d, "W/(m*K)",
        PropertyEvidenceStatus.Illustrative, "coupled-fire-test", "1",
        "temperature", "K", 300d, 400d, "Illustrative range for a deterministic test.");

    private static SurfaceThermalTransportConnection Edge() =>
        new("hot", "target", Conductivity(), 1d, 1d);

    private static SurfaceCombustionSimulationStepResult Run(double durationSeconds)
    {
        var cells = new[]
        {
            Cell("hot", fuel: 0.1d, oxygen: 0.1d, energy: 500d),
            Cell("target", fuel: 0.1d, oxygen: 0.1d, energy: 0d)
        };
        var parameters = new Dictionary<string, SurfaceCombustionCellParameters>(StringComparer.Ordinal)
        {
            ["hot"] = Parameters(),
            ["target"] = Parameters()
        };

        return SurfaceCombustionSimulationStepper.Step(
            cells, parameters, [Edge()], [], [], durationSeconds);
    }

    [Test]
    public void NeighborIgnitesOnlyAfterComputedConductionRaisesTemperatureAboveThreshold()
    {
        var shortStep = Run(0.1d);
        var shortTarget = shortStep.Combustion.Cells.Single(x => x.InitialState.CellId == "target");
        Assert.That(shortTarget.InitialTemperatureKelvin, Is.LessThan(305d));
        Assert.That(shortTarget.FuelConsumedKilograms, Is.Zero);

        var longStep = Run(2d);
        var longTarget = longStep.Combustion.Cells.Single(x => x.InitialState.CellId == "target");
        Assert.That(longTarget.InitialTemperatureKelvin, Is.GreaterThanOrEqualTo(305d));
        Assert.That(longTarget.FuelConsumedKilograms, Is.GreaterThan(0d));
        Assert.That(longTarget.FinalState.FuelMassKilograms, Is.LessThan(longTarget.InitialState.FuelMassKilograms));
    }

    [Test]
    public void CoupledStepKeepsConductionAccountingSeparateFromReactionHeat()
    {
        var result = Run(2d);

        Assert.That(result.ThermalTransport.NetEnergyChangeJoules, Is.EqualTo(0d).Within(1e-9d));
        Assert.That(result.ThermalTransport.Transfers, Has.Count.EqualTo(1));
        Assert.That(result.Combustion.TotalHeatReleasedJoules, Is.GreaterThan(0d));
        Assert.That(result.FinalCells.Select(x => x.CellId), Is.EqualTo(new[] { "hot", "target" }));
        Assert.That(result.FinalCells.All(x => x.IsValid), Is.True);
    }

    [Test]
    public void OxygenStarvedNeighborDoesNotBurnEvenWhenConductionIgnitesIt()
    {
        var cells = new[]
        {
            Cell("hot", fuel: 0.1d, oxygen: 0.1d, energy: 500d),
            Cell("target", fuel: 0.1d, oxygen: 0d, energy: 0d)
        };
        var parameters = new Dictionary<string, SurfaceCombustionCellParameters>(StringComparer.Ordinal)
        {
            ["hot"] = Parameters(),
            ["target"] = Parameters()
        };

        var result = SurfaceCombustionSimulationStepper.Step(
            cells, parameters, [Edge()], [], [], 2d);
        var target = result.Combustion.Cells.Single(x => x.InitialState.CellId == "target");

        Assert.That(target.InitialTemperatureKelvin, Is.GreaterThanOrEqualTo(305d));
        Assert.That(target.FuelConsumedKilograms, Is.Zero);
        Assert.That(target.FinalState.FuelMassKilograms, Is.EqualTo(0.1d));
    }
}
