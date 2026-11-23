using System.Text.Json;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// Generates a bounded, deterministic voxel-solid mountain beneath an existing terrain height field.
/// </summary>
/// <remarks>
/// The height field defines the mountain's exterior ceiling; the declared base elevation defines
/// its lower boundary. Each horizontal voxel column is aligned to one height-field sample and each
/// occupied voxel is classified by its centre. This is a geometric solid, not a geological,
/// geomechanical, or structural-safety model. Cubic voxels require equal horizontal sample spacing.
/// </remarks>
public static class MountainSolidVolume3d
{
    /// <summary>Stable identity of the initial height-field extrusion algorithm.</summary>
    public const string AlgorithmId = "terrain.height-field-solid-extrusion";

    /// <summary>Version of the deterministic height-field extrusion algorithm.</summary>
    public const string AlgorithmVersion = "1.0.0";

    /// <summary>
    /// Creates a solid mountain volume whose sampled top boundary follows the supplied height field.
    /// </summary>
    /// <param name="terrain">Immutable exterior elevation field; samples are the horizontal voxel centres.</param>
    /// <param name="baseElevation">Minimum world-space elevation of the mountain solid.</param>
    /// <param name="materialId">Positive caller-defined material identity for every occupied voxel.</param>
    /// <param name="regionId">Stable parent-region identity.</param>
    /// <param name="datasetId">Stable dataset identity.</param>
    /// <param name="revisionId">Immutable output revision identity.</param>
    /// <param name="materialCatalogId">Identity/version of the caller's material catalogue.</param>
    /// <param name="sourceRepository">Identity of the source repository or system.</param>
    /// <param name="sourceRevision">Source code or dataset revision.</param>
    /// <param name="sourceRunId">Identity of this generation run.</param>
    /// <param name="seed">Optional seed recorded for provenance; this algorithm does not consume randomness.</param>
    /// <param name="chunkSize">Voxels per chunk edge.</param>
    /// <param name="maximumCandidateVoxelCount">Maximum X × Y × Z candidate cells evaluated.</param>
    public static SparseMaterialVoxelVolumeEnvelope3d Generate(
        TerrainHeightField2D terrain,
        double baseElevation,
        int materialId,
        string regionId,
        string datasetId,
        string revisionId,
        string materialCatalogId,
        string sourceRepository,
        string sourceRevision,
        string sourceRunId,
        long? seed = null,
        int chunkSize = 16,
        long maximumCandidateVoxelCount = 5_000_000)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        if (!double.IsFinite(baseElevation))
            throw new ArgumentOutOfRangeException(nameof(baseElevation), "Base elevation must be finite.");
        if (materialId <= 0)
            throw new ArgumentOutOfRangeException(nameof(materialId), "Material ID must be positive.");
        if (terrain.CellWidth != terrain.CellHeight)
            throw new ArgumentException("A cubic voxel grid requires equal X and Y terrain sample spacing.", nameof(terrain));
        if (chunkSize is < 1 or > 64)
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        if (maximumCandidateVoxelCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maximumCandidateVoxelCount));

        var elevations = terrain.SnapshotElevations();
        var maximumElevation = elevations.Max();
        if (maximumElevation <= baseElevation)
            throw new ArgumentException("The base elevation must be below at least one terrain sample.", nameof(baseElevation));

        var voxelSize = terrain.CellWidth;
        var verticalSpan = (maximumElevation - baseElevation) / voxelSize;
        if (!double.IsFinite(verticalSpan) || verticalSpan > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(terrain), "Mountain height exceeds the supported voxel index range.");

        var sizeZ = Math.Max(1, checked((int)Math.Ceiling(verticalSpan)));
        long candidateCount;
        try
        {
            candidateCount = checked((long)terrain.Width * terrain.Height * sizeZ);
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumCandidateVoxelCount), "Mountain candidate count exceeds the supported integer range.");
        }

        if (candidateCount > Math.Min(maximumCandidateVoxelCount, int.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(maximumCandidateVoxelCount),
                $"Mountain generation requires {candidateCount} candidate voxels; the effective limit is {Math.Min(maximumCandidateVoxelCount, int.MaxValue)}.");

        // Shift the minimum-corner origin by half a voxel so voxel centres coincide exactly
        // with the height field's cell-centred horizontal samples.
        var halfVoxel = voxelSize * 0.5d;
        var origin = new Vector3d(terrain.OriginX - halfVoxel, terrain.OriginY - halfVoxel, baseElevation);
        if (!double.IsFinite(origin.X) || !double.IsFinite(origin.Y))
            throw new ArgumentOutOfRangeException(nameof(terrain), "Mountain grid origin is outside the finite coordinate range.");

        var volume = new SparseMaterialVoxelVolume3d(origin, voxelSize, chunkSize);
        var edits = new List<MaterialVoxelEdit3d>();
        for (var y = 0; y < terrain.Height; y++)
        for (var x = 0; x < terrain.Width; x++)
        {
            var surfaceElevation = terrain.SampleCell(x, y);
            var availableHeight = surfaceElevation - baseElevation;
            if (availableHeight <= halfVoxel)
                continue;

            var highestIndex = (int)Math.Floor((availableHeight - halfVoxel) / voxelSize);
            for (var z = 0; z <= highestIndex; z++)
                edits.Add(new MaterialVoxelEdit3d(new GridCoordinate3i(x, y, z), materialId));
        }

        volume.ApplyEdits(edits);
        var snapshot = volume.CaptureSnapshot();
        var parameters = JsonSerializer.Serialize(new
        {
            width = terrain.Width,
            height = terrain.Height,
            voxelSize,
            baseElevation,
            materialId,
            horizontalSampleConvention = "height-field samples align with voxel centres",
            verticalOccupancyConvention = "solid when voxel centre elevation is <= bilinear height-field sample",
            candidateVoxelCount = candidateCount,
            occupiedVoxelCount = snapshot.SolidVoxelCount,
            topBoundaryMaximumQuantizationError = halfVoxel,
            geologicalRealism = false,
            seedConsumed = false
        });

        return new SparseMaterialVoxelVolumeEnvelope3d(
            snapshot,
            regionId,
            datasetId,
            revisionId,
            terrain.CoordinateFrameId,
            terrain.LinearUnit,
            "X=coordinate-frame-X;Y=coordinate-frame-Y;Z=declared-vertical-axis;handedness=caller-declared-not-inferred",
            terrain.VerticalDatum,
            terrain.PositiveVerticalDirection,
            materialCatalogId,
            sourceRepository,
            sourceRevision,
            "model.height-field-bounded-mountain-solid",
            "1.0.0",
            AlgorithmId,
            AlgorithmVersion,
            parameters,
            sourceRunId,
            seed);
    }
}
