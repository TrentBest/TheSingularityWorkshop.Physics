namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Identifies one occupied voxel and its caller-defined material.</summary>
public readonly record struct MaterialVoxelSample3d(GridCoordinate3i Voxel, int MaterialId);

/// <summary>
/// An immutable, deterministic read snapshot of a sparse material voxel volume.
/// </summary>
/// <remarks>
/// The snapshot preserves the source volume's origin, cubic voxel size, chunk size, voxel
/// coordinates, and material IDs. Coordinates and dimensions retain the source volume's
/// caller-defined units and frame; this type does not invent a unit, datum, axis convention,
/// material catalog, or georeferencing transform. Samples are ordered lexicographically by
/// voxel X, then Y, then Z. The snapshot is independent of subsequent source-volume edits.
/// </remarks>
public sealed class SparseMaterialVoxelVolumeSnapshot3d
{
    private readonly IReadOnlyList<MaterialVoxelSample3d> _samples;

    /// <summary>Gets the source volume's world-space minimum corner of voxel (0,0,0).</summary>
    public Vector3d Origin { get; }

    /// <summary>Gets the source volume's cubic voxel edge length in caller-defined units.</summary>
    public double VoxelSize { get; }

    /// <summary>Gets the source volume's voxels-per-chunk edge length.</summary>
    public int ChunkSize { get; }

    /// <summary>Gets the number of occupied voxels represented by this snapshot.</summary>
    public long SolidVoxelCount => _samples.Count;

    /// <summary>Gets occupied voxel samples in deterministic lexicographical order.</summary>
    public IReadOnlyList<MaterialVoxelSample3d> Samples => _samples;

    internal SparseMaterialVoxelVolumeSnapshot3d(
        Vector3d origin,
        double voxelSize,
        int chunkSize,
        MaterialVoxelSample3d[] samples)
    {
        ArgumentNullException.ThrowIfNull(samples);
        Origin = origin;
        VoxelSize = voxelSize;
        ChunkSize = chunkSize;

        var copy = (MaterialVoxelSample3d[])samples.Clone();
        Array.Sort(copy, MaterialVoxelSampleComparer3d.Instance);
        _samples = Array.AsReadOnly(copy);
    }

    /// <summary>Returns the material at a voxel, or zero when it is empty.</summary>
    public int GetMaterialId(GridCoordinate3i voxel)
    {
        var low = 0;
        var high = _samples.Count - 1;
        while (low <= high)
        {
            var middle = low + ((high - low) / 2);
            var sample = _samples[middle];
            var comparison = GridCoordinate3iComparer.Instance.Compare(sample.Voxel, voxel);
            if (comparison == 0)
                return sample.MaterialId;
            if (comparison < 0)
                low = middle + 1;
            else
                high = middle - 1;
        }

        return 0;
    }

    /// <summary>Returns the world-space center of a voxel in the source grid.</summary>
    public Vector3d GetVoxelCenter(GridCoordinate3i voxel)
    {
        var half = VoxelSize * 0.5d;
        return new Vector3d(
            CoordinateToWorld(Origin.X, voxel.X, half),
            CoordinateToWorld(Origin.Y, voxel.Y, half),
            CoordinateToWorld(Origin.Z, voxel.Z, half));
    }

    private double CoordinateToWorld(double origin, int index, double offset)
    {
        var result = origin + (index * VoxelSize) + offset;
        if (!double.IsFinite(result))
            throw new ArithmeticException("Voxel coordinate is outside the finite world-coordinate range.");
        return result;
    }
}

internal sealed class MaterialVoxelSampleComparer3d : IComparer<MaterialVoxelSample3d>
{
    public static MaterialVoxelSampleComparer3d Instance { get; } = new();

    public int Compare(MaterialVoxelSample3d left, MaterialVoxelSample3d right) =>
        GridCoordinate3iComparer.Instance.Compare(left.Voxel, right.Voxel);
}
