namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Executes a validated replay segment through caller-supplied model operations.
/// Physics owns the replay ordering; the caller owns checkpoint decoding, event meaning,
/// and deterministic model advancement.
/// </summary>
public static class TimelineReplayExecutor
{
    /// <summary>
    /// Restores a checkpoint and reconstructs state through a requested tick within one validated segment.
    /// </summary>
    /// <typeparam name="TState">The model's state representation.</typeparam>
    /// <param name="segment">History already validated against a replay plan and timeline interval.</param>
    /// <param name="targetTick">Inclusive target tick; may be earlier than the segment's end for scrubbing.</param>
    /// <param name="restore">Decodes the checkpoint payload into the model state.</param>
    /// <param name="applyEvent">Applies one recorded event to the state, returning the resulting state.</param>
    /// <param name="advance">Advances state from one logical tick to another without crossing recorded events.</param>
    /// <returns>The reconstructed model state at <paramref name="targetTick"/>.</returns>
    /// <remarks>
    /// Events are applied in their validated sequence order. Before each event, the model is advanced
    /// to that event's tick; after the final event, it is advanced to the requested target tick.
    /// Events at a tick are applied before advancing beyond that tick. The supplied operations must be
    /// deterministic for exact replay. This method cannot establish determinism or physical fidelity
    /// on behalf of a model-specific adapter.
    /// </remarks>
    public static TState Reconstruct<TState>(
        TimelineReplaySegment segment,
        long targetTick,
        Func<PhysicsCheckpoint, TState> restore,
        Func<TState, TimelineEvent, TState> applyEvent,
        Func<TState, long, long, TState> advance)
    {
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentNullException.ThrowIfNull(restore);
        ArgumentNullException.ThrowIfNull(applyEvent);
        ArgumentNullException.ThrowIfNull(advance);

        TimelineReplayInterval interval = segment.Interval;
        if (targetTick < interval.StartExclusiveTick || targetTick > interval.EndInclusiveTick)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetTick),
                "The target tick must lie within the validated replay interval.");
        }

        TState state = restore(segment.History.Checkpoint);
        long currentTick = interval.StartExclusiveTick;

        foreach (TimelineEvent item in segment.CopyEvents())
        {
            if (item.Tick > targetTick)
            {
                break;
            }

            state = advance(state, currentTick, item.Tick);
            currentTick = item.Tick;
            state = applyEvent(state, item);
        }

        return advance(state, currentTick, targetTick);
    }
}
