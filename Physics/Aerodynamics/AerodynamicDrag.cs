using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Aerodynamics;

/// <summary>
/// Computes the aerodynamic drag force on a body using a constant-coefficient,
/// low-speed drag model.
/// </summary>
/// <remarks>
/// Velocities are expressed in the same frame and in metres per second; air
/// density is in kg/m³; reference area is in m²; the returned force is in
/// newtons. The drag coefficient and reference area are supplied by the caller
/// and are not inferred from body geometry or flow regime.
/// This model does not calculate lift, moments, wakes, compressibility, or
/// changes to the drag coefficient with speed.
/// </remarks>
public static class AerodynamicDrag
{
    /// <summary>
    /// Calculates drag opposite the body's velocity relative to the air.
    /// </summary>
    /// <param name="bodyVelocity">Body velocity in the shared reference frame (m/s).</param>
    /// <param name="windVelocity">Air velocity in the same frame (m/s).</param>
    /// <param name="airDensity">Air density (kg/m³), finite and non-negative.</param>
    /// <param name="dragCoefficient">Dimensionless drag coefficient, finite and non-negative.</param>
    /// <param name="referenceArea">Reference area (m²), finite and non-negative.</param>
    /// <returns>The drag force vector in newtons.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A scalar input is negative or non-finite.</exception>
    /// <exception cref="ArithmeticException">The calculation overflows the finite numeric range.</exception>
    public static Vector3d CalculateForce(
        Vector3d bodyVelocity,
        Vector3d windVelocity,
        double airDensity,
        double dragCoefficient,
        double referenceArea)
    {
        ValidateNonNegativeFinite(airDensity, nameof(airDensity));
        ValidateNonNegativeFinite(dragCoefficient, nameof(dragCoefficient));
        ValidateNonNegativeFinite(referenceArea, nameof(referenceArea));

        var relativeAirVelocity = bodyVelocity - windVelocity;
        var speed = relativeAirVelocity.Magnitude;

        if (speed == 0d || airDensity == 0d || dragCoefficient == 0d || referenceArea == 0d)
            return Vector3d.Zero;

        // F_drag = -1/2 * rho * |v_rel| * Cd * A * v_rel.
        // Keeping the relative velocity in the expression avoids a separate
        // unit-vector normalization and makes the quadratic speed dependence explicit.
        var scale = -0.5d * airDensity * speed * dragCoefficient * referenceArea;
        if (!double.IsFinite(scale))
            throw new ArithmeticException("Aerodynamic drag calculation produced a non-finite scale.");

        return relativeAirVelocity * scale;
    }

    private static void ValidateNonNegativeFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be finite and non-negative.");
        }
    }
}
