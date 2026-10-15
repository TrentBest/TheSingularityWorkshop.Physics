namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// A validated forward-reconstruction route from a checkpoint to a target tick.
/// This describes which timeline owns each post-checkpoint interval; it does not load events or execute physics.
/// </summary>
public sealed class TimelineReplayPlan
{
    private readonly TimelineReplayInterval[] _intervals;

    /// <summary>Creates a replay route after validating timeline ancestry, target bounds, and checkpoint versions.</summary>
    /// <param name="lineage">Validated root-to-target timeline lineage.</param>
    /// <param name="checkpoint">Starting authoritative state.</param>
    /// <param name="targetTick">Target logical tick on the target timeline.</param>
    /// <param name="requiredSchemaVersion">Schema version required by the caller.</param>
    /// <param name="requiredModelVersion">Model/configuration version required by the caller.</param>
    public TimelineReplayPlan(
        TimelineLineage lineage,
        PhysicsCheckpoint checkpoint,
        long targetTick,
        string requiredSchemaVersion,
        string requiredModelVersion)
    {
        ArgumentNullException.ThrowIfNull(lineage);
        ArgumentNullException.ThrowIfNull(checkpoint);

        if (string.IsNullOrWhiteSpace(requiredSchemaVersion))
        {
            throw new ArgumentException("A required schema version must be supplied.", nameof(requiredSchemaVersion));
        }

        if (string.IsNullOrWhiteSpace(requiredModelVersion))
        {
            throw new ArgumentException("A required model version must be supplied.", nameof(requiredModelVersion));
        }

        if (!lineage.CanReconstructFrom(checkpoint, targetTick))
        {
            throw new ArgumentException(
                "The checkpoint is not eligible for the requested target and lineage.",
                nameof(checkpoint));
        }

        if (targetTick < lineage.Target.ForkTick)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetTick),
                "A target on a child timeline cannot precede that timeline's fork point.");
        }

        if (!string.Equals(checkpoint.SchemaVersion, requiredSchemaVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException("The checkpoint schema version does not match the required version.", nameof(checkpoint));
        }

        if (!string.Equals(checkpoint.ModelVersion, requiredModelVersion, StringComparison.Ordinal))
        {
            throw new ArgumentException("The checkpoint model version does not match the required version.", nameof(checkpoint));
        }

        Checkpoint = checkpoint;
        Lineage = lineage;
        TargetTick = targetTick;
        RequiredSchemaVersion = requiredSchemaVersion;
        RequiredModelVersion = requiredModelVersion;

        IReadOnlyList<TimelineBranch> branches = lineage.CopyBranches();
        int checkpointIndex = -1;
        for (int i = 0; i < branches.Count; i++)
        {
            if (string.Equals(branches[i].Id, checkpoint.TimelineId, StringComparison.Ordinal))
            {
                checkpointIndex = i;
                break;
            }
        }

        var intervals = new List<TimelineReplayInterval>();
        long startExclusive = checkpoint.Tick;

        for (int i = checkpointIndex; i < branches.Count; i++)
        {
            TimelineBranch branch = branches[i];
            long endInclusive = i < branches.Count - 1
                ? branches[i + 1].ForkTick
                : targetTick;

            if (endInclusive > targetTick)
            {
                endInclusive = targetTick;
            }

            if (endInclusive < startExclusive)
            {
                throw new ArgumentException("Timeline fork boundaries do not form a forward replay route.", nameof(lineage));
            }

            intervals.Add(new TimelineReplayInterval(branch.Id, startExclusive, endInclusive));
            if (endInclusive == targetTick)
            {
                break;
            }

            startExclusive = endInclusive;
        }

        _intervals = intervals.ToArray();
    }

    /// <summary>Validated checkpoint used as the route's starting state.</summary>
    public PhysicsCheckpoint Checkpoint { get; }

    /// <summary>Validated root-to-target lineage used to construct the route.</summary>
    public TimelineLineage Lineage { get; }

    /// <summary>Requested logical tick on the target timeline.</summary>
    public long TargetTick { get; }

    /// <summary>Schema version required for this reconstruction.</summary>
    public string RequiredSchemaVersion { get; }

    /// <summary>Model/configuration version required for this reconstruction.</summary>
    public string RequiredModelVersion { get; }

    /// <summary>Number of timeline-owned intervals in the route.</summary>
    public int IntervalCount => _intervals.Length;

    /// <summary>Returns a read-only wrapper over a defensive copy of the route intervals.</summary>
    public IReadOnlyList<TimelineReplayInterval> CopyIntervals() =>
        Array.AsReadOnly((TimelineReplayInterval[])_intervals.Clone());
}

/// <summary>
/// Describes a timeline-owned replay interval with an exclusive start and inclusive end tick.
/// Events at the fork tick belong to the inherited parent history; child-owned replay starts after that tick.
/// </summary>
public sealed record TimelineReplayInterval
{
    /// <summary>Creates an interval for one timeline's owned portion of a replay route.</summary>
    /// <param name="timelineId">Timeline whose events are to be considered.</param>
    /// <param name="startExclusiveTick">Exclusive starting tick.</param>
    /// <param name="endInclusiveTick">Inclusive ending tick.</param>
    public TimelineReplayInterval(string timelineId, long startExclusiveTick, long endInclusiveTick)
    {
        if (string.IsNullOrWhiteSpace(timelineId))
        {
            throw new ArgumentException("Timeline identifier must be non-empty.", nameof(timelineId));
        }

        if (startExclusiveTick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startExclusiveTick), "Logical ticks cannot be negative.");
        }

        if (endInclusiveTick < startExclusiveTick)
        {
            throw new ArgumentOutOfRangeException(nameof(endInclusiveTick), "Interval end cannot precede its start.");
        }

        TimelineId = timelineId;
        StartExclusiveTick = startExclusiveTick;
        EndInclusiveTick = endInclusiveTick;
    }

    /// <summary>Timeline whose owned events belong to this interval.</summary>
    public string TimelineId { get; }

    /// <summary>Exclusive lower tick boundary.</summary>
    public long StartExclusiveTick { get; }

    /// <summary>Inclusive upper tick boundary.</summary>
    public long EndInclusiveTick { get; }
}
