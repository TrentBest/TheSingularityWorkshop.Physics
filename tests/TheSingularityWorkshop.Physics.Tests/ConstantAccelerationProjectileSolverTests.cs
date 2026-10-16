using NUnit.Framework;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ConstantAccelerationProjectileSolverTests
{
    [Test]
    public void Advance_ThreeDimensionalLaunch_MatchesAnalyticalSolution()
    {
        var initial = new ProjectileState(
            new Vector3d(1d, 2d, 3d),
            new Vector3d(4d, -2d, 10d),
            0d);
        var gravity = new Vector3d(0d, 0d, -9.81d);

        var result = ConstantAccelerationProjectileSolver.Advance(initial, gravity, 2d);

        Assert.That(result.Position.X, Is.EqualTo(9d).Within(1e-12));
        Assert.That(result.Position.Y, Is.EqualTo(-2d).Within(1e-12));
        Assert.That(result.Position.Z, Is.EqualTo(3.38d).Within(1e-12));
        Assert.That(result.Velocity.X, Is.EqualTo(4d).Within(1e-12));
        Assert.That(result.Velocity.Y, Is.EqualTo(-2d).Within(1e-12));
        Assert.That(result.Velocity.Z, Is.EqualTo(-9.62d).Within(1e-12));
        Assert.That(result.ElapsedTime, Is.EqualTo(2d));
    }

    [Test]
    public void Advance_RepeatedFixedSteps_MatchesSingleAnalyticalStep()
    {
        var initial = new ProjectileState(
            new Vector3d(-3d, 7d, 2d),
            new Vector3d(12d, -4d, 18d),
            0d);
        var acceleration = new Vector3d(0d, 0d, -9.81d);
        var stepped = initial;

        for (var i = 0; i < 100; i++)
            stepped = ConstantAccelerationProjectileSolver.Advance(stepped, acceleration, 0.01d);

        var single = ConstantAccelerationProjectileSolver.Advance(initial, acceleration, 1d);

        Assert.That(stepped.Position.X, Is.EqualTo(single.Position.X).Within(1e-12));
        Assert.That(stepped.Position.Y, Is.EqualTo(single.Position.Y).Within(1e-12));
        Assert.That(stepped.Position.Z, Is.EqualTo(single.Position.Z).Within(1e-12));
        Assert.That(stepped.Velocity.Z, Is.EqualTo(single.Velocity.Z).Within(1e-12));
        Assert.That(stepped.ElapsedTime, Is.EqualTo(1d).Within(1e-12));
    }

    [Test]
    public void Advance_RejectsInvalidStateAndTimestep()
    {
        var invalidState = new ProjectileState(Vector3d.Zero, Vector3d.Zero, double.NaN);
        Assert.Throws<ArgumentException>(() =>
            ConstantAccelerationProjectileSolver.Advance(invalidState, Vector3d.Zero, 0.1d));

        var initial = new ProjectileState(Vector3d.Zero, Vector3d.Zero, 0d);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConstantAccelerationProjectileSolver.Advance(initial, Vector3d.Zero, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConstantAccelerationProjectileSolver.Advance(initial, Vector3d.Zero, double.PositiveInfinity));
    }

    [Test]
    public void Advance_ThrowsWhenElapsedTimeOverflows()
    {
        var initial = new ProjectileState(Vector3d.Zero, Vector3d.Zero, double.MaxValue);
        Assert.Throws<OverflowException>(() =>
            ConstantAccelerationProjectileSolver.Advance(initial, Vector3d.Zero, double.MaxValue));
    }
}
