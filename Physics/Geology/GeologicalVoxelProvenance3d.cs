using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// Associates every occupied voxel in a material-volume revision with its stratigraphic layer
/// and original formation/deposition event.
/// </summary>
/// <remarks>
/// This is a separate immutable annotation layer over the existing sparse material-volume
/// snapshot. It does not duplicate voxel occupancy, material storage, chunking, or meshing.
/// The envelope's dataset/revision, coordinate-frame, unit, and material-catalogue identities
/// bind the annotations to one exact source snapshot. Every occupied voxel must have exactly
/// one annotation, and the annotation's material ID must match the snapshot. Empty voxels
/// cannot carry geological provenance. Layer identities are caller-resolved stable IDs; this
/// type does not reconstruct continuous surfaces or infer stratigraphic continuity.
/// </remarks>
public sealed class GeologicalVoxelProvenance3d
{
    private readonly IReadOnlyDictionary<GridCoordinate3i, GeologicalVoxelProvenanceSample3d> _byVoxel;
    private readonly IReadOnlyList<GeologicalVoxelProvenanceSample3d> _samples;

    /// <summary>Creates a complete, validated provenance mapping for one immutable volume envelope.</summary>
    /// <param name="envelope">The exact immutable material-volume revision being annotated.</param>
    /// <param name="samples">One layer/event annotation for every occupied voxel.</param>
    public GeologicalVoxelProvenance3d(
        SparseMaterialVoxelVolumeEnvelope3d envelope,
        IReadOnlyList<GeologicalVoxelProvenanceSample3d> samples)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(samples);
        if (samples.Count != envelope.Snapshot.SolidVoxelCount)
            throw new ArgumentException("Every occupied voxel must have exactly one provenance annotation.", nameof(samples));

        Envelope = envelope;
        var byVoxel = new Dictionary<GridCoordinate3i, GeologicalVoxelProvenanceSample3d>(samples.Count);
        var copy = new GeologicalVoxelProvenanceSample3d[samples.Count];
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            if (string.IsNullOrWhiteSpace(sample.LayerId))
                throw new ArgumentException("Layer IDs must be non-empty.", nameof(samples));
            if (string.IsNullOrWhiteSpace(sample.FormationEventId))
                throw new ArgumentException("Formation event IDs must be non-empty.", nameof(samples));
            if (sample.MaterialId <= 0)
                throw new ArgumentOutOfRangeException(nameof(samples), "Material IDs must be positive.");
            if (!byVoxel.TryAdd(sample.Voxel, sample))
                throw new ArgumentException("A voxel can have only one geological provenance annotation.", nameof(samples));

            var snapshotMaterialId = envelope.Snapshot.GetMaterialId(sample.Voxel);
            if (snapshotMaterialId == 0)
                throw new ArgumentException("Empty voxels cannot carry geological provenance.", nameof(samples));
            if (snapshotMaterialId != sample.MaterialId)
                throw new ArgumentException("Annotation material ID must match the immutable volume snapshot.", nameof(samples));
            copy[i] = sample;
        }

        var ordered = (GeologicalVoxelProvenanceSample3d[])copy.Clone();
        Array.Sort(ordered, GeologicalVoxelProvenanceSampleComparer3d.Instance);
        _byVoxel = new System.Collections.ObjectModel.ReadOnlyDictionary<GridCoordinate3i, GeologicalVoxelProvenanceSample3d>(byVoxel);
        _samples = Array.AsReadOnly(ordered);
    }

    /// <summary>The exact source volume envelope whose occupied voxels are annotated.</summary>
    public SparseMaterialVoxelVolumeEnvelope3d Envelope { get; }

    /// <summary>Complete provenance samples in deterministic lexicographical voxel order.</summary>
    public IReadOnlyList<GeologicalVoxelProvenanceSample3d> Samples => _samples;

    /// <summary>Attempts to retrieve layer, material, and formation-event provenance for an occupied voxel.</summary>
    public bool TryGet(GridCoordinate3i voxel, out GeologicalVoxelProvenanceSample3d sample) =>
        _byVoxel.TryGetValue(voxel, out sample);
}

/// <summary>Stratigraphic provenance for one occupied material voxel.</summary>
public readonly record struct GeologicalVoxelProvenanceSample3d(
    GridCoordinate3i Voxel,
    string LayerId,
    int MaterialId,
    string FormationEventId);

internal sealed class GeologicalVoxelProvenanceSampleComparer3d : IComparer<GeologicalVoxelProvenanceSample3d>
{
    public static GeologicalVoxelProvenanceSampleComparer3d Instance { get; } = new();

    public int Compare(GeologicalVoxelProvenanceSample3d left, GeologicalVoxelProvenanceSample3d right) =>
        GridCoordinate3iComparer.Instance.Compare(left.Voxel, right.Voxel);
}
