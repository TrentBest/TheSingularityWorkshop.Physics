namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// An immutable, regular horizontal grid of vertical stratigraphic columns.
/// </summary>
/// <remarks>
/// Columns are stored in row-major order with X varying fastest. The origin identifies the
/// horizontal position of column (0,0); each column is located at origin + integer index ×
/// spacing. Every column must share the grid's coordinate frame, vertical datum, linear unit,
/// and positive vertical direction. This is a sampled 3D stratigraphic representation: it does
/// not interpolate between columns, infer continuity across unsampled space, or represent
/// dipping/folded layer surfaces between samples.
/// </remarks>
public sealed class StratigraphicColumnGrid3d
{
    private readonly StratigraphicColumn3d[] _columns;
    private readonly IReadOnlyList<StratigraphicColumn3d> _readOnlyColumns;

    /// <summary>Creates a validated regular grid from row-major vertical columns.</summary>
    public StratigraphicColumnGrid3d(
        int width,
        int height,
        double spacingX,
        double spacingY,
        double originX,
        double originY,
        string coordinateFrameId,
        string verticalDatum,
        string linearUnit,
        string positiveVerticalDirection,
        IReadOnlyList<StratigraphicColumn3d> columns)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1) throw new ArgumentOutOfRangeException(nameof(height));
        if (!double.IsFinite(spacingX) || spacingX <= 0d) throw new ArgumentOutOfRangeException(nameof(spacingX));
        if (!double.IsFinite(spacingY) || spacingY <= 0d) throw new ArgumentOutOfRangeException(nameof(spacingY));
        if (!double.IsFinite(originX)) throw new ArgumentOutOfRangeException(nameof(originX));
        if (!double.IsFinite(originY)) throw new ArgumentOutOfRangeException(nameof(originY));
        CoordinateFrameId = RequireText(coordinateFrameId, nameof(coordinateFrameId));
        VerticalDatum = RequireText(verticalDatum, nameof(verticalDatum));
        LinearUnit = RequireText(linearUnit, nameof(linearUnit));
        PositiveVerticalDirection = RequireText(positiveVerticalDirection, nameof(positiveVerticalDirection));
        ArgumentNullException.ThrowIfNull(columns);
        var expectedCount = checked(width * height);
        if (columns.Count != expectedCount)
            throw new ArgumentException("Column count must equal width × height.", nameof(columns));

        var maxX = originX + (width - 1) * spacingX;
        var maxY = originY + (height - 1) * spacingY;
        if (!double.IsFinite(maxX) || !double.IsFinite(maxY))
            throw new ArgumentException("Grid bounds must be finite.");

        Width = width;
        Height = height;
        SpacingX = spacingX;
        SpacingY = spacingY;
        OriginX = originX;
        OriginY = originY;

        _columns = new StratigraphicColumn3d[expectedCount];
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width) + x;
            var column = columns[index] ?? throw new ArgumentException("Columns cannot contain null entries.", nameof(columns));
            if (!ids.Add(column.ColumnId))
                throw new ArgumentException("Column IDs must be unique within a grid.", nameof(columns));
            if (!string.Equals(column.CoordinateFrameId, CoordinateFrameId, StringComparison.Ordinal) ||
                !string.Equals(column.VerticalDatum, VerticalDatum, StringComparison.Ordinal) ||
                !string.Equals(column.LinearUnit, LinearUnit, StringComparison.Ordinal) ||
                !string.Equals(column.PositiveVerticalDirection, PositiveVerticalDirection, StringComparison.Ordinal))
                throw new ArgumentException("Every column must share the grid's coordinate and vertical conventions.", nameof(columns));

            var expectedX = originX + (x * spacingX);
            var expectedY = originY + (y * spacingY);
            if (column.WorldX != expectedX || column.WorldY != expectedY)
                throw new ArgumentException("Each column must occupy its exact regular-grid sample position.", nameof(columns));
            _columns[index] = column;
        }

        _readOnlyColumns = Array.AsReadOnly(_columns);
    }

    /// <summary>Number of sampled columns along X.</summary>
    public int Width { get; }
    /// <summary>Number of sampled columns along Y.</summary>
    public int Height { get; }
    /// <summary>Horizontal spacing between adjacent columns along X.</summary>
    public double SpacingX { get; }
    /// <summary>Horizontal spacing between adjacent columns along Y.</summary>
    public double SpacingY { get; }
    /// <summary>World X coordinate of column (0,0).</summary>
    public double OriginX { get; }
    /// <summary>World Y coordinate of column (0,0).</summary>
    public double OriginY { get; }
    /// <summary>Shared horizontal coordinate-frame identity.</summary>
    public string CoordinateFrameId { get; }
    /// <summary>Shared vertical reference datum.</summary>
    public string VerticalDatum { get; }
    /// <summary>Shared linear unit for horizontal positions and elevations.</summary>
    public string LinearUnit { get; }
    /// <summary>Shared direction represented by increasing elevation.</summary>
    public string PositiveVerticalDirection { get; }
    /// <summary>All columns in row-major order, with X varying fastest.</summary>
    public IReadOnlyList<StratigraphicColumn3d> Columns => _readOnlyColumns;

    /// <summary>Gets the column at an integer grid coordinate.</summary>
    public StratigraphicColumn3d GetColumn(int x, int y) => _columns[GetIndex(x, y)];

    /// <summary>Attempts to get a column at an integer grid coordinate.</summary>
    public bool TryGetColumn(int x, int y, out StratigraphicColumn3d? column)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            column = null;
            return false;
        }

        column = _columns[(y * Width) + x];
        return true;
    }

    private int GetIndex(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        return (y * Width) + x;
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }
}
