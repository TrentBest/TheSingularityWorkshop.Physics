namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Describes the desired material state for one voxel; zero represents empty space.</summary>
public readonly record struct MaterialVoxelEdit3d(GridCoordinate3i Voxel, int MaterialId);

/// <summary>Summarizes the changed voxels and affected chunks from one edit batch.</summary>
public sealed class SparseMaterialVoxelEditResult3d
{
    /// <summary>Gets the number of voxels whose material state changed.</summary>
    public int ChangedVoxelCount { get; }

    /// <summary>Gets the distinct chunks containing changed voxels, in lexicographical order.</summary>
    public IReadOnlyList<GridCoordinate3i> DirtyChunks { get; }

    /// <summary>
    /// Gets chunks whose extracted boundary surfaces may have changed, including face-adjacent
    /// chunks when an edited voxel lies on a chunk boundary.
    /// </summary>
    public IReadOnlyList<GridCoordinate3i> AffectedSurfaceChunks { get; }

    internal SparseMaterialVoxelEditResult3d(
        int changedVoxelCount,
        IReadOnlyList<GridCoordinate3i> dirtyChunks,
        IReadOnlyList<GridCoordinate3i> affectedSurfaceChunks)
    {
        ChangedVoxelCount = changedVoxelCount;
        DirtyChunks = dirtyChunks;
        AffectedSurfaceChunks = affectedSurfaceChunks;
    }
}

internal sealed class GridCoordinate3iComparer : IComparer<GridCoordinate3i>
{
    public static GridCoordinate3iComparer Instance { get; } = new();

    public int Compare(GridCoordinate3i left, GridCoordinate3i right)
    {
        var x = left.X.CompareTo(right.X);
        if (x != 0)
            return x;

        var y = left.Y.CompareTo(right.Y);
        return y != 0 ? y : left.Z.CompareTo(right.Z);
    }
}
