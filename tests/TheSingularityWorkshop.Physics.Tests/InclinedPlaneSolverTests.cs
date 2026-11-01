using NUnit.Framework;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class InclinedPlaneSolverTests
{
    private const double Gravity = 9.80665d;

    [Test]
    public void Calculate_MatchesAnalyticalSlidingSolution()
    {
        var result = InclinedPlaneSolver.Calculate(
            massKilograms: 2d,
            angleRadians: Math.PI / 6d,
            verticalDropMeters: 1.5d,
            gravitationalAccelerationMetersPerSecondSquared: Gravity);

        Assert.That(result.PathLengthMeters, Is.EqualTo(3d).Within(1e-12));
        Assert.That(result.AccelerationMetersPerSecondSquared, Is.EqualTo(Gravity * 0.5d).Within(1e-12));
        Assert.That(result.ParallelGravityForceNewtons, Is.EqualTo(Gravity).Within(1e-12));
        Assert.That(result.NormalForceNewtons, Is.EqualTo(2d * Gravity * Math.Cos(Math.PI / 6d)).Within(1e-12));
        Assert.That(result.TravelTimeSeconds,
            Is.EqualTo(Math.Sqrt(2d * 1.5d / Gravity) / 0.5d).Within(1e-12));
        Assert.That(result.FinalSpeedMetersPerSecond,
            Is.EqualTo(Math.Sqrt(2d * Gravity * 1.5d)).Within(1e-12));
    }

    [Test]
    public void Calculate_SameVerticalDropHasSameFinalSpeedAtDifferentAngles()
    {
        var shallow = InclinedPlaneSolver.Calculate(1d, Math.PI / 12d, 2d, Gravity);
        var steep = InclinedPlaneSolver.Calculate(1d, Math.PI / 3d, 2d, Gravity);

        Assert.That(shallow.FinalSpeedMetersPerSecond,
            Is.EqualTo(steep.FinalSpeedMetersPerSecond).Within(1e-12));
        Assert.That(shallow.AccelerationMetersPerSecondSquared,
            Is.LessThan(steep.AccelerationMetersPerSecondSquared));
        Assert.That(shallow.PathLengthMeters, Is.GreaterThan(steep.PathLengthMeters));
        Assert.That(shallow.TravelTimeSeconds, Is.GreaterThan(steep.TravelTimeSeconds));
    }

    [Test]
    public void Calculate_MassChangesForcesButNotIdealMotion()
    {
        var light = InclinedPlaneSolver.Calculate(1d, Math.PI / 4d, 2d, Gravity);
        var heavy = InclinedPlaneSolver.Calculate(10d, Math.PI / 4d, 2d, Gravity);

        Assert.That(heavy.ParallelGravityForceNewtons,
            Is.EqualTo(light.ParallelGravityForceNewtons * 10d).Within(1e-12));
        Assert.That(heavy.NormalForceNewtons,
            Is.EqualTo(light.NormalForceNewtons * 10d).Within(1e-12));
        Assert.That(heavy.AccelerationMetersPerSecondSquared,
            Is.EqualTo(light.AccelerationMetersPerSecondSquared).Within(1e-12));
        Assert.That(heavy.TravelTimeSeconds,
            Is.EqualTo(light.TravelTimeSeconds).Within(1e-12));
        Assert.That(heavy.FinalSpeedMetersPerSecond,
            Is.EqualTo(light.FinalSpeedMetersPerSecond).Within(1e-12));
    }

    [Test]
    public void Calculate_VerticalInclineHasZeroNormalForce()
    {
        var result = InclinedPlaneSolver.Calculate(3d, Math.PI / 2d, 1d, Gravity);

        Assert.That(result.NormalForceNewtons, Is.EqualTo(0d).Within(1e-12));
        Assert.That(result.AccelerationMetersPerSecondSquared, Is.EqualTo(Gravity).Within(1e-12));
        Assert.That(result.PathLengthMeters, Is.EqualTo(1d).Within(1e-12));
    }

    [TestCase(0d, Math.PI / 4d, 1d, Gravity)]
    [TestCase(-1d, Math.PI / 4d, 1d, Gravity)]
    [TestCase(double.NaN, Math.PI / 4d, 1d, Gravity)]
    [TestCase(1d, 0d, 1d, Gravity)]
    [TestCase(1d, -0.1d, 1d, Gravity)]
    [TestCase(1d, Math.PI, 1d, Gravity)]
    [TestCase(1d, double.NaN, 1d, Gravity)]
    [TestCase(1d, Math.PI / 4d, 0d, Gravity)]
    [TestCase(1d, Math.PI / 4d, 1d, 0d)]
    public void Calculate_RejectsInputsOutsideDeclaredDomain(
        double mass,
        double angle,
        double drop,
        double gravity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InclinedPlaneSolver.Calculate(mass, angle, drop, gravity));
    }
}
