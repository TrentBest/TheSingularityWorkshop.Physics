using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class TimelineLineageTests
{
    [Test]
    public void LineageAcceptsRootToTargetChainAndCheckpointFromSharedPast()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(100);
        var child = TimelineBranch.Fork("child", root, 40).WithHeadTick(70);
        var grandchild = TimelineBranch.Fork("grandchild", child, 55).WithHeadTick(60);
        var lineage = new TimelineLineage([root, child, grandchild]);

        var rootCheckpoint = MakeCheckpoint("root", 30);
        var childCheckpoint = MakeCheckpoint("child", 50);
        var tooLateRootCheckpoint = MakeCheckpoint("root", 41);

        Assert.That(lineage.Count, Is.EqualTo(3));
        Assert.That(lineage.Root.Id, Is.EqualTo("root"));
        Assert.That(lineage.Target.Id, Is.EqualTo("grandchild"));
        Assert.That(lineage.RealityId, Is.EqualTo("reality-1"));
        Assert.That(lineage.CanReconstructFrom(rootCheckpoint, 60), Is.True);
        Assert.That(lineage.CanReconstructFrom(childCheckpoint, 60), Is.True);
        Assert.That(lineage.CanReconstructFrom(tooLateRootCheckpoint, 60), Is.False);
        Assert.That(lineage.CanReconstructFrom(rootCheckpoint, 61), Is.False);
    }

    [Test]
    public void LineageRejectsBrokenParentRealityAndRepeatedIds()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var child = TimelineBranch.Fork("child", root, 10);
        var otherRoot = TimelineBranch.CreateRoot("other", "reality-1");
        var otherReality = TimelineBranch.CreateRoot("different-reality", "reality-2");

        Assert.Throws<ArgumentException>(() => new TimelineLineage([child]));
        Assert.Throws<ArgumentException>(() => new TimelineLineage([root, otherRoot]));
        Assert.Throws<ArgumentException>(() => new TimelineLineage([root, otherReality]));
        Assert.Throws<ArgumentException>(() => new TimelineLineage([root, child, root]));
    }

    [Test]
    public void ReconstructionRequiresCheckpointToBelongToAncestryAndValidTicks()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1").WithHeadTick(20);
        var child = TimelineBranch.Fork("child", root, 10).WithHeadTick(15);
        var lineage = new TimelineLineage([root, child]);

        Assert.That(lineage.CanReconstructFrom(MakeCheckpoint("unrelated", 5), 12), Is.False);
        Assert.That(lineage.CanReconstructFrom(MakeCheckpoint("root", 5, "reality-2"), 12), Is.False);
        Assert.That(lineage.CanReconstructFrom(MakeCheckpoint("child", 9), 12), Is.False);
        Assert.That(lineage.CanReconstructFrom(MakeCheckpoint("child", 11), 10), Is.False);
        Assert.That(lineage.CanReconstructFrom(MakeCheckpoint("child", 11), 16), Is.False);
    }

    [Test]
    public void CopyBranchesDoesNotExposeInternalArray()
    {
        var root = TimelineBranch.CreateRoot("root", "reality-1");
        var lineage = new TimelineLineage([root]);
        var copy = lineage.CopyBranches();

        Assert.That(copy, Has.Count.EqualTo(1));
        Assert.Throws<NotSupportedException>(() => ((IList<TimelineBranch>)copy).Add(root));
        Assert.That(lineage.Root.Id, Is.EqualTo("root"));
    }

    private static PhysicsCheckpoint MakeCheckpoint(string timelineId, long tick, string realityId = "reality-1") =>
        new("cp-" + timelineId + "-" + tick, realityId, timelineId, tick, "schema-1", "model-1", new byte[] { 1, 2, 3 });
}
