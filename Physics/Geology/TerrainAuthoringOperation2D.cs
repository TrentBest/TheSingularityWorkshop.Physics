namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// A deterministic radial brush command with a stable caller-owned identity and authoring provenance.
/// </summary>
/// <param name="OperationId">Stable identity for this operation across deterministic replays.</param>
/// <param name="AlgorithmVersion">Versioned brush algorithm identifier.</param>
/// <param name="AuthoringSourceId">Stable identity of the authoring source or lesson that issued the operation.</param>
/// <param name="CenterX">Brush centre in the terrain's declared world X coordinate.</param>
/// <param name="CenterY">Brush centre in the terrain's declared world Y coordinate.</param>
/// <param name="Radius">Positive brush radius in the terrain's declared linear unit.</param>
/// <param name="ElevationDelta">Signed elevation change in the terrain's declared linear unit.</param>
public sealed record TerrainRadialBrushOperation2D(
    string OperationId,
    string AlgorithmVersion,
    string AuthoringSourceId,
    double CenterX,
    double CenterY,
    double Radius,
    double ElevationDelta);

/// <summary>The sample-aligned world-coordinate bounds of terrain samples changed by an operation.</summary>
public readonly record struct TerrainChangedRegion2D(
    double MinX,
    double MinY,
    double MaxX,
    double MaxY);

/// <summary>
/// Result of applying one identified terrain operation to an immutable source snapshot.
/// </summary>
public sealed record TerrainHeightFieldEdit2D(
    TerrainRadialBrushOperation2D Operation,
    TerrainHeightField2D Result,
    int ChangedSampleCount,
    TerrainChangedRegion2D? ChangedRegion);
