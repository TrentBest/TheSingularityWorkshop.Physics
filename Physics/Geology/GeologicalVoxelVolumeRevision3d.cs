using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// An immutable geological volume revision that keeps sparse material state and voxel provenance together.
/// </summary>
/// <remarks>
/// The envelope and provenance mapping are one logical state unit. A revision cannot be constructed
/// with annotations bound to a different envelope, preventing stale provenance from being paired
/// with a newer voxel snapshot. The revision is an in-memory domain object; persistence is handled
/// by a separate storage boundary.
/// </remarks>
public sealed class GeologicalVoxelVolumeRevision3d
{
    /// <summary>Creates an initial immutable geological voxel-volume revision.</summary>
    /// <param name="envelope">Spatially explicit immutable material-volume snapshot.</param>
    /// <param name="provenance">Complete voxel provenance bound to the exact envelope.</param>
    public GeologicalVoxelVolumeRevision3d(
        SparseMaterialVoxelVolumeEnvelope3d envelope,
        GeologicalVoxelProvenance3d provenance)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(provenance);
        if (!ReferenceEquals(provenance.Envelope, envelope))
            throw new ArgumentException("Voxel provenance must be bound to the exact revision envelope.", nameof(provenance));

        Envelope = envelope;
        Provenance = provenance;
    }

    internal GeologicalVoxelVolumeRevision3d(
        string parentRevisionId,
        SparseMaterialVoxelVolumeEnvelope3d envelope,
        GeologicalVoxelProvenance3d provenance,
        GeologicalEventRecord appliedEvent)
        : this(envelope, provenance)
    {
        ParentRevisionId = RequireText(parentRevisionId, nameof(parentRevisionId));
        AppliedEvent = appliedEvent ?? throw new ArgumentNullException(nameof(appliedEvent));
    }

    /// <summary>Stable identity of this immutable voxel-volume revision.</summary>
    public string RevisionId => Envelope.RevisionId;

    /// <summary>Stable dataset identity shared by all revisions in this volume history.</summary>
    public string DatasetId => Envelope.DatasetId;

    /// <summary>Identity of the revision from which this revision was derived; null for an initial revision.</summary>
    public string? ParentRevisionId { get; }

    /// <summary>Spatially explicit immutable sparse material-volume snapshot.</summary>
    public SparseMaterialVoxelVolumeEnvelope3d Envelope { get; }

    /// <summary>Complete stratigraphic and formation-event annotations for the envelope's occupied voxels.</summary>
    public GeologicalVoxelProvenance3d Provenance { get; }

    /// <summary>Geological event that produced this revision; null for an initial revision.</summary>
    public GeologicalEventRecord? AppliedEvent { get; }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }
}

/// <summary>A validated, uncommitted geological voxel-volume revision derived from one source revision.</summary>
public sealed class GeologicalVoxelVolumeCandidate3d
{
    private GeologicalVoxelVolumeCandidate3d(
        GeologicalVoxelVolumeRevision3d sourceRevision,
        SparseMaterialVoxelVolumeEnvelope3d envelope,
        GeologicalVoxelProvenance3d provenance,
        GeologicalEventRecord geologicalEvent)
    {
        SourceRevision = sourceRevision;
        Envelope = envelope;
        Provenance = provenance;
        GeologicalEvent = geologicalEvent;
    }

    /// <summary>Exact immutable source revision used to construct this candidate.</summary>
    public GeologicalVoxelVolumeRevision3d SourceRevision { get; }

    /// <summary>Candidate spatially explicit immutable volume envelope.</summary>
    public SparseMaterialVoxelVolumeEnvelope3d Envelope { get; }

    /// <summary>Complete provenance mapping bound to the candidate envelope.</summary>
    public GeologicalVoxelProvenance3d Provenance { get; }

    /// <summary>Event provenance linking the source revision to this candidate result.</summary>
    public GeologicalEventRecord GeologicalEvent { get; }

    /// <summary>Stable identity requested for the candidate result revision.</summary>
    public string ResultRevisionId => Envelope.RevisionId;

    /// <summary>
    /// Creates a candidate without mutating the source revision or a revision store.
    /// </summary>
    /// <param name="sourceRevision">Exact immutable source revision.</param>
    /// <param name="resultEnvelope">New immutable material-volume envelope.</param>
    /// <param name="resultProvenance">Complete provenance mapping for that exact envelope.</param>
    /// <param name="geologicalEvent">Event whose source and result identities must match this transition.</param>
    /// <returns>A validated candidate that may be offered to a revision store.</returns>
    /// <remarks>
    /// This first transaction boundary preserves dataset and spatial-grid identity. Occupancy and
    /// material IDs may change, but the caller must provide a complete provenance mapping for every
    /// occupied result voxel. The process that produced the envelope is outside this transaction.
    /// </remarks>
    public static GeologicalVoxelVolumeCandidate3d Create(
        GeologicalVoxelVolumeRevision3d sourceRevision,
        SparseMaterialVoxelVolumeEnvelope3d resultEnvelope,
        GeologicalVoxelProvenance3d resultProvenance,
        GeologicalEventRecord geologicalEvent)
    {
        ArgumentNullException.ThrowIfNull(sourceRevision);
        ArgumentNullException.ThrowIfNull(resultEnvelope);
        ArgumentNullException.ThrowIfNull(resultProvenance);
        ArgumentNullException.ThrowIfNull(geologicalEvent);

        if (string.Equals(sourceRevision.RevisionId, resultEnvelope.RevisionId, StringComparison.Ordinal))
            throw new ArgumentException("A result revision must have a new identity.", nameof(resultEnvelope));
        if (!string.Equals(geologicalEvent.SourceRevisionId, sourceRevision.RevisionId, StringComparison.Ordinal))
            throw new ArgumentException("Event source revision does not match the candidate source.", nameof(geologicalEvent));
        if (!string.Equals(geologicalEvent.ResultRevisionId, resultEnvelope.RevisionId, StringComparison.Ordinal))
            throw new ArgumentException("Event result revision does not match the candidate envelope.", nameof(geologicalEvent));
        if (!ReferenceEquals(resultProvenance.Envelope, resultEnvelope))
            throw new ArgumentException("Candidate provenance must be bound to the exact result envelope.", nameof(resultProvenance));

        EnsureSameDatasetAndGrid(sourceRevision.Envelope, resultEnvelope);
        return new GeologicalVoxelVolumeCandidate3d(
            sourceRevision, resultEnvelope, resultProvenance, geologicalEvent);
    }

    private static void EnsureSameDatasetAndGrid(
        SparseMaterialVoxelVolumeEnvelope3d source,
        SparseMaterialVoxelVolumeEnvelope3d result)
    {
        var sourceGrid = source.Snapshot;
        var resultGrid = result.Snapshot;
        if (!string.Equals(source.RegionId, result.RegionId, StringComparison.Ordinal) ||
            !string.Equals(source.DatasetId, result.DatasetId, StringComparison.Ordinal) ||
            !string.Equals(source.CoordinateFrameId, result.CoordinateFrameId, StringComparison.Ordinal) ||
            !string.Equals(source.LinearUnit, result.LinearUnit, StringComparison.Ordinal) ||
            !string.Equals(source.AxisConvention, result.AxisConvention, StringComparison.Ordinal) ||
            !string.Equals(source.VerticalDatum, result.VerticalDatum, StringComparison.Ordinal) ||
            !string.Equals(source.VerticalPositiveDirection, result.VerticalPositiveDirection, StringComparison.Ordinal) ||
            !string.Equals(source.MaterialCatalogId, result.MaterialCatalogId, StringComparison.Ordinal) ||
            sourceGrid.Origin != resultGrid.Origin ||
            sourceGrid.VoxelSize != resultGrid.VoxelSize ||
            sourceGrid.ChunkSize != resultGrid.ChunkSize)
        {
            throw new InvalidOperationException(
                "A voxel-volume revision must preserve dataset, spatial reference, material catalogue, and grid geometry.");
        }
    }
}

/// <summary>
/// Thread-safe in-memory commit point for one current geological voxel-volume revision.
/// </summary>
/// <remarks>
/// The envelope and complete provenance mapping are swapped together as one immutable revision.
/// Optimistic source identity prevents competing candidates derived from the same state from both
/// committing. This is not durable persistence, a distributed transaction, or an event replay engine.
/// </remarks>
public sealed class GeologicalVoxelVolumeRevisionStore3d
{
    private readonly object _gate = new();
    private readonly HashSet<string> _usedRevisionIds;
    private GeologicalVoxelVolumeRevision3d _current;

    /// <summary>Creates a store at the supplied initial immutable voxel-volume revision.</summary>
    /// <param name="initialRevision">Initial authoritative revision.</param>
    public GeologicalVoxelVolumeRevisionStore3d(GeologicalVoxelVolumeRevision3d initialRevision)
    {
        _current = initialRevision ?? throw new ArgumentNullException(nameof(initialRevision));
        _usedRevisionIds = new HashSet<string>(StringComparer.Ordinal) { initialRevision.RevisionId };
    }

    /// <summary>Gets the current committed revision using a thread-safe snapshot read.</summary>
    public GeologicalVoxelVolumeRevision3d Current
    {
        get { lock (_gate) return _current; }
    }

    /// <summary>
    /// Atomically commits a candidate only when its exact source is still current and its result ID is unused.
    /// </summary>
    /// <param name="candidate">Previously constructed and validated candidate.</param>
    /// <param name="expectedSourceRevisionId">Optimistic source revision identity expected by the caller.</param>
    /// <param name="committedRevision">New current revision on success, or current revision on rejection.</param>
    /// <returns>True only when the complete envelope/provenance pair becomes current.</returns>
    public bool TryCommit(
        GeologicalVoxelVolumeCandidate3d candidate,
        string expectedSourceRevisionId,
        out GeologicalVoxelVolumeRevision3d committedRevision)
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

            committedRevision = new GeologicalVoxelVolumeRevision3d(
                _current.RevisionId,
                candidate.Envelope,
                candidate.Provenance,
                candidate.GeologicalEvent);
            _usedRevisionIds.Add(committedRevision.RevisionId);
            _current = committedRevision;
            return true;
        }
    }
}
