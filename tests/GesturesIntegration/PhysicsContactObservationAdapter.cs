using TheSingularityWorkshop.Gestures;
using TheSingularityWorkshop.Physics.Collision;

namespace TheSingularityWorkshop.Physics.GesturesIntegration.Tests;

/// <summary>
/// Test-only translation from Physics contact evidence into Gestures' data-only
/// feedback contract. This adapter intentionally does not infer support or release.
/// </summary>
internal static class PhysicsContactObservationAdapter
{
    public static ContactObservation ToGesturesObservation(
        Physics.Collision.ContactObservation physicsObservation)
    {
        if (!physicsObservation.IsValid)
        {
            throw new ArgumentException(
                "A valid Physics contact observation is required.",
                nameof(physicsObservation));
        }

        float? normalForce = null;
        if (physicsObservation.NormalForceNewtons is double measuredForce)
        {
            if (measuredForce > float.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(physicsObservation),
                    "The measured normal force cannot be represented by Gestures' single-precision contract.");
            }

            normalForce = (float)measuredForce;
        }

        return new ContactObservation(
            physicsObservation.IsActive ? ContactPresence.Contact : ContactPresence.NoContact,
            ContactLoadOutcome.NotEvaluated,
            physicsObservation.ContactId,
            normalForceNewtons: normalForce);
    }
}
