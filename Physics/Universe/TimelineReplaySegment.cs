namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Binds one validated history segment to one interval in a replay plan.
/// This validates replay inputs but does not interpret payloads or execute a model.
/// </summary>
public sealed class TimelineReplaySegment
{
    private readonly TimelineEvent[] _events;

    /// <summary>Validates that history belongs exactly to one interval in the supplied replay route.</summary>
    /// <param name="plan">Previously validated replay route.</param>
    /// <param name="interval">Timeline-owned interval from that route.</param>
    /// <param name="history">History rooted at the interval's exclusive start tick.</param>
    public TimelineReplaySegment(
        TimelineReplayPlan plan,
        TimelineReplayInterval interval,
        TimelineHistorySegment history)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(interval);
        ArgumentNullException.ThrowIfNull(history);

        bool belongsToPlan = plan.CopyIntervals().Any(candidate =>
            string.Equals(candidate.TimelineId, interval.TimelineId, StringComparison.Ordinal)
            && candidate.StartExclusiveTick == interval.StartExclusiveTick
            && candidate.EndInclusiveTick == interval.EndInclusiveTick);

        if (!belongsToPlan)
        {
            throw new ArgumentException("The interval is not part of the supplied replay plan.", nameof(interval));
        }

        PhysicsCheckpoint checkpoint = history.Checkpoint;
        if (!string.Equals(checkpoint.RealityId, plan.Lineage.RealityId, StringComparison.Ordinal))
        {
            throw new ArgumentException("The history checkpoint belongs to a different reality.", nameof(history));
        }

        if (!string.Equals(checkpoint.TimelineId, interval.TimelineId, StringComparison.Ordinal))
        {
            throw new ArgumentException("The history checkpoint belongs to a different timeline than the interval.", nameof(history));
        }

        if (checkpoint.Tick != interval.StartExclusiveTick)
        {
            throw new ArgumentException("The history checkpoint must match the interval's exclusive start tick.", nameof(history));
        }

        if (!string.Equals(checkpoint.SchemaVersion, plan.RequiredSchemaVersion, StringComparison.Ordinal)
            || !string.Equals(checkpoint.ModelVersion, plan.RequiredModelVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException("The history checkpoint versions do not match the replay plan.", nameof(history));
        }

        if (history.ThroughTick != interval.EndInclusiveTick)
        {
            throw new ArgumentException("The history segment must end at the interval's inclusive end tick.", nameof(history));
        }

        _events = history.CopyEvents().ToArray();
        foreach (TimelineEvent item in _events)
        {
            if (item.Tick <= interval.StartExclusiveTick || item.Tick > interval.EndInclusiveTick)
            {
                throw new ArgumentException(
                    "Replay events must fall after the interval start and at or before its end.",
                    nameof(history));
            }
        }

        Plan = plan;
        Interval = interval;
        History = history;
    }

    /// <summary>Replay plan this validated segment belongs to.</summary>
    public TimelineReplayPlan Plan { get; }

    /// <summary>Timeline-owned interval covered by this segment.</summary>
    public TimelineReplayInterval Interval { get; }

    /// <summary>Validated source history envelope.</summary>
    public TimelineHistorySegment History { get; }

    /// <summary>Number of validated events owned by this interval.</summary>
    public int EventCount => _events.Length;

    /// <summary>Returns a read-only wrapper over a defensive copy of events for this interval.</summary>
    public IReadOnlyList<TimelineEvent> CopyEvents() =>
        Array.AsReadOnly((TimelineEvent[])_events.Clone());
}
