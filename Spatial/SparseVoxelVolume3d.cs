namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>An integer coordinate used for voxel and chunk addressing.</summary>
public readonly record struct GridCoordinate3i(int X, int Y, int Z);

/// <summary>
/// A sparse, chunked, binary solid-volume representation on a uniform world-space grid.
/// Empty space is implicit; memory is allocated only for chunks containing solid voxels.
/// </summary>
/// <remarks>
/// Coordinates are integer voxel indices relative to <see cref="Origin"/>. Each voxel
/// occupies a half-open cell [i, i+1) on each axis. World-to-grid conversion uses floor,
/// including for negative coordinates. Chunk addressing uses mathematical floor division.
/// The volume stores occupancy only; it is not a surface mesh and has no per-voxel objects.
/// </remarks>
public sealed class SparseVoxelVolume3d
{
    private readonly Dictionary<GridCoordinate3i, ulong[]> _chunks = new();

    /// <summary>Gets the world-space minimum corner of voxel (0,0,0).</summary>
    public Vector3d Origin { get; }

    /// <summary>Gets the edge length of each cubic voxel in caller-defined world units.</summary>
    public double VoxelSize { get; }

    /// <summary>Gets the number of voxels along each edge of a chunk.</summary>
    public int ChunkSize { get; }

    /// <summary>Gets the number of allocated non-empty chunks.</summary>
    public int ChunkCount => _chunks.Count;

    /// <summary>Gets the number of solid voxels currently stored.</summary>
    public long SolidVoxelCount { get; private set; }

    /// <summary>Creates an empty sparse volume.</summary>
    /// <param name="origin">World-space minimum corner of voxel (0,0,0).</param>
    /// <param name="voxelSize">Finite positive edge length in the volume's world units.</param>
    /// <param name="chunkSize">Voxels per chunk edge; must be between 1 and 64.</param>
    public SparseVoxelVolume3d(Vector3d origin, double voxelSize, int chunkSize = 16)
    {
        if (!double.IsFinite(voxelSize) || voxelSize <= 0d)
            throw new ArgumentOutOfRangeException(nameof(voxelSize), "Voxel size must be finite and positive.");
        if (chunkSize is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be between 1 and 64.");

        Origin = origin;
        VoxelSize = voxelSize;
        ChunkSize = chunkSize;
    }

    /// <summary>Maps a world-space point to the voxel containing it.</summary>
    public GridCoordinate3i WorldToVoxel(Vector3d point) => new(
        ToVoxelIndex(point.X, Origin.X),
        ToVoxelIndex(point.Y, Origin.Y),
        ToVoxelIndex(point.Z, Origin.Z));

    /// <summary>Returns the world-space center of a voxel.</summary>
    public Vector3d GetVoxelCenter(GridCoordinate3i voxel)
    {
        var half = VoxelSize * 0.5d;
        return new Vector3d(
            CoordinateToWorld(Origin.X, voxel.X, half),
            CoordinateToWorld(Origin.Y, voxel.Y, half),
            CoordinateToWorld(Origin.Z, voxel.Z, half));
    }

    /// <summary>Returns the closed bounds of a voxel cell.</summary>
    public Bounds3d GetVoxelBounds(GridCoordinate3i voxel)
    {
        var min = new Vector3d(
            CoordinateToWorld(Origin.X, voxel.X, 0d),
            CoordinateToWorld(Origin.Y, voxel.Y, 0d),
            CoordinateToWorld(Origin.Z, voxel.Z, 0d));
        var max = new Vector3d(
            CoordinateToWorld(Origin.X, voxel.X, VoxelSize),
            CoordinateToWorld(Origin.Y, voxel.Y, VoxelSize),
            CoordinateToWorld(Origin.Z, voxel.Z, VoxelSize));
        return new Bounds3d(min, max);
    }

    /// <summary>Returns whether the addressed voxel is solid.</summary>
    public bool IsSolid(GridCoordinate3i voxel)
    {
        var chunkCoordinate = GetChunkCoordinate(voxel);
        if (!_chunks.TryGetValue(chunkCoordinate, out var bits))
            return false;

        var bit = GetBitIndex(voxel);
        return (bits[bit >> 6] & (1UL << (bit & 63))) != 0;
    }

    /// <summary>Sets a voxel's occupancy, allocating a chunk only when setting solid.</summary>
    /// <returns>True when occupancy changed; false when it already had the requested state.</returns>
    public bool SetSolid(GridCoordinate3i voxel, bool solid)
    {
        var chunkCoordinate = GetChunkCoordinate(voxel);
        if (!_chunks.TryGetValue(chunkCoordinate, out var bits))
        {
            if (!solid)
                return false;

            bits = new ulong[GetWordCount()];
            _chunks.Add(chunkCoordinate, bits);
        }

        var bit = GetBitIndex(voxel);
        var mask = 1UL << (bit & 63);
        var word = bit >> 6;
        var wasSolid = (bits[word] & mask) != 0;
        if (wasSolid == solid)
            return false;

        if (solid)
        {
            bits[word] |= mask;
            SolidVoxelCount++;
        }
        else
        {
            bits[word] &= ~mask;
            SolidVoxelCount--;
            if (IsChunkEmpty(bits))
                _chunks.Remove(chunkCoordinate);
        }

        return true;
    }

    /// <summary>Sets the voxel containing a world-space point to solid or empty.</summary>
    public bool SetSolidAt(Vector3d point, bool solid) => SetSolid(WorldToVoxel(point), solid);

    /// <summary>Returns whether the voxel containing a world-space point is solid.</summary>
    public bool IsSolidAt(Vector3d point) => IsSolid(WorldToVoxel(point));

    /// <summary>Removes all occupied chunks and resets the solid voxel count.</summary>
    public void Clear()
    {
        _chunks.Clear();
        SolidVoxelCount = 0;
    }

    private int ToVoxelIndex(double coordinate, double origin)
    {
        var scaled = (coordinate - origin) / VoxelSize;
        if (!double.IsFinite(scaled))
            throw new ArgumentOutOfRangeException(nameof(coordinate), "World coordinate cannot be represented on this grid.");

        var floored = Math.Floor(scaled);
        if (floored < int.MinValue || floored > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(coordinate), "World coordinate is outside the supported voxel index range.");

        return (int)floored;
    }

    private double CoordinateToWorld(double origin, int index, double offset)
    {
        var result = origin + (index * VoxelSize) + offset;
        if (!double.IsFinite(result))
            throw new ArithmeticException("Voxel coordinate is outside the finite world-coordinate range.");
        return result;
    }

    private GridCoordinate3i GetChunkCoordinate(GridCoordinate3i voxel) => new(
        FloorDivide(voxel.X, ChunkSize),
        FloorDivide(voxel.Y, ChunkSize),
        FloorDivide(voxel.Z, ChunkSize));

    private int GetBitIndex(GridCoordinate3i voxel)
    {
        var x = PositiveRemainder(voxel.X, ChunkSize);
        var y = PositiveRemainder(voxel.Y, ChunkSize);
        var z = PositiveRemainder(voxel.Z, ChunkSize);
        return (z * ChunkSize * ChunkSize) + (y * ChunkSize) + x;
    }

    private int GetWordCount()
    {
        var voxelCount = ChunkSize * ChunkSize * ChunkSize;
        return (voxelCount + 63) / 64;
    }

    private static int FloorDivide(int value, int divisor)
    {
        var quotient = value / divisor;
        var remainder = value % divisor;
        return remainder < 0 ? quotient - 1 : quotient;
    }

    private static int PositiveRemainder(int value, int divisor)
    {
        var remainder = value % divisor;
        return remainder < 0 ? remainder + divisor : remainder;
    }

    private static bool IsChunkEmpty(ulong[] bits)
    {
        foreach (var word in bits)
        {
            if (word != 0UL)
                return false;
        }

        return true;
    }
}
