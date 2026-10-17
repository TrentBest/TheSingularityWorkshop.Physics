using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class MovingTrainProjectileExperimentTests
{
    [Test]
    public void Run_PairsTrainAndGroundViewsAtEveryLogicalTime()
    {
        var definition = new MovingTrainProjectileExperimentDefinition(
            TrainFrameInitialState: new ProjectileState(
                Vector3d.Zero,
                new Vector3d(0d, 0d, 8d),
                0d),
            TrainFrameAcceleration: new Vector3d(0d, 0d, -9.81d),
            TrainVelocityInWorld: new Vector3d(20d, 0d, 0d),
            TrainOriginAtEpoch: Vector3d.Zero,
            DeltaTime: 0.25d,
            StepCount: 8);

        var result = MovingTrainProjectileExperiment.Run(definition);

        Assert.That(result.Samples.Count, Is.EqualTo(9));
        Assert.That(result.CompletedSteps, Is.EqualTo(8));

        foreach (var sample in result.Samples)
        {
            Assert.That(sample.GroundFrameState.ElapsedTime,
                Is.EqualTo(sample.TrainFrameState.ElapsedTime).Within(1e-12));
            Assert.That(sample.GroundFrameState.Position.X,
                Is.EqualTo(20d * sample.TrainFrameState.ElapsedTime).Within(1e-10));
            Assert.That(sample.GroundFrameState.Position.Z,
                Is.EqualTo(sample.TrainFrameState.Position.Z).Within(1e-10));
            Assert.That(sample.GroundFrameState.Velocity.X,
                Is.EqualTo(20d).Within(1e-12));
            Assert.That(sample.GroundFrameState.Velocity.Z,
                Is.EqualTo(sample.TrainFrameState.Velocity.Z).Within(1e-12));
        }

        Assert.That(result.FinalSample.TrainFrameState.ElapsedTime, Is.EqualTo(2d).Within(1e-12));
        Assert.That(result.FinalSample.GroundFrameState.Position.X, Is.EqualTo(40d).Within(1e-10));
    }

    [Test]
    public void Run_RespectsNonZeroTrainOrigin()
    {
        var result = MovingTrainProjectileExperiment.Run(new MovingTrainProjectileExperimentDefinition(
            new ProjectileState(Vector3d.Zero, Vector3d.Zero, 0d),
            Vector3d.Zero,
            new Vector3d(3d, -2d, 0d),
            new Vector3d(100d, 5d, 1d),
            0.5d,
            2));

        var final = result.FinalSample.GroundFrameState;
        Assert.That(final.Position.X, Is.EqualTo(103d).Within(1e-12));
        Assert.That(final.Position.Y, Is.EqualTo(3d).Within(1e-12));
        Assert.That(final.Position.Z, Is.EqualTo(1d).Within(1e-12));
    }
}
