using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ConstantAccelerationExperimentTests
{
    [Test]
    public void Run_RecordsInitialAndEveryStep()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            InitialState: new KinematicState(Position: 0d, Velocity: 0d, ElapsedTime: 0d),
            Acceleration: 2d,
            DeltaTime: 0.5d,
            StepCount: 4);

        var result = ConstantAccelerationExperiment.Run(definition);

        Assert.That(result.CompletedSteps, Is.EqualTo(4));
        Assert.That(result.Samples.Count, Is.EqualTo(5));
        Assert.That(result.Samples[0], Is.EqualTo(definition.InitialState));
        Assert.That(result.FinalState.Position, Is.EqualTo(4d).Within(1e-12));
        Assert.That(result.FinalState.Velocity, Is.EqualTo(4d).Within(1e-12));
        Assert.That(result.FinalState.ElapsedTime, Is.EqualTo(2d).Within(1e-12));
        Assert.That(result.Definition, Is.EqualTo(definition));
    }

    [Test]
    public void Run_RepeatedRunsProduceIdenticalSamples()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            InitialState: new KinematicState(Position: -3d, Velocity: 1.5d, ElapsedTime: 4d),
            Acceleration: -0.75d,
            DeltaTime: 0.125d,
            StepCount: 32);

        var first = ConstantAccelerationExperiment.Run(definition);
        var second = ConstantAccelerationExperiment.Run(definition);

        Assert.That(second.Samples, Is.EqualTo(first.Samples));
        Assert.That(second.FinalState, Is.EqualTo(first.FinalState));
    }

    [Test]
    public void Run_EachSampleMatchesIndependentClosedFormPrediction()
    {
        const double initialPosition = 12.5d;
        const double initialVelocity = -3.25d;
        const double initialElapsedTime = 7d;
        const double acceleration = -1.75d;
        const double deltaTime = 0.125d;
        const int stepCount = 40;

        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(initialPosition, initialVelocity, initialElapsedTime),
            Acceleration: acceleration,
            DeltaTime: deltaTime,
            StepCount: stepCount);

        var result = ConstantAccelerationExperiment.Run(definition);

        for (var step = 0; step <= stepCount; step++)
        {
            var elapsed = step * deltaTime;
            var expectedPosition = initialPosition + (initialVelocity * elapsed) +
                (0.5d * acceleration * elapsed * elapsed);
            var expectedVelocity = initialVelocity + (acceleration * elapsed);
            var expectedElapsedTime = initialElapsedTime + elapsed;
            var sample = result.Samples[step];

            Assert.That(sample.Position, Is.EqualTo(expectedPosition).Within(1e-12),
                $"Position mismatch at sample {step}.");
            Assert.That(sample.Velocity, Is.EqualTo(expectedVelocity).Within(1e-12),
                $"Velocity mismatch at sample {step}.");
            Assert.That(sample.ElapsedTime, Is.EqualTo(expectedElapsedTime).Within(1e-12),
                $"Elapsed-time mismatch at sample {step}.");
        }
    }

    [Test]
    public void Run_RejectsZeroStepCount()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(0d, 0d, 0d),
            Acceleration: 1d,
            DeltaTime: 0.1d,
            StepCount: 0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ConstantAccelerationExperiment.Run(definition));
    }

    [Test]
    public void Run_RejectsStepCountWhoseSampleArrayLengthWouldOverflow()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(0d, 0d, 0d),
            Acceleration: 1d,
            DeltaTime: 0.1d,
            StepCount: int.MaxValue);

        Assert.Throws<OverflowException>(
            () => ConstantAccelerationExperiment.Run(definition));
    }

    [Test]
    public void Run_RejectsInvalidInitialStateBeforeSampling()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(double.NaN, 0d, 0d),
            Acceleration: 1d,
            DeltaTime: 0.1d,
            StepCount: 3);

        Assert.Throws<ArgumentException>(
            () => ConstantAccelerationExperiment.Run(definition));
    }

    [Test]
    public void Run_RejectsInvalidTimestep()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(0d, 0d, 0d),
            Acceleration: 1d,
            DeltaTime: double.PositiveInfinity,
            StepCount: 3);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ConstantAccelerationExperiment.Run(definition));
    }
}
