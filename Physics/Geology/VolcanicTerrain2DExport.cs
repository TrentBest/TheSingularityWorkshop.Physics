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
    /// <summary>Stable identity of the exported terrain dataset.</summary>
    /// <summary>DatasetId supplied by the export caller.</summary>
    public required string DatasetId { get; init; }
    /// <summary>Repository that owns the terrain model or experiment.</summary>
    /// <summary>Repository supplied by the export caller.</summary>
    public required string Repository { get; init; }
    /// <summary>Source revision used to produce the terrain.</summary>
    /// <summary>Revision supplied by the export caller.</summary>
    public required string Revision { get; init; }
    /// <summary>Version of the terrain model.</summary>
    /// <summary>ModelVersion supplied by the export caller.</summary>
    public required string ModelVersion { get; init; }
    /// <summary>Identifier of the experiment run that produced the terrain.</summary>
    /// <summary>RunId supplied by the export caller.</summary>
    public required string RunId { get; init; }
    /// <summary>Identifier for the coordinate reference frame.</summary>
    /// <summary>CoordinateFrameId supplied by the export caller.</summary>
    public required string CoordinateFrameId { get; init; }
    /// <summary>World-coordinate X value at the grid origin, in metres.</summary>
    /// <summary>OriginX supplied by the export caller.</summary>
    public required double OriginX { get; init; }
    /// <summary>World-coordinate Y value at the grid origin, in metres.</summary>
    /// <summary>OriginY supplied by the export caller.</summary>
    public required double OriginY { get; init; }
    /// <summary>Meaning and direction of the positive X axis.</summary>
    /// <summary>PositiveXAxis supplied by the export caller.</summary>
    public required string PositiveXAxis { get; init; }
    /// <summary>Meaning and direction of the positive Y axis.</summary>
    /// <summary>PositiveYAxis supplied by the export caller.</summary>
    public required string PositiveYAxis { get; init; }
    /// <summary>Vertical reference datum for elevation values.</summary>
    /// <summary>VerticalDatum supplied by the export caller.</summary>
    public required string VerticalDatum { get; init; }
    /// <summary>Direction represented by increasing elevation values.</summary>
    /// <summary>PositiveVerticalDirection supplied by the export caller.</summary>
    public required string PositiveVerticalDirection { get; init; }
}

/// <summary>Creates the versioned, lossless interchange envelope for City terrain consumers.</summary>
public static class VolcanicTerrain2DExport
{
    /// <summary>Schema identifier for the terrain height-field interchange format.</summary>
    /// <summary>Versioned Physics terrain interchange schema identifier.</summary>
    public const string Schema = "singularity-physics.terrain-heightfield.v1";
    /// <summary>Stable identifier of the volcanic terrain model.</summary>
    /// <summary>Stable identifier for the source terrain model.</summary>
    public const string Model = "geology.volcanic-terrain-2d";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>Builds a serializable export envelope from a defensive terrain snapshot.</summary>
    /// <param name="terrain">Terrain field to export.</param>
    /// <param name="metadata">Caller-owned provenance and coordinate conventions.</param>
    /// <returns>A versioned interchange envelope containing the terrain snapshot.</returns>
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
    /// <param name="envelope">Interchange document to serialize.</param>
    /// <returns>Indented JSON representation of the document.</returns>
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
/// <param name="Schema">Versioned interchange schema identifier.</param>
/// <param name="DatasetId">Stable identity of the exported dataset.</param>
/// <param name="Source">Source repository, revision, model, and run provenance.</param>
/// <param name="CoordinateFrame">Horizontal coordinate reference and grid origin.</param>
/// <param name="Grid">Grid dimensions, cell spacing, and sample ordering.</param>
/// <param name="Vertical">Elevation unit, datum, and positive direction.</param>
/// <param name="Elevations">Row-major elevation samples, with X varying fastest.</param>
/// <summary>Versioned Physics-to-City terrain interchange document.</summary>
/// <param name="Schema">Schema value.</param>
/// <param name="DatasetId">DatasetId value.</param>
/// <param name="Source">Source value.</param>
/// <param name="CoordinateFrame">CoordinateFrame value.</param>
/// <param name="Grid">Grid value.</param>
/// <param name="Vertical">Vertical value.</param>
/// <param name="Elevations">Elevations value.</param>
public sealed record VolcanicTerrain2DExportEnvelope(
    string Schema,
    string DatasetId,
    VolcanicTerrain2DExportSource Source,
    VolcanicTerrain2DExportCoordinateFrame CoordinateFrame,
    VolcanicTerrain2DExportGrid Grid,
    VolcanicTerrain2DExportVertical Vertical,
    double[] Elevations);

/// <summary>Source provenance for an exported terrain dataset.</summary>
/// <param name="Repository">Repository that owns the source.</param>
/// <param name="Revision">Source revision identifier.</param>
/// <param name="Model">Stable model identifier.</param>
/// <param name="ModelVersion">Version of the source model.</param>
/// <param name="RunId">Experiment run that produced the data.</param>
/// <summary>Source repository and execution provenance.</summary>
/// <param name="Repository">Repository value.</param>
/// <param name="Revision">Revision value.</param>
/// <param name="Model">Model value.</param>
/// <param name="ModelVersion">ModelVersion value.</param>
/// <param name="RunId">RunId value.</param>
public sealed record VolcanicTerrain2DExportSource(
    string Repository, string Revision, string Model, string ModelVersion, string RunId);

/// <summary>Horizontal coordinate reference and origin for a terrain grid.</summary>
/// <param name="Id">Coordinate frame identifier.</param>
/// <param name="Units">Units used by the coordinate frame.</param>
/// <param name="Origin">Two-dimensional world-coordinate origin.</param>
/// <param name="XAxis">Meaning and positive direction of the X axis.</param>
/// <param name="YAxis">Meaning and positive direction of the Y axis.</param>
/// <summary>Horizontal coordinate frame and origin.</summary>
/// <param name="Id">Id value.</param>
/// <param name="Units">Units value.</param>
/// <param name="Origin">Origin value.</param>
/// <param name="XAxis">XAxis value.</param>
/// <param name="YAxis">YAxis value.</param>
public sealed record VolcanicTerrain2DExportCoordinateFrame(
    string Id, string Units, double[] Origin, string XAxis, string YAxis);

/// <summary>Dimensions, spacing, and sample-order conventions for a terrain grid.</summary>
/// <param name="Width">Number of samples along X.</param>
/// <param name="Height">Number of samples along Y.</param>
/// <param name="CellWidth">Horizontal spacing between adjacent X samples.</param>
/// <param name="CellHeight">Horizontal spacing between adjacent Y samples.</param>
/// <param name="RowOrder">Ordering of rows in the serialized sample array.</param>
/// <param name="SampleConvention">Convention locating samples within grid cells.</param>
/// <summary>Regular heightfield dimensions and sample layout.</summary>
/// <param name="Width">Width value.</param>
/// <param name="Height">Height value.</param>
/// <param name="CellWidth">CellWidth value.</param>
/// <param name="CellHeight">CellHeight value.</param>
/// <param name="RowOrder">RowOrder value.</param>
/// <param name="SampleConvention">SampleConvention value.</param>
public sealed record VolcanicTerrain2DExportGrid(
    int Width, int Height, double CellWidth, double CellHeight,
    string RowOrder, string SampleConvention);

/// <summary>Vertical measurement conventions for terrain elevations.</summary>
/// <param name="Unit">Unit used for elevation values.</param>
/// <param name="Datum">Vertical reference datum.</param>
/// <param name="PositiveDirection">Direction represented by increasing elevation.</param>
/// <summary>Vertical unit, datum, and positive direction.</summary>
/// <param name="Unit">Unit value.</param>
/// <param name="Datum">Datum value.</param>
/// <param name="PositiveDirection">PositiveDirection value.</param>
public sealed record VolcanicTerrain2DExportVertical(
    string Unit, string Datum, string PositiveDirection);
