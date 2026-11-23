namespace TheSingularityWorkshop.Physics.Geology;

public sealed class StratigraphicLayer3d
{
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

    public string LayerId { get; }
    public int MaterialId { get; }
    public double TopElevation { get; }
    public double BottomElevation { get; }
    public double FormationStartAgeYearsBeforePresent { get; }
    public double FormationEndAgeYearsBeforePresent { get; }
    public string ProvenanceEventId { get; }
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

    public string ColumnId { get; }
    public double WorldX { get; }
    public double WorldY { get; }
    public string CoordinateFrameId { get; }
    public string VerticalDatum { get; }
    public string LinearUnit { get; }
    public string PositiveVerticalDirection { get; }
    public IReadOnlyList<StratigraphicLayer3d> Layers => _layers;
    public double TopElevation => _layers[0].TopElevation;
    public double BottomElevation => _layers[_layers.Count - 1].BottomElevation;
    public double TotalLayerThickness => _layers.Sum(layer => layer.Thickness);

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }
}
