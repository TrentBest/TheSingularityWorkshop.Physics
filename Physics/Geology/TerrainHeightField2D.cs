namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// Immutable, cell-centred two-dimensional terrain elevation field with explicit spatial conventions.
/// </summary>
/// <remarks>
/// X varies fastest in row-major storage. The origin identifies the centre of sample (0,0);
/// positive X/Y follow the caller's named coordinate frame, and elevation increases in the
/// caller-declared vertical direction. Coordinates and elevations use the same caller-declared
/// linear unit. This height field cannot represent caves, overhangs, or multiple surfaces.
/// </remarks>
public sealed class TerrainHeightField2D
{
    private readonly double[] _elevations;

    /// <summary>Creates a validated terrain snapshot from row-major elevation samples.</summary>
    public TerrainHeightField2D(
        int width, int height, double cellWidth, double cellHeight,
        double originX, double originY, string coordinateFrameId, string verticalDatum,
        string linearUnit, string positiveVerticalDirection, IReadOnlyList<double> elevations)
    {
        if (width < 2) throw new ArgumentOutOfRangeException(nameof(width), "At least two samples are required on X.");
        if (height < 2) throw new ArgumentOutOfRangeException(nameof(height), "At least two samples are required on Y.");
        if (!double.IsFinite(cellWidth) || cellWidth <= 0d) throw new ArgumentOutOfRangeException(nameof(cellWidth));
        if (!double.IsFinite(cellHeight) || cellHeight <= 0d) throw new ArgumentOutOfRangeException(nameof(cellHeight));
        if (!double.IsFinite(originX)) throw new ArgumentOutOfRangeException(nameof(originX));
        if (!double.IsFinite(originY)) throw new ArgumentOutOfRangeException(nameof(originY));
        RequireText(coordinateFrameId, nameof(coordinateFrameId));
        RequireText(verticalDatum, nameof(verticalDatum));
        RequireText(linearUnit, nameof(linearUnit));
        RequireText(positiveVerticalDirection, nameof(positiveVerticalDirection));
        ArgumentNullException.ThrowIfNull(elevations);
        if (elevations.Count != checked(width * height))
            throw new ArgumentException("Elevation count must equal width × height.", nameof(elevations));

        Width = width; Height = height; CellWidth = cellWidth; CellHeight = cellHeight;
        OriginX = originX; OriginY = originY; CoordinateFrameId = coordinateFrameId;
        VerticalDatum = verticalDatum; LinearUnit = linearUnit;
        PositiveVerticalDirection = positiveVerticalDirection;
        _elevations = new double[elevations.Count];
        for (var i = 0; i < elevations.Count; i++)
        {
            if (!double.IsFinite(elevations[i]))
                throw new ArgumentException("Elevations must be finite.", nameof(elevations));
            _elevations[i] = elevations[i];
        }
        var maxX = originX + (width - 1) * cellWidth;
        var maxY = originY + (height - 1) * cellHeight;
        if (!double.IsFinite(maxX) || !double.IsFinite(maxY))
            throw new ArgumentException("Terrain bounds must be finite.");
    }

    /// <summary>Number of samples along X.</summary>
    public int Width { get; }
    /// <summary>Number of samples along Y.</summary>
    public int Height { get; }
    /// <summary>Spacing between adjacent X samples.</summary>
    public double CellWidth { get; }
    /// <summary>Spacing between adjacent Y samples.</summary>
    public double CellHeight { get; }
    /// <summary>World X coordinate of sample (0,0).</summary>
    public double OriginX { get; }
    /// <summary>World Y coordinate of sample (0,0).</summary>
    public double OriginY { get; }
    /// <summary>Caller-defined horizontal coordinate-frame identity.</summary>
    public string CoordinateFrameId { get; }
    /// <summary>Caller-defined elevation reference.</summary>
    public string VerticalDatum { get; }
    /// <summary>Caller-defined linear unit used by horizontal and vertical values.</summary>
    public string LinearUnit { get; }
    /// <summary>Direction represented by increasing elevation.</summary>
    public string PositiveVerticalDirection { get; }
    /// <summary>Returns a defensive copy of the row-major elevations.</summary>
    public double[] SnapshotElevations() => (double[])_elevations.Clone();
    /// <summary>Samples the elevation at an integer grid coordinate.</summary>
    public double SampleCell(int x, int y) => _elevations[GetIndex(x, y)];

    /// <summary>Creates a plane whose elevation is base + slopeX·(x-originX) + slopeY·(y-originY).</summary>
    public static TerrainHeightField2D CreatePlane(
        int width, int height, double cellWidth, double cellHeight, double originX, double originY,
        string coordinateFrameId, string verticalDatum, string linearUnit, string positiveVerticalDirection,
        double baseElevation, double slopeX, double slopeY)
    {
        RequireFinite(baseElevation, nameof(baseElevation));
        RequireFinite(slopeX, nameof(slopeX));
        RequireFinite(slopeY, nameof(slopeY));
        var values = new double[checked(width * height)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = baseElevation + slopeX * (x * cellWidth) + slopeY * (y * cellHeight);
            if (!double.IsFinite(value)) throw new ArithmeticException("Plane generation produced a non-finite elevation.");
            values[y * width + x] = value;
        }
        return new(width, height, cellWidth, cellHeight, originX, originY,
            coordinateFrameId, verticalDatum, linearUnit, positiveVerticalDirection, values);
    }

    /// <summary>Creates a linear X-axis ramp between two elevations across the sampled width.</summary>
    public static TerrainHeightField2D CreateRamp(
        int width, int height, double cellWidth, double cellHeight, double originX, double originY,
        string coordinateFrameId, string verticalDatum, string linearUnit, string positiveVerticalDirection,
        double startElevation, double endElevation)
    {
        RequireFinite(startElevation, nameof(startElevation));
        RequireFinite(endElevation, nameof(endElevation));
        if (width < 2) throw new ArgumentOutOfRangeException(nameof(width));
        var slope = (endElevation - startElevation) / ((width - 1) * cellWidth);
        return CreatePlane(width, height, cellWidth, cellHeight, originX, originY,
            coordinateFrameId, verticalDatum, linearUnit, positiveVerticalDirection,
            startElevation, slope, 0d);
    }

    /// <summary>Creates a parabolic bowl with zero depth at radius and maximum depth at its centre.</summary>
    public static TerrainHeightField2D CreateBowl(
        int width, int height, double cellWidth, double cellHeight, double originX, double originY,
        string coordinateFrameId, string verticalDatum, string linearUnit, string positiveVerticalDirection,
        double rimElevation, double depth, double centerX, double centerY, double radius)
    {
        RequireFinite(rimElevation, nameof(rimElevation));
        RequireFinite(depth, nameof(depth));
        RequireFinite(centerX, nameof(centerX));
        RequireFinite(centerY, nameof(centerY));
        if (depth < 0d) throw new ArgumentOutOfRangeException(nameof(depth));
        if (!double.IsFinite(radius) || radius <= 0d) throw new ArgumentOutOfRangeException(nameof(radius));
        var values = new double[checked(width * height)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var dx = (originX + x * cellWidth) - centerX;
            var dy = (originY + y * cellHeight) - centerY;
            var normalized = Math.Sqrt((dx * dx) + (dy * dy)) / radius;
            var weight = normalized < 1d ? 1d - (normalized * normalized) : 0d;
            values[y * width + x] = rimElevation - (depth * weight);
            if (!double.IsFinite(values[y * width + x]))
                throw new ArithmeticException("Bowl generation produced a non-finite elevation.");
        }
        return new(width, height, cellWidth, cellHeight, originX, originY,
            coordinateFrameId, verticalDatum, linearUnit, positiveVerticalDirection, values);
    }

    /// <summary>Attempts bilinear elevation sampling in world coordinates; returns false outside the grid.</summary>
    public bool TrySampleElevation(double worldX, double worldY, out double elevation)
    {
        elevation = 0d;
        if (!double.IsFinite(worldX) || !double.IsFinite(worldY)) return false;
        var gx = (worldX - OriginX) / CellWidth;
        var gy = (worldY - OriginY) / CellHeight;
        if (!double.IsFinite(gx) || !double.IsFinite(gy) || gx < 0d || gy < 0d ||
            gx > Width - 1d || gy > Height - 1d) return false;
        var x0 = (int)Math.Floor(gx); var y0 = (int)Math.Floor(gy);
        var x1 = Math.Min(x0 + 1, Width - 1); var y1 = Math.Min(y0 + 1, Height - 1);
        var tx = gx - x0; var ty = gy - y0;
        var a = Lerp(_elevations[y0 * Width + x0], _elevations[y0 * Width + x1], tx);
        var b = Lerp(_elevations[y1 * Width + x0], _elevations[y1 * Width + x1], tx);
        elevation = Lerp(a, b, ty);
        return double.IsFinite(elevation);
    }

    /// <summary>Returns the upward-facing unit normal using one-sided boundary and centred interior differences.</summary>
    public System.Numerics.Vector3 GetSurfaceNormal(int x, int y)
    {
        _ = GetIndex(x, y);
        var left = x == 0 ? x : x - 1; var right = x == Width - 1 ? x : x + 1;
        var down = y == 0 ? y : y - 1; var up = y == Height - 1 ? y : y + 1;
        var dx = (_elevations[y * Width + right] - _elevations[y * Width + left]) /
                 ((right - left) * CellWidth);
        var dy = (_elevations[up * Width + x] - _elevations[down * Width + x]) /
                 ((up - down) * CellHeight);
        var length = Math.Sqrt((dx * dx) + (dy * dy) + 1d);
        return new System.Numerics.Vector3((float)(-dx / length), (float)(-dy / length), (float)(1d / length));
    }

    /// <summary>
    /// Applies a deterministic smooth radial elevation edit and returns a new snapshot.
    /// Positive delta raises terrain; negative delta lowers it. No partial result is returned on failure.
    /// </summary>
    public TerrainHeightField2D ApplyRadialBrush(double centerX, double centerY, double radius, double delta)
    {
        RequireFinite(centerX, nameof(centerX)); RequireFinite(centerY, nameof(centerY));
        if (!double.IsFinite(radius) || radius <= 0d) throw new ArgumentOutOfRangeException(nameof(radius));
        RequireFinite(delta, nameof(delta));
        var next = (double[])_elevations.Clone();
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var dx = OriginX + x * CellWidth - centerX;
            var dy = OriginY + y * CellHeight - centerY;
            var distance = Math.Sqrt((dx * dx) + (dy * dy));
            if (distance >= radius) continue;
            var t = distance / radius;
            var weight = 1d - (t * t * (3d - (2d * t)));
            var candidate = next[y * Width + x] + delta * weight;
            if (!double.IsFinite(candidate))
                throw new ArithmeticException("Brush produced non-finite terrain; source snapshot remains unchanged.");
            next[y * Width + x] = candidate;
        }
        return new(Width, Height, CellWidth, CellHeight, OriginX, OriginY,
            CoordinateFrameId, VerticalDatum, LinearUnit, PositiveVerticalDirection, next);
    }

    private int GetIndex(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        return y * Width + x;
    }
    private static double Lerp(double a, double b, double t) => a + ((b - a) * t);
    private static void RequireFinite(double value, string name)
    {
        if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(name, "Value must be finite.");
    }
    private static void RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value must be non-empty.", name);
    }
}
