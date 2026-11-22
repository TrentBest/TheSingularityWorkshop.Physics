using System.Text.Json;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// Caller-supplied provenance and spatial conventions required to export a terrain field.
/// </summary>
/// <remarks>
/// The terrain model does not own repository revisions, run identifiers, world-coordinate
/// origins, axis conventions, or vertical datums. Export therefore requires these values
/// explicitly instead of manufacturing them.
/// </remarks>
public sealed record VolcanicTerrain2DExportMetadata
{
    public required string DatasetId { get; init; }
    public required string Repository { get; init; }
    public required string Revision { get; init; }
    public required string ModelVersion { get; init; }
    public required string RunId { get; init; }
    public required string CoordinateFrameId { get; init; }
    public required double OriginX { get; init; }
    public required double OriginY { get; init; }
    public required string PositiveXAxis { get; init; }
    public required string PositiveYAxis { get; init; }
    public required string VerticalDatum { get; init; }
    public required string PositiveVerticalDirection { get; init; }
}

/// <summary>Creates the versioned, lossless interchange envelope for City terrain consumers.</summary>
public static class VolcanicTerrain2DExport
{
    public const string Schema = "singularity-physics.terrain-heightfield.v1";
    public const string Model = "geology.volcanic-terrain-2d";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>Builds a serializable export envelope from a defensive terrain snapshot.</summary>
    public static VolcanicTerrain2DExportEnvelope Create(
        VolcanicTerrain2D terrain,
        VolcanicTerrain2DExportMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        ArgumentNullException.ThrowIfNull(metadata);
        Require(metadata.DatasetId, nameof(metadata.DatasetId));
        Require(metadata.Repository, nameof(metadata.Repository));
        Require(metadata.Revision, nameof(metadata.Revision));
        Require(metadata.ModelVersion, nameof(metadata.ModelVersion));
        Require(metadata.RunId, nameof(metadata.RunId));
        Require(metadata.CoordinateFrameId, nameof(metadata.CoordinateFrameId));
        Require(metadata.PositiveXAxis, nameof(metadata.PositiveXAxis));
        Require(metadata.PositiveYAxis, nameof(metadata.PositiveYAxis));
        Require(metadata.VerticalDatum, nameof(metadata.VerticalDatum));
        Require(metadata.PositiveVerticalDirection, nameof(metadata.PositiveVerticalDirection));
        if (!double.IsFinite(metadata.OriginX) || !double.IsFinite(metadata.OriginY))
            throw new ArgumentException("Coordinate origin must be finite.", nameof(metadata));

        return new VolcanicTerrain2DExportEnvelope(
            Schema,
            metadata.DatasetId,
            new VolcanicTerrain2DExportSource(
                metadata.Repository, metadata.Revision, Model, metadata.ModelVersion, metadata.RunId),
            new VolcanicTerrain2DExportCoordinateFrame(
                metadata.CoordinateFrameId, "metres", [metadata.OriginX, metadata.OriginY],
                metadata.PositiveXAxis, metadata.PositiveYAxis),
            new VolcanicTerrain2DExportGrid(
                terrain.Width, terrain.Height, terrain.CellWidth, terrain.CellHeight,
                "y-ascending; x-fastest", "grid-index-origin"),
            new VolcanicTerrain2DExportVertical(
                "metres", metadata.VerticalDatum, metadata.PositiveVerticalDirection),
            terrain.SnapshotElevation());
    }

    /// <summary>Serializes an export envelope using the City interchange field names.</summary>
    public static string Serialize(VolcanicTerrain2DExportEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return JsonSerializer.Serialize(envelope, JsonOptions);
    }

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Export metadata values must be non-empty.", name);
    }
}

/// <summary>Versioned Physics-to-City terrain interchange document.</summary>
public sealed record VolcanicTerrain2DExportEnvelope(
    string Schema,
    string DatasetId,
    VolcanicTerrain2DExportSource Source,
    VolcanicTerrain2DExportCoordinateFrame CoordinateFrame,
    VolcanicTerrain2DExportGrid Grid,
    VolcanicTerrain2DExportVertical Vertical,
    double[] Elevations);

public sealed record VolcanicTerrain2DExportSource(
    string Repository, string Revision, string Model, string ModelVersion, string RunId);

public sealed record VolcanicTerrain2DExportCoordinateFrame(
    string Id, string Units, double[] Origin, string XAxis, string YAxis);

public sealed record VolcanicTerrain2DExportGrid(
    int Width, int Height, double CellWidth, double CellHeight,
    string RowOrder, string SampleConvention);

public sealed record VolcanicTerrain2DExportVertical(
    string Unit, string Datum, string PositiveDirection);
