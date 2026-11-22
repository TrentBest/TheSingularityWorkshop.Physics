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
    private VoxelSurfaceMesh3d? _cachedBoundarySurface;
    private readonly Dictionary<GridCoordinate3i, VoxelSurfaceMesh3d> _cachedChunkSurfaces = new();

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
    /// <param name="origin">Finite world-space minimum corner of voxel (0,0,0).</param>
    /// <param name="voxelSize">Finite positive edge length in the volume's world units.</param>
    /// <param name="chunkSize">Voxels per chunk edge; must be between 1 and 64.</param>
    public SparseMaterialVoxelVolume3d(Vector3d origin, double voxelSize, int chunkSize = 16)
    {
        if (!double.IsFinite(origin.X) || !double.IsFinite(origin.Y) || !double.IsFinite(origin.Z))
            throw new ArgumentOutOfRangeException(nameof(origin), "Origin coordinates must be finite.");
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
        InvalidateSurfaceChunks(voxel);
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

        InvalidateSurfaceChunks(voxel);
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
        var affectedSurfaceChunks = new HashSet<GridCoordinate3i>();
        for (var i = 0; i < edits.Count; i++)
        {
            var edit = edits[i];
            var changed = edit.MaterialId == 0
                ? SetEmpty(edit.Voxel)
                : SetMaterial(edit.Voxel, edit.MaterialId);

            if (!changed)
                continue;

            changedCount++;
            var chunk = GetChunkCoordinate(edit.Voxel);
            dirtyChunks.Add(chunk);
            affectedSurfaceChunks.Add(chunk);

            // A changed voxel on a chunk boundary can expose or hide faces owned by
            // the voxel in the adjacent chunk. Only those face-neighbor chunks need invalidation.
            var localX = PositiveRemainder(edit.Voxel.X, ChunkSize);
            var localY = PositiveRemainder(edit.Voxel.Y, ChunkSize);
            var localZ = PositiveRemainder(edit.Voxel.Z, ChunkSize);
            if (localX == 0)
                AddSurfaceChunk((long)chunk.X - 1, chunk.Y, chunk.Z);
            if (localX == ChunkSize - 1)
                AddSurfaceChunk((long)chunk.X + 1, chunk.Y, chunk.Z);
            if (localY == 0)
                AddSurfaceChunk(chunk.X, (long)chunk.Y - 1, chunk.Z);
            if (localY == ChunkSize - 1)
                AddSurfaceChunk(chunk.X, (long)chunk.Y + 1, chunk.Z);
            if (localZ == 0)
                AddSurfaceChunk(chunk.X, chunk.Y, (long)chunk.Z - 1);
            if (localZ == ChunkSize - 1)
                AddSurfaceChunk(chunk.X, chunk.Y, (long)chunk.Z + 1);

            void AddSurfaceChunk(long x, long y, long z)
            {
                if (x is < int.MinValue or > int.MaxValue ||
                    y is < int.MinValue or > int.MaxValue ||
                    z is < int.MinValue or > int.MaxValue)
                    return;

                affectedSurfaceChunks.Add(new GridCoordinate3i((int)x, (int)y, (int)z));
            }
        }

        var orderedDirtyChunks = dirtyChunks.ToArray();
        Array.Sort(orderedDirtyChunks, GridCoordinate3iComparer.Instance);
        var orderedSurfaceChunks = affectedSurfaceChunks.ToArray();
        Array.Sort(orderedSurfaceChunks, GridCoordinate3iComparer.Instance);
        return new SparseMaterialVoxelEditResult3d(
            changedCount,
            Array.AsReadOnly(orderedDirtyChunks),
            Array.AsReadOnly(orderedSurfaceChunks));
    }

    /// <summary>Removes occupied voxels whose centers lie inside or on a spherical brush.</summary>
    /// <param name="center">World-space center of the spherical brush.</param>
    /// <param name="radius">Finite, non-negative brush radius in the volume's world units.</param>
    /// <returns>The changed voxel count and sorted dirty chunks.</returns>
    /// <remarks>
    /// The brush is evaluated against voxel centers. It scans allocated chunks only and does not
    /// allocate empty space. This is a deterministic voxel-center approximation of a sphere, not
    /// an exact continuous sphere/voxel intersection.
    /// </remarks>
    public SparseMaterialVoxelEditResult3d RemoveSphere(Vector3d center, double radius)
    {
        if (!double.IsFinite(center.X) || !double.IsFinite(center.Y) || !double.IsFinite(center.Z))
            throw new ArgumentOutOfRangeException(nameof(center), "Brush center must be finite.");
        if (!double.IsFinite(radius) || radius < 0d)
            throw new ArgumentOutOfRangeException(nameof(radius), "Brush radius must be finite and non-negative.");

        var radiusSquared = radius * radius;
        var edits = new List<MaterialVoxelEdit3d>();
        foreach (var chunk in _chunks)
        {
            var chunkOriginX = (long)chunk.Key.X * ChunkSize;
            var chunkOriginY = (long)chunk.Key.Y * ChunkSize;
            var chunkOriginZ = (long)chunk.Key.Z * ChunkSize;
            var materials = chunk.Value;

            for (var z = 0; z < ChunkSize; z++)
            for (var y = 0; y < ChunkSize; y++)
            for (var x = 0; x < ChunkSize; x++)
            {
                var index = (z * ChunkSize * ChunkSize) + (y * ChunkSize) + x;
                if (materials[index] == 0)
                    continue;

                var voxelX = chunkOriginX + x;
                var voxelY = chunkOriginY + y;
                var voxelZ = chunkOriginZ + z;
                if (voxelX is < int.MinValue or > int.MaxValue ||
                    voxelY is < int.MinValue or > int.MaxValue ||
                    voxelZ is < int.MinValue or > int.MaxValue)
                    continue;

                var voxel = new GridCoordinate3i((int)voxelX, (int)voxelY, (int)voxelZ);
                var voxelCenter = GetVoxelCenter(voxel);
                var dx = voxelCenter.X - center.X;
                var dy = voxelCenter.Y - center.Y;
                var dz = voxelCenter.Z - center.Z;
                var distanceSquared = (dx * dx) + (dy * dy) + (dz * dz);

                if (distanceSquared <= radiusSquared)
                    edits.Add(new MaterialVoxelEdit3d(voxel, 0));
            }
        }

        return ApplyEdits(edits);
    }

    /// <summary>Returns a cached boundary mesh, rebuilding it after the next volume mutation.</summary>
    /// <remarks>
    /// This cache avoids repeating a full extraction when the volume has not changed. It is not
    /// an incremental chunk mesher: any changed voxel invalidates the complete cached mesh.
    /// Use <see cref="ExtractBoundarySurface"/> as the uncached reference extraction.
    /// </remarks>
    public VoxelSurfaceMesh3d ExtractBoundarySurfaceCached() =>
        _cachedBoundarySurface ??= ExtractBoundarySurface();

    /// <summary>Extracts the deterministic, outward-facing boundary triangles of the occupied voxels.</summary>
    /// <remarks>
    /// Shared faces between occupied voxels are omitted, including across chunk boundaries.
    /// Each exposed voxel face becomes two triangles and retains the owning voxel's material ID.
    /// The output is a discrete block-face mesh, not a smoothed continuous isosurface.
    /// </remarks>
    public VoxelSurfaceMesh3d ExtractBoundarySurface() => ExtractBoundarySurfaceCore(null);

    /// <summary>Extracts the complete boundary mesh while reusing cached meshes for unchanged chunks.</summary>
    /// <remarks>
    /// Each cached chunk mesh evaluates its six face neighbors against the complete volume, so
    /// shared faces and chunk seams remain correct. Mutations invalidate only the edited chunk and
    /// face-adjacent chunk meshes. The returned aggregate mesh is recomposed on each call; use
    /// <see cref="ExtractBoundarySurface"/> as the uncached correctness reference.
    /// </remarks>
    public VoxelSurfaceMesh3d ExtractBoundarySurfaceIncremental()
    {
        var chunkCoordinates = _chunks.Keys.ToArray();
        Array.Sort(chunkCoordinates, GridCoordinate3iComparer.Instance);
        var vertices = new List<Vector3d>();
        var vertexIndices = new Dictionary<Vector3d, int>();
        var triangleIndices = new List<int>();
        var triangleMaterials = new List<int>();
        var triangleNormals = new List<Vector3d>();

        foreach (var chunkCoordinate in chunkCoordinates)
        {
            if (!_cachedChunkSurfaces.TryGetValue(chunkCoordinate, out var chunkMesh))
            {
                chunkMesh = ExtractBoundarySurfaceCore(chunkCoordinate);
                _cachedChunkSurfaces.Add(chunkCoordinate, chunkMesh);
            }

            var remap = new int[chunkMesh.Vertices.Count];
            for (var i = 0; i < chunkMesh.Vertices.Count; i++)
            {
                var position = chunkMesh.Vertices[i];
                if (!vertexIndices.TryGetValue(position, out var index))
                {
                    index = vertices.Count;
                    vertices.Add(position);
                    vertexIndices.Add(position, index);
                }
                remap[i] = index;
            }
            foreach (var index in chunkMesh.TriangleIndices)
                triangleIndices.Add(remap[index]);
            foreach (var materialId in chunkMesh.TriangleMaterialIds)
                triangleMaterials.Add(materialId);
            foreach (var normal in chunkMesh.TriangleNormals)
                triangleNormals.Add(normal);
        }

        return new VoxelSurfaceMesh3d(
            Array.AsReadOnly(vertices.ToArray()),
            Array.AsReadOnly(triangleIndices.ToArray()),
            Array.AsReadOnly(triangleMaterials.ToArray()),
            Array.AsReadOnly(triangleNormals.ToArray()));
    }

    private VoxelSurfaceMesh3d ExtractBoundarySurfaceCore(GridCoordinate3i? onlyChunk)
    {
        var occupied = new List<GridCoordinate3i>();
        foreach (var chunk in _chunks)
        {
            if (onlyChunk.HasValue && chunk.Key != onlyChunk.Value)
                continue;

            var chunkX = (long)chunk.Key.X * ChunkSize;
            var chunkY = (long)chunk.Key.Y * ChunkSize;
            var chunkZ = (long)chunk.Key.Z * ChunkSize;
            var materials = chunk.Value;

            for (var z = 0; z < ChunkSize; z++)
            for (var y = 0; y < ChunkSize; y++)
            for (var x = 0; x < ChunkSize; x++)
            {
                var index = (z * ChunkSize * ChunkSize) + (y * ChunkSize) + x;
                if (materials[index] == 0)
                    continue;

                var voxelX = chunkX + x;
                var voxelY = chunkY + y;
                var voxelZ = chunkZ + z;
                if (voxelX is < int.MinValue or > int.MaxValue ||
                    voxelY is < int.MinValue or > int.MaxValue ||
                    voxelZ is < int.MinValue or > int.MaxValue)
                    continue;

                occupied.Add(new GridCoordinate3i((int)voxelX, (int)voxelY, (int)voxelZ));
            }
        }

        occupied.Sort(GridCoordinate3iComparer.Instance);

        var vertices = new List<Vector3d>();
        var vertexIndices = new Dictionary<Vector3d, int>();
        var triangleIndices = new List<int>();
        var triangleMaterials = new List<int>();
        var triangleNormals = new List<Vector3d>();

        foreach (var voxel in occupied)
        {
            var materialId = GetMaterialId(voxel);
            var x = (long)voxel.X;
            var y = (long)voxel.Y;
            var z = (long)voxel.Z;

            if (!IsOccupied(x - 1, y, z))
                AddFace(
                    (x, y, z), (x, y, z + 1), (x, y + 1, z + 1), (x, y + 1, z),
                    new Vector3d(-1d, 0d, 0d), materialId);
            if (!IsOccupied(x + 1, y, z))
                AddFace(
                    (x + 1, y, z), (x + 1, y + 1, z), (x + 1, y + 1, z + 1), (x + 1, y, z + 1),
                    new Vector3d(1d, 0d, 0d), materialId);
            if (!IsOccupied(x, y - 1, z))
                AddFace(
                    (x, y, z), (x + 1, y, z), (x + 1, y, z + 1), (x, y, z + 1),
                    new Vector3d(0d, -1d, 0d), materialId);
            if (!IsOccupied(x, y + 1, z))
                AddFace(
                    (x, y + 1, z), (x, y + 1, z + 1), (x + 1, y + 1, z + 1), (x + 1, y + 1, z),
                    new Vector3d(0d, 1d, 0d), materialId);
            if (!IsOccupied(x, y, z - 1))
                AddFace(
                    (x, y, z), (x, y + 1, z), (x + 1, y + 1, z), (x + 1, y, z),
                    new Vector3d(0d, 0d, -1d), materialId);
            if (!IsOccupied(x, y, z + 1))
                AddFace(
                    (x, y, z + 1), (x + 1, y, z + 1), (x + 1, y + 1, z + 1), (x, y + 1, z + 1),
                    new Vector3d(0d, 0d, 1d), materialId);
        }

        return new VoxelSurfaceMesh3d(
            Array.AsReadOnly(vertices.ToArray()),
            Array.AsReadOnly(triangleIndices.ToArray()),
            Array.AsReadOnly(triangleMaterials.ToArray()),
            Array.AsReadOnly(triangleNormals.ToArray()));

        bool IsOccupied(long voxelX, long voxelY, long voxelZ)
        {
            if (voxelX is < int.MinValue or > int.MaxValue ||
                voxelY is < int.MinValue or > int.MaxValue ||
                voxelZ is < int.MinValue or > int.MaxValue)
                return false;

            return IsSolid(new GridCoordinate3i((int)voxelX, (int)voxelY, (int)voxelZ));
        }

        void AddFace(
            (long X, long Y, long Z) a,
            (long X, long Y, long Z) b,
            (long X, long Y, long Z) c,
            (long X, long Y, long Z) d,
            Vector3d normal,
            int faceMaterialId)
        {
            var ia = AddVertex(a);
            var ib = AddVertex(b);
            var ic = AddVertex(c);
            var id = AddVertex(d);

            triangleIndices.Add(ia);
            triangleIndices.Add(ib);
            triangleIndices.Add(ic);
            triangleIndices.Add(ia);
            triangleIndices.Add(ic);
            triangleIndices.Add(id);
            triangleMaterials.Add(faceMaterialId);
            triangleMaterials.Add(faceMaterialId);
            triangleNormals.Add(normal);
            triangleNormals.Add(normal);
        }

        int AddVertex((long X, long Y, long Z) corner)
        {
            var position = new Vector3d(
                SurfaceCoordinate(Origin.X, corner.X),
                SurfaceCoordinate(Origin.Y, corner.Y),
                SurfaceCoordinate(Origin.Z, corner.Z));

            if (vertexIndices.TryGetValue(position, out var existing))
                return existing;

            var index = vertices.Count;
            vertices.Add(position);
            vertexIndices.Add(position, index);
            return index;
        }
    }

    private void InvalidateSurfaceChunks(GridCoordinate3i voxel)
    {
        _cachedBoundarySurface = null;
        var chunk = GetChunkCoordinate(voxel);
        _cachedChunkSurfaces.Remove(chunk);
        var localX = PositiveRemainder(voxel.X, ChunkSize);
        var localY = PositiveRemainder(voxel.Y, ChunkSize);
        var localZ = PositiveRemainder(voxel.Z, ChunkSize);
        if (localX == 0) RemoveChunkSurface((long)chunk.X - 1, chunk.Y, chunk.Z);
        if (localX == ChunkSize - 1) RemoveChunkSurface((long)chunk.X + 1, chunk.Y, chunk.Z);
        if (localY == 0) RemoveChunkSurface(chunk.X, (long)chunk.Y - 1, chunk.Z);
        if (localY == ChunkSize - 1) RemoveChunkSurface(chunk.X, (long)chunk.Y + 1, chunk.Z);
        if (localZ == 0) RemoveChunkSurface(chunk.X, chunk.Y, (long)chunk.Z - 1);
        if (localZ == ChunkSize - 1) RemoveChunkSurface(chunk.X, chunk.Y, (long)chunk.Z + 1);

        void RemoveChunkSurface(long x, long y, long z)
        {
            if (x is < int.MinValue or > int.MaxValue ||
                y is < int.MinValue or > int.MaxValue ||
                z is < int.MinValue or > int.MaxValue)
                return;
            _cachedChunkSurfaces.Remove(new GridCoordinate3i((int)x, (int)y, (int)z));
        }
    }

    private double SurfaceCoordinate(double origin, long index)
    {
        var coordinate = origin + (index * VoxelSize);
        if (!double.IsFinite(coordinate))
            throw new ArithmeticException("Voxel surface lies outside the finite world-coordinate range.");
        return coordinate;
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
        _cachedBoundarySurface = null;
        _cachedChunkSurfaces.Clear();
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
