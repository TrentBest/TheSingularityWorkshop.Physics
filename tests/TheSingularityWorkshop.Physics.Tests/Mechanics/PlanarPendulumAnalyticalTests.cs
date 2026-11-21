using NUnit.Framework;
using TheSingularityWorkshop.Physics.Mechanics;

namespace TheSingularityWorkshop.Physics.Tests.Mechanics;

[TestFixture]
public sealed class PlanarPendulumAnalyticalTests
{
    private static readonly PlanarPendulumDefinition EarthPendulum = new(1d, 1d, 9.80665d);

    [Test]
    public void NaturalFrequencyAndPeriod_MatchSmallAngleEquations()
    {
        var frequency = PlanarPendulumAnalytical.NaturalAngularFrequencyRadiansPerSecond(EarthPendulum);
        var period = PlanarPendulumAnalytical.SmallAnglePeriodSeconds(EarthPendulum);

        Assert.That(frequency, Is.EqualTo(Math.Sqrt(9.80665d)).Within(1e-14d));
        Assert.That(period, Is.EqualTo(2d * Math.PI * Math.Sqrt(1d / 9.80665d)).Within(1e-14d));
        Assert.That(frequency * period, Is.EqualTo(2d * Math.PI).Within(1e-14d));
    }

    [Test]
    public void Evaluate_AtInitialTime_ReturnsInitialConditions()
    {
        var state = PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0.3d, -0.2d, 0d);

        Assert.That(state.AngleRadians, Is.EqualTo(0.3d));
        Assert.That(state.AngularVelocityRadiansPerSecond, Is.EqualTo(-0.2d));
    }

    [Test]
    public void Evaluate_AtQuarterPeriod_MapsInitialDisplacementToVelocity()
    {
        var period = PlanarPendulumAnalytical.SmallAnglePeriodSeconds(EarthPendulum);
        var frequency = PlanarPendulumAnalytical.NaturalAngularFrequencyRadiansPerSecond(EarthPendulum);

        var state = PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0.4d, 0d, period / 4d);

        Assert.That(state.AngleRadians, Is.EqualTo(0d).Within(1e-14d));
        Assert.That(state.AngularVelocityRadiansPerSecond, Is.EqualTo(-0.4d * frequency).Within(1e-14d));
    }

    [Test]
    public void Evaluate_ZeroInitialConditions_RemainsAtEquilibrium()
    {
        var state = PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0d, 0d, 100d);

        Assert.That(state.AngleRadians, Is.Zero);
        Assert.That(state.AngularVelocityRadiansPerSecond, Is.Zero);
    }

    [Test]
    public void Evaluate_NumericalRk4ConvergesTowardSmallAngleReferenceAsStepShrinks()
    {
        const double initialAngle = 1e-4d;
        const double duration = 2d;
        var expected = PlanarPendulumAnalytical.Evaluate(EarthPendulum, initialAngle, 0d, duration);

        var coarse = Integrate(initialAngle, 0.02d, duration);
        var fine = Integrate(initialAngle, 0.01d, duration);
        var coarseError = StateError(coarse, expected);
        var fineError = StateError(fine, expected);

        Assert.That(fineError, Is.LessThan(coarseError));
        Assert.That(fineError, Is.LessThan(1e-11d));
    }

    [Test]
    public void Evaluate_NonlinearModelApproachesLinearReferenceAsAmplitudeShrinks()
    {
        const double duration = 1.3d;
        var referenceAtLargeAmplitude = PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0.1d, 0d, duration);
        var referenceAtSmallAmplitude = PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0.01d, 0d, duration);
        var nonlinearAtLargeAmplitude = Integrate(0.1d, 0.0005d, duration);
        var nonlinearAtSmallAmplitude = Integrate(0.01d, 0.0005d, duration);

        var largeError = StateError(nonlinearAtLargeAmplitude, referenceAtLargeAmplitude);
        var smallError = StateError(nonlinearAtSmallAmplitude, referenceAtSmallAmplitude);

        Assert.That(smallError, Is.LessThan(largeError));
    }

    [Test]
    public void Evaluate_InvalidInputs_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumAnalytical.NaturalAngularFrequencyRadiansPerSecond(
                new PlanarPendulumDefinition(1d, 0d, 9.81d)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumAnalytical.Evaluate(EarthPendulum, double.NaN, 0d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0d, double.PositiveInfinity, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0d, 0d, -1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarPendulumAnalytical.Evaluate(EarthPendulum, 0d, 0d, double.NaN));
    }

    [Test]
    public void Evaluate_NonFiniteDerivedPhase_Throws()
    {
        Assert.Throws<OverflowException>(() =>
            PlanarPendulumAnalytical.Evaluate(
                new PlanarPendulumDefinition(1d, 1e-300d, 9.81d),
                0d,
                0d,
                double.MaxValue));
    }

    private static PlanarPendulumState Integrate(double initialAngle, double deltaTime, double duration)
    {
        var steps = (int)Math.Round(duration / deltaTime);
        Assert.That(steps * deltaTime, Is.EqualTo(duration).Within(1e-12d));

        var state = new PlanarPendulumState(initialAngle, 0d, 0, 0d);
        for (var i = 0; i < steps; i++)
            state = PlanarPendulumStepper.Advance(EarthPendulum, state, deltaTime);

        return state;
    }

    private static double StateError(
        PlanarPendulumState numerical,
        PlanarPendulumAnalyticalState analytical)
    {
        return Math.Sqrt(
            Math.Pow(numerical.AngleRadians - analytical.AngleRadians, 2d) +
            Math.Pow(numerical.AngularVelocityRadiansPerSecond -
                analytical.AngularVelocityRadiansPerSecond, 2d));
    }
}
