using NUnit.Framework;
using TheSingularityWorkshop.Physics.Collision;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class AxisAlignedBoundsTests
{
    [Test]
    public void Intersects_IncludesTouchingFacesButRejectsSeparatedBounds()
    {
        var left = new AxisAlignedBounds(0, 0, 0, 1, 1, 1);
        var touching = new AxisAlignedBounds(1, 0, 0, 2, 1, 1);
        var separated = new AxisAlignedBounds(1.01, 0, 0, 2, 1, 1);

        Assert.That(left.Intersects(touching), Is.True);
        Assert.That(left.Intersects(separated), Is.False);
    }

    [Test]
    public void SweptBounds_EncloseStartAndEndForPositiveAndNegativeMotion()
    {
        var start = new AxisAlignedBounds(0, 0, 0, 1, 1, 1);
        var positive = start.SweptBy(4, 0, 0);
        var negative = start.SweptBy(-4, 0, 0);

        Assert.That(positive, Is.EqualTo(new AxisAlignedBounds(0, 0, 0, 5, 1, 1)));
        Assert.That(negative, Is.EqualTo(new AxisAlignedBounds(-4, 0, 0, 1, 1, 1)));
    }

    [Test]
    public void Constructor_RejectsInvalidBounds()
    {
        Assert.Throws<ArgumentException>(() => new AxisAlignedBounds(2, 0, 0, 1, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AxisAlignedBounds(double.NaN, 0, 0, 1, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AxisAlignedBounds(0, 0, 0, 1, 1, 1).Expanded(-1, 0, 0));
    }
}

[TestFixture]
public sealed class SweepAndPruneBroadphaseTests
{
    [Test]
    public void FindCandidates_ReturnsOnlyIntersectingBoundsInStablePairOrder()
    {
        var proxies = new[]
        {
            new BroadphaseProxy(30, new AxisAlignedBounds(0, 0, 0, 2, 2, 2)),
            new BroadphaseProxy(10, new AxisAlignedBounds(1, 1, 1, 3, 3, 3)),
            new BroadphaseProxy(20, new AxisAlignedBounds(5, 5, 5, 6, 6, 6)),
            new BroadphaseProxy(40, new AxisAlignedBounds(2, 0, 0, 4, 1, 1))
        };

        var pairs = SweepAndPruneBroadphase.FindCandidates(proxies);

        Assert.That(pairs, Is.EqualTo(new[]
        {
            new BroadphasePair(10, 30),
            new BroadphasePair(30, 40)
        }));
    }

    [Test]
    public void FindCandidates_UsesSweptBoundsToKeepFastCrossingObjectsAsCandidates()
    {
        var moverStart = new AxisAlignedBounds(0, 0, 0, 1, 1, 1);
        var stationary = new AxisAlignedBounds(3, 0, 0, 4, 1, 1);
        var proxies = new[]
        {
            new BroadphaseProxy(1, moverStart.SweptBy(4, 0, 0)),
            new BroadphaseProxy(2, stationary)
        };

        Assert.That(SweepAndPruneBroadphase.FindCandidates(proxies),
            Is.EqualTo(new[] { new BroadphasePair(1, 2) }));
    }

    [Test]
    public void FindCandidates_RejectsDuplicateIds()
    {
        var bounds = new AxisAlignedBounds(0, 0, 0, 1, 1, 1);
        Assert.Throws<ArgumentException>(() => SweepAndPruneBroadphase.FindCandidates(new[]
        {
            new BroadphaseProxy(7, bounds),
            new BroadphaseProxy(7, bounds)
        }));
    }
}
