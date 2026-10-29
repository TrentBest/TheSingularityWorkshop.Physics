using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceCombustionStepperTests
{
    private static SurfaceCombustionCellState Cell(
        string id,
        double fuel = 0.1d,
        double oxygen = 0.1d,
        double energy = 100d) =>
        new(id, "material:test", 0.01d, fuel, oxygen, energy, 0d, 0d, 1d);

    private static SurfaceCombustionCellParameters Parameters(
        double rate = 0.01d,
        double oxygenRatio = 1d,
        double heat = 1000d,
        double ignition = 305d,
        double capacity = 10d,
        double reference = 300d) =>
        new(rate, oxygenRatio, heat, ignition, capacity, reference);

    private static IReadOnlyDictionary<string, SurfaceCombustionCellParameters> ParameterMap(
        params (string Id, SurfaceCombustionCellParameters Parameters)[] entries) =>
        entries.ToDictionary(x => x.Id, x => x.Parameters, StringComparer.Ordinal);

    [Test]
    public void IgnitedCellConsumesFuelAndOxygenAndAddsDeclaredHeat()
    {
        var cell = Cell("hot", fuel: 0.02d, oxygen: 0.005d, energy: 100d);
        var result = SurfaceCombustionStepper.Step(
            [cell], ParameterMap(("hot", Parameters(rate: 0.01d, oxygenRatio: 0.5d, heat: 2000d))],
            [], durationSeconds: 1d);

        var step = result.Cells.Single();
        Assert.That(step.FuelConsumedKilograms, Is.EqualTo(0.01d).Within(1e-12d));
        Assert.That(step.OxygenConsumedKilograms, Is.EqualTo(0.005d).Within(1e-12d));
        Assert.That(step.FinalState.FuelMassKilograms, Is.EqualTo(0.01d).Within(1e-12d));
        Assert.That(step.FinalState.OxygenMassKilograms, Is.EqualTo(0d).Within(1e-12d));
        Assert.That(step.HeatReleasedJoules, Is.EqualTo(20d).Within(1e-12d));
        Assert.That(step.FinalState.ThermalEnergyJoules, Is.EqualTo(120d).Within(1e-12d));
        Assert.That(result.TotalFuelConsumedKilograms, Is.EqualTo(0.01d).Within(1e-12d));
    }

    [Test]
    public void CellBelowIgnitionThresholdDoesNotReact()
    {
        var cell = Cell("cold", energy: 0d);
        var result = SurfaceCombustionStepper.Step(
            [cell], ParameterMap(("cold", Parameters())), [], durationSeconds: 1d);

        Assert.That(result.Cells.Single().FuelConsumedKilograms, Is.Zero);
        Assert.That(result.Cells.Single().FinalState, Is.EqualTo(cell));
    }

    [Test]
    public void OxygenTransportIsConservativeAndBlockedConnectionPreventsExchange()
    {
        var rich = Cell("rich", fuel: 0d, oxygen: 0.1d, energy: 0d);
        var starved = Cell("starved", fuel: 0d, oxygen: 0d, energy: 0d);
        var parameters = ParameterMap(("rich", Parameters()), ("starved", Parameters()));
        var open = new SurfaceOxygenTransportConnection("rich", "starved", 0.5d);
        var blocked = open with { IsBlocked = true };

        var openResult = SurfaceCombustionStepper.Step([rich, starved], parameters, [open], 1d);
        var blockedResult = SurfaceCombustionStepper.Step([rich, starved], parameters, [blocked], 1d);

        Assert.That(openResult.Cells.Single(x => x.InitialState.CellId == "rich").OxygenAfterTransportKilograms,
            Is.EqualTo(0.05d).Within(1e-12d));
        Assert.That(openResult.Cells.Single(x => x.InitialState.CellId == "starved").OxygenAfterTransportKilograms,
            Is.EqualTo(0.05d).Within(1e-12d));
        Assert.That(openResult.OxygenTransportResidualKilograms, Is.EqualTo(0d).Within(1e-12d));
        Assert.That(blockedResult.Cells.Single(x => x.InitialState.CellId == "rich").OxygenAfterTransportKilograms,
            Is.EqualTo(0.1d).Within(1e-12d));
        Assert.That(blockedResult.Cells.Single(x => x.InitialState.CellId == "starved").OxygenAfterTransportKilograms,
            Is.Zero);
    }

    [Test]
    public void MultipleConnectionsCannotTransportMoreOxygenThanSourceOwns()
    {
        var source = Cell("source", fuel: 0d, oxygen: 0.01d, energy: 0d);
        var left = Cell("left", fuel: 0d, oxygen: 0d, energy: 0d);
        var right = Cell("right", fuel: 0d, oxygen: 0d, energy: 0d);
        var parameters = ParameterMap(
            ("source", Parameters()), ("left", Parameters()), ("right", Parameters()));
        var edges = new[]
        {
            new SurfaceOxygenTransportConnection("source", "left", 1d),
            new SurfaceOxygenTransportConnection("source", "right", 1d)
        };

        var result = SurfaceCombustionStepper.Step([source, left, right], parameters, edges, 1d);

        Assert.That(result.Cells.Sum(x => x.OxygenAfterTransportKilograms),
            Is.EqualTo(0.01d).Within(1e-12d));
        Assert.That(result.Cells.Single(x => x.InitialState.CellId == "source").OxygenAfterTransportKilograms,
            Is.EqualTo(0d).Within(1e-12d));
        Assert.That(result.OxygenTransportResidualKilograms, Is.EqualTo(0d).Within(1e-12d));
    }

    [Test]
    public void BlockedOxygenPathLeavesHotFuelUnburnedWhenCellIsOxygenStarved()
    {
        var source = Cell("source", fuel: 0d, oxygen: 0.1d, energy: 0d);
        var hotTarget = Cell("target", fuel: 0.1d, oxygen: 0d, energy: 100d);
        var parameters = ParameterMap(("source", Parameters()), ("target", Parameters()));
        var connection = new SurfaceOxygenTransportConnection("source", "target", 1d, IsBlocked: true);

        var result = SurfaceCombustionStepper.Step([source, hotTarget], parameters, [connection], 1d);
        var target = result.Cells.Single(x => x.InitialState.CellId == "target");

        Assert.That(target.InitialState.FuelMassKilograms, Is.EqualTo(0.1d));
        Assert.That(target.OxygenAfterTransportKilograms, Is.Zero);
        Assert.That(target.FuelConsumedKilograms, Is.Zero);
        Assert.That(target.FinalState.ThermalEnergyJoules, Is.EqualTo(hotTarget.ThermalEnergyJoules));
    }

    [Test]
    public void ResultsAreOrderedByStableCellIdRegardlessOfInputOrder()
    {
        var a = Cell("a");
        var z = Cell("z");
        var parameters = ParameterMap(("a", Parameters()), ("z", Parameters()));

        var forward = SurfaceCombustionStepper.Step([z, a], parameters, [], 1d);
        var reverse = SurfaceCombustionStepper.Step([a, z], parameters, [], 1d);

        Assert.That(forward.Cells.Select(x => x.InitialState.CellId), Is.EqualTo(new[] { "a", "z" }));
        Assert.That(reverse.Cells.Select(x => x.InitialState.CellId), Is.EqualTo(new[] { "a", "z" }));
        Assert.That(forward.Cells.Select(x => x.FinalState), Is.EqualTo(reverse.Cells.Select(x => x.FinalState)));
    }

    [Test]
    public void RejectsDuplicateCellIdsAndMissingParameters()
    {
        var cell = Cell("same");
        Assert.Throws<ArgumentException>(() => SurfaceCombustionStepper.Step(
            [cell, cell], ParameterMap(("same", Parameters())), [], 1d));
        Assert.Throws<ArgumentException>(() => SurfaceCombustionStepper.Step(
            [cell], new Dictionary<string, SurfaceCombustionCellParameters>(), [], 1d));
    }

    [Test]
    public void RejectsInvalidTimestepAndDuplicateUnorderedConnections()
    {
        var a = Cell("a", fuel: 0d, energy: 0d);
        var b = Cell("b", fuel: 0d, energy: 0d);
        var parameters = ParameterMap(("a", Parameters()), ("b", Parameters()));
        var edge = new SurfaceOxygenTransportConnection("a", "b", 0.2d);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SurfaceCombustionStepper.Step([a, b], parameters, [edge], 0d));
        Assert.Throws<ArgumentException>(() =>
            SurfaceCombustionStepper.Step(
                [a, b], parameters,
                [edge, new SurfaceOxygenTransportConnection("b", "a", 0.2d)], 1d));
    }
}
