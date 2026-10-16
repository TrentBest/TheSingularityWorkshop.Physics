namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// A deterministic terrain-height field with a volume-accounted volcanic deposition primitive.
/// </summary>
/// <remarks>
/// This is a geometry-changing source model, not a magma-flow, heat-transfer, or eruption-dynamics
/// solver. An eruption deposits a radially weighted cone over a cell-centered footprint; its
/// requested volume is conserved by normalizing the discrete footprint. Elevation is metres,
/// horizontal dimensions are metres, and deposited volume is cubic metres.
/// </remarks>
public sealed class VolcanicTerrain2D
{
    private double[] _elevation;

    /// <summary>Creates a terrain field from a row-major elevation array.</summary>
    public VolcanicTerrain2D(
        int width,
        int height,
        double cellWidth,
        double cellHeight,
        IReadOnlyList<double> initialElevation)
    {
        if (width < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height < 1)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        if (!double.IsFinite(cellWidth) || cellWidth <= 0d)
            throw new ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be finite and positive.");
        if (!double.IsFinite(cellHeight) || cellHeight <= 0d)
            throw new ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be finite and positive.");
        ArgumentNullException.ThrowIfNull(initialElevation);
        if (initialElevation.Count != checked(width * height))
            throw new ArgumentException("Elevation count must equal width × height.", nameof(initialElevation));

        Width = width;
        Height = height;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        _elevation = new double[initialElevation.Count];

        for (var i = 0; i < initialElevation.Count; i++)
        {
            if (!double.IsFinite(initialElevation[i]))
                throw new ArgumentException("Initial elevations must be finite.", nameof(initialElevation));
            _elevation[i] = initialElevation[i];
        }
    }

    /// <summary>Number of cells along X.</summary>
    public int Width { get; }

    /// <summary>Number of cells along Y.</summary>
    public int Height { get; }

    /// <summary>Cell width in metres.</summary>
    public double CellWidth { get; }

    /// <summary>Cell height in metres.</summary>
    public double CellHeight { get; }

    /// <summary>Total terrain volume change relative to the initial field, in cubic metres.</summary>
    public double NetDepositedVolume { get; private set; }

    /// <summary>Returns a defensive copy of the row-major elevation field, in metres.</summary>
    public double[] SnapshotElevation() => (double[])_elevation.Clone();

    /// <summary>Samples terrain elevation at a cell, in metres.</summary>
    public double SampleElevation(int x, int y) => _elevation[GetIndex(x, y)];

    /// <summary>
    /// Deposits an idealized volcanic cone centered on a grid cell while conserving the requested
    /// discrete volume. The cone tapers linearly to zero at the supplied radius.
    /// </summary>
    /// <param name="ventX">Vent cell X coordinate.</param>
    /// <param name="ventY">Vent cell Y coordinate.</param>
    /// <param name="radius">Horizontal footprint radius in metres.</param>
    /// <param name="eruptedVolume">Volume deposited in cubic metres; must be finite and positive.</param>
    /// <returns>The maximum elevation increase in this discrete deposition, in metres.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The vent or physical parameters are invalid.</exception>
    /// <exception cref="ArithmeticException">The deposition cannot be represented as finite terrain state.</exception>
    public double DepositEruption(int ventX, int ventY, double radius, double eruptedVolume)
    {
        _ = GetIndex(ventX, ventY);
        if (!double.IsFinite(radius) || radius <= 0d)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");
        if (!double.IsFinite(eruptedVolume) || eruptedVolume <= 0d)
            throw new ArgumentOutOfRangeException(nameof(eruptedVolume), "Erupted volume must be finite and positive.");

        var weights = new double[_elevation.Length];
        var totalWeight = 0d;
        var ventCellX = ventX * CellWidth;
        var ventCellY = ventY * CellHeight;

        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var dx = x * CellWidth - ventCellX;
            var dy = y * CellHeight - ventCellY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var weight = distance < radius ? 1d - distance / radius : 0d;
            var index = y * Width + x;
            weights[index] = weight;
            totalWeight += weight;
        }

        if (!double.IsFinite(totalWeight) || totalWeight <= 0d)
            throw new ArithmeticException("The eruption footprint has no representable deposition weight.");

        var cellArea = CellWidth * CellHeight;
        if (!double.IsFinite(cellArea) || cellArea <= 0d)
            throw new ArithmeticException("Cell area is not representable.");

        var nextNetVolume = NetDepositedVolume + eruptedVolume;
        if (!double.IsFinite(nextNetVolume))
            throw new ArithmeticException("Cumulative deposited volume would overflow.");

        var nextElevation = (double[])_elevation.Clone();
        var maximumRise = 0d;
        for (var i = 0; i < nextElevation.Length; i++)
        {
            var rise = eruptedVolume * (weights[i] / totalWeight) / cellArea;
            var candidate = nextElevation[i] + rise;
            if (!double.IsFinite(rise) || !double.IsFinite(candidate))
                throw new ArithmeticException("Eruption produced non-finite terrain; no state was committed.");
            nextElevation[i] = candidate;
            maximumRise = Math.Max(maximumRise, rise);
        }

        _elevation = nextElevation;
        NetDepositedVolume = nextNetVolume;
        return maximumRise;
    }

    private int GetIndex(int x, int y)
    {
        if ((uint)x >= (uint)Width)
            throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y));
        return y * Width + x;
    }
}
