using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>A bounded, renderer-independent escape-time sample of the Mandelbrot set.</summary>
/// <remarks>
/// Samples are evaluated at pixel-cell centers in a half-open complex-plane window. This is a
/// mathematical classification field, not a physical simulation. Coordinates use the conventional
/// real and imaginary axes and have no physical units.
/// </remarks>
public static class MandelbrotEscapeTime2d
{
    /// <summary>Maximum number of samples returned by one call.</summary>
    public const int MaximumSampleCount = 262_144;

    /// <summary>Maximum iteration count for each complex-plane sample.</summary>
    public const int MaximumIterations = 4_096;

    /// <summary>Maximum worst-case sample-iteration operations permitted by one call.</summary>
    public const long MaximumWork = 50_000_000;

    /// <summary>
    /// Evaluates a row-major grid at pixel-cell centers inside
    /// [minReal, maxReal) × [minImaginary, maxImaginary).
    /// </summary>
    /// <param name="width">Number of columns, from one through the sample limit.</param>
    /// <param name="height">Number of rows, from one through the sample limit.</param>
    /// <param name="minReal">Inclusive left edge of the complex-plane window.</param>
    /// <param name="minImaginary">Inclusive bottom edge of the complex-plane window.</param>
    /// <param name="maxReal">Exclusive right edge of the complex-plane window.</param>
    /// <param name="maxImaginary">Exclusive top edge of the complex-plane window.</param>
    /// <param name="maxIterations">Maximum iterations evaluated for each sample.</param>
    /// <returns>Immutable samples in stable row-major order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A dimension, bound, or iteration limit is invalid.</exception>
    public static IReadOnlyList<MandelbrotSample2d> Generate(
        int width,
        int height,
        double minReal,
        double minImaginary,
        double maxReal,
        double maxImaginary,
        int maxIterations)
    {
        if (width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must both be positive.");
        if (maxIterations < 1 || maxIterations > MaximumIterations)
            throw new ArgumentOutOfRangeException(nameof(maxIterations),
                $"Iteration count must be between 1 and {MaximumIterations}.");

        var sampleCount = (long)width * height;
        if (sampleCount > MaximumSampleCount)
            throw new ArgumentOutOfRangeException(nameof(width),
                $"Grid must contain no more than {MaximumSampleCount} samples.");
        if (sampleCount * maxIterations > MaximumWork)
            throw new ArgumentOutOfRangeException(nameof(maxIterations),
                $"Worst-case work must not exceed {MaximumWork} sample-iterations.");

        ValidateRange(minReal, maxReal, nameof(minReal), nameof(maxReal));
        ValidateRange(minImaginary, maxImaginary, nameof(minImaginary), nameof(maxImaginary));

        var samples = new MandelbrotSample2d[(int)sampleCount];
        var index = 0;
        for (var row = 0; row < height; row++)
        {
            var imaginary = MapCellCenter(minImaginary, maxImaginary, row, height);
            for (var column = 0; column < width; column++)
            {
                var real = MapCellCenter(minReal, maxReal, column, width);
                var zReal = 0d;
                var zImaginary = 0d;
                var iterations = 0;
                var escaped = false;

                while (iterations < maxIterations)
                {
                    var nextReal = (zReal * zReal) - (zImaginary * zImaginary) + real;
                    var nextImaginary = (2d * zReal * zImaginary) + imaginary;
                    zReal = nextReal;
                    zImaginary = nextImaginary;
                    iterations++;

                    var magnitudeSquared = (zReal * zReal) + (zImaginary * zImaginary);
                    if (magnitudeSquared > 4d || !double.IsFinite(magnitudeSquared))
                    {
                        escaped = true;
                        break;
                    }
                }

                samples[index++] = new MandelbrotSample2d(
                    column, row, real, imaginary, iterations, escaped);
            }
        }

        return new ReadOnlyCollection<MandelbrotSample2d>(samples);
    }

    private static void ValidateRange(double minimum, double maximum, string minimumName, string maximumName)
    {
        if (!double.IsFinite(minimum))
            throw new ArgumentOutOfRangeException(minimumName, "Bound must be finite.");
        if (!double.IsFinite(maximum) || maximum <= minimum)
            throw new ArgumentOutOfRangeException(maximumName,
                "Exclusive upper bound must be finite and greater than the lower bound.");
        if (!double.IsFinite(maximum - minimum))
            throw new ArgumentOutOfRangeException(maximumName, "Range difference must be finite.");
    }

    private static double MapCellCenter(double minimum, double maximum, int index, int count)
    {
        var fraction = (index + 0.5d) / count;
        var coordinate = minimum + ((maximum - minimum) * fraction);
        if (coordinate >= maximum)
            return Math.BitDecrement(maximum);
        return coordinate < minimum ? minimum : coordinate;
    }
}

/// <summary>One pixel-cell-center sample from a Mandelbrot escape-time evaluation.</summary>
/// <param name="Column">Zero-based horizontal grid index.</param>
/// <param name="Row">Zero-based vertical grid index.</param>
/// <param name="Real">Real coordinate evaluated.</param>
/// <param name="Imaginary">Imaginary coordinate evaluated.</param>
/// <param name="Iterations">Number of iterations performed, from one through the configured limit.</param>
/// <param name="Escaped">Whether the orbit crossed the squared-magnitude threshold of four.</param>
public readonly record struct MandelbrotSample2d(
    int Column,
    int Row,
    double Real,
    double Imaginary,
    int Iterations,
    bool Escaped);
