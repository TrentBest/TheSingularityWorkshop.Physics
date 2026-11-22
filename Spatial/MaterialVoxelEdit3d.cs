namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Describes the desired material state for one voxel; zero represents empty space.</summary>
public readonly record struct MaterialVoxelEdit3d(GridCoordinate3i Voxel, int MaterialId);

/// <summary>Summarizes the changed voxels and affected chunks from one edit batch.</summary>
public sealed class SparseMaterialVoxelEditResult3d
{
    /// <summary>Gets the number of voxels whose material state changed.</summary>
    public int ChangedVoxelCount { get; }

    /// <summary>Gets the distinct affected chunk coordinates in lexicographical order.</summary>
    public IReadOnlyList<GridCoordinate3i> DirtyChunks { get; }

    internal SparseMaterialVoxelEditResult3d(int changedVoxelCount, IReadOnlyList<GridCoordinate3i> dirtyChunks)
    {
        ChangedVoxelCount = changedVoxelCount;
        DirtyChunks = dirtyChunks;
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
