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
        var advancedParent = root with { };
        // A branch's represented head is immutable; a persistence/replay layer will later publish
        // a new descriptor as the parent history advances.
        var historical = TimelineBranch.Fork("timeline-b", advancedParent, 0);

        Assert.That(historical.ParentTimelineId, Is.EqualTo("timeline-a"));
        Assert.That(historical.RealityId, Is.EqualTo(root.RealityId));
        Assert.That(historical.ForkTick, Is.Zero);
        Assert.That(historical.HeadTick, Is.Zero);
        Assert.That(root.ParentTimelineId, Is.Null);
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
