namespace TheSingularityWorkshop.Physics.Hydrodynamics;

/// <summary>
/// Immutable, cell-centred precipitation-rate input for a two-dimensional surface grid.
/// </summary>
/// <remarks>
/// Rates are expressed in metres of water equivalent per second. This contract converts
/// precipitation forcing into per-cell depth increments; it does not model infiltration,
/// runoff routing, evaporation, snow accumulation/melt, wind drift, or hydrodynamic response.
/// The caller owns spatial alignment and coupling the increments into a compatible model.
/// </remarks>
public sealed class RainfallRateField2D
{
    private readonly double[] _rates;

    /// <summary>Creates a row-major precipitation-rate field.</summary>
    /// <param name="width">Number of cells along X.</param>
    /// <param name="height">Number of cells along Y.</param>
    /// <param name="rateMetresPerSecond">Non-negative water-equivalent precipitation rate for each cell.</param>
    public RainfallRateField2D(int width, int height, IReadOnlyList<double> rateMetresPerSecond)
    {
        if (width < 1)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1)
            throw new ArgumentOutOfRangeException(nameof(height));
        ArgumentNullException.ThrowIfNull(rateMetresPerSecond);
        var count = checked(width * height);
        if (rateMetresPerSecond.Count != count)
            throw new ArgumentException("Rate count must equal width × height.", nameof(rateMetresPerSecond));

        Width = width;
        Height = height;
        _rates = new double[count];
        for (var i = 0; i < count; i++)
        {
            var rate = rateMetresPerSecond[i];
            if (!double.IsFinite(rate) || rate < 0d)
                throw new ArgumentException("Every precipitation rate must be finite and non-negative.", nameof(rateMetresPerSecond));
            _rates[i] = rate;
        }
    }

    /// <summary>Number of cells along X.</summary>
    public int Width { get; }

    /// <summary>Number of cells along Y.</summary>
    public int Height { get; }

    /// <summary>Returns a defensive copy of the cell-centred rates, in metres per second.</summary>
    public double[] SnapshotRates() => (double[])_rates.Clone();

    /// <summary>Returns the water-equivalent depth added to each cell over an elapsed duration.</summary>
    /// <param name="durationSeconds">Finite, non-negative elapsed time in seconds.</param>
    /// <returns>A row-major array of depth increments in metres.</returns>
    public double[] CalculateDepthIncrement(double durationSeconds)
    {
        if (!double.IsFinite(durationSeconds) || durationSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));

        var increments = new double[_rates.Length];
        for (var i = 0; i < increments.Length; i++)
        {
            increments[i] = _rates[i] * durationSeconds;
            if (!double.IsFinite(increments[i]))
                throw new ArithmeticException("Precipitation depth increment overflowed; no result was returned.");
        }

        return increments;
    }
}
