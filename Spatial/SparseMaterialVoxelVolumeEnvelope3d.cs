using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>Gets the stable schema identity for the versioned JSON interchange payload.</summary>
    public const string SchemaId = "singularity-physics.sparse-material-voxel-volume.v1";

    private static readonly JsonSerializerOptions InterchangeJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
        MaxDepth = 32
    };

    /// <summary>
    /// Serializes this envelope and its occupied voxel samples as the versioned Physics JSON contract.
    /// </summary>
    public string Serialize()
    {
        using var parameters = JsonDocument.Parse(GenerationParametersJson);
        var record = new SerializedEnvelope
        {
            Schema = SchemaId,
            RegionId = RegionId,
            DatasetId = DatasetId,
            RevisionId = RevisionId,
            CoordinateFrameId = CoordinateFrameId,
            LinearUnit = LinearUnit,
            AxisConvention = AxisConvention,
            VerticalDatum = VerticalDatum,
            VerticalPositiveDirection = VerticalPositiveDirection,
            MaterialCatalogId = MaterialCatalogId,
            SourceRepository = SourceRepository,
            SourceRevision = SourceRevision,
            ModelId = ModelId,
            ModelVersion = ModelVersion,
            AlgorithmId = AlgorithmId,
            AlgorithmVersion = AlgorithmVersion,
            GenerationParameters = parameters.RootElement.Clone(),
            SourceRunId = SourceRunId,
            Seed = Seed,
            GridConvention = GridConvention,
            Origin = new SpatialVectorRecord
            {
                X = Snapshot.Origin.X,
                Y = Snapshot.Origin.Y,
                Z = Snapshot.Origin.Z
            },
            VoxelSize = Snapshot.VoxelSize,
            ChunkSize = Snapshot.ChunkSize,
            Samples = Snapshot.Samples.Select(sample => new MaterialVoxelRecord
            {
                X = sample.Voxel.X,
                Y = sample.Voxel.Y,
                Z = sample.Voxel.Z,
                MaterialId = sample.MaterialId
            }).ToList()
        };

        return JsonSerializer.Serialize(record, InterchangeJsonOptions);
    }

    /// <summary>
    /// Deserializes and validates a versioned Physics sparse material-volume JSON payload.
    /// </summary>
    /// <param name="json">The complete JSON payload.</param>
    /// <param name="maximumSampleCount">Maximum number of occupied voxels accepted; defaults to five million.</param>
    /// <exception cref="ArgumentException">The payload is malformed or violates the schema contract.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The sample limit is invalid or exceeded.</exception>
    public static SparseMaterialVoxelVolumeEnvelope3d Deserialize(
        string json,
        int maximumSampleCount = 5_000_000)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("A JSON payload is required.", nameof(json));
        if (maximumSampleCount < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumSampleCount), "Maximum sample count cannot be negative.");

        SerializedEnvelope record;
        try
        {
            record = JsonSerializer.Deserialize<SerializedEnvelope>(json, InterchangeJsonOptions)
                ?? throw new ArgumentException("The JSON payload cannot be null.", nameof(json));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("The JSON payload is malformed or does not match the versioned schema.", nameof(json), exception);
        }

        if (!string.Equals(record.Schema, SchemaId, StringComparison.Ordinal))
            throw new ArgumentException($"Unsupported spatial-volume schema '{record.Schema}'.", nameof(json));
        if (!string.Equals(record.GridConvention, GridConvention, StringComparison.Ordinal))
            throw new ArgumentException("The voxel grid convention is not supported.", nameof(json));
        if (record.Origin is null || record.Samples is null || record.GenerationParameters.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("The payload is missing required grid, sample, or generation metadata.", nameof(json));
        if (record.Samples.Count > maximumSampleCount)
            throw new ArgumentOutOfRangeException(nameof(maximumSampleCount), "The payload exceeds the configured occupied-voxel sample limit.");
        if (!double.IsFinite(record.Origin.X) || !double.IsFinite(record.Origin.Y) || !double.IsFinite(record.Origin.Z))
            throw new ArgumentException("Voxel origin coordinates must be finite.", nameof(json));
        if (!double.IsFinite(record.VoxelSize) || record.VoxelSize <= 0d)
            throw new ArgumentException("Voxel size must be finite and positive.", nameof(json));
        if (record.ChunkSize is < 1 or > 64)
            throw new ArgumentException("Chunk size must be between 1 and 64.", nameof(json));

        var samples = new MaterialVoxelSample3d[record.Samples.Count];
        var occupied = new HashSet<GridCoordinate3i>();
        for (var index = 0; index < record.Samples.Count; index++)
        {
            var sample = record.Samples[index];
            if (sample is null || sample.MaterialId <= 0)
                throw new ArgumentException("Every occupied voxel sample must have a positive material ID.", nameof(json));

            var coordinate = new GridCoordinate3i(sample.X, sample.Y, sample.Z);
            if (!occupied.Add(coordinate))
                throw new ArgumentException("The payload contains duplicate voxel coordinates.", nameof(json));

            samples[index] = new MaterialVoxelSample3d(coordinate, sample.MaterialId);
        }

        var snapshot = new SparseMaterialVoxelVolumeSnapshot3d(
            new Vector3d(record.Origin.X, record.Origin.Y, record.Origin.Z),
            record.VoxelSize,
            record.ChunkSize,
            samples);

        return new SparseMaterialVoxelVolumeEnvelope3d(
            snapshot,
            record.RegionId,
            record.DatasetId,
            record.RevisionId,
            record.CoordinateFrameId,
            record.LinearUnit,
            record.AxisConvention,
            record.VerticalDatum,
            record.VerticalPositiveDirection,
            record.MaterialCatalogId,
            record.SourceRepository,
            record.SourceRevision,
            record.ModelId,
            record.ModelVersion,
            record.AlgorithmId,
            record.AlgorithmVersion,
            JsonSerializer.Serialize(record.GenerationParameters, InterchangeJsonOptions),
            record.SourceRunId,
            record.Seed);
    }

    private sealed class SerializedEnvelope
    {
        public SerializedEnvelope() { }
        public required string Schema { get; init; }
        public required string RegionId { get; init; }
        public required string DatasetId { get; init; }
        public required string RevisionId { get; init; }
        public required string CoordinateFrameId { get; init; }
        public required string LinearUnit { get; init; }
        public required string AxisConvention { get; init; }
        public required string VerticalDatum { get; init; }
        public required string VerticalPositiveDirection { get; init; }
        public required string MaterialCatalogId { get; init; }
        public required string SourceRepository { get; init; }
        public required string SourceRevision { get; init; }
        public required string ModelId { get; init; }
        public required string ModelVersion { get; init; }
        public required string AlgorithmId { get; init; }
        public required string AlgorithmVersion { get; init; }
        public required JsonElement GenerationParameters { get; init; }
        public required string SourceRunId { get; init; }
        public required long? Seed { get; init; }
        public required string GridConvention { get; init; }
        public required SpatialVectorRecord Origin { get; init; }
        public required double VoxelSize { get; init; }
        public required int ChunkSize { get; init; }
        public required List<MaterialVoxelRecord> Samples { get; init; }
    }

    private sealed class SpatialVectorRecord
    {
        public SpatialVectorRecord() { }
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }
    }

    private sealed class MaterialVoxelRecord
    {
        public MaterialVoxelRecord() { }
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }
        public int MaterialId { get; init; }
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

        return JsonSerializer.Serialize(document.RootElement);
    }
}
