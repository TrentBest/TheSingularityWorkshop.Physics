using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>Applies bounded inverse-mass-weighted positional correction to one penetrating contact.</summary>
/// <remarks>
/// This is a positional stabilization step, not a force or impulse. It translates body
/// origins along the observed contact normal, preserves orientation and velocities, and
/// caps the total separation correction. It does not advance time, correct angular
/// penetration, or solve coupled contacts.
/// </remarks>
public static class ContactPositionCorrector
{
    /// <summary>Corrected contact and immutable body snapshots after positional stabilization.</summary>
    public sealed record Result(
        ContactObservation Contact,
        RigidBodyState BodyA,
        RigidBodyState BodyB,
        double AppliedCorrectionMetres);

    /// <summary>
    /// Moves the bodies apart by a bounded fraction of penetration beyond the allowed
    /// slop, weighted by inverse mass. The supplied contact geometry is not recomputed;
    /// callers should run narrowphase detection again after correction.
    /// </summary>
    /// <param name="contact">Active contact with signed separation in metres.</param>
    /// <param name="bodyA">Current body A snapshot.</param>
    /// <param name="massA">Positive body A mass in kilograms.</param>
    /// <param name="bodyB">Current body B snapshot.</param>
    /// <param name="massB">Positive body B mass in kilograms.</param>
    /// <param name="penetrationSlopMetres">Penetration depth tolerated without correction.</param>
    /// <param name="correctionFraction">Fraction of excess penetration to correct, in (0, 1].</param>
    /// <param name="maximumCorrectionMetres">Maximum total separation correction in metres.</param>
    public static Result Correct(
        ContactObservation contact,
        RigidBodyState bodyA,
        double massA,
        RigidBodyState bodyB,
        double massB,
        double penetrationSlopMetres = 0.001d,
        double correctionFraction = 0.8d,
        double maximumCorrectionMetres = 0.2d)
    {
        if (!contact.IsValid)
            throw new ArgumentException("A valid contact observation is required.", nameof(contact));
        if (!contact.IsActive)
            throw new ArgumentException("Only an active contact can be corrected.", nameof(contact));
        ValidateBody(bodyA, massA, nameof(bodyA), nameof(massA));
        ValidateBody(bodyB, massB, nameof(bodyB), nameof(massB));
        if (!string.Equals(contact.BodyAId, bodyA.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("Body A does not match the contact identity.", nameof(bodyA));
        if (!string.Equals(contact.BodyBId, bodyB.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("Body B does not match the contact identity.", nameof(bodyB));
        if (!double.IsFinite(penetrationSlopMetres) || penetrationSlopMetres < 0d)
            throw new ArgumentOutOfRangeException(nameof(penetrationSlopMetres), "Penetration slop must be finite and non-negative.");
        if (!double.IsFinite(correctionFraction) || correctionFraction <= 0d || correctionFraction > 1d)
            throw new ArgumentOutOfRangeException(nameof(correctionFraction), "Correction fraction must be finite and in (0, 1].");
        if (!double.IsFinite(maximumCorrectionMetres) || maximumCorrectionMetres <= 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumCorrectionMetres), "Maximum correction must be finite and positive.");

        var penetrationBeyondSlop = Math.Max(0d, -contact.SignedSeparation - penetrationSlopMetres);
        var requestedCorrection = penetrationBeyondSlop * correctionFraction;
        if (!double.IsFinite(requestedCorrection))
            throw new OverflowException("Requested positional correction is not finite.");
        var correction = Math.Min(requestedCorrection, maximumCorrectionMetres);

        var inverseMassA = 1d / massA;
        var inverseMassB = 1d / massB;
        var inverseMassSum = inverseMassA + inverseMassB;
        if (!double.IsFinite(inverseMassSum) || inverseMassSum <= 0d)
            throw new OverflowException("The combined inverse mass is not finite and positive.");

        var displacementA = contact.NormalFromAToB * (-correction * inverseMassA / inverseMassSum);
        var displacementB = contact.NormalFromAToB * (correction * inverseMassB / inverseMassSum);
        var positionA = bodyA.Pose.Position + displacementA;
        var positionB = bodyB.Pose.Position + displacementB;
        if (!IsFinite(positionA) || !IsFinite(positionB))
            throw new OverflowException("Positional correction produced a non-finite body position.");

        var achievedA = new RigidBodyState(
            bodyA.BodyId, new Pose3d(positionA, bodyA.Pose.Orientation),
            bodyA.LinearVelocity, bodyA.AngularVelocity, contact.StepIndex, contact.ElapsedTime);
        var achievedB = new RigidBodyState(
            bodyB.BodyId, new Pose3d(positionB, bodyB.Pose.Orientation),
            bodyB.LinearVelocity, bodyB.AngularVelocity, contact.StepIndex, contact.ElapsedTime);

        var correctedContact = new ContactObservation(
            contact.ContactId, contact.BodyAId, contact.BodyBId,
            contact.Point, contact.NormalFromAToB, contact.RelativeVelocity,
            contact.SignedSeparation + correction, contact.IsActive,
            contact.StepIndex, contact.ElapsedTime,
            contact.NormalForceNewtons, contact.NormalImpulseNewtonSeconds);

        return new Result(correctedContact, achievedA, achievedB, correction);
    }

    private static void ValidateBody(RigidBodyState body, double mass, string bodyParameter, string massParameter)
    {
        if (!body.IsValid)
            throw new ArgumentException("A valid body state is required.", bodyParameter);
        if (!double.IsFinite(mass) || mass <= 0d)
            throw new ArgumentOutOfRangeException(massParameter, "Mass must be positive and finite.");
    }

    private static bool IsFinite(Vector3d value) =>
        double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
}
