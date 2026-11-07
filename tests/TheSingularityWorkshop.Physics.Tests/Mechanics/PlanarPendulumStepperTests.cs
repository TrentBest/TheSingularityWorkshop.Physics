using NUnit.Framework;
using TheSingularityWorkshop.Physics.Mechanics;

namespace TheSingularityWorkshop.Physics.Tests.Mechanics;

[TestFixture]
public sealed class PlanarPendulumStepperTests
{
    private static readonly PlanarPendulumDefinition EarthPendulum = new(1d, 1d, 9.80665d);

    [Test]
    public void Advance_AtRestAtLowestPoint_RemainsAtRest()
    {
        var initial = new PlanarPendulumState(0d, 0d, 0, 0d);

        var next = PlanarPendulumStepper.Advance(EarthPendulum, initial, 0.01d);

        Assert.That(next.AngleRadians, Is.Zero);
        Assert.That(next.AngularVelocityRadiansPerSecond, Is.Zero);
        Assert.That(next.StepIndex, Is.EqualTo(1));
        Assert.That(next.ElapsedTimeSeconds, Is.EqualTo(0.01d));
    }

    [Test]
    public void Advance_SmallAngle_ApproachesLinearizedAcceleration()
    {
        const double angle = 1e-7d;
        const double deltaTime = 1e-4d;
        var initial = new PlanarPendulumState(angle, 0d, 0, 0d);

        var next = PlanarPendulumStepper.Advance(EarthPendulum, initial, deltaTime);
        var measuredAcceleration = next.AngularVelocityRadiansPerSecond / deltaTime;
        var linearizedAcceleration = -(9.80665d / 1d) * angle;

        Assert.That(measuredAcceleration, Is.EqualTo(linearizedAcceleration).Within(1e-12d));
    }

    [Test]
    public void Advance_LargeAngle_UsesNonlinearRestoringAcceleration()
    {
        const double angle = 1.2d;
        const double deltaTime = 1e-5d;
        var initial = new PlanarPendulumState(angle, 0d, 0, 0d);

        var next = PlanarPendulumStepper.Advance(EarthPendulum, initial, deltaTime);
        var measuredAcceleration = next.AngularVelocityRadiansPerSecond / deltaTime;
        var nonlinear = -(9.80665d / 1d) * Math.Sin(angle);
        var smallAngle = -(9.80665d / 1d) * angle;

        Assert.That(measuredAcceleration, Is.EqualTo(nonlinear).Within(1e-8d));
        Assert.That(Math.Abs(measuredAcceleration - smallAngle), Is.GreaterThan(1d));
    }

    [Test]
    public void Advance_UndampedPendulum_ApproximatelyConservesMechanicalEnergy()
    {
        var initial = new PlanarPendulumState(0.8d, 0.25d, 0, 0d);
        var initialEnergy = PlanarPendulumStepper.TotalEnergyJoules(EarthPendulum, initial);
        var state = initial;

        for (var i = 0; i < 10_000; i++)
            state = PlanarPendulumStepper.Advance(EarthPendulum, state, 0.001d);

        var finalEnergy = PlanarPendulumStepper.TotalEnergyJoules(EarthPendulum, state);

        Assert.That(finalEnergy, Is.EqualTo(initialEnergy).Within(1e-9d));
        Assert.That(state.ElapsedTimeSeconds, Is.EqualTo(10d).Within(1e-10d));
    }

    [Test]
    public void Advance_RepeatedRuns_AreDeterministic()
    {
        var first = new PlanarPendulumState(1.1d, -0.3d, 0, 0d);
        var second = first;

        for (var i = 0; i < 500; i++)
        {
            first = PlanarPendulumStepper.Advance(EarthPendulum, first, 0.002d);
            second = PlanarPendulumStepper.Advance(EarthPendulum, second, 0.002d);
        }

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Advance_InvalidDefinitionStateOrTimestep_Throws()
    {
        var validState = new PlanarPendulumState(0d, 0d, 0, 0d);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumStepper.Advance(new PlanarPendulumDefinition(0d, 1d, 9.81d), validState, 0.01d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumStepper.Advance(new PlanarPendulumDefinition(1d, double.NaN, 9.81d), validState, 0.01d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumStepper.Advance(EarthPendulum, validState, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumStepper.Advance(EarthPendulum, validState, double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() =>
            PlanarPendulumStepper.Advance(EarthPendulum, default, 0.01d));
    }

    [Test]
    public void Advance_StepIndexOverflow_Throws()
    {
        var state = new PlanarPendulumState(0d, 0d, long.MaxValue, 0d);

        Assert.Throws<OverflowException>(() =>
            PlanarPendulumStepper.Advance(EarthPendulum, state, 0.01d));
    }

    [Test]
    public void EnergyDiagnostics_InvalidState_Throw()
    {
        Assert.Throws<ArgumentException>(() =>
            PlanarPendulumStepper.TotalEnergyJoules(EarthPendulum, default));
    }
}
