namespace TheSingularityWorkshop.Physics.Kinematics;

/// <summary>
/// Provides the analytical solution for a body sliding from rest down a straight,
/// frictionless incline through a specified vertical drop.
/// </summary>
/// <remarks>
/// This is a constrained teaching model, not a contact or rigid-body solver.
/// It excludes friction, rolling, air resistance, deformation, and changes in gravity.
/// All inputs and outputs use SI units.
/// </remarks>
public static class InclinedPlaneSolver
{
    /// <summary>
    /// Calculates the ideal sliding motion for a body released from rest.
    /// </summary>
    /// <param name="massKilograms">Body mass in kilograms; must be positive and finite.</param>
    /// <param name="angleRadians">Incline angle above horizontal in radians; must be in (0, pi/2].</param>
    /// <param name="verticalDropMeters">Vertical drop in metres; must be positive and finite.</param>
    /// <param name="gravitationalAccelerationMetersPerSecondSquared">Gravitational acceleration magnitude; must be positive and finite.</param>
    /// <returns>Analytical forces, acceleration, path length, travel time, and final speed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An input is outside its declared domain.</exception>
    /// <exception cref="OverflowException">A calculated result is outside the finite double range.</exception>
    public static InclinedPlaneResult Calculate(
        double massKilograms,
        double angleRadians,
        double verticalDropMeters,
        double gravitationalAccelerationMetersPerSecondSquared = 9.80665d)
    {
        RequirePositiveFinite(massKilograms, nameof(massKilograms));
        RequirePositiveFinite(verticalDropMeters, nameof(verticalDropMeters));
        RequirePositiveFinite(gravitationalAccelerationMetersPerSecondSquared,
            nameof(gravitationalAccelerationMetersPerSecondSquared));

        if (!double.IsFinite(angleRadians) ||
            angleRadians <= 0d ||
            angleRadians > (Math.PI / 2d))
        {
            throw new ArgumentOutOfRangeException(
                nameof(angleRadians),
                "The incline angle must be finite and greater than zero and no greater than pi/2 radians.");
        }

        var sine = Math.Sin(angleRadians);
        var cosine = Math.Cos(angleRadians);
        var acceleration = gravitationalAccelerationMetersPerSecondSquared * sine;
        var pathLength = verticalDropMeters / sine;
        var travelTime = Math.Sqrt((2d * verticalDropMeters) /
            gravitationalAccelerationMetersPerSecondSquared) / sine;
        var finalSpeed = Math.Sqrt(2d * gravitationalAccelerationMetersPerSecondSquared *
            verticalDropMeters);
        var parallelForce = massKilograms * gravitationalAccelerationMetersPerSecondSquared * sine;
        var normalForce = massKilograms * gravitationalAccelerationMetersPerSecondSquared * cosine;

        if (!double.IsFinite(acceleration) ||
            !double.IsFinite(pathLength) ||
            !double.IsFinite(travelTime) ||
            !double.IsFinite(finalSpeed) ||
            !double.IsFinite(parallelForce) ||
            !double.IsFinite(normalForce))
        {
            throw new OverflowException("The inclined-plane result is outside the finite double range.");
        }

        return new InclinedPlaneResult(
            massKilograms,
            angleRadians,
            verticalDropMeters,
            gravitationalAccelerationMetersPerSecondSquared,
            pathLength,
            acceleration,
            parallelForce,
            normalForce,
            travelTime,
            finalSpeed);
    }

    private static void RequirePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "The value must be positive and finite.");
        }
    }
}

/// <summary>
/// Analytical output for frictionless sliding from rest down a straight incline.
/// </summary>
/// <param name="MassKilograms">Body mass in kilograms.</param>
/// <param name="AngleRadians">Incline angle above horizontal in radians.</param>
/// <param name="VerticalDropMeters">Vertical drop in metres.</param>
/// <param name="GravitationalAccelerationMetersPerSecondSquared">Gravitational acceleration magnitude.</param>
/// <param name="PathLengthMeters">Distance travelled along the incline in metres.</param>
/// <param name="AccelerationMetersPerSecondSquared">Constant acceleration down the incline.</param>
/// <param name="ParallelGravityForceNewtons">Down-slope component of weight.</param>
/// <param name="NormalForceNewtons">Magnitude of the normal constraint force.</param>
/// <param name="TravelTimeSeconds">Time from rest to the bottom.</param>
/// <param name="FinalSpeedMetersPerSecond">Speed at the bottom.</param>
public readonly record struct InclinedPlaneResult(
    double MassKilograms,
    double AngleRadians,
    double VerticalDropMeters,
    double GravitationalAccelerationMetersPerSecondSquared,
    double PathLengthMeters,
    double AccelerationMetersPerSecondSquared,
    double ParallelGravityForceNewtons,
    double NormalForceNewtons,
    double TravelTimeSeconds,
    double FinalSpeedMetersPerSecond);
