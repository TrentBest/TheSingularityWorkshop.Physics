namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Composite Simpson quadrature for deterministic scalar functions on finite intervals.
/// </summary>
/// <remarks>
/// Simpson's rule combines parabolic interpolants over pairs of equal-width subintervals.
/// For sufficiently smooth functions its leading global truncation error is fourth order
/// in the subinterval width. The caller controls the even number of subintervals.
/// </remarks>
public static class Quadrature
{
    private const int MaximumRefinedIntervalCount = 1_000_000;
    /// <summary>
    /// Approximates a definite integral using composite Simpson's rule.
    /// </summary>
    /// <param name="function">A deterministic scalar function returning finite samples.</param>
    /// <param name="lowerBound">The finite lower integration bound.</param>
    /// <param name="upperBound">The finite upper integration bound.</param>
    /// <param name="intervalCount">The positive, even number of equal-width subintervals (at most 500,000 when refinement is requested).</param>
    /// <returns>The signed integral estimate. Reversed bounds produce a negated result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="function"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A bound is non-finite, or the interval count is not positive, even, and safely refinable.</exception>
    /// <exception cref="ArithmeticException">A sample coordinate, function value, weighted sum, or result is non-finite.</exception>
    public static double CompositeSimpson(
        Func<double, double> function,
        double lowerBound,
        double upperBound,
        int intervalCount)
    {
        ArgumentNullException.ThrowIfNull(function);
        ValidateInputs(lowerBound, upperBound, intervalCount);

        if (lowerBound == upperBound)
            return 0d;

        return EvaluateCompositeSimpson(function, lowerBound, upperBound, intervalCount);
    }

    /// <summary>
    /// Refines a composite Simpson estimate by doubling its interval count and applies
    /// Richardson extrapolation using Simpson's fourth-order error model.
    /// </summary>
    /// <remarks>
    /// If the function is sufficiently smooth and both estimates are in the asymptotic
    /// regime, the leading Simpson error decreases by approximately sixteen when the
    /// interval width is halved. The correction (fine - coarse) / 15 cancels that leading
    /// term. Its magnitude is an asymptotic error indicator, not a rigorous bound. It may
    /// be misleading for nonsmooth functions, unresolved features, or roundoff-dominated
    /// calculations. The function is evaluated independently for both grids and should
    /// therefore be deterministic and side-effect free.
    /// </remarks>
    /// <param name="function">A deterministic scalar function returning finite samples.</param>
    /// <param name="lowerBound">The finite lower integration bound.</param>
    /// <param name="upperBound">The finite upper integration bound.</param>
    /// <param name="intervalCount">The positive, even coarse interval count; the fine count is twice this value.</param>
    /// <returns>The extrapolated integral, correction magnitude, and coarse/fine interval counts.</returns>
    public static IntegralEstimate CompositeSimpsonWithErrorEstimate(
        Func<double, double> function,
        double lowerBound,
        double upperBound,
        int intervalCount)
    {
        ArgumentNullException.ThrowIfNull(function);
        ValidateInputs(lowerBound, upperBound, intervalCount);

        if (lowerBound == upperBound)
            return new IntegralEstimate(0d, 0d, intervalCount, intervalCount * 2);

        var coarse = EvaluateCompositeSimpson(function, lowerBound, upperBound, intervalCount);
        var fineIntervalCount = intervalCount * 2;
        var fine = EvaluateCompositeSimpson(function, lowerBound, upperBound, fineIntervalCount);
        var correction = (fine - coarse) / 15d;
        var value = fine + correction;
        var estimatedAbsoluteError = Math.Abs(correction);

        if (!double.IsFinite(correction) || !double.IsFinite(value) || !double.IsFinite(estimatedAbsoluteError))
            throw new ArithmeticException("The Simpson Richardson extrapolation produced a non-finite result.");

        return new IntegralEstimate(value, estimatedAbsoluteError, intervalCount, fineIntervalCount);
    }

    private static double EvaluateCompositeSimpson(
        Func<double, double> function,
        double lowerBound,
        double upperBound,
        int intervalCount)
    {
        var width = upperBound - lowerBound;
        var step = width / intervalCount;
        if (!double.IsFinite(width) || !double.IsFinite(step) || step == 0d)
            throw new ArithmeticException("The integration interval cannot be represented with a finite non-zero step.");

        var weightedSum = 0d;
        var previousCoordinate = lowerBound;
        for (var index = 0; index <= intervalCount; index++)
        {
            var coordinate = index == intervalCount ? upperBound : lowerBound + (step * index);
            if (!double.IsFinite(coordinate) ||
                (index > 0 && (step > 0d ? coordinate <= previousCoordinate : coordinate >= previousCoordinate)))
                throw new ArithmeticException("The integration grid contains a non-finite or repeated sample coordinate.");

            var sample = function(coordinate);
            if (!double.IsFinite(sample))
                throw new ArithmeticException("The function returned a non-finite integration sample.");

            var weight = index == 0 || index == intervalCount
                ? 1d
                : index % 2 == 0 ? 2d : 4d;
            var contribution = weight * sample;
            weightedSum += contribution;
            if (!double.IsFinite(contribution) || !double.IsFinite(weightedSum))
                throw new ArithmeticException("The weighted Simpson sum exceeded the finite numeric range.");

            previousCoordinate = coordinate;
        }

        var result = (step / 3d) * weightedSum;
        if (!double.IsFinite(result))
            throw new ArithmeticException("The Simpson integral estimate is non-finite.");

        return result;
    }

    private static void ValidateInputs(double lowerBound, double upperBound, int intervalCount)
    {
        if (!double.IsFinite(lowerBound))
            throw new ArgumentOutOfRangeException(nameof(lowerBound), "The lower bound must be finite.");
        if (!double.IsFinite(upperBound))
            throw new ArgumentOutOfRangeException(nameof(upperBound), "The upper bound must be finite.");
        if (intervalCount <= 0 || intervalCount % 2 != 0 || intervalCount > MaximumRefinedIntervalCount / 2)
            throw new ArgumentOutOfRangeException(nameof(intervalCount), "The interval count must be positive, even, and no greater than half the one-million-interval work limit.");
    }
}

/// <summary>
/// A Richardson-refined composite Simpson integral and its asymptotic error indicator.
/// </summary>
/// <param name="Value">The extrapolated signed integral estimate.</param>
/// <param name="EstimatedAbsoluteError">The magnitude of the extrapolation correction; not a rigorous error bound.</param>
/// <param name="CoarseIntervalCount">The original even number of subintervals.</param>
/// <param name="FineIntervalCount">Twice the original number of subintervals.</param>
public readonly record struct IntegralEstimate(
    double Value,
    double EstimatedAbsoluteError,
    int CoarseIntervalCount,
    int FineIntervalCount);
