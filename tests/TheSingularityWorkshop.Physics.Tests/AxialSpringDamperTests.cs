using NUnit.Framework;
using TheSingularityWorkshop.Physics.Mechanics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class AxialSpringDamperTests
{
    private static AxialSpringDamper.Definition Link(double restLength = 1d, double stiffness = 100d, double damping = 0d) =>
        new("link-1", restLength, stiffness, damping);

    [Test]
    public void Advance_StretchedSpring_ComputesLoadAndEqualOppositeAcceleration()
    {
        var a = new AxialSpringDamper.BodyState("A", 2d, 0d, 0d);
        var b = new AxialSpringDamper.BodyState("B", 4d, 2d, 0d);

        var result = AxialSpringDamper.Advance(Link(), a, b, 7, 0.6d, 0.1d);

        Assert.That(result.LinkId, Is.EqualTo("link-1"));
        Assert.That(result.StepIndex, Is.EqualTo(7));
        Assert.That(result.ElapsedTime, Is.EqualTo(0.7d).Within(1e-15));
        Assert.That(result.AxialForceOnBodyA, Is.EqualTo(100d));
        Assert.That(result.BodyA.Velocity, Is.EqualTo(5d));
        Assert.That(result.BodyB.Velocity, Is.EqualTo(-2.5d));
        Assert.That(result.BodyA.Position, Is.EqualTo(0.5d));
        Assert.That(result.BodyB.Position, Is.EqualTo(1.75d));
    }

    [Test]
    public void Advance_DamperOpposesRelativeMotion()
    {
        var a = new AxialSpringDamper.BodyState("A", 1d, 0d, 0d);
        var b = new AxialSpringDamper.BodyState("B", 1d, 1d, 2d);

        var result = AxialSpringDamper.Advance(Link(stiffness: 0d, damping: 3d), a, b, 1, 0d, 0.1d);

        Assert.That(result.AxialForceOnBodyA, Is.EqualTo(6d));
        Assert.That(result.BodyA.Velocity, Is.EqualTo(0.6d));
        Assert.That(result.BodyB.Velocity, Is.EqualTo(1.4d));
    }

    [Test]
    public void Advance_AtRestLengthAndNoRelativeMotion_ProducesZeroLoad()
    {
        var a = new AxialSpringDamper.BodyState("A", 1d, 0d, 2d);
        var b = new AxialSpringDamper.BodyState("B", 1d, 1d, 2d);

        var result = AxialSpringDamper.Advance(Link(), a, b, 0, 0d, 0.25d);

        Assert.That(result.AxialForceOnBodyA, Is.Zero);
        Assert.That(result.BodyA.Velocity, Is.EqualTo(2d));
        Assert.That(result.BodyB.Velocity, Is.EqualTo(2d));
        Assert.That(result.BodyA.Position, Is.EqualTo(0.5d));
        Assert.That(result.BodyB.Position, Is.EqualTo(1.5d));
    }

    [Test]
    public void Advance_IsRepeatableForIdenticalInputs()
    {
        var a = new AxialSpringDamper.BodyState("A", 2d, -0.25d, 0.3d);
        var b = new AxialSpringDamper.BodyState("B", 3d, 1.4d, -0.1d);

        var first = AxialSpringDamper.Advance(Link(damping: 2d), a, b, 12, 2d, 0.01d);
        var second = AxialSpringDamper.Advance(Link(damping: 2d), a, b, 12, 2d, 0.01d);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Advance_RejectsSameBodyIdentity()
    {
        var a = new AxialSpringDamper.BodyState("same", 1d, 0d, 0d);
        var b = new AxialSpringDamper.BodyState("same", 1d, 1d, 0d);

        Assert.Throws<ArgumentException>(() => AxialSpringDamper.Advance(Link(), a, b, 0, 0d, 0.1d));
    }

    [Test]
    public void Advance_RejectsInvalidMassAndTimestep()
    {
        var a = new AxialSpringDamper.BodyState("A", 0d, 0d, 0d);
        var b = new AxialSpringDamper.BodyState("B", 1d, 1d, 0d);
        Assert.Throws<ArgumentException>(() => AxialSpringDamper.Advance(Link(), a, b, 0, 0d, 0.1d));

        a = new AxialSpringDamper.BodyState("A", 1d, 0d, 0d);
        Assert.Throws<ArgumentOutOfRangeException>(() => AxialSpringDamper.Advance(Link(), a, b, 0, 0d, 0d));
    }

    [Test]
    public void Advance_ThrowsWhenForceOrStateOverflows()
    {
        var a = new AxialSpringDamper.BodyState("A", 1d, -double.MaxValue, 0d);
        var b = new AxialSpringDamper.BodyState("B", 1d, double.MaxValue, 0d);

        Assert.Throws<OverflowException>(() => AxialSpringDamper.Advance(Link(), a, b, 0, 0d, 0.1d));
    }
}
