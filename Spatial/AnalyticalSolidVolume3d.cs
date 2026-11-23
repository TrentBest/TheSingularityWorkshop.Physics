namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// Builds bounded, analytically defined solid volumes on the sparse material-voxel grid.
/// </summary>
/// <remarks>
/// These generators create geometric reference fixtures, not geological formations or
/// material-property models. Occupancy is decided by testing each voxel center against
/// the declared analytic solid. The resulting boundary is therefore a discrete,
/// resolution-dependent approximation of the continuous shape.
/// </remarks>
public static class AnalyticalSolidVolume3d
{
    /// <summary>
    /// Creates a bounded ellipsoid using deterministic voxel-center inclusion.
    /// </summary>
    /// <param name="origin">World-space minimum corner of voxel (0,0,0).</param>
    /// <param name="voxelSize">Cubic voxel edge length in caller-defined units.</param>
    /// <param name="sizeX">Number of candidate voxels along X.</param>
    /// <param name="sizeY">Number of candidate voxels along Y.</param>
    /// <param name="sizeZ">Number of candidate voxels along Z.</param>
    /// <param name="center">World-space center of the continuous ellipsoid.</param>
    /// <param name="semiAxes">Positive X, Y, and Z semi-axis lengths.</param>
    /// <param name="materialId">Positive caller-defined material identity.</param>
    /// <param name="chunkSize">Voxels per chunk edge.</param>
    /// <param name="maximumCandidateVoxelCount">Maximum number of candidate voxel centers to examine.</param>
    /// <returns>A sparse material volume containing the voxel centers inside or on the ellipsoid.</returns>
    public static SparseMaterialVoxelVolume3d CreateEllipsoid(
        Vector3d origin,
        double voxelSize,
        int sizeX,
        int sizeY,
        int sizeZ,
        Vector3d center,
        Vector3d semiAxes,
        int materialId,
        int chunkSize = 16,
        long maximumCandidateVoxelCount = 5_000_000)
    {
        if (sizeX <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeX), "Grid dimensions must be positive.");
        if (sizeY <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeY), "Grid dimensions must be positive.");
        if (sizeZ <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeZ), "Grid dimensions must be positive.");
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(nameof(materialId), "Material ID must be positive.");
        if (maximumCandidateVoxelCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumCandidateVoxelCount), "Candidate limit must be positive.");
        if (!double.IsFinite(center.X) || !double.IsFinite(center.Y) || !double.IsFinite(center.Z))
            throw new ArgumentOutOfRangeException(nameof(center), "Ellipsoid center coordinates must be finite.");
        if (!double.IsFinite(semiAxes.X) || semiAxes.X <= 0d
            || !double.IsFinite(semiAxes.Y) || semiAxes.Y <= 0d
            || !double.IsFinite(semiAxes.Z) || semiAxes.Z <= 0d)
            throw new ArgumentOutOfRangeException(nameof(semiAxes), "Semi-axis lengths must be finite and positive.");

        long candidateCount;
        try
        {
            candidateCount = checked(checked((long)sizeX * sizeY) * sizeZ);
        }
        catch (OverflowException exception)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeX), "Grid candidate count exceeds the supported integer range.", exception);
        }

        if (candidateCount > maximumCandidateVoxelCount)
            throw new ArgumentOutOfRangeException(nameof(maximumCandidateVoxelCount), "Grid exceeds the configured candidate-voxel limit.");

        var volume = new SparseMaterialVoxelVolume3d(origin, voxelSize, chunkSize);
        for (var z = 0; z < sizeZ; z++)
        for (var y = 0; y < sizeY; y++)
        for (var x = 0; x < sizeX; x++)
        {
            var voxel = new GridCoordinate3i(x, y, z);
            var point = volume.GetVoxelCenter(voxel);
            var normalizedX = (point.X - center.X) / semiAxes.X;
            var normalizedY = (point.Y - center.Y) / semiAxes.Y;
            var normalizedZ = (point.Z - center.Z) / semiAxes.Z;
            var normalizedRadiusSquared =
                (normalizedX * normalizedX)
                + (normalizedY * normalizedY)
                + (normalizedZ * normalizedZ);

            if (normalizedRadiusSquared <= 1d)
                volume.SetMaterial(voxel, materialId);
        }

        return volume;
    }
}
