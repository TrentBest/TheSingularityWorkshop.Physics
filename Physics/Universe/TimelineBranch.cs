namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Immutable lineage metadata for one timeline within a reality.
/// This identifies a branch point; it does not itself persist or restore physical state.
/// </summary>
public sealed record TimelineBranch
{
    private TimelineBranch(string id, string realityId, string? parentTimelineId, long forkTick, long headTick)
    {
        Id = RequireIdentifier(id, nameof(id));
        RealityId = RequireIdentifier(realityId, nameof(realityId));

        if (parentTimelineId is not null)
        {
            ParentTimelineId = RequireIdentifier(parentTimelineId, nameof(parentTimelineId));
            if (string.Equals(Id, ParentTimelineId, StringComparison.Ordinal))
            {
                throw new ArgumentException("A timeline cannot be its own parent.", nameof(parentTimelineId));
            }
        }

        if (forkTick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(forkTick), "Logical ticks cannot be negative.");
        }

        if (headTick < forkTick)
        {
            throw new ArgumentOutOfRangeException(nameof(headTick), "The head cannot precede the branch's fork point.");
        }

        ForkTick = forkTick;
        HeadTick = headTick;
    }

    /// <summary>Stable caller-assigned identifier for this timeline.</summary>
    public string Id { get; }

    /// <summary>Identifier of the reality containing this timeline.</summary>
    public string RealityId { get; }

    /// <summary>Parent timeline identifier, or null for a root timeline.</summary>
    public string? ParentTimelineId { get; }

    /// <summary>Logical tick at which this timeline begins or diverges from its parent.</summary>
    public long ForkTick { get; }

    /// <summary>Latest logical tick represented by this timeline's known history.</summary>
    public long HeadTick { get; }

    /// <summary>Creates the root timeline for a reality at logical tick zero.</summary>
    public static TimelineBranch CreateRoot(string id, string realityId) =>
        new(id, realityId, null, 0, 0);

    /// <summary>
    /// Creates a child timeline at a historical tick represented by the parent.
    /// The parent remains unchanged; shared history and physical snapshots are managed by a persistence layer.
    /// </summary>
    public static TimelineBranch Fork(string id, TimelineBranch parent, long forkTick)
    {
        ArgumentNullException.ThrowIfNull(parent);

        if (forkTick < parent.ForkTick || forkTick > parent.HeadTick)
        {
            throw new ArgumentOutOfRangeException(
                nameof(forkTick),
                "The fork point must be within the parent's represented history.");
        }

        return new TimelineBranch(id, parent.RealityId, parent.Id, forkTick, forkTick);
    }

    private static string RequireIdentifier(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Identifiers must be non-empty.", parameterName);
        }

        return value;
    }
}
