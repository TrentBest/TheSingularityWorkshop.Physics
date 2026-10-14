namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// An immutable, ordered input or event recorded for deterministic reconstruction.
/// Payload interpretation and event execution belong to a model-specific replay layer.
/// </summary>
public sealed record TimelineEvent
{
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

    public long Sequence { get; }
    public long Tick { get; }
    public string Kind { get; }
    public string Payload { get; }
}
