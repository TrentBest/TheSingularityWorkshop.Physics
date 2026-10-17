using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Math;

[TestFixture]
public sealed class OnlineStatisticsTests
{
    [Test]
    public void Add_ComputesMeanAndPopulationAndSampleVariance()
    {
        var stats = new OnlineStatistics();
        stats.Add(2d);
        stats.Add(4d);
        stats.Add(6d);

        Assert.That(stats.Count, Is.EqualTo(3));
        Assert.That(stats.Mean, Is.EqualTo(4d).Within(1e-14));
        Assert.That(stats.PopulationVariance, Is.EqualTo(8d / 3d).Within(1e-14));
        Assert.That(stats.SampleVariance, Is.EqualTo(4d).Within(1e-14));
        Assert.That(stats.PopulationStandardDeviation, Is.EqualTo(Math.Sqrt(8d / 3d)).Within(1e-14));
        Assert.That(stats.SampleStandardDeviation, Is.EqualTo(2d).Within(1e-14));
    }

    [Test]
    public void Add_IsSinglePassAndStableForLargeOffsetSmallSpread()
    {
        var stats = new OnlineStatistics();
        stats.Add(1_000_000_000_001d);
        stats.Add(1_000_000_000_002d);
        stats.Add(1_000_000_000_003d);

        Assert.That(stats.Mean, Is.EqualTo(1_000_000_000_002d).Within(1e-6));
        Assert.That(stats.SampleVariance, Is.EqualTo(1d).Within(1e-6));
    }

    [Test]
    public void EmptyAndSingletonSamplesHaveExplicitVarianceContracts()
    {
        var stats = new OnlineStatistics();

        Assert.Throws<InvalidOperationException>(() => _ = stats.Mean);
        Assert.Throws<InvalidOperationException>(() => _ = stats.PopulationVariance);
        Assert.Throws<InvalidOperationException>(() => _ = stats.SampleVariance);

        stats.Add(42d);
        Assert.That(stats.Mean, Is.EqualTo(42d));
        Assert.That(stats.PopulationVariance, Is.EqualTo(0d));
        Assert.Throws<InvalidOperationException>(() => _ = stats.SampleVariance);
    }

    [Test]
    public void Add_RejectsNonFiniteInputWithoutChangingState()
    {
        var stats = new OnlineStatistics();
        stats.Add(3d);

        Assert.Throws<ArgumentOutOfRangeException>(() => stats.Add(double.NaN));
        Assert.That(stats.Count, Is.EqualTo(1));
        Assert.That(stats.Mean, Is.EqualTo(3d));
    }

    [Test]
    public void Clear_ResetsTheAccumulator()
    {
        var stats = new OnlineStatistics();
        stats.Add(2d);
        stats.Add(8d);
        stats.Clear();

        Assert.That(stats.Count, Is.EqualTo(0));
        Assert.Throws<InvalidOperationException>(() => _ = stats.Mean);
    }
}
