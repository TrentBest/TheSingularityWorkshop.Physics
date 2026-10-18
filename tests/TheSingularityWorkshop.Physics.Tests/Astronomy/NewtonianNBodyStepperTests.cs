using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class NewtonianNBodyStepperTests
{
    private const double G = 1d;

    [Test]
    public void Step_TwoEqualBodiesOnCircularOrbit_PreservesCenterOfMassAndMomentum()
    {
        var bodies = CircularBinary();
        var momentumBefore = TotalMomentum(bodies);
        var centerBefore = CenterOfMass(bodies);

        var advanced = bodies;
        for (var i = 0; i < 1000; i++)
            advanced = NewtonianNBodyStepper.Step(advanced, 0.01d, G);

        var momentumAfter = TotalMomentum(advanced);
        var centerAfter = CenterOfMass(advanced);

        Assert.That(centerAfter.X, Is.EqualTo(centerBefore.X).Within(1e-12));
        Assert.That(centerAfter.Y, Is.EqualTo(centerBefore.Y).Within(1e-12));
        Assert.That(centerAfter.Z, Is.EqualTo(centerBefore.Z).Within(1e-12));
        Assert.That(momentumAfter.X, Is.EqualTo(momentumBefore.X).Within(1e-12));
        Assert.That(momentumAfter.Y, Is.EqualTo(momentumBefore.Y).Within(1e-12));
        Assert.That(momentumAfter.Z, Is.EqualTo(momentumBefore.Z).Within(1e-12));
        Assert.That(advanced[0].PositionMeters.Y, Is.GreaterThan(0d));
        Assert.That(advanced[1].PositionMeters.Y, Is.LessThan(0d));
    }


    [Test]
    public void Step_CircularBinaryReturnsToAnalyticState_WithSecondOrderPositionConvergence()
    {
        // For equal unit masses at separation 2 with G=1, each body orbits at radius 1.
        // The exact angular velocity is 0.5 rad/s, so the period is 4*pi seconds.
        const double period = 4d * Math.PI;
        var coarseError = CircularOrbitPositionError(128, period);
        var mediumError = CircularOrbitPositionError(256, period);
        var fineError = CircularOrbitPositionError(512, period);

        Assert.That(mediumError, Is.LessThan(coarseError),
            "Halving the timestep should reduce the full-period trajectory error.");
        Assert.That(fineError, Is.LessThan(mediumError),
            "Halving the timestep again should further reduce the full-period trajectory error.");
        Assert.That(coarseError / mediumError, Is.GreaterThan(3d),
            "Velocity Verlet should approach second-order global position convergence in this smooth orbit.");
        Assert.That(mediumError / fineError, Is.GreaterThan(3d),
            "The finer refinement should retain approximately second-order convergence.");
    }

    private static double CircularOrbitPositionError(int steps, double period)
    {
        var initial = CircularBinary();
        var advanced = initial;
        var deltaTime = period / steps;
        for (var i = 0; i < steps; i++)
            advanced = NewtonianNBodyStepper.Step(advanced, deltaTime, G);

        return advanced.Zip(initial, (actual, expected) =>
                (actual.PositionMeters - expected.PositionMeters).Magnitude)
            .Max();
    }

    [Test]
    public void Step_IsDeterministicAndIndependentOfInputEnumerationOrder()
    {
        var bodies = CircularBinary();
        var forward = NewtonianNBodyStepper.Step(bodies, 0.01d, G);
        var reverse = NewtonianNBodyStepper.Step(bodies.Reverse(), 0.01d, G);

        Assert.That(forward, Is.EqualTo(reverse));
    }

    [Test]
    public void Step_RejectsInvalidTimestepDuplicateIdsAndCoincidentBodies()
    {
        var body = new OrbitalBodyState("body", 1d, 0.1d, Vector3d.Zero, Vector3d.Zero);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NewtonianNBodyStepper.Step([body], 0d, G));
        Assert.Throws<ArgumentException>(() =>
            NewtonianNBodyStepper.Step(
                [body, body with { PositionMeters = new Vector3d(1d, 0d, 0d) }],
                0.01d,
                G));
        Assert.Throws<ArgumentException>(() =>
            NewtonianNBodyStepper.Step(
                [body, body with { BodyId = "other" }],
                0.01d,
                G));
    }

    [Test]
    public void Step_EmptyStateRemainsEmpty()
    {
        Assert.That(NewtonianNBodyStepper.Step([], 0.01d, G), Is.Empty);
    }

    private static OrbitalBodyState[] CircularBinary() =>
    [
        new OrbitalBodyState("a", 1d, 0.01d, new Vector3d(-1d, 0d, 0d), new Vector3d(0d, -0.5d, 0d)),
        new OrbitalBodyState("b", 1d, 0.01d, new Vector3d(1d, 0d, 0d), new Vector3d(0d, 0.5d, 0d))
    ];

    private static Vector3d TotalMomentum(IEnumerable<OrbitalBodyState> bodies) =>
        bodies.Aggregate(Vector3d.Zero, (total, body) =>
            total + body.VelocityMetersPerSecond * body.MassKilograms);

    private static Vector3d CenterOfMass(IReadOnlyCollection<OrbitalBodyState> bodies)
    {
        var totalMass = bodies.Sum(body => body.MassKilograms);
        return bodies.Aggregate(Vector3d.Zero, (weighted, body) =>
            weighted + body.PositionMeters * body.MassKilograms) / totalMass;
    }
}
