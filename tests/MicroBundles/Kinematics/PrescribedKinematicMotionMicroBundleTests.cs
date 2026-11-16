using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.MicroBundles.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;
using Xunit;

namespace TheSingularityWorkshop.Physics.MicroBundles.Tests.Kinematics;

public sealed class PrescribedKinematicMotionMicroBundleTests
{
    [Fact]
    public void Bundle_DeclaresStableProviderAndImplementsTypedContract()
    {
        IMicroBundle bundle = new PrescribedKinematicMotionMicroBundle();

        Assert.Equal(5203UL, bundle.Descriptor.Id);
        Assert.Contains(
            bundle.Descriptor.Providers,
            provider => provider.Id == PrescribedKinematicMotionMicroBundle.ProviderId);
        Assert.IsAssignableFrom<IPrescribedKinematicMotionProvider>(bundle);
        Assert.Empty(bundle.Dependencies);
    }

    [Fact]
    public void Provider_EvaluatesFixedStepAndValidatesAchievedCommit()
    {
        var provider = new PrescribedKinematicMotionMicroBundle();
        var definition = new PrescribedKinematicMotionDefinition(
            "motion:microbundle-test",
            "actor:microbundle-test",
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(new Vector3d(2d, 0d, 0d), Quaterniond.Identity),
            4d,
            2d,
            1d,
            1d);

        var requested = provider.EvaluateFixedStep(
            definition, stepIndex: 2, fixedTimeStepSeconds: 1d, totalStepCount: 2);
        var achieved = requested.State;
        var committed = provider.Commit(definition, requested, achieved, 0d, 0d);

        Assert.Equal(6d, requested.State.ElapsedTime);
        Assert.Equal(new Vector3d(2d, 0d, 0d), requested.State.Pose.Position);
        Assert.True(committed.IsArrived);
        Assert.Equal(achieved, committed.Achieved);
    }
}
