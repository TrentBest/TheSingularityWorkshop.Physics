using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>Applies a frictionless, translational normal impulse to one active contact.</summary>
/// <remarks>
/// This bounded response model handles only linear velocity. It treats the supplied
/// contact normal as the response direction, ignores angular inertia and friction,
/// and does not correct positional penetration. Restitution is in [0, 1]. A positive
/// gap accepted only by detector tolerance does not receive an impulse.
/// </remarks>
public static class ContactImpulseResolver
{
    /// <summary>Resolved contact and the body snapshots after the instantaneous velocity change.</summary>
    public sealed record Result(
        ContactObservation Contact,
        RigidBodyState BodyA,
        RigidBodyState BodyB);

    /// <summary>
    /// Resolves the normal velocity of a contact using an instantaneous impulse.
    /// The returned snapshots use the contact's logical step/time; no physical time
    /// is advanced by this operation.
    /// </summary>
    public static Result Resolve(
        ContactObservation contact,
        RigidBodyState bodyA,
        double massA,
        RigidBodyState bodyB,
        double massB,
        double restitution = 0d)
    {
        if (!contact.IsValid)
            throw new ArgumentException("A valid contact observation is required.", nameof(contact));
        if (!contact.IsActive)
            throw new ArgumentException("Only an active contact can be resolved.", nameof(contact));
        ValidateBody(bodyA, massA, nameof(bodyA), nameof(massA));
        ValidateBody(bodyB, massB, nameof(bodyB), nameof(massB));
        if (!string.Equals(contact.BodyAId, bodyA.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("Body A does not match the contact identity.", nameof(bodyA));
        if (!string.Equals(contact.BodyBId, bodyB.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("Body B does not match the contact identity.", nameof(bodyB));
        if (!double.IsFinite(restitution) || restitution < 0d || restitution > 1d)
            throw new ArgumentOutOfRangeException(nameof(restitution), "Restitution must be finite and within [0, 1].");

        var inverseMassA = 1d / massA;
        var inverseMassB = 1d / massB;
        var inverseMassSum = inverseMassA + inverseMassB;
        if (!double.IsFinite(inverseMassSum) || inverseMassSum <= 0d)
            throw new OverflowException("The combined inverse mass is not finite and positive.");

        var relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;
        var normalSpeed = relativeVelocity.Dot(contact.NormalFromAToB);
        var impulseMagnitude = 0d;

        // Never pull bodies together, and do not resolve a positive gap merely
        // because the detector elected to report it within its contact tolerance.
        if (contact.SignedSeparation <= 0d && normalSpeed < 0d)
        {
            impulseMagnitude = -((1d + restitution) * normalSpeed) / inverseMassSum;
            if (!double.IsFinite(impulseMagnitude) || impulseMagnitude < 0d)
                throw new OverflowException("The normal contact impulse is not finite and non-negative.");
        }

        var impulse = contact.NormalFromAToB * impulseMagnitude;
        var velocityA = bodyA.LinearVelocity - (impulse * inverseMassA);
        var velocityB = bodyB.LinearVelocity + (impulse * inverseMassB);
        var achievedRelativeVelocity = velocityB - velocityA;
        if (!IsFinite(velocityA) || !IsFinite(velocityB) || !IsFinite(achievedRelativeVelocity))
            throw new OverflowException("Contact response produced a non-finite body or relative velocity.");

        var achievedA = new RigidBodyState(
            bodyA.BodyId, bodyA.Pose, velocityA, bodyA.AngularVelocity,
            contact.StepIndex, contact.ElapsedTime);
        var achievedB = new RigidBodyState(
            bodyB.BodyId, bodyB.Pose, velocityB, bodyB.AngularVelocity,
            contact.StepIndex, contact.ElapsedTime);
        var resolvedContact = new ContactObservation(
            contact.ContactId, contact.BodyAId, contact.BodyBId,
            contact.Point, contact.NormalFromAToB, achievedRelativeVelocity,
            contact.SignedSeparation, contact.IsActive, contact.StepIndex,
            contact.ElapsedTime, contact.NormalForceNewtons, impulseMagnitude);

        return new Result(resolvedContact, achievedA, achievedB);
    }

    private static void ValidateBody(RigidBodyState body, double mass, string bodyParameter, string massParameter)
    {
        if (!body.IsValid)
            throw new ArgumentException("A valid body state is required.", bodyParameter);
        if (!IsFinite(body.LinearVelocity) || !IsFinite(body.AngularVelocity))
            throw new ArgumentException("Body velocities must be finite.", bodyParameter);
        if (!double.IsFinite(mass) || mass <= 0d)
            throw new ArgumentOutOfRangeException(massParameter, "Mass must be positive and finite.");
    }

    private static bool IsFinite(Vector3d value) =>
        double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
}
