namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Executes a complete fork-aware replay route assembled from one validated history segment per plan interval.
/// Each segment after the first must begin from a checkpoint that represents the preceding segment's boundary state.
/// </summary>
public static class TimelineReplayRouteExecutor
{
    /// <summary>
    /// Reconstructs a target state across all timeline intervals, checking state continuity at each fork boundary.
    /// </summary>
    /// <typeparam name="TState">The model's state representation.</typeparam>
    /// <param name="plan">Validated root-to-target replay plan.</param>
    /// <param name="segments">One validated history segment for every plan interval, in route order.</param>
    /// <param name="restore">Decodes a checkpoint payload into model state.</param>
    /// <param name="applyEvent">Applies a recorded event to state.</param>
    /// <param name="advance">Advances state between logical ticks.</param>
    /// <param name="statesMatch">Determines whether reconstructed and checkpoint states are equivalent at a fork boundary.</param>
    /// <returns>Reconstructed model state at the plan's target tick.</returns>
    /// <remarks>
    /// This coordinator does not own persistence or model semantics. A boundary comparison is required because
    /// every interval is independently anchored by its supplied checkpoint. For exact replay, the adapter must
    /// provide deterministic operations and a comparison appropriate to its state representation.
    /// </remarks>
    public static TState Reconstruct<TState>(
        TimelineReplayPlan plan,
        IReadOnlyList<TimelineReplaySegment> segments,
        Func<PhysicsCheckpoint, TState> restore,
        Func<TState, TimelineEvent, TState> applyEvent,
        Func<TState, long, long, TState> advance,
        Func<TState, TState, bool> statesMatch)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(restore);
        ArgumentNullException.ThrowIfNull(applyEvent);
        ArgumentNullException.ThrowIfNull(advance);
        ArgumentNullException.ThrowIfNull(statesMatch);

        IReadOnlyList<TimelineReplayInterval> intervals = plan.CopyIntervals();
        if (segments.Count != intervals.Count || segments.Count == 0)
        {
            throw new ArgumentException(
                "A complete replay route requires exactly one validated segment for each plan interval.",
                nameof(segments));
        }

        for (int i = 0; i < segments.Count; i++)
        {
            TimelineReplaySegment segment = segments[i]
                ?? throw new ArgumentException("Replay segments cannot contain null entries.", nameof(segments));

            if (!ReferenceEquals(segment.Plan, plan)
                || !string.Equals(segment.Interval.TimelineId, intervals[i].TimelineId, StringComparison.Ordinal)
                || segment.Interval.StartExclusiveTick != intervals[i].StartExclusiveTick
                || segment.Interval.EndInclusiveTick != intervals[i].EndInclusiveTick)
            {
                throw new ArgumentException(
                    "Replay segments must belong to this plan and appear in exact interval order.",
                    nameof(segments));
            }
        }

        PhysicsCheckpoint firstCheckpoint = segments[0].History.Checkpoint;
        if (!string.Equals(firstCheckpoint.Id, plan.Checkpoint.Id, StringComparison.Ordinal)
            || !string.Equals(firstCheckpoint.StateSha256, plan.Checkpoint.StateSha256, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The first replay segment must use the checkpoint that anchors the plan.",
                nameof(segments));
        }

        TState? previousBoundaryState = default;
        bool hasPreviousBoundary = false;

        for (int i = 0; i < segments.Count; i++)
        {
            TimelineReplaySegment segment = segments[i];
            TState boundaryState = TimelineReplayExecutor.Reconstruct(
                segment,
                segment.Interval.StartExclusiveTick,
                restore,
                applyEvent,
                advance);

            if (hasPreviousBoundary && !statesMatch(previousBoundaryState!, boundaryState))
            {
                throw new InvalidOperationException(
                    $"The checkpoint state at timeline boundary tick {segment.Interval.StartExclusiveTick} " +
                    $"does not match the state reconstructed from the preceding interval.");
            }

            TState reconstructed = TimelineReplayExecutor.Reconstruct(
                segment,
                segment.Interval.EndInclusiveTick,
                restore,
                applyEvent,
                advance);

            previousBoundaryState = reconstructed;
            hasPreviousBoundary = true;

            if (i == segments.Count - 1)
            {
                return TimelineReplayExecutor.Reconstruct(
                    segment,
                    plan.TargetTick,
                    restore,
                    applyEvent,
                    advance);
            }
        }

        throw new InvalidOperationException("The replay route did not produce a target state.");
    }
}
