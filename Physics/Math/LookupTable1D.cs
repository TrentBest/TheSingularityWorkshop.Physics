namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Immutable one-dimensional, uniformly sampled lookup table with linear interpolation.
/// </summary>
/// <remarks>
/// Inputs outside the declared closed interval are rejected; this class never silently extrapolates.
/// The table is a numerical approximation, not evidence that the sampled function is physically valid.
/// </remarks>
public sealed class LookupTable1D
{
    private readonly double[] _samples;

    private LookupTable1D(double minimumInput, double maximumInput, double[] samples)
    {
        MinimumInput = minimumInput;
        MaximumInput = maximumInput;
        _samples = samples;
    }

    /// <summary>Gets the inclusive lower input bound.</summary>
    public double MinimumInput { get; }

    /// <summary>Gets the inclusive upper input bound.</summary>
    public double MaximumInput { get; }

    /// <summary>Gets the number of stored samples.</summary>
    public int Count => _samples.Length;

    /// <summary>Gets the table payload size in bytes, excluding object/array headers.</summary>
    public long PayloadBytes => (long)_samples.Length * sizeof(double);

    /// <summary>Creates a table by sampling a function at uniformly spaced points, including both endpoints.</summary>
    public static LookupTable1D Sample(
        double minimumInput,
        double maximumInput,
        int sampleCount,
        Func<double, double> function)
    {
        ArgumentNullException.ThrowIfNull(function);
        ValidateBounds(minimumInput, maximumInput);

        if (sampleCount < 2)
            throw new ArgumentOutOfRangeException(nameof(sampleCount), "At least two endpoint samples are required.");

        var span = maximumInput - minimumInput;
        var samples = new double[sampleCount];

        for (var i = 0; i < sampleCount; i++)
        {
            var input = i == sampleCount - 1
                ? maximumInput
                : minimumInput + span * i / (sampleCount - 1d);
            var value = function(input);
            if (!double.IsFinite(value))
            {
                throw new ArgumentException($"The function returned a non-finite value at sample index {i}.", nameof(function));
            }

            samples[i] = value;
        }

        return new LookupTable1D(minimumInput, maximumInput, samples);
    }

    /// <summary>Evaluates the table at an input inside the inclusive declared domain.</summary>
    public double Evaluate(double input)
    {
        if (!double.IsFinite(input) || input < MinimumInput || input > MaximumInput)
            throw new ArgumentOutOfRangeException(nameof(input), "Input must be finite and inside the table's inclusive domain.");

        if (input == MinimumInput)
            return _samples[0];
        if (input == MaximumInput)
            return _samples[^1];

        var normalized = (input - MinimumInput) / (MaximumInput - MinimumInput);
        var samplePosition = normalized * (_samples.Length - 1);
        var lowerIndex = (int)samplePosition;
        var fraction = samplePosition - lowerIndex;
        var lower = _samples[lowerIndex];
        var upper = _samples[lowerIndex + 1];

        // Weighted interpolation avoids overflowing an intermediate (upper - lower)
        // when the finite endpoints have large magnitude and opposite signs.
        return lower * (1d - fraction) + upper * fraction;
    }

    /// <summary>Gets the stored value at a zero-based sample index.</summary>
    public double GetSample(int index) => _samples[index];

    private static void ValidateBounds(double minimumInput, double maximumInput)
    {
        if (!double.IsFinite(minimumInput) || !double.IsFinite(maximumInput) ||
            maximumInput <= minimumInput || !double.IsFinite(maximumInput - minimumInput))
        {
            throw new ArgumentOutOfRangeException(nameof(maximumInput),
                "Bounds must be finite, increasing, and have a finite difference.");
        }
    }
}
