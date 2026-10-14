using TheSingularityWorkshop.Physics.Kinematics;
using NUnit.Framework;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ConstantAccelerationSolverTests
{
    [Test]
    public void Advance_ConstantVelocity_MatchesAnalyticalPosition()
    {
        var initial = new KinematicState(Position: 2d, Velocity: 3d, ElapsedTime: 0d);

        var result = ConstantAccelerationSolver.Advance(initial, acceleration: 0d, deltaTime: 4d);

        Assert.That(result.Position, Is.EqualTo(14d));
        Assert.That(result.Velocity, Is.EqualTo(3d));
        Assert.That(result.ElapsedTime, Is.EqualTo(4d));
    }

    [Test]
    public void Advance_ConstantAcceleration_MatchesAnalyticalSolution()
    {
        var initial = new KinematicState(Position: 0d, Velocity: 0d, ElapsedTime: 0d);

        var result = ConstantAccelerationSolver.Advance(initial, acceleration: 9.81d, deltaTime: 2d);

        Assert.That(result.Position, Is.EqualTo(19.62d).Within(1e-12));
        Assert.That(result.Velocity, Is.EqualTo(19.62d).Within(1e-12));
        Assert.That(result.ElapsedTime, Is.EqualTo(2d));
    }

    [Test]
    public void Advance_EquivalentFixedSteps_MatchesOneAnalyticalStep()
    {
        var initial = new KinematicState(Position: 1.25d, Velocity: -2d, ElapsedTime: 0d);
        var stepped = initial;

        for (var i = 0; i < 100; i++)
        {
            stepped = ConstantAccelerationSolver.Advance(stepped, acceleration: 3.5d, deltaTime: 0.01d);
        }

        var single = ConstantAccelerationSolver.Advance(initial, acceleration: 3.5d, deltaTime: 1d);

        Assert.That(stepped.Position, Is.EqualTo(single.Position).Within(1e-12));
        Assert.That(stepped.Velocity, Is.EqualTo(single.Velocity).Within(1e-12));
        Assert.That(stepped.ElapsedTime, Is.EqualTo(1d).Within(1e-12));
    }

    [Test]
    public void Advance_RejectsNonPositiveTimestep()
    {
        var initial = new KinematicState(0d, 0d, 0d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ConstantAccelerationSolver.Advance(initial, 1d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ConstantAccelerationSolver.Advance(initial, 1d, -0.1d));
    }

    [Test]
    public void Advance_RejectsInvalidInitialState()
    {
        var invalid = new KinematicState(double.NaN, 0d, 0d);

        Assert.Throws<ArgumentException>(
            () => ConstantAccelerationSolver.Advance(invalid, 1d, 0.1d));
    }

    [Test]
    public void Advance_RejectsNonFiniteAcceleration()
    {
        var initial = new KinematicState(0d, 0d, 0d);

        Assert.Throws<ArgumentException>(
            () => ConstantAccelerationSolver.Advance(initial, double.PositiveInfinity, 0.1d));
    }

    [Test]
    public void Advance_ThrowsWhenCalculationOverflows()
    {
        var initial = new KinematicState(0d, double.MaxValue, 0d);

        Assert.Throws<OverflowException>(
            () => ConstantAccelerationSolver.Advance(initial, 0d, 2d));
    }
}
