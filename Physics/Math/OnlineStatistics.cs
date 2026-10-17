namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Allocation-free, single-pass descriptive statistics using Welford's online algorithm.
/// </summary>
/// <remarks>
/// This accumulator describes the values explicitly added to it; it does not infer a probability
/// distribution or uncertainty model. Variance is calculated from the observed finite sample.
/// If an intermediate cannot be represented as a finite double, Add throws without committing
/// the new sample.
/// </remarks>
public sealed class OnlineStatistics
{
    private long _count;
    private double _mean;
    private double _sumSquaredDeviations;

    /// <summary>Number of samples successfully added.</summary>
    public long Count => _count;

    /// <summary>Gets the sample mean.</summary>
    /// <exception cref="InvalidOperationException">No samples have been added.</exception>
    public double Mean => _count > 0 ? _mean : throw new InvalidOperationException("Mean is undefined for an empty sample.");

    /// <summary>Gets the population variance, dividing the squared deviations by N.</summary>
    /// <exception cref="InvalidOperationException">No samples have been added.</exception>
    public double PopulationVariance => _count > 0
        ? _sumSquaredDeviations / _count
        : throw new InvalidOperationException("Variance is undefined for an empty sample.");

    /// <summary>Gets the unbiased sample variance, dividing the squared deviations by N - 1.</summary>
    /// <exception cref="InvalidOperationException">Fewer than two samples have been added.</exception>
    public double SampleVariance => _count > 1
        ? _sumSquaredDeviations / (_count - 1)
        : throw new InvalidOperationException("Sample variance requires at least two samples.");

    /// <summary>Gets the square root of the population variance.</summary>
    public double PopulationStandardDeviation => Math.Sqrt(PopulationVariance);

    /// <summary>Gets the square root of the unbiased sample variance.</summary>
    public double SampleStandardDeviation => Math.Sqrt(SampleVariance);

    /// <summary>Adds one finite sample using Welford's numerically stable online update.</summary>
    public void Add(double value)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Samples must be finite.");

        if (_count == long.MaxValue)
            throw new InvalidOperationException("The sample count cannot exceed Int64.MaxValue.");

        var nextCount = _count + 1;
        var delta = value - _mean;
        var nextMean = _mean + (delta / nextCount);
        var deltaAfterUpdate = value - nextMean;
        var nextSumSquaredDeviations = _sumSquaredDeviations + (delta * deltaAfterUpdate);

        if (!double.IsFinite(delta) || !double.IsFinite(nextMean) ||
            !double.IsFinite(deltaAfterUpdate) || !double.IsFinite(nextSumSquaredDeviations))
        {
            throw new ArithmeticException("The updated statistics exceed the finite range of double.");
        }

        _count = nextCount;
        _mean = nextMean;
        _sumSquaredDeviations = nextSumSquaredDeviations;
    }

    /// <summary>Clears all accumulated samples.</summary>
    public void Clear()
    {
        _count = 0;
        _mean = 0d;
        _sumSquaredDeviations = 0d;
    }
}
