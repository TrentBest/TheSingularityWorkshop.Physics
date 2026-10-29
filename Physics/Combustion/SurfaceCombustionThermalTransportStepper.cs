using TheSingularityWorkshop.Physics.Combustion;
using TheSingularityWorkshop.Physics.Materials;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Combustion;

/// <summary>
/// Explicit conducting path between two stable surface-cell identities.
/// </summary>
/// <param name="FirstCellId">First endpoint identity.</param>
/// <param name="SecondCellId">Second endpoint identity.</param>
/// <param name="ThermalConductivity">Declared conductivity and its evidence/validity metadata.</param>
/// <param name="ContactAreaSquareMeters">Effective conducting area in square metres.</param>
/// <param name="SeparationMeters">Effective conduction path length in metres.</param>
/// <param name="IsBlocked">When true, no heat is transferred along this path.</param>
/// <remarks>Connectivity is explicit; it is never inferred from UVs, texture adjacency, or rendered geometry.</remarks>
public sealed record SurfaceThermalTransportConnection(
    string FirstCellId,
    string SecondCellId,
    MaterialPropertyDefinition ThermalConductivity,
    double ContactAreaSquareMeters,
    double SeparationMeters,
    bool IsBlocked = false)
{
    /// <summary>Validates endpoint identity, geometry, and the declared conductivity.</summary>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(FirstCellId) ||
            string.IsNullOrWhiteSpace(SecondCellId) ||
            string.Equals(FirstCellId, SecondCellId, StringComparison.Ordinal))
            throw new ArgumentException("Thermal transport connections require distinct, non-empty cell IDs.");

        ArgumentNullException.ThrowIfNull(ThermalConductivity);
        ThermalConductivity.EnsureValid();
        if (!double.IsFinite(ContactAreaSquareMeters) || ContactAreaSquareMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(ContactAreaSquareMeters), "Contact area must be finite and positive.");
        if (!double.IsFinite(SeparationMeters) || SeparationMeters <= 0d)
            throw new ArgumentOutOfRangeException(nameof(SeparationMeters), "Separation must be finite and positive.");
    }
}

/// <summary>One explicit heat-transfer result for a surface-cell connection.</summary>
public sealed record SurfaceThermalTransportTransfer(
    string FirstCellId,
    string SecondCellId,
    double EnergyTransferredFromFirstToSecondJoules,
    double ThermalConductanceWattsPerKelvin);

/// <summary>Immutable result of applying deterministic, explicit conduction connections to surface cells.</summary>
public sealed record SurfaceThermalTransportResult(
    double DurationSeconds,
    IReadOnlyList<SurfaceCombustionCellState> FinalCells,
    IReadOnlyList<SurfaceThermalTransportTransfer> Transfers,
    double NetEnergyChangeJoules);

/// <summary>
/// Applies the existing lumped-body conduction primitive to combustion cells without inventing cell mass.
/// </summary>
/// <remarks>
/// The cell model declares a lumped capacity in J/K, not a physical mass and specific heat separately.
/// The adapter therefore uses a mathematical equivalent of 1 kg × (declared capacity J/K), then maps
/// the resulting temperature back to the cell's relative thermal energy. It does not assert that the
/// cell physically has 1 kg mass. Connections are processed in canonical endpoint order for reproducibility.
/// This sequential explicit scheme can depend on topology and ordering of distinct edges; it is not a
/// simultaneous spatial heat equation. A transfer that would make either cell's stored energy negative
/// relative to its declared reference temperature is rejected rather than silently clamped.
/// </remarks>
public static class SurfaceCombustionThermalTransportStepper
{
    /// <summary>Transfers heat along explicit paths, returning updated cells without mutating inputs.</summary>
    public static SurfaceThermalTransportResult Step(
        IReadOnlyList<SurfaceCombustionCellState> cells,
        IReadOnlyDictionary<string, SurfaceCombustionCellParameters> parametersByCellId,
        IReadOnlyList<SurfaceThermalTransportConnection> connections,
        double durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(parametersByCellId);
        ArgumentNullException.ThrowIfNull(connections);
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be finite and positive.");

        var states = new Dictionary<string, SurfaceCombustionCellState>(StringComparer.Ordinal);
        foreach (var cell in cells)
        {
            cell.EnsureValid();
            if (!states.TryAdd(cell.CellId, cell))
                throw new ArgumentException($"Duplicate cell ID '{cell.CellId}'.", nameof(cells));
            if (!parametersByCellId.TryGetValue(cell.CellId, out var parameters) || parameters is null)
                throw new ArgumentException($"Missing thermal parameters for cell '{cell.CellId}'.", nameof(parametersByCellId));
            parameters.EnsureValid();
        }

        var seenEdges = new HashSet<(string First, string Second)>();
        var ordered = new List<(SurfaceThermalTransportConnection Connection, string First, string Second)>();
        foreach (var connection in connections)
        {
            ArgumentNullException.ThrowIfNull(connection);
            connection.EnsureValid();
            if (!states.ContainsKey(connection.FirstCellId) || !states.ContainsKey(connection.SecondCellId))
                throw new ArgumentException("Every thermal connection endpoint must identify a supplied cell.", nameof(connections));
            var first = string.CompareOrdinal(connection.FirstCellId, connection.SecondCellId) < 0
                ? connection.FirstCellId : connection.SecondCellId;
            var second = string.CompareOrdinal(connection.FirstCellId, connection.SecondCellId) < 0
                ? connection.SecondCellId : connection.FirstCellId;
            if (!seenEdges.Add((first, second)))
                throw new ArgumentException("Only one thermal connection may exist per unordered cell pair.", nameof(connections));
            ordered.Add((connection, first, second));
        }

        var transfers = new List<SurfaceThermalTransportTransfer>();
        var initialEnergy = states.Values.Sum(x => x.ThermalEnergyJoules);
        if (!double.IsFinite(initialEnergy))
            throw new OverflowException("Initial total thermal energy is outside finite double range.");

        foreach (var item in ordered.OrderBy(x => x.First, StringComparer.Ordinal)
                     .ThenBy(x => x.Second, StringComparer.Ordinal))
        {
            var connection = item.Connection;
            if (connection.IsBlocked)
                continue;

            var first = states[connection.FirstCellId];
            var second = states[connection.SecondCellId];
            var firstParameters = parametersByCellId[first.CellId];
            var secondParameters = parametersByCellId[second.CellId];
            var firstTemperature = SurfaceCombustionStepper.GetTemperature(first, firstParameters);
            var secondTemperature = SurfaceCombustionStepper.GetTemperature(second, secondParameters);
            var firstThermal = new LumpedThermalState(1d, firstTemperature,
                firstParameters.ThermalCapacityJoulesPerKelvin, firstParameters.ReferenceTemperatureKelvin);
            var secondThermal = new LumpedThermalState(1d, secondTemperature,
                secondParameters.ThermalCapacityJoulesPerKelvin, secondParameters.ReferenceTemperatureKelvin);

            var result = ThermalConduction.Transfer(firstThermal, secondThermal,
                connection.ThermalConductivity, connection.ContactAreaSquareMeters,
                connection.SeparationMeters, durationSeconds);
            var firstEnergy = firstParameters.ThermalCapacityJoulesPerKelvin *
                (result.FinalFirst.TemperatureKelvin - firstParameters.ReferenceTemperatureKelvin);
            var secondEnergy = secondParameters.ThermalCapacityJoulesPerKelvin *
                (result.FinalSecond.TemperatureKelvin - secondParameters.ReferenceTemperatureKelvin);
            if (!double.IsFinite(firstEnergy) || !double.IsFinite(secondEnergy))
                throw new OverflowException("Thermal transport produced energy outside finite double range.");
            if (firstEnergy < 0d || secondEnergy < 0d)
                throw new InvalidOperationException("Thermal transfer would make a cell's reference-relative thermal energy negative; reduce the timestep or revise the model reference.");

            states[first.CellId] = first with { ThermalEnergyJoules = firstEnergy };
            states[second.CellId] = second with { ThermalEnergyJoules = secondEnergy };
            transfers.Add(new SurfaceThermalTransportTransfer(
                connection.FirstCellId, connection.SecondCellId,
                result.EnergyTransferredFromFirstToSecondJoules,
                result.ThermalConductanceWattsPerKelvin));
        }

        var finalEnergy = states.Values.Sum(x => x.ThermalEnergyJoules);
        if (!double.IsFinite(finalEnergy))
            throw new OverflowException("Final total thermal energy is outside finite double range.");
        var netChange = finalEnergy - initialEnergy;
        var tolerance = 1e-10d * Math.Max(1d, Math.Abs(initialEnergy));
        if (Math.Abs(netChange) > tolerance)
            throw new InvalidOperationException("Internal thermal transport failed its energy-conservation check.");

        return new SurfaceThermalTransportResult(
            durationSeconds,
            states.Values.OrderBy(x => x.CellId, StringComparer.Ordinal).ToArray(),
            transfers,
            netChange);
    }
}
