namespace TheSingularityWorkshop.Physics.Combustion;

/// <summary>
/// Declared, deliberately bounded parameters for one surface cell's reaction model.
/// These are model inputs, not inferred material properties.
/// </summary>
/// <param name="MaximumFuelConsumptionRateKilogramsPerSecond">Upper bound on fuel consumed per second when ignition and reactant conditions permit reaction.</param>
/// <param name="OxygenKilogramsPerKilogramFuel">Oxygen mass required per kilogram of fuel consumed.</param>
/// <param name="HeatReleaseJoulesPerKilogramFuel">Thermal energy added per kilogram of fuel consumed.</param>
/// <param name="IgnitionTemperatureKelvin">Minimum cell temperature at which this simplified reaction may proceed.</param>
/// <param name="ThermalCapacityJoulesPerKelvin">Declared lumped sensible heat capacity of this cell in J/K.</param>
/// <param name="ReferenceTemperatureKelvin">Temperature corresponding to zero stored thermal energy for this cell model.</param>
/// <remarks>
/// This bounded model does not derive parameters from material identity, model products or losses,
/// or claim a general chemical-kinetics law. Use sourced/validated values for physical predictions;
/// illustrative values are suitable only for explicitly illustrative experiments.
/// </remarks>
public sealed record SurfaceCombustionCellParameters(
    double MaximumFuelConsumptionRateKilogramsPerSecond,
    double OxygenKilogramsPerKilogramFuel,
    double HeatReleaseJoulesPerKilogramFuel,
    double IgnitionTemperatureKelvin,
    double ThermalCapacityJoulesPerKelvin,
    double ReferenceTemperatureKelvin)
{
    /// <summary>Whether all declared parameters are finite and admissible.</summary>
    public bool IsValid =>
        double.IsFinite(MaximumFuelConsumptionRateKilogramsPerSecond) &&
        MaximumFuelConsumptionRateKilogramsPerSecond >= 0d &&
        double.IsFinite(OxygenKilogramsPerKilogramFuel) &&
        OxygenKilogramsPerKilogramFuel > 0d &&
        double.IsFinite(HeatReleaseJoulesPerKilogramFuel) &&
        HeatReleaseJoulesPerKilogramFuel >= 0d &&
        double.IsFinite(IgnitionTemperatureKelvin) &&
        IgnitionTemperatureKelvin >= 0d &&
        double.IsFinite(ThermalCapacityJoulesPerKelvin) &&
        ThermalCapacityJoulesPerKelvin > 0d &&
        double.IsFinite(ReferenceTemperatureKelvin) &&
        ReferenceTemperatureKelvin >= 0d;

    /// <summary>Throws when a parameter is invalid.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Combustion parameters must be finite and physically admissible.", nameof(SurfaceCombustionCellParameters));
    }
}

/// <summary>
/// One possible oxygen-exchange path between two cells. The connection is an explicit topology
/// edge, not an inference from proximity or a rendered texture.
/// </summary>
/// <param name="FirstCellId">Stable ID of the first cell.</param>
/// <param name="SecondCellId">Stable ID of the second cell.</param>
/// <param name="ExchangeFractionPerStep">Fraction of the two cells' oxygen-mass difference exchanged per step, before outgoing-inventory limiting.</param>
/// <param name="IsBlocked">When true, this edge transports no oxygen.</param>
/// <remarks>
/// The initial exchange rule assumes equal oxygen control-volume sizes. It is a bounded graph
/// transport approximation, not a model of pressure, buoyancy, wind, or fluid airflow.
/// </remarks>
public sealed record SurfaceOxygenTransportConnection(
    string FirstCellId,
    string SecondCellId,
    double ExchangeFractionPerStep,
    bool IsBlocked = false)
{
    /// <summary>Whether the connection has distinct non-empty endpoints and a bounded exchange fraction.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(FirstCellId) &&
        !string.IsNullOrWhiteSpace(SecondCellId) &&
        !string.Equals(FirstCellId, SecondCellId, StringComparison.Ordinal) &&
        double.IsFinite(ExchangeFractionPerStep) &&
        ExchangeFractionPerStep >= 0d &&
        ExchangeFractionPerStep <= 1d;

    /// <summary>Throws when the connection is invalid.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Oxygen transport connections require distinct cell IDs and a finite exchange fraction in [0, 1].", nameof(SurfaceOxygenTransportConnection));
    }
}


/// <summary>Declared oxygen supply from an external ambient reservoir to one cell.</summary>
/// <param name="CellId">Stable identity of the receiving cell.</param>
/// <param name="SupplyRateKilogramsPerSecond">Maximum oxygen mass supplied per second while this boundary is open.</param>
/// <param name="IsBlocked">When true, the boundary supplies no oxygen.</param>
/// <remarks>The reservoir is external to the modeled cells; supplied mass is reported separately.</remarks>
public sealed record SurfaceOxygenBoundaryCondition(
    string CellId,
    double SupplyRateKilogramsPerSecond,
    bool IsBlocked = false)
{
    /// <summary>Whether this boundary has a non-empty cell ID and finite non-negative supply rate.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(CellId) &&
        double.IsFinite(SupplyRateKilogramsPerSecond) &&
        SupplyRateKilogramsPerSecond >= 0d;

    /// <summary>Throws when this boundary condition is invalid.</summary>
    public void EnsureValid()
    {
        if (!IsValid)
            throw new ArgumentException("Oxygen boundary conditions require a cell ID and finite non-negative supply rate.", nameof(SurfaceOxygenBoundaryCondition));
    }
}

/// <summary>Per-cell accounting for one bounded combustion step.</summary>
public sealed record SurfaceCombustionCellStepResult(
    SurfaceCombustionCellState InitialState,
    double OxygenAfterTransportKilograms,
    SurfaceCombustionCellState FinalState,
    double FuelConsumedKilograms,
    double OxygenConsumedKilograms,
    double HeatReleasedJoules,
    double InitialTemperatureKelvin,
    double FinalTemperatureKelvin);

/// <summary>Immutable output and accounting totals for one deterministic surface-combustion step.</summary>
public sealed record SurfaceCombustionStepResult(
    double DurationSeconds,
    IReadOnlyList<SurfaceCombustionCellStepResult> Cells,
    double TotalFuelConsumedKilograms,
    double TotalOxygenConsumedKilograms,
    double TotalHeatReleasedJoules,
    double OxygenTransportResidualKilograms,
    double TotalOxygenSuppliedByBoundariesKilograms);

/// <summary>
/// Deterministic CPU reference step for a bounded, cell-based surface reaction.
/// Oxygen is exchanged over explicit, optionally blocked graph connections before reaction.
/// Reaction consumes available fuel and oxygen and adds declared heat to the cell's thermal energy.
/// </summary>
/// <remarks>
/// The model assumes equal oxygen control-volume sizes for graph exchange, one lumped thermal
/// capacity per cell, and a temperature-threshold/max-rate reaction law. It does not model ambient
/// oxygen boundaries, products, heat transfer between cells, convection, radiation, or fluid flow.
/// Cell IDs define identity; texture/UV layouts are presentation mappings only.
/// </remarks>
public static class SurfaceCombustionStepper
{
    /// <summary>Advances all cells by one explicit, deterministic interval.</summary>
    /// <param name="cells">Current cell states. IDs must be unique.</param>
    /// <param name="parametersByCellId">Declared reaction and thermal parameters for every cell.</param>
    /// <param name="connections">Explicit graph edges; blocked edges are ignored for oxygen exchange.</param>
    /// <param name="durationSeconds">Positive logical timestep in seconds.</param>
    public static SurfaceCombustionStepResult Step(
        IReadOnlyList<SurfaceCombustionCellState> cells,
        IReadOnlyDictionary<string, SurfaceCombustionCellParameters> parametersByCellId,
        IReadOnlyList<SurfaceOxygenTransportConnection> connections,
        double durationSeconds) =>
        Step(cells, parametersByCellId, connections, [], durationSeconds);

    /// <summary>Advances one interval with explicit external oxygen reservoirs and internal graph transport.</summary>
    /// <param name="cells">Current cell states. IDs must be unique.</param>
    /// <param name="parametersByCellId">Declared reaction and thermal parameters for every cell.</param>
    /// <param name="connections">Explicit graph edges; blocked edges are ignored for oxygen exchange.</param>
    /// <param name="boundaries">External oxygen reservoirs; each cell may have at most one boundary.</param>
    /// <param name="durationSeconds">Positive logical timestep in seconds.</param>
    public static SurfaceCombustionStepResult Step(
        IReadOnlyList<SurfaceCombustionCellState> cells,
        IReadOnlyDictionary<string, SurfaceCombustionCellParameters> parametersByCellId,
        IReadOnlyList<SurfaceOxygenTransportConnection> connections,
        IReadOnlyList<SurfaceOxygenBoundaryCondition> boundaries,
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
                throw new ArgumentException($"Missing combustion parameters for cell '{cell.CellId}'.", nameof(parametersByCellId));
            parameters.EnsureValid();
            var temperature = GetTemperature(cell, parameters);
            if (!double.IsFinite(temperature))
                throw new OverflowException($"Temperature for cell '{cell.CellId}' is outside finite double range.");
        }

        var boundaryIds = new HashSet<string>(StringComparer.Ordinal);
        var oxygenSupplied = 0d;
        var transportStates = new Dictionary<string, SurfaceCombustionCellState>(states, StringComparer.Ordinal);
        foreach (var boundary in boundaries)
        {
            ArgumentNullException.ThrowIfNull(boundary);
            boundary.EnsureValid();
            if (!states.ContainsKey(boundary.CellId))
                throw new ArgumentException("Every oxygen boundary must identify a supplied cell.", nameof(boundaries));
            if (!boundaryIds.Add(boundary.CellId))
                throw new ArgumentException("Only one oxygen boundary condition may exist per cell.", nameof(boundaries));
            if (boundary.IsBlocked || boundary.SupplyRateKilogramsPerSecond == 0d)
                continue;
            var supplied = boundary.SupplyRateKilogramsPerSecond * durationSeconds;
            if (!double.IsFinite(supplied))
                throw new OverflowException("Ambient oxygen supply is outside finite double range.");
            var updated = transportStates[boundary.CellId] with
            {
                OxygenMassKilograms = transportStates[boundary.CellId].OxygenMassKilograms + supplied
            };
            if (!double.IsFinite(updated.OxygenMassKilograms))
                throw new OverflowException("Ambient oxygen inventory is outside finite double range.");
            transportStates[boundary.CellId] = updated;
            oxygenSupplied += supplied;
            if (!double.IsFinite(oxygenSupplied))
                throw new OverflowException("Total ambient oxygen supply is outside finite double range.");
        }

        var edgeKeys = new HashSet<(string First, string Second)>();
        var outgoing = new Dictionary<string, List<(string DestinationId, double Amount)>>(StringComparer.Ordinal);
        foreach (var connection in connections)
        {
            connection.EnsureValid();
            if (!states.ContainsKey(connection.FirstCellId) || !states.ContainsKey(connection.SecondCellId))
                throw new ArgumentException("Every transport connection endpoint must identify a supplied cell.", nameof(connections));

            var edgeKey = string.CompareOrdinal(connection.FirstCellId, connection.SecondCellId) < 0
                ? (connection.FirstCellId, connection.SecondCellId)
                : (connection.SecondCellId, connection.FirstCellId);
            if (!edgeKeys.Add(edgeKey))
                throw new ArgumentException("Only one transport connection may exist for an unordered cell pair.", nameof(connections));

            if (connection.IsBlocked || connection.ExchangeFractionPerStep == 0d)
                continue;

            var firstOxygen = transportStates[connection.FirstCellId].OxygenMassKilograms;
            var secondOxygen = transportStates[connection.SecondCellId].OxygenMassKilograms;
            var difference = firstOxygen - secondOxygen;
            if (difference == 0d)
                continue;

            var sourceId = difference > 0d ? connection.FirstCellId : connection.SecondCellId;
            var destinationId = difference > 0d ? connection.SecondCellId : connection.FirstCellId;
            var amount = Math.Abs(difference) * connection.ExchangeFractionPerStep;
            if (!double.IsFinite(amount))
                throw new OverflowException("Oxygen exchange amount is outside finite double range.");
            if (!outgoing.TryGetValue(sourceId, out var transfers))
                outgoing[sourceId] = transfers = [];
            transfers.Add((destinationId, amount));
        }

        var oxygenDeltas = states.Keys.ToDictionary(id => id, _ => 0d, StringComparer.Ordinal);
        foreach (var sourceId in outgoing.Keys.OrderBy(id => id, StringComparer.Ordinal))
        {
            var transfers = outgoing[sourceId].OrderBy(x => x.DestinationId, StringComparer.Ordinal).ToArray();
            var requestedTotal = transfers.Sum(x => x.Amount);
            if (!double.IsFinite(requestedTotal))
                throw new OverflowException("Total requested oxygen exchange is outside finite double range.");
            var available = transportStates[sourceId].OxygenMassKilograms;
            var scale = requestedTotal > available && requestedTotal > 0d ? available / requestedTotal : 1d;
            foreach (var transfer in transfers)
            {
                var amount = transfer.Amount * scale;
                oxygenDeltas[sourceId] -= amount;
                oxygenDeltas[transfer.DestinationId] += amount;
            }
        }

        var oxygenResidual = oxygenDeltas.Values.Sum();
        if (!double.IsFinite(oxygenResidual))
            throw new OverflowException("Oxygen transport residual is outside finite double range.");

        var results = new List<SurfaceCombustionCellStepResult>(states.Count);
        var totalFuelConsumed = 0d;
        var totalOxygenConsumed = 0d;
        var totalHeatReleased = 0d;

        foreach (var cellId in states.Keys.OrderBy(id => id, StringComparer.Ordinal))
        {
            var initial = states[cellId];
            var parameters = parametersByCellId[cellId];
            var oxygenAfterTransport = transportStates[cellId].OxygenMassKilograms + oxygenDeltas[cellId];
            if (oxygenAfterTransport < 0d && oxygenAfterTransport > -1e-14d)
                oxygenAfterTransport = 0d;
            if (!double.IsFinite(oxygenAfterTransport) || oxygenAfterTransport < 0d)
                throw new InvalidOperationException($"Oxygen transport produced invalid inventory for cell '{cellId}'.");

            var initialTemperature = GetTemperature(initial, parameters);
            var availableFuelRate = parameters.MaximumFuelConsumptionRateKilogramsPerSecond * durationSeconds;
            if (!double.IsFinite(availableFuelRate))
                throw new OverflowException("Maximum fuel consumption over this interval is outside finite double range.");

            var fuelConsumed = 0d;
            var oxygenConsumed = 0d;
            var heatReleased = 0d;
            if (initialTemperature >= parameters.IgnitionTemperatureKelvin &&
                initial.FuelMassKilograms > 0d &&
                oxygenAfterTransport > 0d &&
                availableFuelRate > 0d)
            {
                fuelConsumed = Math.Min(
                    Math.Min(initial.FuelMassKilograms, availableFuelRate),
                    oxygenAfterTransport / parameters.OxygenKilogramsPerKilogramFuel);
                oxygenConsumed = fuelConsumed * parameters.OxygenKilogramsPerKilogramFuel;
                heatReleased = fuelConsumed * parameters.HeatReleaseJoulesPerKilogramFuel;
                if (!double.IsFinite(fuelConsumed) || !double.IsFinite(oxygenConsumed) || !double.IsFinite(heatReleased))
                    throw new OverflowException("Reaction accounting exceeded finite double range.");
            }

            var remainingFuel = initial.FuelMassKilograms - fuelConsumed;
            var remainingOxygen = oxygenAfterTransport - oxygenConsumed;
            var nextEnergy = initial.ThermalEnergyJoules + heatReleased;
            if (!double.IsFinite(nextEnergy))
                throw new OverflowException($"Thermal energy for cell '{cellId}' is outside finite double range.");
            if (remainingFuel < 0d && remainingFuel > -1e-14d)
                remainingFuel = 0d;
            if (remainingOxygen < 0d && remainingOxygen > -1e-14d)
                remainingOxygen = 0d;
            if (remainingFuel < 0d || remainingOxygen < 0d)
                throw new InvalidOperationException($"Reaction accounting produced negative inventory for cell '{cellId}'.");

            var final = initial with
            {
                FuelMassKilograms = remainingFuel,
                OxygenMassKilograms = remainingOxygen,
                ThermalEnergyJoules = nextEnergy
            };
            var finalTemperature = GetTemperature(final, parameters);
            if (!double.IsFinite(finalTemperature))
                throw new OverflowException($"Final temperature for cell '{cellId}' is outside finite double range.");

            results.Add(new SurfaceCombustionCellStepResult(
                initial, oxygenAfterTransport, final, fuelConsumed, oxygenConsumed,
                heatReleased, initialTemperature, finalTemperature));
            totalFuelConsumed += fuelConsumed;
            totalOxygenConsumed += oxygenConsumed;
            totalHeatReleased += heatReleased;
        }

        if (!double.IsFinite(totalFuelConsumed) || !double.IsFinite(totalOxygenConsumed) || !double.IsFinite(totalHeatReleased))
            throw new OverflowException("Combustion step totals are outside finite double range.");

        return new SurfaceCombustionStepResult(
            durationSeconds, results, totalFuelConsumed, totalOxygenConsumed, totalHeatReleased, oxygenResidual, oxygenSupplied);
    }

    /// <summary>Derives temperature from declared lumped heat capacity and reference temperature.</summary>
    public static double GetTemperature(
        SurfaceCombustionCellState cell,
        SurfaceCombustionCellParameters parameters)
    {
        cell.EnsureValid();
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.EnsureValid();
        return parameters.ReferenceTemperatureKelvin +
            cell.ThermalEnergyJoules / parameters.ThermalCapacityJoulesPerKelvin;
    }
}
