namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// An immutable, ordered input or event recorded for deterministic reconstruction.
/// Payload interpretation and event execution belong to a model-specific replay layer.
/// </summary>
public sealed record TimelineEvent
{
    /// <summary>Creates a sequenced event or input record.</summary>
    /// <param name="sequence">Strictly increasing position within its history segment.</param>
    /// <param name="tick">Logical tick at which the record applies.</param>
    /// <param name="kind">Model-defined event/input kind.</param>
    /// <param name="payload">Opaque payload interpreted by the replay layer.</param>
    public TimelineEvent(long sequence, long tick, string kind, string payload)
    {
        if (sequence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), "Event sequence numbers cannot be negative.");
        }

        if (tick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tick), "Logical ticks cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(kind))
        {
            throw new ArgumentException("Event kind must be non-empty.", nameof(kind));
        }

        ArgumentNullException.ThrowIfNull(payload);

        Sequence = sequence;
        Tick = tick;
        Kind = kind;
        Payload = payload;
    }

    /// <summary>Sequence position within the recorded history.</summary>
    public long Sequence { get; }
    /// <summary>Logical tick at which this record applies.</summary>
    public long Tick { get; }
    /// <summary>Model-defined event/input kind.</summary>
    public string Kind { get; }
    /// <summary>Opaque event/input payload.</summary>
    public string Payload { get; }
}
