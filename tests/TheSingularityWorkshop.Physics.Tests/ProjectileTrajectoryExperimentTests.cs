using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ProjectileTrajectoryExperimentTests
{
    private static ProjectileTrajectoryExperimentDefinition CreateDefinition() =>
        new(
            InitialState: new ProjectileState(
                new Vector3d(1d, 2d, 3d),
                new Vector3d(4d, -2d, 10d),
                0d),
            Acceleration: new Vector3d(0d, 0d, -9.81d),
            DeltaTime: 0.1d,
            StepCount: 20,
            CoordinateFrame: "world, Z-up");

    [Test]
    public void Run_RecordsInitialStateAndEveryStep()
    {
        var definition = CreateDefinition();

        var result = ProjectileTrajectoryExperiment.Run(definition);

        Assert.That(result.Definition, Is.EqualTo(definition));
        Assert.That(result.CompletedSteps, Is.EqualTo(20));
        Assert.That(result.Samples.Count, Is.EqualTo(21));
        Assert.That(result.Samples[0], Is.EqualTo(definition.InitialState));
        Assert.That(result.FinalState.ElapsedTime, Is.EqualTo(2d).Within(1e-12));
        Assert.That(result.SimulatedDuration, Is.EqualTo(2d).Within(1e-12));
    }

    [Test]
    public void Run_EverySampleMatchesIndependentClosedFormPrediction()
    {
        var definition = CreateDefinition();
        var result = ProjectileTrajectoryExperiment.Run(definition);

        for (var step = 0; step <= definition.StepCount; step++)
        {
            var t = step * definition.DeltaTime;
            var initial = definition.InitialState;
            var a = definition.Acceleration;
            var expectedPosition = initial.Position +
                (initial.Velocity * t) +
                (a * (0.5d * t * t));
            var expectedVelocity = initial.Velocity + (a * t);
            var sample = result.Samples[step];

            Assert.That(sample.Position.X, Is.EqualTo(expectedPosition.X).Within(1e-11), $"X position at sample {step}.");
            Assert.That(sample.Position.Y, Is.EqualTo(expectedPosition.Y).Within(1e-11), $"Y position at sample {step}.");
            Assert.That(sample.Position.Z, Is.EqualTo(expectedPosition.Z).Within(1e-11), $"Z position at sample {step}.");
            Assert.That(sample.Velocity.X, Is.EqualTo(expectedVelocity.X).Within(1e-11), $"X velocity at sample {step}.");
            Assert.That(sample.Velocity.Y, Is.EqualTo(expectedVelocity.Y).Within(1e-11), $"Y velocity at sample {step}.");
            Assert.That(sample.Velocity.Z, Is.EqualTo(expectedVelocity.Z).Within(1e-11), $"Z velocity at sample {step}.");
            Assert.That(sample.ElapsedTime, Is.EqualTo(initial.ElapsedTime + t).Within(1e-11), $"Elapsed time at sample {step}.");
        }
    }

    [Test]
    public void Run_RepeatedRunsProduceIdenticalSamples()
    {
        var definition = CreateDefinition();

        var first = ProjectileTrajectoryExperiment.Run(definition);
        var second = ProjectileTrajectoryExperiment.Run(definition);

        Assert.That(second.Samples, Is.EqualTo(first.Samples));
        Assert.That(second.FinalState, Is.EqualTo(first.FinalState));
    }

    [Test]
    public void Run_RejectsInvalidStepCountAndMissingCoordinateConvention()
    {
        var definition = CreateDefinition();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProjectileTrajectoryExperiment.Run(definition with { StepCount = 0 }));
        Assert.Throws<ArgumentException>(() =>
            ProjectileTrajectoryExperiment.Run(definition with { CoordinateFrame = " " }));
    }

    [Test]
    public void Run_RejectsInvalidTimestepAndSampleCountOverflow()
    {
        var definition = CreateDefinition();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProjectileTrajectoryExperiment.Run(definition with { DeltaTime = double.NaN }));
        Assert.Throws<OverflowException>(() =>
            ProjectileTrajectoryExperiment.Run(definition with { StepCount = int.MaxValue }));
    }

    [Test]
    public void RunRecord_ContainsActualSamplesAndSerializesDeterministically()
    {
        var result = ProjectileTrajectoryExperiment.Run(CreateDefinition());

        var record = ProjectileTrajectoryRunRecord.FromResult(result);
        var firstJson = record.ToJson();

        Assert.That(record.ModelId, Is.EqualTo("kinematics.constant-acceleration-projectile"));
        Assert.That(record.SchemaVersion, Is.EqualTo(1));
        Assert.That(record.Definition, Is.EqualTo(result.Definition));
        Assert.That(record.Samples, Is.EqualTo(result.Samples));
        Assert.That(record.FinalState, Is.EqualTo(result.FinalState));
        Assert.That(record.Units["position"], Is.EqualTo("m"));
        Assert.That(record.Units["velocity"], Is.EqualTo("m/s"));
        Assert.That(record.Units["acceleration"], Is.EqualTo("m/s^2"));
        Assert.That(record.ToJson(), Is.EqualTo(firstJson));
        Assert.That(firstJson, Does.Contain("\"coordinateFrame\": \"world, Z-up\""));
        Assert.That(firstJson, Does.Contain("\"samples\""));
    }
}
