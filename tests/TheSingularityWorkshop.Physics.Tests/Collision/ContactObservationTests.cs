using NUnit.Framework;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Collision;

[TestFixture]
public sealed class ContactObservationTests
{
    private static ContactObservation Contact(
        double? force = null,
        double? impulse = null,
        bool active = true,
        double separation = 0d) =>
        new("contact:A:B", "body:A", "body:B",
            new Vector3d(1d, 2d, 3d),
            new Vector3d(0d, 1d, 0d),
            new Vector3d(0d, -0.5d, 0d),
            separation, active, 8, 0.4d, force, impulse);

    [Test]
    public void Constructor_PreservesGeometryTimeAndOptionalLoadEvidence()
    {
        var contact = Contact(force: 12d, impulse: 0.25d, separation: -0.01d);

        Assert.That(contact.ContactId, Is.EqualTo("contact:A:B"));
        Assert.That(contact.BodyAId, Is.EqualTo("body:A"));
        Assert.That(contact.BodyBId, Is.EqualTo("body:B"));
        Assert.That(contact.Point, Is.EqualTo(new Vector3d(1d, 2d, 3d)));
        Assert.That(contact.NormalFromAToB, Is.EqualTo(new Vector3d(0d, 1d, 0d)));
        Assert.That(contact.RelativeVelocity, Is.EqualTo(new Vector3d(0d, -0.5d, 0d)));
        Assert.That(contact.SignedSeparation, Is.EqualTo(-0.01d));
        Assert.That(contact.IsActive, Is.True);
        Assert.That(contact.NormalForceNewtons, Is.EqualTo(12d));
        Assert.That(contact.NormalImpulseNewtonSeconds, Is.EqualTo(0.25d));
        Assert.That(contact.StepIndex, Is.EqualTo(8));
        Assert.That(contact.ElapsedTime, Is.EqualTo(0.4d));
        Assert.That(contact.IsValid, Is.True);
    }

    [Test]
    public void UncomputedLoadEvidence_RemainsExplicitlyUnavailable()
    {
        var contact = Contact();

        Assert.That(contact.NormalForceNewtons, Is.Null);
        Assert.That(contact.NormalImpulseNewtonSeconds, Is.Null);
        Assert.That(contact.IsValid, Is.True);
    }

    [Test]
    public void DefaultObservation_IsExplicitlyInvalid()
    {
        Assert.That(default(ContactObservation).IsValid, Is.False);
    }

    [Test]
    public void Constructor_RejectsMissingOrIdenticalBodyIdentity()
    {
        Assert.Throws<ArgumentException>(() => new ContactObservation(
            "", "body:A", "body:B", Vector3d.Zero, new Vector3d(1d, 0d, 0d),
            Vector3d.Zero, 0d, true, 0, 0d));
        Assert.Throws<ArgumentException>(() => new ContactObservation(
            "contact", "body:A", "body:A", Vector3d.Zero, new Vector3d(1d, 0d, 0d),
            Vector3d.Zero, 0d, true, 0, 0d));
    }

    [Test]
    public void Constructor_RejectsNonUnitNormalAndInvalidTime()
    {
        Assert.Throws<ArgumentException>(() => new ContactObservation(
            "contact", "body:A", "body:B", Vector3d.Zero, new Vector3d(2d, 0d, 0d),
            Vector3d.Zero, 0d, true, 0, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ContactObservation(
            "contact", "body:A", "body:B", Vector3d.Zero, new Vector3d(1d, 0d, 0d),
            Vector3d.Zero, 0d, true, -1, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ContactObservation(
            "contact", "body:A", "body:B", Vector3d.Zero, new Vector3d(1d, 0d, 0d),
            Vector3d.Zero, 0d, true, 0, double.NaN));
    }

    [Test]
    public void Constructor_RejectsInvalidOptionalLoadsAndSeparation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Contact(force: -1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contact(impulse: double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => Contact(separation: double.NaN));
    }

    [Test]
    public void ActiveContactDoesNotImplySupportOrNonzeroLoad()
    {
        var contact = Contact(active: true);

        Assert.That(contact.IsActive, Is.True);
        Assert.That(contact.NormalForceNewtons, Is.Null);
        Assert.That(contact.NormalImpulseNewtonSeconds, Is.Null);
    }
}
