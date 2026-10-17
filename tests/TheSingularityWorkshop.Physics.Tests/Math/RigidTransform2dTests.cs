using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class RigidTransform2dTests
{
    private static readonly CoordinateFrame2d A = new("A");
    private static readonly CoordinateFrame2d B = new("B");
    private static readonly CoordinateFrame2d C = new("C");

    [Test]
    public void Transform_RotatesThenAddsDestinationFrameTranslation()
    {
        var transform = new RigidTransform2d(A, B, System.Math.PI / 2d, new Vector2d(10, 20));
        var result = transform.Transform(new Point2d(A, new Vector2d(2, 0)));

        Assert.That(result.Frame, Is.EqualTo(B));
        Assert.That(result.Coordinates.X, Is.EqualTo(10).Within(1e-12));
        Assert.That(result.Coordinates.Y, Is.EqualTo(22).Within(1e-12));
    }

    [Test]
    public void Transform_RejectsPointInWrongSourceFrame()
    {
        var transform = new RigidTransform2d(A, B, 0d, new Vector2d(0, 0));
        Assert.Throws<InvalidOperationException>(() =>
            transform.Transform(new Point2d(C, new Vector2d(1, 2))));
    }

    [Test]
    public void TransformDirection_DoesNotApplyTranslation()
    {
        var transform = new RigidTransform2d(A, B, System.Math.PI / 2d, new Vector2d(100, 200));
        var result = transform.TransformDirection(new Vector2d(1, 0));
        Assert.That(result.X, Is.EqualTo(0).Within(1e-12));
        Assert.That(result.Y, Is.EqualTo(1).Within(1e-12));
    }

    [Test]
    public void Then_ComposesThisTransformBeforeNextTransform()
    {
        var first = new RigidTransform2d(A, B, System.Math.PI / 2d, new Vector2d(10, 0));
        var second = new RigidTransform2d(B, C, 0d, new Vector2d(0, 5));
        var point = new Point2d(A, new Vector2d(2, 0));

        var sequential = second.Transform(first.Transform(point));
        var composed = first.Then(second).Transform(point);

        Assert.That(composed.Frame, Is.EqualTo(C));
        Assert.That(composed.Coordinates.X, Is.EqualTo(sequential.Coordinates.X).Within(1e-12));
        Assert.That(composed.Coordinates.Y, Is.EqualTo(sequential.Coordinates.Y).Within(1e-12));
        Assert.That(composed.Coordinates.X, Is.EqualTo(10).Within(1e-12));
        Assert.That(composed.Coordinates.Y, Is.EqualTo(7).Within(1e-12));
    }

    [Test]
    public void Then_RejectsMismatchedIntermediateFrames()
    {
        var first = new RigidTransform2d(A, B, 0d, new Vector2d(0, 0));
        var second = new RigidTransform2d(C, A, 0d, new Vector2d(0, 0));
        Assert.Throws<InvalidOperationException>(() => first.Then(second));
    }

    [Test]
    public void Inverse_RoundTripsPointWithinFloatingPointTolerance()
    {
        var transform = new RigidTransform2d(A, B, 0.73d, new Vector2d(12.5, -8.25));
        var original = new Point2d(A, new Vector2d(-3.5, 7.25));

        var restored = transform.Inverse().Transform(transform.Transform(original));

        Assert.That(restored.Frame, Is.EqualTo(A));
        Assert.That(restored.Coordinates.X, Is.EqualTo(original.Coordinates.X).Within(1e-12));
        Assert.That(restored.Coordinates.Y, Is.EqualTo(original.Coordinates.Y).Within(1e-12));
    }

    [Test]
    public void SameFrameZeroTransform_ActsAsIdentity()
    {
        var identity = new RigidTransform2d(A, A, 0d, new Vector2d(0, 0));
        var point = new Point2d(A, new Vector2d(-12.5, 4.75));
        var result = identity.Transform(point);
        Assert.That(result, Is.EqualTo(point));
    }

    [Test]
    public void Constructor_RejectsNonFiniteAngleAndDefaultFrames()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RigidTransform2d(A, B, double.NaN, new Vector2d(0, 0)));
        Assert.Throws<ArgumentException>(() =>
            new RigidTransform2d(default, B, 0d, new Vector2d(0, 0)));
        Assert.Throws<ArgumentException>(() =>
            new RigidTransform2d(A, default, 0d, new Vector2d(0, 0)));
    }

    [Test]
    public void Composition_RejectsAngleOverflow()
    {
        var first = new RigidTransform2d(A, B, double.MaxValue, new Vector2d(0, 0));
        var second = new RigidTransform2d(B, C, double.MaxValue, new Vector2d(0, 0));
        Assert.Throws<ArithmeticException>(() => first.Then(second));
    }
}
