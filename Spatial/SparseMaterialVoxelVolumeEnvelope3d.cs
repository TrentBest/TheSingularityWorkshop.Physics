using System.Text.Json;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// A provenance-bearing, spatially explicit envelope around an immutable sparse material-volume snapshot.
/// </summary>
/// <remarks>
/// The snapshot stores voxel occupancy/material IDs and grid geometry. This envelope adds the
/// coordinate, unit, datum, material-catalog, identity, and generation provenance required to
/// interpret and trace a spatial volume. It does not perform coordinate transforms, unit conversion, georeferencing,
/// material-property lookup, resampling, meshing, or terrain interpretation. The declared axis
/// convention and vertical datum are metadata, not executable transforms.
/// </remarks>
public sealed class SparseMaterialVoxelVolumeEnvelope3d
{
    /// <summary>Gets the immutable source snapshot.</summary>
    public SparseMaterialVoxelVolumeSnapshot3d Snapshot { get; }

    /// <summary>Gets the stable world or parent-region identity.</summary>
    public string RegionId { get; }

    /// <summary>Gets the stable dataset identity across revisions.</summary>
    public string DatasetId { get; }

    /// <summary>Gets the immutable source revision identity.</summary>
    public string RevisionId { get; }

    /// <summary>Gets the stable identity of the declared three-dimensional coordinate frame.</summary>
    public string CoordinateFrameId { get; }

    /// <summary>Gets the linear unit used by origin coordinates and voxel edge length.</summary>
    public string LinearUnit { get; }

    /// <summary>Gets the explicit mapping of X, Y, and Z axes to domain directions.</summary>
    public string AxisConvention { get; }

    /// <summary>Gets the vertical reference datum, such as a declared model-relative datum.</summary>
    public string VerticalDatum { get; }

    /// <summary>Gets the declared positive direction of the vertical axis.</summary>
    public string VerticalPositiveDirection { get; }

    /// <summary>Gets the stable identity/version of the material-ID catalogue.</summary>
    public string MaterialCatalogId { get; }

    /// <summary>Gets the source repository or system identity.</summary>
    public string SourceRepository { get; }

    /// <summary>Gets the source code or dataset revision that produced this snapshot.</summary>
    public string SourceRevision { get; }

    /// <summary>Gets the stable identity and version of the source physical model.</summary>
    public string ModelId { get; }

    /// <summary>Gets the model version.</summary>
    public string ModelVersion { get; }

    /// <summary>Gets the stable identity and version of the generation algorithm.</summary>
    public string AlgorithmId { get; }

    /// <summary>Gets the generation algorithm version.</summary>
    public string AlgorithmVersion { get; }

    /// <summary>Gets the exact caller-supplied generation parameters as a JSON object.</summary>
    public string GenerationParametersJson { get; }

    /// <summary>Gets the source experiment or workflow run identity.</summary>
    public string SourceRunId { get; }

    /// <summary>Gets the optional deterministic seed used by the source generator.</summary>
    public long? Seed { get; }

    /// <summary>Gets the fixed grid convention used by the wrapped snapshot.</summary>
    public const string GridConvention = "voxel-min-corner-origin; cubic-voxels; integer-x-y-z-index";

    /// <summary>
    /// Creates a spatially explicit envelope without altering or reinterpreting the source snapshot.
    /// </summary>
    public SparseMaterialVoxelVolumeEnvelope3d(
        SparseMaterialVoxelVolumeSnapshot3d snapshot,
        string regionId,
        string datasetId,
        string revisionId,
        string coordinateFrameId,
        string linearUnit,
        string axisConvention,
        string verticalDatum,
        string verticalPositiveDirection,
        string materialCatalogId,
        string sourceRepository,
        string sourceRevision,
        string modelId,
        string modelVersion,
        string algorithmId,
        string algorithmVersion,
        string generationParametersJson,
        string sourceRunId,
        long? seed = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Snapshot = snapshot;
        RegionId = RequireText(regionId, nameof(regionId));
        DatasetId = RequireText(datasetId, nameof(datasetId));
        RevisionId = RequireText(revisionId, nameof(revisionId));
        CoordinateFrameId = RequireText(coordinateFrameId, nameof(coordinateFrameId));
        LinearUnit = RequireText(linearUnit, nameof(linearUnit));
        AxisConvention = RequireText(axisConvention, nameof(axisConvention));
        VerticalDatum = RequireText(verticalDatum, nameof(verticalDatum));
        VerticalPositiveDirection = RequireText(verticalPositiveDirection, nameof(verticalPositiveDirection));
        MaterialCatalogId = RequireText(materialCatalogId, nameof(materialCatalogId));
        SourceRepository = RequireText(sourceRepository, nameof(sourceRepository));
        SourceRevision = RequireText(sourceRevision, nameof(sourceRevision));
        ModelId = RequireText(modelId, nameof(modelId));
        ModelVersion = RequireText(modelVersion, nameof(modelVersion));
        AlgorithmId = RequireText(algorithmId, nameof(algorithmId));
        AlgorithmVersion = RequireText(algorithmVersion, nameof(algorithmVersion));
        GenerationParametersJson = RequireJsonObject(generationParametersJson);
        SourceRunId = RequireText(sourceRunId, nameof(sourceRunId));
        Seed = seed;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Spatial envelope metadata must be non-empty.", parameterName);
        return value.Trim();
    }

    private static string RequireJsonObject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Generation parameters must be a JSON object.", nameof(value));

        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Generation parameters must be a JSON object.", nameof(value));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Generation parameters must be valid JSON.", nameof(value), exception);
        }

        return value;
    }
}
