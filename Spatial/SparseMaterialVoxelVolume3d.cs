namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// A sparse, chunked three-dimensional volume whose occupied voxels carry a caller-defined material ID.
/// </summary>
/// <remarks>
/// Material ID zero is reserved for empty space; positive IDs are stable identities supplied by the
/// caller. This type does not own a material catalog or infer physical properties from an ID.
/// Coordinates use the same floor-based world and chunk addressing convention as
/// <see cref="SparseVoxelVolume3d"/>. Voxel dimensions are cubic and expressed in caller-defined units.
/// Chunks store compact integer IDs rather than allocating one object per voxel.
/// </remarks>
public sealed class SparseMaterialVoxelVolume3d
{
    private readonly Dictionary<GridCoordinate3i, int[]> _chunks = new();
    private readonly Dictionary<int, long> _materialCounts = new();

    /// <summary>Gets the world-space minimum corner of voxel (0,0,0).</summary>
    public Vector3d Origin { get; }

    /// <summary>Gets the edge length of each cubic voxel in caller-defined world units.</summary>
    public double VoxelSize { get; }

    /// <summary>Gets the number of voxels along each edge of a chunk.</summary>
    public int ChunkSize { get; }

    /// <summary>Gets the number of allocated non-empty chunks.</summary>
    public int ChunkCount => _chunks.Count;

    /// <summary>Gets the total number of occupied voxels.</summary>
    public long SolidVoxelCount { get; private set; }

    /// <summary>Creates an empty sparse material volume.</summary>
    /// <param name="origin">World-space minimum corner of voxel (0,0,0).</param>
    /// <param name="voxelSize">Finite positive edge length in the volume's world units.</param>
    /// <param name="chunkSize">Voxels per chunk edge; must be between 1 and 64.</param>
    public SparseMaterialVoxelVolume3d(Vector3d origin, double voxelSize, int chunkSize = 16)
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

    /// <summary>Returns the material ID at a voxel, or zero when the voxel is empty.</summary>
    public int GetMaterialId(GridCoordinate3i voxel)
    {
        var chunkCoordinate = GetChunkCoordinate(voxel);
        return _chunks.TryGetValue(chunkCoordinate, out var materials)
            ? materials[GetLocalIndex(voxel)]
            : 0;
    }

    /// <summary>Returns whether the addressed voxel contains material.</summary>
    public bool IsSolid(GridCoordinate3i voxel) => GetMaterialId(voxel) != 0;

    /// <summary>Returns the number of occupied voxels carrying the specified material ID.</summary>
    /// <param name="materialId">Positive caller-defined material identity.</param>
    public long GetMaterialVoxelCount(int materialId)
    {
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(nameof(materialId), "Material ID must be positive.");
        return _materialCounts.TryGetValue(materialId, out var count) ? count : 0L;
    }

    /// <summary>Assigns a positive material ID to a voxel.</summary>
    /// <returns>True when the material changed; false when the voxel already had that material.</returns>
    public bool SetMaterial(GridCoordinate3i voxel, int materialId)
    {
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(nameof(materialId), "Material ID must be positive.");

        var chunkCoordinate = GetChunkCoordinate(voxel);
        if (!_chunks.TryGetValue(chunkCoordinate, out var materials))
        {
            materials = new int[GetVoxelCountPerChunk()];
            _chunks.Add(chunkCoordinate, materials);
        }

        var index = GetLocalIndex(voxel);
        var previousMaterialId = materials[index];
        if (previousMaterialId == materialId)
            return false;

        if (previousMaterialId == 0)
            SolidVoxelCount++;
        else
            DecrementMaterialCount(previousMaterialId);

        materials[index] = materialId;
        _materialCounts[materialId] = GetMaterialVoxelCount(materialId) + 1;
        return true;
    }

    /// <summary>Removes material from a voxel.</summary>
    /// <returns>True when occupied material was removed; false when the voxel was already empty.</returns>
    public bool SetEmpty(GridCoordinate3i voxel)
    {
        var chunkCoordinate = GetChunkCoordinate(voxel);
        if (!_chunks.TryGetValue(chunkCoordinate, out var materials))
            return false;

        var index = GetLocalIndex(voxel);
        var previousMaterialId = materials[index];
        if (previousMaterialId == 0)
            return false;

        materials[index] = 0;
        SolidVoxelCount--;
        DecrementMaterialCount(previousMaterialId);

        if (IsChunkEmpty(materials))
            _chunks.Remove(chunkCoordinate);

        return true;
    }

    /// <summary>Applies a validated set of non-overlapping voxel material edits as one logical operation.</summary>
    /// <remarks>
    /// Material ID zero means empty space. Duplicate voxel coordinates and negative material IDs
    /// are rejected before volume state changes. Missing destination chunks are allocated before
    /// edits are applied. The returned dirty chunk coordinates are sorted lexicographically.
    /// </remarks>
    public SparseMaterialVoxelEditResult3d ApplyEdits(IReadOnlyList<MaterialVoxelEdit3d> edits)
    {
        ArgumentNullException.ThrowIfNull(edits);

        var seenVoxels = new HashSet<GridCoordinate3i>();
        var requiredChunks = new HashSet<GridCoordinate3i>();

        for (var i = 0; i < edits.Count; i++)
        {
            var edit = edits[i];
            if (edit.MaterialId < 0)
                throw new ArgumentOutOfRangeException(nameof(edits), "Material IDs cannot be negative.");
            if (!seenVoxels.Add(edit.Voxel))
                throw new ArgumentException("An edit batch cannot address the same voxel more than once.", nameof(edits));

            if (edit.MaterialId != 0)
            {
                var chunk = GetChunkCoordinate(edit.Voxel);
                if (!_chunks.ContainsKey(chunk))
                    requiredChunks.Add(chunk);
            }
        }

        // Allocate all new chunks before changing existing voxel contents.
        var stagedChunks = new Dictionary<GridCoordinate3i, int[]>(requiredChunks.Count);
        foreach (var chunk in requiredChunks)
            stagedChunks.Add(chunk, new int[GetVoxelCountPerChunk()]);

        foreach (var pair in stagedChunks)
            _chunks.Add(pair.Key, pair.Value);

        var changedCount = 0;
        var dirtyChunks = new HashSet<GridCoordinate3i>();
        for (var i = 0; i < edits.Count; i++)
        {
            var edit = edits[i];
            var changed = edit.MaterialId == 0
                ? SetEmpty(edit.Voxel)
                : SetMaterial(edit.Voxel, edit.MaterialId);

            if (!changed)
                continue;

            changedCount++;
            dirtyChunks.Add(GetChunkCoordinate(edit.Voxel));
        }

        var orderedDirtyChunks = dirtyChunks.ToArray();
        Array.Sort(orderedDirtyChunks, GridCoordinate3iComparer.Instance);
        return new SparseMaterialVoxelEditResult3d(changedCount, Array.AsReadOnly(orderedDirtyChunks));
    }

    /// <summary>Sets the voxel containing a world-space point to the specified material.</summary>
    public bool SetMaterialAt(Vector3d point, int materialId) =>
        SetMaterial(WorldToVoxel(point), materialId);

    /// <summary>Returns the material ID at the voxel containing a world-space point.</summary>
    public int GetMaterialIdAt(Vector3d point) => GetMaterialId(WorldToVoxel(point));

    /// <summary>Removes all occupied chunks and resets all material counts.</summary>
    public void Clear()
    {
        _chunks.Clear();
        _materialCounts.Clear();
        SolidVoxelCount = 0;
    }

    private void DecrementMaterialCount(int materialId)
    {
        var nextCount = _materialCounts[materialId] - 1;
        if (nextCount == 0)
            _materialCounts.Remove(materialId);
        else
            _materialCounts[materialId] = nextCount;
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

    private int GetLocalIndex(GridCoordinate3i voxel)
    {
        var x = PositiveRemainder(voxel.X, ChunkSize);
        var y = PositiveRemainder(voxel.Y, ChunkSize);
        var z = PositiveRemainder(voxel.Z, ChunkSize);
        return (z * ChunkSize * ChunkSize) + (y * ChunkSize) + x;
    }

    private int GetVoxelCountPerChunk() => ChunkSize * ChunkSize * ChunkSize;

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

    private static bool IsChunkEmpty(int[] materials)
    {
        foreach (var materialId in materials)
        {
            if (materialId != 0)
                return false;
        }

        return true;
    }
}
