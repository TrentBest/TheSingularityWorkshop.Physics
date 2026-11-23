namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>An immutable layer with material identity, vertical extent, formation age, and event provenance.</summary>
/// <remarks>Material IDs resolve through a caller-owned catalogue. Elevation ordering follows the column's declared vertical convention.</remarks>
public sealed class StratigraphicLayer3d
{
    /// <summary>Creates a validated stratigraphic layer.</summary>
    /// <param name="layerId">Stable identity within the stratigraphic dataset.</param>
    /// <param name="materialId">Positive caller-defined material catalogue identity.</param>
    /// <param name="topElevation">Higher elevation boundary in the declared vertical convention.</param>
    /// <param name="bottomElevation">Lower elevation boundary in the declared vertical convention.</param>
    /// <param name="formationStartAgeYearsBeforePresent">Older formation-age bound.</param>
    /// <param name="formationEndAgeYearsBeforePresent">Younger formation-age bound.</param>
    /// <param name="provenanceEventId">Identity of the event that formed or deposited the layer.</param>
    public StratigraphicLayer3d(string layerId, int materialId, double topElevation, double bottomElevation,
        double formationStartAgeYearsBeforePresent, double formationEndAgeYearsBeforePresent, string provenanceEventId)
    {
        LayerId = RequireText(layerId, nameof(layerId));
        if (materialId <= 0) throw new ArgumentOutOfRangeException(nameof(materialId), "Material ID must be positive.");
        if (!double.IsFinite(topElevation)) throw new ArgumentOutOfRangeException(nameof(topElevation));
        if (!double.IsFinite(bottomElevation)) throw new ArgumentOutOfRangeException(nameof(bottomElevation));
        if (topElevation <= bottomElevation) throw new ArgumentException("Top elevation must exceed bottom elevation.", nameof(topElevation));
        if (!double.IsFinite(formationStartAgeYearsBeforePresent) || formationStartAgeYearsBeforePresent < 0d)
            throw new ArgumentOutOfRangeException(nameof(formationStartAgeYearsBeforePresent));
        if (!double.IsFinite(formationEndAgeYearsBeforePresent) || formationEndAgeYearsBeforePresent < 0d)
            throw new ArgumentOutOfRangeException(nameof(formationEndAgeYearsBeforePresent));
        if (formationStartAgeYearsBeforePresent < formationEndAgeYearsBeforePresent)
            throw new ArgumentException("Formation age must progress from older to younger.");
        ProvenanceEventId = RequireText(provenanceEventId, nameof(provenanceEventId));
        MaterialId = materialId;
        TopElevation = topElevation;
        BottomElevation = bottomElevation;
        FormationStartAgeYearsBeforePresent = formationStartAgeYearsBeforePresent;
        FormationEndAgeYearsBeforePresent = formationEndAgeYearsBeforePresent;
    }

    /// <summary>Stable layer identity.</summary>
    public string LayerId { get; }
    /// <summary>Positive caller-defined material catalogue identity.</summary>
    public int MaterialId { get; }
    /// <summary>Upper elevation boundary.</summary>
    public double TopElevation { get; }
    /// <summary>Lower elevation boundary.</summary>
    public double BottomElevation { get; }
    /// <summary>Older formation-age bound in years before present.</summary>
    public double FormationStartAgeYearsBeforePresent { get; }
    /// <summary>Younger formation-age bound in years before present.</summary>
    public double FormationEndAgeYearsBeforePresent { get; }
    /// <summary>Identity of the formation or deposition event.</summary>
    public string ProvenanceEventId { get; }
    /// <summary>Vertical thickness in the declared linear unit.</summary>
    public double Thickness => TopElevation - BottomElevation;

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }
}

/// <summary>
/// Immutable vertical stack sampled at one horizontal location. Layers are ordered from
/// highest to lowest elevation; touching is allowed, overlap is not. Upper layers cannot
/// be older than the layer beneath them. Age gaps represent unresolved hiatuses.
/// </summary>
public sealed class StratigraphicColumn3d
{
    private readonly IReadOnlyList<StratigraphicLayer3d> _layers;

    /// <summary>Creates a validated immutable vertical column.</summary>
    /// <param name="columnId">Stable identity of this sampled column.</param>
    /// <param name="worldX">Horizontal X coordinate.</param>
    /// <param name="worldY">Horizontal Y coordinate.</param>
    /// <param name="coordinateFrameId">Identity of the horizontal coordinate frame.</param>
    /// <param name="verticalDatum">Declared vertical reference datum.</param>
    /// <param name="linearUnit">Unit for horizontal coordinates and elevations.</param>
    /// <param name="positiveVerticalDirection">Meaning of increasing elevation.</param>
    /// <param name="layers">Layers ordered from highest to lowest elevation.</param>
    public StratigraphicColumn3d(string columnId, double worldX, double worldY, string coordinateFrameId,
        string verticalDatum, string linearUnit, string positiveVerticalDirection,
        IReadOnlyList<StratigraphicLayer3d> layers)
    {
        ColumnId = RequireText(columnId, nameof(columnId));
        if (!double.IsFinite(worldX)) throw new ArgumentOutOfRangeException(nameof(worldX));
        if (!double.IsFinite(worldY)) throw new ArgumentOutOfRangeException(nameof(worldY));
        WorldX = worldX;
        WorldY = worldY;
        CoordinateFrameId = RequireText(coordinateFrameId, nameof(coordinateFrameId));
        VerticalDatum = RequireText(verticalDatum, nameof(verticalDatum));
        LinearUnit = RequireText(linearUnit, nameof(linearUnit));
        PositiveVerticalDirection = RequireText(positiveVerticalDirection, nameof(positiveVerticalDirection));
        ArgumentNullException.ThrowIfNull(layers);
        if (layers.Count == 0) throw new ArgumentException("A column must contain at least one layer.", nameof(layers));

        var copy = new StratigraphicLayer3d[layers.Count];
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i] ?? throw new ArgumentException("Layers cannot contain null entries.", nameof(layers));
            if (!ids.Add(layer.LayerId)) throw new ArgumentException("Layer IDs must be unique within a column.", nameof(layers));
            if (i > 0)
            {
                var above = layers[i - 1];
                if (above.BottomElevation < layer.TopElevation)
                    throw new ArgumentException("Layers overlap or are not ordered from highest to lowest elevation.", nameof(layers));
                if (above.FormationStartAgeYearsBeforePresent > layer.FormationEndAgeYearsBeforePresent)
                    throw new ArgumentException("An upper layer cannot be older than the layer beneath it.", nameof(layers));
            }
            copy[i] = layer;
        }
        _layers = Array.AsReadOnly(copy);
    }

    /// <summary>Stable column identity.</summary>
    public string ColumnId { get; }
    /// <summary>Horizontal X coordinate of this column.</summary>
    public double WorldX { get; }
    /// <summary>Horizontal Y coordinate of this column.</summary>
    public double WorldY { get; }
    /// <summary>Horizontal coordinate-frame identity.</summary>
    public string CoordinateFrameId { get; }
    /// <summary>Vertical reference datum.</summary>
    public string VerticalDatum { get; }
    /// <summary>Linear unit for coordinates and elevations.</summary>
    public string LinearUnit { get; }
    /// <summary>Declared direction represented by increasing elevation.</summary>
    public string PositiveVerticalDirection { get; }
    /// <summary>Layers ordered from highest to lowest elevation.</summary>
    public IReadOnlyList<StratigraphicLayer3d> Layers => _layers;
    /// <summary>Elevation of the highest represented boundary.</summary>
    public double TopElevation => _layers[0].TopElevation;
    /// <summary>Elevation of the lowest represented boundary.</summary>
    public double BottomElevation => _layers[_layers.Count - 1].BottomElevation;
    /// <summary>Sum of layer thicknesses, excluding any vertical gaps.</summary>
    public double TotalLayerThickness => _layers.Sum(layer => layer.Thickness);

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }
}
