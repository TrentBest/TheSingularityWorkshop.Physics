using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>Performs narrowphase contact confirmation for two spherical bodies.</summary>
/// <remarks>
/// This deterministic geometric test treats each sphere as centred at its body's
/// world-space origin. It reports geometry only: normal force and impulse remain
/// unavailable, and no collision response or body-state mutation is performed.
/// Coincident centres have no unique geometric normal and therefore produce no
/// observation. Contact tolerance is an explicit distance in metres.
/// </remarks>
public static class SphereSphereContactDetector
{
    /// <summary>
    /// Returns a contact observation when two spheres touch, overlap, or fall within
    /// the caller-specified contact tolerance; otherwise returns null.
    /// </summary>
    public static ContactObservation? Detect(
        string contactId,
        RigidBodyState bodyA,
        double radiusA,
        RigidBodyState bodyB,
        double radiusB,
        long stepIndex,
        double elapsedTime,
        double contactTolerance = 0d)
    {
        if (string.IsNullOrWhiteSpace(contactId))
            throw new ArgumentException("A stable contact identity is required.", nameof(contactId));
        if (!bodyA.IsValid)
            throw new ArgumentException("Body A must have a valid state.", nameof(bodyA));
        if (!bodyB.IsValid)
            throw new ArgumentException("Body B must have a valid state.", nameof(bodyB));
        if (string.Equals(bodyA.BodyId, bodyB.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("A contact requires two distinct body identities.", nameof(bodyB));
        ValidateRadius(radiusA, nameof(radiusA));
        ValidateRadius(radiusB, nameof(radiusB));
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex), "Step index must be non-negative.");
        if (!double.IsFinite(elapsedTime) || elapsedTime < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedTime), "Elapsed time must be finite and non-negative.");
        if (!double.IsFinite(contactTolerance) || contactTolerance < 0d)
            throw new ArgumentOutOfRangeException(nameof(contactTolerance), "Contact tolerance must be finite and non-negative.");

        var centreDelta = bodyB.Pose.Position - bodyA.Pose.Position;
        var centreDistance = centreDelta.Magnitude;
        if (centreDistance == 0d)
            return null;

        var signedSeparation = centreDistance - radiusA - radiusB;
        if (!double.IsFinite(signedSeparation))
            throw new OverflowException("Sphere separation exceeded the finite double range.");
        if (signedSeparation > contactTolerance)
            return null;

        var normal = centreDelta / centreDistance;
        var point = bodyA.Pose.Position + (normal * (radiusA + (signedSeparation / 2d)));
        var relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

        return new ContactObservation(
            contactId,
            bodyA.BodyId,
            bodyB.BodyId,
            point,
            normal,
            relativeVelocity,
            signedSeparation,
            isActive: true,
            stepIndex,
            elapsedTime);
    }

    private static void ValidateRadius(double radius, string parameterName)
    {
        if (!double.IsFinite(radius) || radius < 0d)
            throw new ArgumentOutOfRangeException(parameterName, "Sphere radius must be finite and non-negative.");
    }
}
