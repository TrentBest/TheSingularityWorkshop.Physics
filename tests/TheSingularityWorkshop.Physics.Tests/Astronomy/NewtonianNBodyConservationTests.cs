using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class NewtonianNBodyConservationTests
{
    private const double G = 1d;

    [Test]
    public void Step_CircularBinary_KeepsEnergyAndAngularMomentumDriftBounded()
    {
        var initial = CircularBinary();
        var initialEnergy = TotalEnergy(initial);
        var initialAngularMomentum = TotalAngularMomentumZ(initial);
        var state = initial;

        // One full analytic orbital period for this equal-mass, unit-separation binary.
        var period = 4d * Math.PI;
        const int steps = 4096;
        var deltaTime = period / steps;

        for (var i = 0; i < steps; i++)
            state = NewtonianNBodyStepper.Step(state, deltaTime, G);

        var energyDrift = Math.Abs(TotalEnergy(state) - initialEnergy);
        var angularMomentumDrift = Math.Abs(TotalAngularMomentumZ(state) - initialAngularMomentum);

        Assert.That(energyDrift, Is.LessThan(2e-7),
            $"Energy drift {energyDrift:R} exceeded the bounded-drift threshold.");
        Assert.That(angularMomentumDrift, Is.LessThan(2e-10),
            $"Angular momentum drift {angularMomentumDrift:R} exceeded the bounded-drift threshold.");
    }

    [Test]
    public void Step_CircularBinary_HalvingTimestepReducesPositionErrorAtSecondOrder()
    {
        const double elapsedSeconds = 2d;
        var coarseError = PositionErrorAfter(elapsedSeconds, 0.1d);
        var mediumError = PositionErrorAfter(elapsedSeconds, 0.05d);
        var fineError = PositionErrorAfter(elapsedSeconds, 0.025d);

        Assert.That(mediumError, Is.LessThan(coarseError / 3d),
            $"Halving timestep should materially reduce error: coarse={coarseError:R}, medium={mediumError:R}.");
        Assert.That(fineError, Is.LessThan(mediumError / 3d),
            $"Halving timestep should materially reduce error again: medium={mediumError:R}, fine={fineError:R}.");
    }

    private static double PositionErrorAfter(double elapsedSeconds, double deltaTimeSeconds)
    {
        var steps = (int)Math.Round(elapsedSeconds / deltaTimeSeconds);
        Assert.That(steps * deltaTimeSeconds, Is.EqualTo(elapsedSeconds).Within(1e-14));

        var state = CircularBinary();
        for (var i = 0; i < steps; i++)
            state = NewtonianNBodyStepper.Step(state, deltaTimeSeconds, G);

        // With G=m=1 and initial separation 2, each body has radius 1 and angular speed 0.5 rad/s.
        var angle = 0.5d * elapsedSeconds;
        var expectedA = new Vector3d(-Math.Cos(angle), -Math.Sin(angle), 0d);
        var expectedB = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0d);

        return (state[0].PositionMeters - expectedA).Magnitude +
               (state[1].PositionMeters - expectedB).Magnitude;
    }

    private static double TotalEnergy(IReadOnlyList<OrbitalBodyState> bodies)
    {
        var kinetic = bodies.Sum(body =>
            0.5d * body.MassKilograms * Dot(body.VelocityMetersPerSecond, body.VelocityMetersPerSecond));

        var potential = 0d;
        for (var i = 0; i < bodies.Count; i++)
        for (var j = i + 1; j < bodies.Count; j++)
        {
            var distance = (bodies[j].PositionMeters - bodies[i].PositionMeters).Magnitude;
            potential -= G * bodies[i].MassKilograms * bodies[j].MassKilograms / distance;
        }

        return kinetic + potential;
    }

    private static double TotalAngularMomentumZ(IEnumerable<OrbitalBodyState> bodies) =>
        bodies.Sum(body => body.MassKilograms *
            (body.PositionMeters.X * body.VelocityMetersPerSecond.Y -
             body.PositionMeters.Y * body.VelocityMetersPerSecond.X));

    private static double Dot(Vector3d left, Vector3d right) =>
        left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    private static OrbitalBodyState[] CircularBinary() =>
    [
        new OrbitalBodyState("a", 1d, 0.01d, new Vector3d(-1d, 0d, 0d), new Vector3d(0d, -0.5d, 0d)),
        new OrbitalBodyState("b", 1d, 0.01d, new Vector3d(1d, 0d, 0d), new Vector3d(0d, 0.5d, 0d))
    ];
}
