using NUnit.Framework;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;
using TheSingularityWorkshop.Physics.World;

namespace TheSingularityWorkshop.Physics.Tests.World;

[TestFixture]
public sealed class DeterministicCpuWorldStepperTests
{
    [Test]
    public void Step_OneBodyGravity_UsesSemiImplicitReferenceIntegration()
    {
        var result = DeterministicCpuWorldStepper.Step(
            new CpuWorldConfiguration("gravity-v1", 0.1d),
            [Body("falling", Vector3d.Zero, Vector3d.Zero, mass: 1d, radius: 0.1d,
                acceleration: new Vector3d(0d, -9.81d, 0d))]);

        Assert.Multiple(() =>
        {
            Assert.That(result.ModelId, Is.EqualTo("gravity-v1"));
            Assert.That(result.StepIndex, Is.EqualTo(1));
            Assert.That(result.ElapsedTimeSeconds, Is.EqualTo(0.1d).Within(1e-12));
            Assert.That(result.Bodies, Has.Count.EqualTo(1));
            Assert.That(result.Bodies[0].LinearVelocity.Y, Is.EqualTo(-0.981d).Within(1e-12));
            Assert.That(result.Bodies[0].Pose.Position.Y, Is.EqualTo(-0.0981d).Within(1e-12));
            Assert.That(result.Contacts, Is.Empty);
            Assert.That(result.Limitations, Does.Contain(CpuWorldStepLimitation.DiscreteEndpointNarrowphase));
        });
    }

    [Test]
    public void Step_EqualMassSphereImpact_ResolvesNormalVelocityAndReportsFinalGeometry()
    {
        var result = DeterministicCpuWorldStepper.Step(
            new CpuWorldConfiguration("two-sphere-impact-v1", 0.5d, restitution: 0d),
            [
                Body("a", new Vector3d(-1d, 0d, 0d), new Vector3d(1d, 0d, 0d)),
                Body("b", new Vector3d(1d, 0d, 0d), new Vector3d(-1d, 0d, 0d))
            ]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Bodies[0].BodyId, Is.EqualTo("a"));
            Assert.That(result.Bodies[1].BodyId, Is.EqualTo("b"));
            Assert.That(result.Bodies[0].LinearVelocity.X, Is.EqualTo(0d).Within(1e-12));
            Assert.That(result.Bodies[1].LinearVelocity.X, Is.EqualTo(0d).Within(1e-12));
            Assert.That(result.Contacts, Has.Count.EqualTo(1));
            Assert.That(result.Contacts[0].SignedSeparation, Is.EqualTo(0d).Within(1e-12));
            Assert.That(result.Contacts[0].NormalImpulseNewtonSeconds, Is.EqualTo(1d).Within(1e-12));
            Assert.That(result.Contacts[0].NormalForceNewtons, Is.Null);
        });
    }

    [Test]
    public void Step_PenetratingPair_RecomputesContactAfterPositionCorrection()
    {
        var result = DeterministicCpuWorldStepper.Step(
            new CpuWorldConfiguration("penetration-v1", 0.2d, penetrationSlopMetres: 0d,
                correctionFraction: 1d, maximumCorrectionMetres: 1d),
            [
                Body("a", new Vector3d(-0.6d, 0d, 0d), new Vector3d(1d, 0d, 0d)),
                Body("b", new Vector3d(0.6d, 0d, 0d), new Vector3d(-1d, 0d, 0d))
            ]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Contacts, Has.Count.EqualTo(1));
            Assert.That(result.Contacts[0].SignedSeparation, Is.EqualTo(-0.04d).Within(1e-12));
            Assert.That(result.Bodies[0].Pose.Position.X, Is.EqualTo(-0.48d).Within(1e-12));
            Assert.That(result.Bodies[1].Pose.Position.X, Is.EqualTo(0.48d).Within(1e-12));
            Assert.That(result.Contacts[0].RelativeVelocity.X, Is.EqualTo(0d).Within(1e-12));
        });
    }

    [Test]
    public void Step_PermutedInputEnumeration_ProducesIdenticalOrderedResults()
    {
        var configuration = new CpuWorldConfiguration("repeat-v1", 0.01d);
        var a = Body("a", new Vector3d(-2d, 0d, 0d), Vector3d.Zero);
        var b = Body("b", new Vector3d(2d, 0d, 0d), Vector3d.Zero);
        var c = Body("c", new Vector3d(0d, 4d, 0d), Vector3d.Zero);

        var first = DeterministicCpuWorldStepper.Step(configuration, [a, b, c]);
        var second = DeterministicCpuWorldStepper.Step(configuration, [c, a, b]);

        Assert.Multiple(() =>
        {
            Assert.That(second.Bodies, Is.EqualTo(first.Bodies));
            Assert.That(second.Contacts, Is.EqualTo(first.Contacts));
            Assert.That(second.StepIndex, Is.EqualTo(first.StepIndex));
            Assert.That(second.ElapsedTimeSeconds, Is.EqualTo(first.ElapsedTimeSeconds));
        });
    }

    [Test]
    public void Step_RejectsDuplicateIdentityAndMismatchedInputClocks()
    {
        var duplicate = Body("same", Vector3d.Zero, Vector3d.Zero);
        var other = Body("same", new Vector3d(2d, 0d, 0d), Vector3d.Zero);
        var mismatchedClock = new CpuWorldBody(
            Definition("later", 1d, 0.5d, Vector3d.Zero),
            new RigidBodyState("later", new Pose3d(new Vector3d(3d, 0d, 0d), Quaterniond.Identity),
                Vector3d.Zero, Vector3d.Zero, 1, 0.1d));

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
                DeterministicCpuWorldStepper.Step(new CpuWorldConfiguration("invalid", 0.01d), [duplicate, other]));
            Assert.Throws<ArgumentException>(() =>
                DeterministicCpuWorldStepper.Step(new CpuWorldConfiguration("invalid", 0.01d),
                    [Body("first", Vector3d.Zero, Vector3d.Zero), mismatchedClock]));
        });
    }

    [Test]
    public void Step_RejectsMultiContactGraphInsteadOfApplyingOrderDependentImpulses()
    {
        var bodies = new[]
        {
            Body("centre", Vector3d.Zero, Vector3d.Zero),
            Body("left", new Vector3d(-0.8d, 0d, 0d), Vector3d.Zero),
            Body("right", new Vector3d(0.8d, 0d, 0d), Vector3d.Zero)
        };

        Assert.Throws<NotSupportedException>(() =>
            DeterministicCpuWorldStepper.Step(new CpuWorldConfiguration("multi-contact", 0.01d), bodies));
    }

    [Test]
    public void Step_RejectsInvalidMassRadiusAndTimestep()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Definition("bad-mass", 0d, 0.5d, Vector3d.Zero));
            Assert.Throws<ArgumentOutOfRangeException>(() => Definition("bad-radius", 1d, 0d, Vector3d.Zero));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CpuWorldConfiguration("bad-dt", double.NaN));
        });
    }

    [Test]
    public void Step_SeparatedSweptCandidateDoesNotInventEndpointContact()
    {
        var result = DeterministicCpuWorldStepper.Step(
            new CpuWorldConfiguration("separated-endpoints", 1d),
            [
                Body("a", new Vector3d(-2d, 0d, 0d), new Vector3d(4d, 0d, 0d)),
                Body("b", new Vector3d(2d, 0d, 0d), new Vector3d(-4d, 0d, 0d))
            ]);

        Assert.That(result.Contacts, Is.Empty);
    }

    private static CpuWorldBody Body(
        string id,
        Vector3d position,
        Vector3d velocity,
        double mass = 1d,
        double radius = 0.5d,
        Vector3d? acceleration = null)
    {
        var definition = Definition(id, mass, radius, acceleration ?? Vector3d.Zero);
        var state = new RigidBodyState(id, new Pose3d(position, Quaterniond.Identity),
            velocity, Vector3d.Zero, 0, 0d);
        return new CpuWorldBody(definition, state);
    }

    private static CpuWorldBodyDefinition Definition(
        string id, double mass, double radius, Vector3d acceleration) =>
        new(id, mass, radius, acceleration);
}
