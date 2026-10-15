namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Validated root-to-target ancestry for timelines within one reality.
/// This is lineage metadata only; it does not load history or execute replay.
/// </summary>
public sealed class TimelineLineage
{
    private readonly TimelineBranch[] _branches;

    /// <summary>
    /// Creates a lineage ordered from the root timeline to the selected target timeline.
    /// </summary>
    /// <param name="branches">Non-empty root-to-target sequence of timeline descriptors.</param>
    public TimelineLineage(IEnumerable<TimelineBranch> branches)
    {
        ArgumentNullException.ThrowIfNull(branches);

        _branches = branches.ToArray();
        if (_branches.Length == 0)
        {
            throw new ArgumentException("A lineage must contain at least one timeline.", nameof(branches));
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        string realityId = _branches[0]?.RealityId
            ?? throw new ArgumentException("A lineage cannot contain null timeline descriptors.", nameof(branches));

        for (int i = 0; i < _branches.Length; i++)
        {
            TimelineBranch branch = _branches[i]
                ?? throw new ArgumentException("A lineage cannot contain null timeline descriptors.", nameof(branches));

            if (!ids.Add(branch.Id))
            {
                throw new ArgumentException("A lineage cannot contain a timeline more than once.", nameof(branches));
            }

            if (!string.Equals(branch.RealityId, realityId, StringComparison.Ordinal))
            {
                throw new ArgumentException("All timelines in a lineage must belong to the same reality.", nameof(branches));
            }

            if (i == 0)
            {
                if (branch.ParentTimelineId is not null)
                {
                    throw new ArgumentException("The first timeline in a lineage must be a root.", nameof(branches));
                }

                continue;
            }

            TimelineBranch parent = _branches[i - 1]
                ?? throw new ArgumentException("A lineage cannot contain null timeline descriptors.", nameof(branches));

            if (!string.Equals(branch.ParentTimelineId, parent.Id, StringComparison.Ordinal))
            {
                throw new ArgumentException("Each timeline must name the preceding timeline as its parent.", nameof(branches));
            }

            if (branch.ForkTick > parent.HeadTick)
            {
                throw new ArgumentException("A child timeline cannot fork beyond the represented parent head.", nameof(branches));
            }
        }

        RealityId = realityId;
    }

    /// <summary>Reality identifier shared by all timelines in this lineage.</summary>
    public string RealityId { get; }

    /// <summary>Number of timeline descriptors in the root-to-target lineage.</summary>
    public int Count => _branches.Length;

    /// <summary>The root timeline in this lineage.</summary>
    public TimelineBranch Root => _branches[0];

    /// <summary>The selected target timeline in this lineage.</summary>
    public TimelineBranch Target => _branches[^1];

    /// <summary>Returns a read-only wrapper over a defensive copy of the lineage.</summary>
    public IReadOnlyList<TimelineBranch> CopyBranches() =>
        Array.AsReadOnly((TimelineBranch[])_branches.Clone());

    /// <summary>
    /// Determines whether a checkpoint timeline appears in this lineage and whether
    /// the checkpoint tick lies within the history shared with the selected target.
    /// </summary>
    /// <param name="checkpoint">Checkpoint to assess.</param>
    /// <param name="targetTick">Requested tick on the target timeline.</param>
    /// <returns>True when the checkpoint is on this ancestry and no later than the requested tick.</returns>
    public bool CanReconstructFrom(PhysicsCheckpoint checkpoint, long targetTick)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);

        if (!string.Equals(checkpoint.RealityId, RealityId, StringComparison.Ordinal)
            || targetTick < checkpoint.Tick
            || targetTick < Target.ForkTick
            || targetTick > Target.HeadTick)
        {
            return false;
        }

        int checkpointIndex = Array.FindIndex(
            _branches,
            branch => string.Equals(branch.Id, checkpoint.TimelineId, StringComparison.Ordinal));

        if (checkpointIndex < 0 || checkpoint.Tick < _branches[checkpointIndex].ForkTick)
        {
            return false;
        }

        // A checkpoint on an ancestor is reusable only through the next branch's fork point.
        if (checkpointIndex < _branches.Length - 1
            && checkpoint.Tick > _branches[checkpointIndex + 1].ForkTick)
        {
            return false;
        }

        return true;
    }
}
