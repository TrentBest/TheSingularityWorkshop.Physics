using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Physics.Timing;

/// <summary>
/// Configuration for a constant-tempo, non-mechanical timing oscillator.
/// Phase is measured in radians and tempo in beats per minute.
/// </summary>
public sealed record IdealMetronomeDefinition(
    string ParticipantId,
    double TempoBeatsPerMinute,
    double InitialPhaseRadians,
    int SubdivisionsPerBeat,
    double StartTimeSeconds)
{
    /// <summary>Angular phase rate in radians per second.</summary>
    public double AngularFrequencyRadiansPerSecond =>
        (2d * Math.PI * TempoBeatsPerMinute) / 60d;

    /// <summary>Validates the complete timing-source definition.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ParticipantId))
        {
            throw new ArgumentException("A stable participant ID is required.", nameof(ParticipantId));
        }

        if (!double.IsFinite(TempoBeatsPerMinute) || TempoBeatsPerMinute <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TempoBeatsPerMinute),
                "Tempo must be positive and finite.");
        }

        if (!double.IsFinite(InitialPhaseRadians) ||
            InitialPhaseRadians < 0d ||
            InitialPhaseRadians >= 2d * Math.PI)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialPhaseRadians),
                "Initial phase must be finite and in [0, 2π).");
        }

        if (SubdivisionsPerBeat <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SubdivisionsPerBeat),
                "At least one subdivision per beat is required.");
        }

        if (!double.IsFinite(StartTimeSeconds) || StartTimeSeconds < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StartTimeSeconds),
                "Start time must be non-negative and finite.");
        }

        if (!double.IsFinite(AngularFrequencyRadiansPerSecond))
        {
            throw new ArgumentOutOfRangeException(
                nameof(TempoBeatsPerMinute),
                "The resulting angular frequency must be finite.");
        }
    }
}

/// <summary>
/// One analytically timed subdivision crossing. EventIndex is the integer phase-grid
/// crossing; BeatIndex is zero-based and SubdivisionIndex is zero for a beat boundary.
/// </summary>
public readonly record struct TimingEvent(
    long EventIndex,
    long BeatIndex,
    int SubdivisionIndex,
    double TimeSeconds,
    bool IsBeat);

/// <summary>
/// Generates metronome events from an analytic phase function. Queries use half-open
/// intervals [start, end); an event exactly at start is not repeated.
/// </summary>
public static class IdealMetronome
{
    /// <summary>
    /// Returns all subdivision crossings in [startTimeSeconds, endTimeSeconds).
    /// The query is bounded by maxEvents and does not accumulate rounded tick intervals.
    /// </summary>
    public static ReadOnlyCollection<TimingEvent> GetEvents(
        IdealMetronomeDefinition definition,
        double startTimeSeconds,
        double endTimeSeconds,
        int maxEvents = 100_000)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        if (!double.IsFinite(startTimeSeconds) ||
            startTimeSeconds < definition.StartTimeSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startTimeSeconds),
                "Query start must be finite and not precede the oscillator start time.");
        }

        if (!double.IsFinite(endTimeSeconds) || endTimeSeconds < startTimeSeconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endTimeSeconds),
                "Query end must be finite and not precede query start.");
        }

        if (maxEvents <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxEvents),
                "The event budget must be positive.");
        }

        if (startTimeSeconds == endTimeSeconds)
        {
            return Array.AsReadOnly(Array.Empty<TimingEvent>());
        }

        var subdivisions = definition.SubdivisionsPerBeat;
        var phaseStep = (2d * Math.PI) / subdivisions;
        var angularFrequency = definition.AngularFrequencyRadiansPerSecond;
        var elapsedAtStart = startTimeSeconds - definition.StartTimeSeconds;
        var phaseAtStart = definition.InitialPhaseRadians + (angularFrequency * elapsedAtStart);

        if (!double.IsFinite(phaseAtStart))
        {
            throw new OverflowException("The queried phase exceeds finite double range.");
        }

        var firstIndexValue = Math.Floor(phaseAtStart / phaseStep) + 1d;
        if (!double.IsFinite(firstIndexValue) || firstIndexValue < 0d ||
            firstIndexValue >= (double)long.MaxValue)
        {
            throw new OverflowException("The timing event index exceeds Int64 range.");
        }

        var eventIndex = (long)firstIndexValue;
        var events = new List<TimingEvent>(Math.Min(maxEvents, 256));

        while (true)
        {
            var eventPhase = eventIndex * phaseStep;
            var eventElapsed = (eventPhase - definition.InitialPhaseRadians) / angularFrequency;
            var eventTime = definition.StartTimeSeconds + eventElapsed;

            if (!double.IsFinite(eventTime))
            {
                throw new OverflowException("A timing event exceeds finite double time.");
            }

            // The interval is half-open. This also corrects a candidate rounded onto
            // the query boundary by floating-point division.
            if (eventTime <= startTimeSeconds)
            {
                if (eventIndex == long.MaxValue)
                {
                    throw new OverflowException("The timing event index exceeds Int64 range.");
                }

                eventIndex++;
                continue;
            }

            if (eventTime >= endTimeSeconds)
            {
                break;
            }

            if (events.Count == maxEvents)
            {
                throw new InvalidOperationException(
                    $"The query exceeds its configured event budget of {maxEvents}.");
            }

            var subdivisionIndex = (int)(eventIndex % subdivisions);
            var beatIndex = eventIndex / subdivisions;
            events.Add(new TimingEvent(
                eventIndex,
                beatIndex,
                subdivisionIndex,
                eventTime,
                subdivisionIndex == 0));

            if (eventIndex == long.MaxValue)
            {
                throw new OverflowException("The timing event index exceeds Int64 range.");
            }

            eventIndex++;
        }

        return Array.AsReadOnly(events.ToArray());
    }
}
