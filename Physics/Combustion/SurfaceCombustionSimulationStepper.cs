namespace TheSingularityWorkshop.Physics.Combustion;

using TheSingularityWorkshop.Physics.Materials;

/// <summary>Result of one coupled heat-transport and surface-reaction interval.</summary>
public sealed record SurfaceCombustionSimulationStepResult(
    SurfaceThermalTransportResult ThermalTransport,
    SurfaceCombustionStepResult Combustion)
{
    /// <summary>Final cell state after both internal heat transport and the reaction step.</summary>
    public IReadOnlyList<SurfaceCombustionCellState> FinalCells =>
        Combustion.Cells.Select(cell => cell.FinalState).ToArray();
}

/// <summary>
/// Deterministic CPU reference step that couples explicit conductive heat transfer to the existing
/// bounded surface-reaction model.
/// </summary>
/// <remarks>
/// Integration order is intentional: first conduct heat over the declared contact graph, then
/// evaluate reaction eligibility using the resulting temperatures. A cell heated above its declared
/// ignition threshold may therefore react in this same interval. Reaction heat is added after
/// conduction and affects subsequent intervals. This model does not infer contacts from rendered
/// adjacency and does not yet model convection, radiation, airflow, flame transport, smoke, or
/// temperature-dependent reaction kinetics.
/// </remarks>
public static class SurfaceCombustionSimulationStepper
{
    /// <summary>Conducts heat, then evaluates oxygen-limited reaction for one explicit interval.</summary>
    public static SurfaceCombustionSimulationStepResult Step(
        IReadOnlyList<SurfaceCombustionCellState> cells,
        IReadOnlyDictionary<string, SurfaceCombustionCellParameters> parametersByCellId,
        IReadOnlyList<SurfaceThermalTransportConnection> thermalConnections,
        IReadOnlyList<SurfaceOxygenTransportConnection> oxygenConnections,
        IReadOnlyList<SurfaceOxygenBoundaryCondition> oxygenBoundaries,
        double durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(parametersByCellId);
        ArgumentNullException.ThrowIfNull(thermalConnections);
        ArgumentNullException.ThrowIfNull(oxygenConnections);
        ArgumentNullException.ThrowIfNull(oxygenBoundaries);

        var thermal = SurfaceCombustionThermalTransportStepper.Step(
            cells, parametersByCellId, thermalConnections, durationSeconds);
        var combustion = SurfaceCombustionStepper.Step(
            thermal.FinalCells, parametersByCellId, oxygenConnections, oxygenBoundaries, durationSeconds);

        return new SurfaceCombustionSimulationStepResult(thermal, combustion);
    }
}
