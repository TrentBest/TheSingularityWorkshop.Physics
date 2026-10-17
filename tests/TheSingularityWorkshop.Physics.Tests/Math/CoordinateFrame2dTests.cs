using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class CoordinateFrame2dTests
{
    [Test]
    public void FrameIdentity_IsOrdinalAndCaseSensitive()
    {
        var world = new CoordinateFrame2d("world");
        Assert.That(world, Is.EqualTo(new CoordinateFrame2d("world")));
        Assert.That(new CoordinateFrame2d("world"), Is.Not.EqualTo(new CoordinateFrame2d("World")));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void FrameIdentity_RejectsEmptyIdentifier(string? id) =>
        Assert.Throws<ArgumentException>(() => new CoordinateFrame2d(id!));

    [Test]
    public void Point_RejectsDefaultUninitializedFrame() =>
        Assert.Throws<ArgumentException>(() => new Point2d(default, new Vector2d(1, 2)));

    [Test]
    public void PointsInSameFrame_PassFrameGuard()
    {
        var frame = new CoordinateFrame2d("world/metres");
        var left = new Point2d(frame, new Vector2d(1, 2));
        var right = new Point2d(frame, new Vector2d(4, 6));
        Assert.DoesNotThrow(() => left.RequireSameFrame(right));
        Assert.That(left, Is.Not.EqualTo(right));
    }

    [Test]
    public void PointsInDifferentFrames_FailFrameGuardEvenAtSameCoordinates()
    {
        var left = new Point2d(new CoordinateFrame2d("world"), new Vector2d(1, 2));
        var right = new Point2d(new CoordinateFrame2d("screen"), new Vector2d(1, 2));
        Assert.Throws<InvalidOperationException>(() => left.RequireSameFrame(right));
        Assert.That(left, Is.Not.EqualTo(right));
    }

    [Test]
    public void EqualCoordinatesDoNotMakeDifferentFramesEquivalent()
    {
        var metres = new Point2d(new CoordinateFrame2d("world/metres"), new Vector2d(2, 3));
        var pixels = new Point2d(new CoordinateFrame2d("screen/pixels"), new Vector2d(2, 3));
        Assert.That(metres.Coordinates, Is.EqualTo(pixels.Coordinates));
        Assert.That(metres, Is.Not.EqualTo(pixels));
    }
}
