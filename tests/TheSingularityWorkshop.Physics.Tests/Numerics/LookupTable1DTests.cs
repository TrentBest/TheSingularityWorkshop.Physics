using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Numerics;

[TestFixture]
public sealed class LookupTable1DTests
{
    [Test]
    public void Sample_IncludesBothEndpointsAndReportsPayloadSize()
    {
        var table = LookupTable1D.Sample(-2d, 2d, 5, x => x * x);

        Assert.That(table.MinimumInput, Is.EqualTo(-2d));
        Assert.That(table.MaximumInput, Is.EqualTo(2d));
        Assert.That(table.Count, Is.EqualTo(5));
        Assert.That(table.PayloadBytes, Is.EqualTo(5 * sizeof(double)));
        Assert.That(table.GetSample(0), Is.EqualTo(4d));
        Assert.That(table.GetSample(4), Is.EqualTo(4d));
        Assert.That(table.Evaluate(-2d), Is.EqualTo(4d));
        Assert.That(table.Evaluate(2d), Is.EqualTo(4d));
    }

    [Test]
    public void Evaluate_LinearlyInterpolatesBetweenStoredSamples()
    {
        var table = LookupTable1D.Sample(0d, 10d, 3, x => 2d * x + 1d);

        Assert.That(table.Evaluate(2.5d), Is.EqualTo(6d).Within(1e-12));
        Assert.That(table.Evaluate(7.5d), Is.EqualTo(16d).Within(1e-12));
    }

    [Test]
    public void Evaluate_ApproximatesSineWithinDeclaredTestTolerance()
    {
        var table = LookupTable1D.Sample(0d, Math.PI / 2d, 257, Math.Sin);
        var maximumAbsoluteError = 0d;

        for (var i = 0; i <= 10_000; i++)
        {
            var input = (Math.PI / 2d) * i / 10_000d;
            maximumAbsoluteError = Math.Max(maximumAbsoluteError, Math.Abs(table.Evaluate(input) - Math.Sin(input)));
        }

        Assert.That(maximumAbsoluteError, Is.LessThan(1.3e-5));
    }

    [Test]
    public void Evaluate_RejectsNonFiniteAndOutOfDomainInputs()
    {
        var table = LookupTable1D.Sample(-1d, 1d, 3, x => x);

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Evaluate(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Evaluate(double.NegativeInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Evaluate(-1.0001d));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.Evaluate(1.0001d));
    }

    [Test]
    public void Sample_RejectsInvalidDomainAndSampleCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LookupTable1D.Sample(1d, 1d, 2, x => x));
        Assert.Throws<ArgumentOutOfRangeException>(() => LookupTable1D.Sample(0d, 1d, 1, x => x));
        Assert.Throws<ArgumentOutOfRangeException>(() => LookupTable1D.Sample(-double.MaxValue, double.MaxValue, 2, x => x));
    }

    [Test]
    public void Sample_RejectsNonFiniteFunctionOutputs()
    {
        Assert.Throws<ArgumentException>(() => LookupTable1D.Sample(0d, 1d, 3, x => x == 0.5d ? double.NaN : x));
        Assert.Throws<ArgumentNullException>(() => LookupTable1D.Sample(0d, 1d, 3, null!));
    }
}
