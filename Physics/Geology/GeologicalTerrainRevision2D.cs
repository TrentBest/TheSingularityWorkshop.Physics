namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// An immutable, identified terrain state revision with optional parent and applied geological event.
/// </summary>
public sealed class GeologicalTerrainRevision2D
{
    /// <summary>Creates the initial revision of a terrain history.</summary>
    /// <param name="revisionId">Stable identity of this revision.</param>
    /// <param name="terrain">Immutable terrain snapshot represented by this revision.</param>
    public GeologicalTerrainRevision2D(string revisionId, TerrainHeightField2D terrain)
    {
        RevisionId = RequireText(revisionId, nameof(revisionId));
        Terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
    }

    internal GeologicalTerrainRevision2D(
        string revisionId,
        string parentRevisionId,
        TerrainHeightField2D terrain,
        GeologicalEventRecord appliedEvent)
    {
        RevisionId = RequireText(revisionId, nameof(revisionId));
        ParentRevisionId = RequireText(parentRevisionId, nameof(parentRevisionId));
        Terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        AppliedEvent = appliedEvent ?? throw new ArgumentNullException(nameof(appliedEvent));
    }

    /// <summary>Stable identity of this immutable terrain revision.</summary>
    public string RevisionId { get; }

    /// <summary>Identity of the revision from which this revision was derived; null for an initial revision.</summary>
    public string? ParentRevisionId { get; }

    /// <summary>Immutable exterior terrain snapshot.</summary>
    public TerrainHeightField2D Terrain { get; }

    /// <summary>Geological event that produced this revision; null for an initial revision.</summary>
    public GeologicalEventRecord? AppliedEvent { get; }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }
}

/// <summary>
/// A validated, uncommitted terrain candidate derived from exactly one source revision.
/// </summary>
public sealed class GeologicalTerrainCandidate2D
{
    private GeologicalTerrainCandidate2D(
        GeologicalTerrainRevision2D sourceRevision,
        string resultRevisionId,
        TerrainHeightField2D terrain,
        GeologicalEventRecord geologicalEvent)
    {
        SourceRevision = sourceRevision;
        ResultRevisionId = resultRevisionId;
        Terrain = terrain;
        GeologicalEvent = geologicalEvent;
    }

    /// <summary>Exact immutable source revision used to construct this candidate.</summary>
    public GeologicalTerrainRevision2D SourceRevision { get; }

    /// <summary>Stable identity requested for the candidate result revision.</summary>
    public string ResultRevisionId { get; }

    /// <summary>Validated immutable candidate terrain snapshot.</summary>
    public TerrainHeightField2D Terrain { get; }

    /// <summary>Event provenance linking the source revision to this candidate result.</summary>
    public GeologicalEventRecord GeologicalEvent { get; }

    /// <summary>
    /// Builds and validates an uncommitted candidate without changing the source revision or any store.
    /// </summary>
    /// <param name="sourceRevision">Immutable revision from which the candidate is derived.</param>
    /// <param name="resultRevisionId">Stable identity for the candidate result.</param>
    /// <param name="geologicalEvent">Event whose source/result IDs must match this transaction.</param>
    /// <param name="transform">Pure or deterministic operation that returns a new terrain snapshot.</param>
    /// <param name="validateCandidate">Optional domain validator; false rejects the candidate.</param>
    /// <returns>A candidate that may later be offered to a revision store.</returns>
    /// <remarks>
    /// This first transaction contract preserves the source height-field geometry and coordinate
    /// semantics. It permits elevation changes only; resampling, reprojection, and dimension changes
    /// require separate explicit operations. The callback must not mutate external authoritative state.
    /// </remarks>
    public static GeologicalTerrainCandidate2D Create(
        GeologicalTerrainRevision2D sourceRevision,
        string resultRevisionId,
        GeologicalEventRecord geologicalEvent,
        Func<TerrainHeightField2D, TerrainHeightField2D> transform,
        Func<TerrainHeightField2D, bool>? validateCandidate = null)
    {
        ArgumentNullException.ThrowIfNull(sourceRevision);
        ArgumentNullException.ThrowIfNull(geologicalEvent);
        ArgumentNullException.ThrowIfNull(transform);
        if (string.IsNullOrWhiteSpace(resultRevisionId))
            throw new ArgumentException("A non-empty result revision ID is required.", nameof(resultRevisionId));
        if (string.Equals(sourceRevision.RevisionId, resultRevisionId, StringComparison.Ordinal))
            throw new ArgumentException("A result revision must have a new identity.", nameof(resultRevisionId));
        if (!string.Equals(geologicalEvent.SourceRevisionId, sourceRevision.RevisionId, StringComparison.Ordinal))
            throw new ArgumentException("Event source revision does not match the candidate source.", nameof(geologicalEvent));
        if (!string.Equals(geologicalEvent.ResultRevisionId, resultRevisionId, StringComparison.Ordinal))
            throw new ArgumentException("Event result revision does not match the candidate result.", nameof(geologicalEvent));

        var candidateTerrain = transform(sourceRevision.Terrain)
            ?? throw new InvalidOperationException("Terrain transformation returned null.");

        EnsureSameGeometry(sourceRevision.Terrain, candidateTerrain);
        if (validateCandidate is not null && !validateCandidate(candidateTerrain))
            throw new InvalidOperationException("Geological candidate validation rejected the result.");

        return new GeologicalTerrainCandidate2D(
            sourceRevision, resultRevisionId, candidateTerrain, geologicalEvent);
    }

    private static void EnsureSameGeometry(TerrainHeightField2D source, TerrainHeightField2D candidate)
    {
        if (source.Width != candidate.Width ||
            source.Height != candidate.Height ||
            source.CellWidth != candidate.CellWidth ||
            source.CellHeight != candidate.CellHeight ||
            source.OriginX != candidate.OriginX ||
            source.OriginY != candidate.OriginY ||
            !string.Equals(source.CoordinateFrameId, candidate.CoordinateFrameId, StringComparison.Ordinal) ||
            !string.Equals(source.VerticalDatum, candidate.VerticalDatum, StringComparison.Ordinal) ||
            !string.Equals(source.LinearUnit, candidate.LinearUnit, StringComparison.Ordinal) ||
            !string.Equals(source.PositiveVerticalDirection, candidate.PositiveVerticalDirection, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "A geological terrain candidate must preserve source dimensions, spacing, origin, frame, datum, units, and vertical direction.");
        }
    }
}

/// <summary>
/// Thread-safe in-memory commit point for a single current geological terrain revision.
/// </summary>
/// <remarks>
/// Candidate construction and validation happen before commit. Commit uses optimistic source identity
/// and a lock so only one candidate derived from the current revision can advance the store. This is
/// an in-memory state boundary, not durable persistence, a multi-writer event ledger, or a replay engine.
/// </remarks>
public sealed class GeologicalTerrainRevisionStore2D
{
    private readonly object _gate = new();
    private readonly HashSet<string> _usedRevisionIds;
    private GeologicalTerrainRevision2D _current;

    /// <summary>Creates a store at the supplied initial immutable terrain revision.</summary>
    /// <param name="initialRevision">Initial authoritative revision.</param>
    public GeologicalTerrainRevisionStore2D(GeologicalTerrainRevision2D initialRevision)
    {
        _current = initialRevision ?? throw new ArgumentNullException(nameof(initialRevision));
        _usedRevisionIds = new HashSet<string>(StringComparer.Ordinal) { initialRevision.RevisionId };
    }

    /// <summary>Gets the current committed revision using a thread-safe snapshot read.</summary>
    public GeologicalTerrainRevision2D Current
    {
        get { lock (_gate) return _current; }
    }

    /// <summary>
    /// Attempts to commit a candidate if it was derived from the exact current revision and the
    /// expected source identity still matches. A rejected candidate never changes the store.
    /// </summary>
    /// <param name="candidate">Previously constructed and validated candidate.</param>
    /// <param name="expectedSourceRevisionId">Optimistic concurrency identity expected by the caller.</param>
    /// <param name="committedRevision">The new revision on success, or current revision on rejection.</param>
    /// <returns>True only when the candidate becomes the new current revision.</returns>
    public bool TryCommit(
        GeologicalTerrainCandidate2D candidate,
        string expectedSourceRevisionId,
        out GeologicalTerrainRevision2D committedRevision)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        if (string.IsNullOrWhiteSpace(expectedSourceRevisionId))
            throw new ArgumentException("A non-empty expected source revision ID is required.", nameof(expectedSourceRevisionId));

        lock (_gate)
        {
            if (!ReferenceEquals(candidate.SourceRevision, _current) ||
                !string.Equals(expectedSourceRevisionId, _current.RevisionId, StringComparison.Ordinal) ||
                !string.Equals(candidate.GeologicalEvent.SourceRevisionId, _current.RevisionId, StringComparison.Ordinal) ||
                _usedRevisionIds.Contains(candidate.ResultRevisionId))
            {
                committedRevision = _current;
                return false;
            }

            committedRevision = new GeologicalTerrainRevision2D(
                candidate.ResultRevisionId,
                _current.RevisionId,
                candidate.Terrain,
                candidate.GeologicalEvent);
            _usedRevisionIds.Add(committedRevision.RevisionId);
            _current = committedRevision;
            return true;
        }
    }
}
