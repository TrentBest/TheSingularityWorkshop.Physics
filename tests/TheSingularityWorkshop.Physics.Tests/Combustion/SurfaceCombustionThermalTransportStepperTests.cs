using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceCombustionThermalTransportStepperTests
{
    private static SurfaceCombustionCellState Cell(string id, double energy) =>
        new(id, "material:test", 0.01d, 0d, 0d, energy, 0d, 0d, 1d);

    private static SurfaceCombustionCellParameters Parameters(double capacity = 10d, double reference = 300d) =>
        new(0d, 1d, 0d, 300d, capacity, reference);

    private static IReadOnlyDictionary<string, SurfaceCombustionCellParameters> Map(params (string Id, SurfaceCombustionCellParameters Value)[] values) =>
        values.ToDictionary(x => x.Id, x => x.Value, StringComparer.Ordinal);

    private static MaterialPropertyDefinition Conductivity() => new(
        "illustrative-solid", "thermal-conductivity", 1d, "W/(m*K)",
        PropertyEvidenceStatus.Illustrative, "surface-thermal-test", "1",
        "temperature", "K", 300d, 400d, "Illustrative test range.");

    private static SurfaceThermalTransportConnection Edge(string first, string second, bool blocked = false) =>
        new(first, second, Conductivity(), 1d, 1d, blocked);

    [Test]
    public void TransfersHeatDownGradientAndConservesCellThermalEnergy()
    {
        var hot = Cell("hot", 500d); // 350 K at 10 J/K, reference 300 K
        var cold = Cell("cold", 0d);
        var result = SurfaceCombustionThermalTransportStepper.Step(
            [hot, cold], Map(("hot", Parameters()), ("cold", Parameters())),
            [Edge("hot", "cold")], 1d);

        Assert.That(result.FinalCells.Single(x => x.CellId == "hot").ThermalEnergyJoules, Is.EqualTo(450d).Within(1e-9));
        Assert.That(result.FinalCells.Single(x => x.CellId == "cold").ThermalEnergyJoules, Is.EqualTo(50d).Within(1e-9));
        Assert.That(result.NetEnergyChangeJoules, Is.EqualTo(0d).Within(1e-9));
        Assert.That(result.Transfers.Single().EnergyTransferredFromFirstToSecondJoules, Is.EqualTo(50d).Within(1e-9));
    }

    [Test]
    public void BlockedConnectionLeavesCellsUnchanged()
    {
        var hot = Cell("hot", 500d);
        var cold = Cell("cold", 0d);
        var result = SurfaceCombustionThermalTransportStepper.Step(
            [hot, cold], Map(("hot", Parameters()), ("cold", Parameters())),
            [Edge("hot", "cold", blocked: true)], 1d);

        Assert.That(result.FinalCells.Single(x => x.CellId == "hot"), Is.EqualTo(hot));
        Assert.That(result.FinalCells.Single(x => x.CellId == "cold"), Is.EqualTo(cold));
        Assert.That(result.Transfers, Is.Empty);
    }

    [Test]
    public void RejectsStepThatWouldDriveCellBelowReferenceEnergy()
    {
        var hot = Cell("hot", 1d);
        var cold = Cell("cold", 0d);
        Assert.Throws<InvalidOperationException>(() =>
            SurfaceCombustionThermalTransportStepper.Step(
                [hot, cold], Map(("hot", Parameters()), ("cold", Parameters())),
                [Edge("hot", "cold")], 10d));
    }

    [Test]
    public void ResultsAreStableByCellAndEdgeOrder()
    {
        var a = Cell("a", 100d);
        var b = Cell("b", 0d);
        var c = Cell("c", 50d);
        var map = Map(("a", Parameters()), ("b", Parameters()), ("c", Parameters()));
        var edges = new[] { Edge("a", "b"), Edge("a", "c") };

        var forward = SurfaceCombustionThermalTransportStepper.Step([a, b, c], map, edges, 0.1d);
        var reverse = SurfaceCombustionThermalTransportStepper.Step([c, b, a], map, edges.Reverse().ToArray(), 0.1d);

        Assert.That(forward.FinalCells, Is.EqualTo(reverse.FinalCells));
        Assert.That(forward.Transfers, Is.EqualTo(reverse.Transfers));
    }

    [Test]
    public void RejectsDuplicateUnorderedEdgesAndUnknownEndpoints()
    {
        var a = Cell("a", 10d);
        var b = Cell("b", 0d);
        var map = Map(("a", Parameters()), ("b", Parameters()));
        Assert.Throws<ArgumentException>(() =>
            SurfaceCombustionThermalTransportStepper.Step(
                [a, b], map, [Edge("a", "b"), Edge("b", "a")], 0.1d));
        Assert.Throws<ArgumentException>(() =>
            SurfaceCombustionThermalTransportStepper.Step(
                [a, b], map, [Edge("a", "missing")], 0.1d));
    }
}
