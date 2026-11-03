using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class QuadratureTests
{
    [Test]
    public void CompositeSimpson_CubicPolynomialMatchesAnalyticalIntegral()
    {
        // Integral from -1 to 2 of (x^3 - 2x + 4) is 12.75.
        var result = Quadrature.CompositeSimpson(x => (x * x * x) - (2d * x) + 4d, -1d, 2d, 8);

        Assert.That(result, Is.EqualTo(12.75d).Within(1e-12));
    }

    [Test]
    public void CompositeSimpson_SineMatchesAnalyticalIntegral()
    {
        var result = Quadrature.CompositeSimpson(Math.Sin, 0d, Math.PI, 100);

        Assert.That(result, Is.EqualTo(2d).Within(1e-8));
    }

    [Test]
    public void CompositeSimpson_ReversedBoundsNegateTheIntegral()
    {
        var forward = Quadrature.CompositeSimpson(x => x * x, 0d, 3d, 20);
        var reverse = Quadrature.CompositeSimpson(x => x * x, 3d, 0d, 20);

        Assert.That(reverse, Is.EqualTo(-forward).Within(1e-12));
    }

    [Test]
    public void CompositeSimpson_ZeroWidthReturnsZeroWithoutEvaluatingFunction()
    {
        var calls = 0;
        var result = Quadrature.CompositeSimpson(_ => { calls++; return 1d; }, 2d, 2d, 4);

        Assert.That(result, Is.Zero);
        Assert.That(calls, Is.Zero);
    }

    [Test]
    public void CompositeSimpson_RejectsInvalidArguments()
    {
        Assert.Throws<ArgumentNullException>(() => Quadrature.CompositeSimpson(null!, 0d, 1d, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quadrature.CompositeSimpson(x => x, double.NaN, 1d, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quadrature.CompositeSimpson(x => x, 0d, double.PositiveInfinity, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quadrature.CompositeSimpson(x => x, 0d, 1d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quadrature.CompositeSimpson(x => x, 0d, 1d, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => Quadrature.CompositeSimpson(x => x, 0d, 1d, int.MaxValue));
    }

    [Test]
    public void CompositeSimpson_RejectsNonFiniteSamplesAndOverflow()
    {
        Assert.Throws<ArithmeticException>(() =>
            Quadrature.CompositeSimpson(_ => double.NaN, 0d, 1d, 2));
        Assert.Throws<ArithmeticException>(() =>
            Quadrature.CompositeSimpson(_ => double.MaxValue, 0d, 1d, 2));
        Assert.Throws<ArithmeticException>(() =>
            Quadrature.CompositeSimpson(x => x, -double.MaxValue, double.MaxValue, 2));
        Assert.Throws<ArithmeticException>(() =>
            Quadrature.CompositeSimpson(x => x, 1e16, 1e16 + 4d, 4));
    }

    [Test]
    public void RichardsonEstimate_RefinesExponentialIntegralAndReportsCorrection()
    {
        var estimate = Quadrature.CompositeSimpsonWithErrorEstimate(Math.Exp, 0d, 1d, 4);
        var actualError = Math.Abs(estimate.Value - (Math.E - 1d));

        Assert.That(estimate.Value, Is.EqualTo(Math.E - 1d).Within(1e-9));
        Assert.That(estimate.EstimatedAbsoluteError, Is.GreaterThan(0d));
        Assert.That(actualError, Is.LessThan(estimate.EstimatedAbsoluteError * 1.1d));
        Assert.That(estimate.CoarseIntervalCount, Is.EqualTo(4));
        Assert.That(estimate.FineIntervalCount, Is.EqualTo(8));
    }

    [Test]
    public void RichardsonEstimate_ZeroWidthReturnsZeroWithoutEvaluatingFunction()
    {
        var calls = 0;
        var estimate = Quadrature.CompositeSimpsonWithErrorEstimate(_ => { calls++; return 1d; }, 4d, 4d, 2);

        Assert.That(estimate.Value, Is.Zero);
        Assert.That(estimate.EstimatedAbsoluteError, Is.Zero);
        Assert.That(calls, Is.Zero);
    }

    [Test]
    public void RichardsonEstimate_EvaluatesBothGridsIndependently()
    {
        var calls = 0;
        _ = Quadrature.CompositeSimpsonWithErrorEstimate(x => { calls++; return x * x; }, 0d, 1d, 4);

        Assert.That(calls, Is.EqualTo(4 + 1 + 8 + 1));
    }
}
