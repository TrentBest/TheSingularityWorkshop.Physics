using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class FiniteFaultSegment3dTests
{
    private static FaultKinematics3d CreateFault() =>
        new(
            faultId: "fault/demo-001",
            coordinateFrameId: "local-east-north-up",
            linearUnit: "m",
            planeOrigin: Vector3d.Zero,
            strikeDirection: new Vector3d(1d, 0d, 0d),
            downDipDirection: new Vector3d(0d, 1d, 0d),
            hangingWallSide: FaultHangingWallSide3d.PositiveNormalSide,
            mechanism: FaultMechanism3d.ObliqueSlip,
            strikeSlip: 2d,
            dipSlip: -1d);

    [Test]
    public void Constructor_RejectsNullFaultAndNonPositiveOrNonFiniteDimensions()
    {
        Assert.Throws<ArgumentNullException>(() => new FiniteFaultSegment3d(null!, 1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FiniteFaultSegment3d(CreateFault(), 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FiniteFaultSegment3d(CreateFault(), 1d, -1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FiniteFaultSegment3d(CreateFault(), double.NaN, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FiniteFaultSegment3d(CreateFault(), 1d, double.PositiveInfinity));
    }

    [Test]
    public void Projection_IsBoundedAlongStrikeAndDownDipAndIncludesEdges()
    {
        var segment = new FiniteFaultSegment3d(CreateFault(), halfStrikeLength: 5d, halfDownDipWidth: 2d);

        Assert.That(segment.ContainsProjection(new Vector3d(5d, 2d, 10d)), Is.True);
        Assert.That(segment.ContainsProjection(new Vector3d(-5d, -2d, -10d)), Is.True);
        Assert.That(segment.ContainsProjection(new Vector3d(5.001d, 0d, 0d)), Is.False);
        Assert.That(segment.ContainsProjection(new Vector3d(0d, -2.001d, 0d)), Is.False);
    }

    [Test]
    public void Displacement_AppliesOnlyInsideFiniteFootprintAndOnHangingWall()
    {
        var segment = new FiniteFaultSegment3d(CreateFault(), halfStrikeLength: 5d, halfDownDipWidth: 2d);
        var expected = new Vector3d(2d, -1d, 0d);

        Assert.That(segment.GetBlockDisplacement(new Vector3d(1d, 1d, 3d)), Is.EqualTo(expected));
        Assert.That(segment.GetBlockDisplacement(new Vector3d(6d, 1d, 3d)), Is.EqualTo(Vector3d.Zero));
        Assert.That(segment.GetBlockDisplacement(new Vector3d(1d, 3d, 3d)), Is.EqualTo(Vector3d.Zero));
        Assert.That(segment.GetBlockDisplacement(new Vector3d(1d, 1d, -3d)), Is.EqualTo(Vector3d.Zero));
        Assert.That(segment.GetBlockDisplacement(new Vector3d(1d, 1d, 0d)), Is.EqualTo(Vector3d.Zero));
    }

    [Test]
    public void Segment_PreservesUnderlyingFaultIdentityAndSlipSemantics()
    {
        var fault = CreateFault();
        var segment = new FiniteFaultSegment3d(fault, 5d, 2d);

        Assert.That(segment.Kinematics, Is.SameAs(fault));
        Assert.That(segment.Kinematics.FaultId, Is.EqualTo("fault/demo-001"));
        Assert.That(segment.Kinematics.HangingWallDisplacement, Is.EqualTo(new Vector3d(2d, -1d, 0d)));
    }
}
