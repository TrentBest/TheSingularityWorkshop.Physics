using NUnit.Framework;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Kinematics;

[TestFixture]
public sealed class PrescribedKinematicMotionTests
{
    [Test]
    public void Evaluate_InterpolatesTranslationAtAbsoluteLogicalTime()
    {
        var definition = Definition(
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(new Vector3d(4d, 0d, 0d), Quaterniond.Identity),
            duration: 2d);

        var sample = PrescribedKinematicMotion.Evaluate(definition, 11d, 5);

        Assert.Multiple(() =>
        {
            Assert.That(sample.State.Pose.Position, Is.EqualTo(new Vector3d(2d, 0d, 0d)));
            Assert.That(sample.State.LinearVelocity, Is.EqualTo(new Vector3d(2d, 0d, 0d)));
            Assert.That(sample.State.StepIndex, Is.EqualTo(5));
            Assert.That(sample.State.ElapsedTime, Is.EqualTo(11d));
            Assert.That(sample.IsComplete, Is.False);
        });
    }

    [Test]
    public void Evaluate_ReportsCompletionOnlyAtDestinationSample()
    {
        var definition = Definition(Vector3d.Zero, new Vector3d(1d, 0d, 0d), duration: 1d);

        var before = PrescribedKinematicMotion.Evaluate(definition, 10.999d, 8);
        var arrival = PrescribedKinematicMotion.Evaluate(definition, 11d, 9);

        Assert.That(before.IsComplete, Is.False);
        Assert.That(arrival.IsComplete, Is.True);
        Assert.That(arrival.State.Pose.Position, Is.EqualTo(new Vector3d(1d, 0d, 0d)));
    }

    [Test]
    public void Evaluate_InterpolatesOrientationAlongShortestWorldRotation()
    {
        var half = Math.PI / 4d;
        var destination = new Quaterniond(Math.Cos(half), 0d, 0d, Math.Sin(half));
        var definition = Definition(
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(Vector3d.Zero, destination),
            duration: 2d,
            maxAngularSpeed: 1d);

        var sample = PrescribedKinematicMotion.Evaluate(definition, 11d, 1);
        var rotated = sample.State.Pose.TransformDirectionToWorld(new Vector3d(1d, 0d, 0d));

        Assert.Multiple(() =>
        {
            Assert.That(rotated.X, Is.EqualTo(Math.Sqrt(0.5d)).Within(1e-12));
            Assert.That(rotated.Y, Is.EqualTo(Math.Sqrt(0.5d)).Within(1e-12));
            Assert.That(sample.State.AngularVelocity.Z, Is.EqualTo(Math.PI / 4d).Within(1e-12));
        });
    }

    [Test]
    public void Definition_RejectsMotionAboveDeclaredSpeed()
    {
        Assert.Throws<ArgumentException>(() =>
            Definition(Vector3d.Zero, new Vector3d(3d, 0d, 0d), duration: 1d, maxLinearSpeed: 2d));
    }

    [Test]
    public void Evaluate_RejectsOutOfRangeTimeAndInvalidStep()
    {
        var definition = Definition(Vector3d.Zero, new Vector3d(1d, 0d, 0d), duration: 1d);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PrescribedKinematicMotion.Evaluate(definition, 9.9d, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => PrescribedKinematicMotion.Evaluate(definition, 11.1d, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => PrescribedKinematicMotion.Evaluate(definition, 10d, -1));
        });
    }

    [Test]
    public void Evaluate_IsIndependentOfSamplingCadenceAndReplayIsDeterministic()
    {
        var definition = Definition(Vector3d.Zero, new Vector3d(8d, 2d, -4d), duration: 4d);
        var coarse = PrescribedKinematicMotion.Evaluate(definition, 12d, 240);
        var fine = PrescribedKinematicMotion.Evaluate(definition, 12d, 240);
        var otherSample = PrescribedKinematicMotion.Evaluate(definition, 11d, 120);

        Assert.That(coarse, Is.EqualTo(fine));
        Assert.That(otherSample.State.Pose.Position, Is.EqualTo(new Vector3d(2d, 0.5d, -1d)));
    }

    [Test]
    public void Evaluate_ZeroDistanceIsStationaryAndCompletesAtEnd()
    {
        var pose = new Pose3d(new Vector3d(2d, 3d, 4d), Quaterniond.Identity);
        var definition = Definition(pose, pose, duration: 3d);

        var sample = PrescribedKinematicMotion.Evaluate(definition, 13d, 3);

        Assert.That(sample.State.Pose, Is.EqualTo(pose));
        Assert.That(sample.State.LinearVelocity, Is.EqualTo(Vector3d.Zero));
        Assert.That(sample.IsComplete, Is.True);
    }

    [Test]
    public void EvaluateFixedStep_DerivesLogicalTimeFromStepIdentity()
    {
        var definition = Definition(
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(new Vector3d(4d, 0d, 0d), Quaterniond.Identity),
            duration: 2d);

        var sample = PrescribedKinematicMotion.EvaluateFixedStep(
            definition, stepIndex: 1, fixedTimeStepSeconds: 1d, totalStepCount: 2);

        Assert.Multiple(() =>
        {
            Assert.That(sample.State.StepIndex, Is.EqualTo(1));
            Assert.That(sample.State.ElapsedTime, Is.EqualTo(11d));
            Assert.That(sample.State.Pose.Position.X, Is.EqualTo(2d).Within(1e-12));
        });
    }

    [Test]
    public void EvaluateFixedStep_RejectsScheduleThatDoesNotSpanDefinition()
    {
        var definition = Definition(
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(new Vector3d(4d, 0d, 0d), Quaterniond.Identity),
            duration: 2d);

        Assert.Throws<ArgumentException>(() =>
            PrescribedKinematicMotion.EvaluateFixedStep(definition, 1, 0.5d, 2));
    }

    [Test]
    public void Evaluate_ExactComputedEndTimeAlwaysProducesExactDestination()
    {
        const double startTime = 0.1d;
        const double duration = 0.2d;
        var destination = new Pose3d(new Vector3d(3d, 2d, 1d), Quaterniond.Identity);
        var definition = new PrescribedKinematicMotionDefinition(
            "motion:floating-endpoint",
            "actor:floating-endpoint",
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            destination,
            startTime,
            duration,
            maximumLinearSpeed: 100d,
            maximumAngularSpeed: 1d,
            profile: PrescribedKinematicMotionProfile.QuinticSmoothStep,
            maximumLinearAcceleration: 100d,
            maximumAngularAcceleration: 1d);

        var sample = PrescribedKinematicMotion.Evaluate(
            definition, startTime + duration, stepIndex: 20);

        Assert.Multiple(() =>
        {
            Assert.That(sample.IsComplete, Is.True);
            Assert.That(sample.State.Pose, Is.EqualTo(destination));
            Assert.That(sample.State.LinearVelocity, Is.EqualTo(Vector3d.Zero));
        });
    }

    [Test]
    public void Evaluate_QuinticSmoothStepStartsAndEndsAtRest()
    {
        var definition = new PrescribedKinematicMotionDefinition(
            "motion:smooth",
            "actor:smooth",
            new Pose3d(Vector3d.Zero, Quaterniond.Identity),
            new Pose3d(new Vector3d(2d, 0d, 0d), Quaterniond.Identity),
            10d,
            2d,
            maximumLinearSpeed: 2d,
            maximumAngularSpeed: 1d,
            profile: PrescribedKinematicMotionProfile.QuinticSmoothStep,
            maximumLinearAcceleration: 4d,
            maximumAngularAcceleration: 2d);

        var start = PrescribedKinematicMotion.Evaluate(definition, 10d, 0);
        var middle = PrescribedKinematicMotion.Evaluate(definition, 11d, 1);
        var end = PrescribedKinematicMotion.Evaluate(definition, 12d, 2);

        Assert.Multiple(() =>
        {
            Assert.That(start.State.Pose.Position, Is.EqualTo(Vector3d.Zero));
            Assert.That(start.State.LinearVelocity, Is.EqualTo(Vector3d.Zero));
            Assert.That(middle.State.Pose.Position.X, Is.EqualTo(1d).Within(1e-12));
            Assert.That(middle.State.LinearVelocity.X, Is.EqualTo(1.875d).Within(1e-12));
            Assert.That(end.State.Pose.Position.X, Is.EqualTo(2d).Within(1e-12));
            Assert.That(end.State.LinearVelocity, Is.EqualTo(Vector3d.Zero));
            Assert.That(end.IsComplete, Is.True);
        });
    }

    [Test]
    public void Definition_QuinticSmoothStepRejectsSpeedOrAccelerationLimitViolation()
    {
        var start = new Pose3d(Vector3d.Zero, Quaterniond.Identity);
        var end = new Pose3d(new Vector3d(2d, 0d, 0d), Quaterniond.Identity);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new PrescribedKinematicMotionDefinition(
                "motion:smooth-speed", "actor:smooth", start, end, 10d, 2d,
                maximumLinearSpeed: 1.5d, maximumAngularSpeed: 1d,
                profile: PrescribedKinematicMotionProfile.QuinticSmoothStep,
                maximumLinearAcceleration: 4d, maximumAngularAcceleration: 2d));
            Assert.Throws<ArgumentException>(() => new PrescribedKinematicMotionDefinition(
                "motion:smooth-acceleration", "actor:smooth", start, end, 10d, 2d,
                maximumLinearSpeed: 2d, maximumAngularSpeed: 1d,
                profile: PrescribedKinematicMotionProfile.QuinticSmoothStep,
                maximumLinearAcceleration: 2d, maximumAngularAcceleration: 2d));
        });
    }

    private static PrescribedKinematicMotionDefinition Definition(
        Vector3d start,
        Vector3d end,
        double duration,
        double maxLinearSpeed = 100d) =>
        Definition(
            new Pose3d(start, Quaterniond.Identity),
            new Pose3d(end, Quaterniond.Identity),
            duration,
            maxLinearSpeed);

    private static PrescribedKinematicMotionDefinition Definition(
        Pose3d start,
        Pose3d end,
        double duration,
        double maxLinearSpeed = 100d,
        double maxAngularSpeed = 100d) =>
        new("motion:test", "actor:test", start, end, 10d, duration, maxLinearSpeed, maxAngularSpeed);
}
