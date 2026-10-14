namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Validates a contiguous event-history segment relative to a checkpoint.
/// This is a data contract, not a persistence store or replay engine.
/// </summary>
public sealed class TimelineHistorySegment
{
    private readonly TimelineEvent[] _events;

    public TimelineHistorySegment(PhysicsCheckpoint checkpoint, IEnumerable<TimelineEvent> events, long throughTick)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(events);

        if (throughTick < checkpoint.Tick)
        {
            throw new ArgumentOutOfRangeException(nameof(throughTick), "History cannot end before its checkpoint.");
        }

        Checkpoint = checkpoint;
        ThroughTick = throughTick;
        _events = events.ToArray();

        long previousSequence = -1;
        long previousTick = checkpoint.Tick;

        foreach (var item in _events)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.Sequence <= previousSequence)
            {
                throw new ArgumentException("Event sequence numbers must be strictly increasing.", nameof(events));
            }

            if (item.Tick < checkpoint.Tick || item.Tick < previousTick || item.Tick > throughTick)
            {
                throw new ArgumentException(
                    "Events must be ordered by nondecreasing tick and lie within the checkpoint-to-target range.",
                    nameof(events));
            }

            previousSequence = item.Sequence;
            previousTick = item.Tick;
        }
    }

    public PhysicsCheckpoint Checkpoint { get; }
    public long ThroughTick { get; }
    public int EventCount => _events.Length;

    /// <summary>Returns a read-only wrapper over a defensive copy of the event list.</summary>
    public IReadOnlyList<TimelineEvent> CopyEvents() => Array.AsReadOnly((TimelineEvent[])_events.Clone());
}
