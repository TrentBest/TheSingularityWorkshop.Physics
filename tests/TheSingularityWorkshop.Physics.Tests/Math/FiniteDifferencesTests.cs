using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class FiniteDifferencesTests
{
    [Test]
    public void CentralDifference_QuadraticMatchesAnalyticalDerivative()
    {
        var derivative = FiniteDifferences.CentralDifference(x => (3d * x * x) + (2d * x) - 7d, 4d, 1e-3);

        Assert.That(derivative, Is.EqualTo(26d).Within(1e-8));
    }

    [Test]
    public void CentralDifference_SineApproachesCosineDerivative()
    {
        var derivative = FiniteDifferences.CentralDifference(Math.Sin, 0.7d, 1e-4);

        Assert.That(derivative, Is.EqualTo(Math.Cos(0.7d)).Within(1e-8));
    }

    [Test]
    public void CentralDifference_ConstantFunctionReturnsZero()
    {
        var derivative = FiniteDifferences.CentralDifference(_ => 12d, -3d, 0.25d);

        Assert.That(derivative, Is.Zero);
    }

    [Test]
    public void CentralDifference_RejectsInvalidArguments()
    {
        Assert.Throws<ArgumentNullException>(() => FiniteDifferences.CentralDifference(null!, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiniteDifferences.CentralDifference(x => x, double.NaN, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiniteDifferences.CentralDifference(x => x, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => FiniteDifferences.CentralDifference(x => x, 0d, double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => FiniteDifferences.CentralDifference(x => x, 1e300, 1d));
    }

    [Test]
    public void CentralDifference_RejectsNonFiniteFunctionSamples()
    {
        Assert.Throws<ArithmeticException>(() =>
            FiniteDifferences.CentralDifference(_ => double.NaN, 0d, 0.1d));
    }

    [Test]
    public void CentralDifference_RejectsNonFiniteIntermediateResult()
    {
        Assert.Throws<ArithmeticException>(() =>
            FiniteDifferences.CentralDifference(x => x < 0d ? -double.MaxValue : double.MaxValue, 0d, 1d));
    }

    [Test]
    public void RichardsonEstimate_RefinesSineDerivativeAndReportsCorrection()
    {
        var estimate = FiniteDifferences.CentralDifferenceWithErrorEstimate(Math.Sin, 0.7d, 0.1d);
        var actualError = Math.Abs(estimate.Value - Math.Cos(0.7d));

        Assert.That(estimate.Value, Is.EqualTo(Math.Cos(0.7d)).Within(1e-6));
        Assert.That(estimate.EstimatedAbsoluteError, Is.GreaterThan(0d));
        Assert.That(actualError, Is.LessThan(estimate.EstimatedAbsoluteError * 1.1d));
        Assert.That(estimate.CoarseStep, Is.EqualTo(0.1d));
        Assert.That(estimate.FineStep, Is.EqualTo(0.05d));
    }

    [Test]
    public void RichardsonEstimate_CancelsLeadingErrorForQuarticPolynomial()
    {
        var estimate = FiniteDifferences.CentralDifferenceWithErrorEstimate(x => x * x * x * x, 2d, 0.25d);

        Assert.That(estimate.Value, Is.EqualTo(32d).Within(1e-12));
        Assert.That(estimate.EstimatedAbsoluteError, Is.EqualTo(0.125d).Within(1e-12));
    }

    [Test]
    public void RichardsonEstimate_StepSizeSweepShowsTruncationAndRoundoffTradeoff()
    {
        var steps = Enumerable.Range(1, 12).Select(power => Math.Pow(10d, -power)).ToArray();
        var errors = steps.Select(step =>
        {
            var estimate = FiniteDifferences.CentralDifferenceWithErrorEstimate(Math.Sin, 0.7d, step);
            return Math.Abs(estimate.Value - Math.Cos(0.7d));
        }).ToArray();

        var minimumErrorIndex = Array.IndexOf(errors, errors.Min());

        Assert.That(minimumErrorIndex, Is.GreaterThan(0), "The coarsest step should still show truncation error.");
        Assert.That(minimumErrorIndex, Is.LessThan(errors.Length - 1), "The finest step should show roundoff influence.");
        Assert.That(errors[minimumErrorIndex], Is.LessThan(errors[0]));
        Assert.That(errors[minimumErrorIndex], Is.LessThan(errors[^1]));
    }

    [Test]
    public void RichardsonEstimate_RejectsNullAndUnrepresentableFineStep()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FiniteDifferences.CentralDifferenceWithErrorEstimate(null!, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FiniteDifferences.CentralDifferenceWithErrorEstimate(x => x, 0d, double.Epsilon));
    }

    [Test]
    public void RichardsonEstimate_EvaluatesFunctionAtFourSamples()
    {
        var calls = 0;

        _ = FiniteDifferences.CentralDifferenceWithErrorEstimate(x =>
        {
            calls++;
            return x * x;
        }, 3d, 0.1d);

        Assert.That(calls, Is.EqualTo(4));
    }
}
