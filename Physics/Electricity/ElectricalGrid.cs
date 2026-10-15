namespace TheSingularityWorkshop.Physics.Electricity;

/// <summary>
/// Describes an ideal resistive electrical load connected to a nominal DC bus.
/// Resistance is in ohms. This baseline excludes startup transients and
/// voltage-dependent or switched-mode behavior.
/// </summary>
public sealed record ResistiveElectricalLoad
{
    /// <summary>Creates a resistive load with a stable identity and positive resistance.</summary>
    public ResistiveElectricalLoad(string id, double resistanceOhms, bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A load identity is required.", nameof(id));
        if (!double.IsFinite(resistanceOhms) || resistanceOhms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(resistanceOhms), "Resistance must be positive and finite.");

        Id = id;
        ResistanceOhms = resistanceOhms;
        IsEnabled = isEnabled;
    }

    /// <summary>Stable identity for this load.</summary>
    public string Id { get; }

    /// <summary>Electrical resistance in ohms.</summary>
    public double ResistanceOhms { get; }

    /// <summary>Whether the load's switch is closed.</summary>
    public bool IsEnabled { get; }

    /// <summary>Returns a new load snapshot with its switch set to the requested state.</summary>
    public ResistiveElectricalLoad WithEnabled(bool isEnabled) => new(Id, ResistanceOhms, isEnabled);
}

/// <summary>Immutable result for one load observed on an ideal fixed-voltage DC bus.</summary>
public sealed record ElectricalLoadObservation(
    string LoadId,
    bool IsEnabled,
    double VoltageVolts,
    double ResistanceOhms,
    double CurrentAmperes,
    double PowerWatts);

/// <summary>
/// Immutable aggregate observation of an ideal fixed-voltage DC grid.
/// Capacity identifies overload; this baseline does not solve voltage droop,
/// generator dynamics, protection, or load shedding.
/// </summary>
public sealed record ElectricalGridObservation(
    double BusVoltageVolts,
    double GenerationCapacityWatts,
    double DemandWatts,
    double TotalCurrentAmperes,
    double RemainingCapacityWatts,
    bool IsOverloaded,
    IReadOnlyList<ElectricalLoadObservation> Loads);

/// <summary>
/// Calculates demand from enabled ideal resistive loads on a nominal fixed-voltage DC bus.
/// Uses Ohm's law and P = V I. This is analytical accounting, not a network solver
/// or a model of AC power systems.
/// </summary>
public static class ElectricalGrid
{
    /// <summary>Observes loads and compares aggregate demand with declared generation capacity.</summary>
    /// <param name="busVoltageVolts">Positive nominal DC bus voltage in volts.</param>
    /// <param name="generationCapacityWatts">Non-negative available generation capacity in watts.</param>
    /// <param name="loads">Loads to observe. Identities must be unique.</param>
    /// <returns>A deterministic, identity-sorted snapshot of load and grid demand.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Voltage or capacity is outside its valid range.</exception>
    /// <exception cref="ArgumentException">The load collection is null or contains duplicate identities.</exception>
    /// <exception cref="OverflowException">A calculated current or power is not finite.</exception>
    public static ElectricalGridObservation Observe(
        double busVoltageVolts,
        double generationCapacityWatts,
        IEnumerable<ResistiveElectricalLoad> loads)
    {
        if (!double.IsFinite(busVoltageVolts) || busVoltageVolts <= 0d)
            throw new ArgumentOutOfRangeException(nameof(busVoltageVolts), "Bus voltage must be positive and finite.");
        if (!double.IsFinite(generationCapacityWatts) || generationCapacityWatts < 0d)
            throw new ArgumentOutOfRangeException(nameof(generationCapacityWatts), "Generation capacity must be non-negative and finite.");
        ArgumentNullException.ThrowIfNull(loads);

        var orderedLoads = loads.OrderBy(load => load?.Id, StringComparer.Ordinal).ToArray();
        if (orderedLoads.Any(load => load is null))
            throw new ArgumentException("The load collection cannot contain null entries.", nameof(loads));
        if (orderedLoads.Select(load => load.Id).Distinct(StringComparer.Ordinal).Count() != orderedLoads.Length)
            throw new ArgumentException("Load identities must be unique within a grid observation.", nameof(loads));

        var observations = new ElectricalLoadObservation[orderedLoads.Length];
        var totalPower = 0d;
        var totalCurrent = 0d;

        for (var index = 0; index < orderedLoads.Length; index++)
        {
            var load = orderedLoads[index];
            var current = load.IsEnabled ? busVoltageVolts / load.ResistanceOhms : 0d;
            var power = load.IsEnabled ? busVoltageVolts * current : 0d;
            if (!double.IsFinite(current) || !double.IsFinite(power))
                throw new OverflowException($"The calculated electrical state for load '{load.Id}' is not finite.");

            totalCurrent += current;
            totalPower += power;
            if (!double.IsFinite(totalCurrent) || !double.IsFinite(totalPower))
                throw new OverflowException("The aggregate electrical demand is outside the finite double range.");

            observations[index] = new ElectricalLoadObservation(
                load.Id, load.IsEnabled, busVoltageVolts, load.ResistanceOhms, current, power);
        }

        var remainingCapacity = generationCapacityWatts - totalPower;
        return new ElectricalGridObservation(
            busVoltageVolts,
            generationCapacityWatts,
            totalPower,
            totalCurrent,
            remainingCapacity,
            totalPower > generationCapacityWatts,
            Array.AsReadOnly(observations));
    }
}
