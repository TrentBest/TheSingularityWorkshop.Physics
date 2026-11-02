namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// Scalar finite-difference approximations for derivatives of caller-supplied functions.
/// </summary>
/// <remarks>
/// The central-difference formula has second-order truncation error for sufficiently smooth
/// functions. The caller selects the absolute step size; an excessively large step increases
/// truncation error, while an excessively small step can amplify floating-point cancellation.
/// This routine does not estimate its own error or infer physical units.
/// </remarks>
public static class FiniteDifferences
{
    /// <summary>
    /// Approximates the first derivative at <paramref name="x"/> using a symmetric
    /// two-point central difference.
    /// </summary>
    /// <param name="function">A scalar function that returns a finite value for both sample points.</param>
    /// <param name="x">The finite coordinate at which the derivative is approximated.</param>
    /// <param name="step">The positive finite absolute sample offset, in the same coordinate units as <paramref name="x"/>.</param>
    /// <returns>The derivative estimate, in function-output units per coordinate unit.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="function"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is non-finite, or <paramref name="step"/> is not positive and finite.</exception>
    /// <exception cref="ArgumentException">The step cannot produce two distinct finite sample coordinates.</exception>
    /// <exception cref="ArithmeticException">A function sample or intermediate result is non-finite.</exception>
    public static double CentralDifference(Func<double, double> function, double x, double step)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x), "The evaluation coordinate must be finite.");

        if (!double.IsFinite(step) || step <= 0d)
            throw new ArgumentOutOfRangeException(nameof(step), "The sample step must be positive and finite.");

        var lower = x - step;
        var upper = x + step;
        if (!double.IsFinite(lower) || !double.IsFinite(upper) || lower == x || upper == x || lower == upper)
            throw new ArgumentException("The sample step must produce two distinct finite coordinates.", nameof(step));

        var lowerValue = function(lower);
        var upperValue = function(upper);
        if (!double.IsFinite(lowerValue) || !double.IsFinite(upperValue))
            throw new ArithmeticException("The function returned a non-finite sample.");

        var numerator = upperValue - lowerValue;
        var denominator = 2d * step;
        if (!double.IsFinite(numerator) || !double.IsFinite(denominator) || denominator == 0d)
            throw new ArithmeticException("The finite-difference intermediate exceeds the finite numeric range.");

        var derivative = numerator / denominator;
        if (!double.IsFinite(derivative))
            throw new ArithmeticException("The derivative estimate is non-finite.");

        return derivative;
    }
}
