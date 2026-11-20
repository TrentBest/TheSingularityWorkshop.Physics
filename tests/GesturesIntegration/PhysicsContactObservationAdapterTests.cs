using NUnit.Framework;
using TheSingularityWorkshop.Gestures;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.GesturesIntegration.Tests;

[TestFixture]
public sealed class PhysicsContactObservationAdapterTests
{
    [Test]
    public void ActiveContactPreservesIdentityAndRepresentableNormalForceWithoutClaimingSupport()
    {
        var physics = CreateObservation(
            isActive: true,
            normalForceNewtons: 12.5d,
            normalImpulseNewtonSeconds: 0.75d);

        var gestures = PhysicsContactObservationAdapter.ToGesturesObservation(physics);

        Assert.Multiple(() =>
        {
            Assert.That(gestures.Presence, Is.EqualTo(ContactPresence.Contact));
            Assert.That(gestures.LoadOutcome, Is.EqualTo(ContactLoadOutcome.NotEvaluated));
            Assert.That(gestures.ContactId, Is.EqualTo("contact-17"));
            Assert.That(gestures.NormalForceNewtons, Is.EqualTo(12.5f));
            Assert.That(gestures.TangentialLoadNewtons, Is.Null);
            Assert.That(gestures.AvailableTangentialForceNewtons, Is.Null);
            Assert.That(ContactRecoveryPolicy.Recommend(gestures),
                Is.EqualTo(ContactRecoveryAction.AwaitObservation));
        });
    }

    [Test]
    public void InactiveContactMapsToNoContactRatherThanInventingReleaseHistory()
    {
        var physics = CreateObservation(isActive: false);

        var gestures = PhysicsContactObservationAdapter.ToGesturesObservation(physics);

        Assert.Multiple(() =>
        {
            Assert.That(gestures.Presence, Is.EqualTo(ContactPresence.NoContact));
            Assert.That(gestures.LoadOutcome, Is.EqualTo(ContactLoadOutcome.NotEvaluated));
            Assert.That(ContactRecoveryPolicy.Recommend(gestures),
                Is.EqualTo(ContactRecoveryAction.SeekContact));
        });
    }

    [Test]
    public void MissingNormalForceRemainsUnavailableAndImpulseIsNotConvertedToForce()
    {
        var physics = CreateObservation(
            isActive: true,
            normalForceNewtons: null,
            normalImpulseNewtonSeconds: 9.0d);

        var gestures = PhysicsContactObservationAdapter.ToGesturesObservation(physics);

        Assert.Multiple(() =>
        {
            Assert.That(gestures.NormalForceNewtons, Is.Null);
            Assert.That(gestures.LoadOutcome, Is.EqualTo(ContactLoadOutcome.NotEvaluated));
        });
    }

    [Test]
    public void ForceOutsideGesturesPrecisionRangeIsRejectedRatherThanSilentlyLost()
    {
        var physics = CreateObservation(
            isActive: true,
            normalForceNewtons: (double)float.MaxValue * 2d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => PhysicsContactObservationAdapter.ToGesturesObservation(physics));
    }

    [Test]
    public void InvalidDefaultPhysicsObservationIsRejected()
    {
        Assert.Throws<ArgumentException>(
            () => PhysicsContactObservationAdapter.ToGesturesObservation(default));
    }

    private static ContactObservation CreateObservation(
        bool isActive,
        double? normalForceNewtons = null,
        double? normalImpulseNewtonSeconds = null) =>
        new(
            contactId: "contact-17",
            bodyAId: "body-a",
            bodyBId: "body-b",
            point: new Vector3d(0d, 0d, 0d),
            normalFromAToB: new Vector3d(1d, 0d, 0d),
            relativeVelocity: new Vector3d(0d, 0d, 0d),
            signedSeparation: 0d,
            isActive: isActive,
            stepIndex: 4,
            elapsedTime: 0.04d,
            normalForceNewtons: normalForceNewtons,
            normalImpulseNewtonSeconds: normalImpulseNewtonSeconds);
}
