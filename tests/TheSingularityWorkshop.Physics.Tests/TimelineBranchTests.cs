using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineBranchTests
{
    [Test]
    public void RootTimeline_BeginsAtTickZeroWithoutParent()
    {
        var root = TimelineBranch.CreateRoot("timeline-a", "reality-1");

        Assert.That(root.Id, Is.EqualTo("timeline-a"));
        Assert.That(root.RealityId, Is.EqualTo("reality-1"));
        Assert.That(root.ParentTimelineId, Is.Null);
        Assert.That(root.ForkTick, Is.Zero);
        Assert.That(root.HeadTick, Is.Zero);
    }

    [Test]
    public void ForkAtHistoricalTick_CreatesChildAndLeavesParentUnchanged()
    {
        var root = TimelineBranch.CreateRoot("timeline-a", "reality-1");
        var advancedParent = root.WithHeadTick(100);
        var historical = TimelineBranch.Fork("timeline-b", advancedParent, 42);

        Assert.That(historical.ParentTimelineId, Is.EqualTo("timeline-a"));
        Assert.That(historical.RealityId, Is.EqualTo(root.RealityId));
        Assert.That(historical.ForkTick, Is.EqualTo(42));
        Assert.That(historical.HeadTick, Is.EqualTo(42));
        Assert.That(advancedParent.HeadTick, Is.EqualTo(100));
        Assert.That(root.HeadTick, Is.Zero);
        Assert.That(root.ParentTimelineId, Is.Null);
    }

    [Test]
    public void TimelineHeadCannotMoveBackward()
    {
        var root = TimelineBranch.CreateRoot("timeline-a", "reality-1").WithHeadTick(10);

        Assert.Throws<ArgumentOutOfRangeException>(() => root.WithHeadTick(9));
        Assert.That(root.WithHeadTick(20).HeadTick, Is.EqualTo(20));
        Assert.That(root.HeadTick, Is.EqualTo(10));
    }

    [Test]
    public void Fork_RejectsTickOutsideRepresentedParentHistory()
    {
        var root = TimelineBranch.CreateRoot("timeline-a", "reality-1");

        Assert.Throws<ArgumentOutOfRangeException>(() => TimelineBranch.Fork("timeline-b", root, 1));
    }

    [Test]
    public void IdentifiersMustBeDistinctAndNonEmpty()
    {
        Assert.Throws<ArgumentException>(() => TimelineBranch.CreateRoot(" ", "reality-1"));
        Assert.Throws<ArgumentException>(() => TimelineBranch.CreateRoot("timeline-a", ""));
        var root = TimelineBranch.CreateRoot("timeline-a", "reality-1");
        Assert.Throws<ArgumentException>(() => TimelineBranch.Fork("timeline-a", root, 0));
    }

    [Test]
    public void ForkCannotPrecedeParentsForkPoint()
    {
        var root = TimelineBranch.CreateRoot("timeline-a", "reality-1");
        var child = TimelineBranch.Fork("timeline-b", root, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => TimelineBranch.Fork("timeline-c", child, -1));
    }
}
