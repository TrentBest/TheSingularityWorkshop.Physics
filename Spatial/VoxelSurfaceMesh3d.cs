namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// An immutable triangle mesh extracted from the exposed faces of a material voxel volume.
/// </summary>
/// <remarks>
/// Triangle material IDs and normals are parallel to triangle order. Vertices are shared where
/// positions are exactly equal. This is a block-face surface representation; it does not smooth
/// voxel boundaries or claim a continuous isosurface.
/// </remarks>
public sealed class VoxelSurfaceMesh3d
{
    /// <summary>Gets the unique world-space vertex positions.</summary>
    public IReadOnlyList<Vector3d> Vertices { get; }

    /// <summary>Gets the flattened triangle vertex indices, three per triangle.</summary>
    public IReadOnlyList<int> TriangleIndices { get; }

    /// <summary>Gets the material ID for each triangle.</summary>
    public IReadOnlyList<int> TriangleMaterialIds { get; }

    /// <summary>Gets the outward unit normal for each triangle.</summary>
    public IReadOnlyList<Vector3d> TriangleNormals { get; }

    /// <summary>Gets the number of triangles in the mesh.</summary>
    public int TriangleCount => TriangleMaterialIds.Count;

    internal VoxelSurfaceMesh3d(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<int> triangleIndices,
        IReadOnlyList<int> triangleMaterialIds,
        IReadOnlyList<Vector3d> triangleNormals)
    {
        Vertices = vertices;
        TriangleIndices = triangleIndices;
        TriangleMaterialIds = triangleMaterialIds;
        TriangleNormals = triangleNormals;
    }
}
