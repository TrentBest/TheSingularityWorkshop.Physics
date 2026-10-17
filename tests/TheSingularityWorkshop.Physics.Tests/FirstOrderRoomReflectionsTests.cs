using NUnit.Framework;
using TheSingularityWorkshop.Physics.Acoustics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class FirstOrderRoomReflectionsTests
{
    [Test]
    public void Observe_ReturnsReflectionPointPathDelayAndPerPathLevelForOneEnabledWall()
    {
        var room = new AcousticRoomBox(
            "room",
            new AcousticPosition3(0d, 0d, 0d),
            new AcousticPosition3(10d, 10d, 10d),
            xMinReflectionCoefficient: 0d,
            xMaxReflectionCoefficient: 0.5d,
            yMinReflectionCoefficient: 0d,
            yMaxReflectionCoefficient: 0d,
            zMinReflectionCoefficient: 0d,
            zMaxReflectionCoefficient: 0d);
        var source = new AcousticSource("source", new AcousticPosition3(2d, 5d, 5d), 80d);
        var listener = new AcousticListener("listener", new AcousticPosition3(8d, 5d, 5d));

        var observations = FirstOrderRoomReflections.Observe(source, listener, room);

        var reflection = Is.Single(observations);
        Assert.That(reflection.SurfaceId, Is.EqualTo("x-max"));
        Assert.That(reflection.ReflectionPoint.X, Is.EqualTo(10d).Within(1e-12));
        Assert.That(reflection.ReflectionPoint.Y, Is.EqualTo(5d).Within(1e-12));
        Assert.That(reflection.ReflectionPoint.Z, Is.EqualTo(5d).Within(1e-12));
        Assert.That(reflection.PathLengthMeters, Is.EqualTo(10d).Within(1e-12));
        Assert.That(reflection.PropagationDelaySeconds, Is.EqualTo(10d / 343d).Within(1e-12));
        Assert.That(reflection.ReflectionCoefficient, Is.EqualTo(0.5d));
        Assert.That(reflection.ReceivedLevelDbSpl, Is.EqualTo(80d - 20d * Math.Log10(10d) + 20d * Math.Log10(0.5d)).Within(1e-12));
        Assert.That(reflection.RoomId, Is.EqualTo("room"));
    }

    [Test]
    public void Observe_ReturnsOnlyValidInteriorFaceIntersectionsInStableSurfaceOrder()
    {
        var room = new AcousticRoomBox("room", new AcousticPosition3(0d, 0d, 0d), new AcousticPosition3(10d, 10d, 10d));
        var source = new AcousticSource("source", new AcousticPosition3(2d, 3d, 4d), 80d);
        var listener = new AcousticListener("listener", new AcousticPosition3(8d, 7d, 6d));

        var first = FirstOrderRoomReflections.Observe(source, listener, room);
        var second = FirstOrderRoomReflections.Observe(source, listener, room);

        Assert.That(first.Select(path => path.SurfaceId), Is.EqualTo(second.Select(path => path.SurfaceId)));
        Assert.That(first.Count, Is.GreaterThan(0));
        Assert.That(first.All(path => path.PathLengthMeters > 0d && path.PropagationDelaySeconds >= 0d), Is.True);
        Assert.That(first.All(path => path.ReflectionCoefficient is > 0d and <= 1d), Is.True);
    }

    [Test]
    public void Observe_RejectsSourceOrListenerOutsideOrOnRoomBoundary()
    {
        var room = new AcousticRoomBox("room", new AcousticPosition3(0d, 0d, 0d), new AcousticPosition3(10d, 10d, 10d));
        var inside = new AcousticListener("inside", new AcousticPosition3(5d, 5d, 5d));
        var outsideSource = new AcousticSource("outside", new AcousticPosition3(-1d, 5d, 5d), 80d);
        var boundarySource = new AcousticSource("boundary", new AcousticPosition3(0d, 5d, 5d), 80d);
        var insideSource = new AcousticSource("inside", new AcousticPosition3(2d, 5d, 5d), 80d);
        var boundaryListener = new AcousticListener("boundary", new AcousticPosition3(10d, 5d, 5d));

        Assert.Throws<ArgumentOutOfRangeException>(() => FirstOrderRoomReflections.Observe(outsideSource, inside, room));
        Assert.Throws<ArgumentOutOfRangeException>(() => FirstOrderRoomReflections.Observe(boundarySource, inside, room));
        Assert.Throws<ArgumentOutOfRangeException>(() => FirstOrderRoomReflections.Observe(insideSource, boundaryListener, room));
    }

    [Test]
    public void Constructors_RejectInvalidBoundsAndReflectionCoefficients()
    {
        Assert.Throws<ArgumentException>(() => new AcousticRoomBox("room", new AcousticPosition3(0d, 0d, 0d), new AcousticPosition3(0d, 1d, 1d)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcousticRoomBox("room", new AcousticPosition3(0d, 0d, 0d), new AcousticPosition3(1d, 1d, 1d), xMaxReflectionCoefficient: double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcousticRoomBox("room", new AcousticPosition3(0d, 0d, 0d), new AcousticPosition3(1d, 1d, 1d), yMinReflectionCoefficient: -0.1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcousticRoomBox("room", new AcousticPosition3(0d, 0d, 0d), new AcousticPosition3(1d, 1d, 1d), zMaxReflectionCoefficient: 1.1d));
    }
}
