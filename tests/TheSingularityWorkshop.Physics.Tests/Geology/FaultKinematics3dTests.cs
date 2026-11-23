using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class FaultKinematics3dTests
{
    private static FaultKinematics3d Create(
        FaultMechanism3d mechanism = FaultMechanism3d.ObliqueSlip,
        FaultHangingWallSide3d hangingWallSide = FaultHangingWallSide3d.PositiveNormalSide,
        double strikeSlip = 3d,
        double dipSlip = -4d) =>
        new(
            faultId: "fault-001",
            coordinateFrameId: "geology-local",
            linearUnit: "metre",
            planeOrigin: new Vector3d(10d, 20d, 30d),
            strikeDirection: new Vector3d(2d, 0d, 0d),
            downDipDirection: new Vector3d(0d, 5d, 0d),
            hangingWallSide: hangingWallSide,
            mechanism: mechanism,
            strikeSlip: strikeSlip,
            dipSlip: dipSlip);

    [Test]
    public void Constructor_NormalizesBasisAndBuildsFullObliqueSlipVector()
    {
        var fault = Create();

        Assert.That(fault.StrikeDirection, Is.EqualTo(new Vector3d(1d, 0d, 0d)));
        Assert.That(fault.DownDipDirection, Is.EqualTo(new Vector3d(0d, 1d, 0d)));
        Assert.That(fault.UnitNormal, Is.EqualTo(new Vector3d(0d, 0d, 1d)));
        Assert.That(fault.HangingWallDisplacement, Is.EqualTo(new Vector3d(3d, -4d, 0d)));
        Assert.That(fault.Mechanism, Is.EqualTo(FaultMechanism3d.ObliqueSlip));
        Assert.That(fault.CoordinateFrameId, Is.EqualTo("geology-local"));
        Assert.That(fault.LinearUnit, Is.EqualTo("metre"));
    }

    [Test]
    public void GetBlockDisplacement_MovesOnlyDeclaredPositiveNormalHangingWall()
    {
        var fault = Create();

        Assert.That(fault.SignedDistanceToPlane(new Vector3d(10d, 20d, 31d)), Is.EqualTo(1d));
        Assert.That(fault.IsHangingWall(new Vector3d(10d, 20d, 31d)), Is.True);
        Assert.That(fault.GetBlockDisplacement(new Vector3d(10d, 20d, 31d)),
            Is.EqualTo(new Vector3d(3d, -4d, 0d)));
        Assert.That(fault.GetBlockDisplacement(new Vector3d(10d, 20d, 29d)),
            Is.EqualTo(Vector3d.Zero));
        Assert.That(fault.GetBlockDisplacement(new Vector3d(10d, 20d, 30d)),
            Is.EqualTo(Vector3d.Zero));
    }

    [Test]
    public void GetBlockDisplacement_SupportsExplicitNegativeNormalHangingWall()
    {
        var fault = Create(hangingWallSide: FaultHangingWallSide3d.NegativeNormalSide);

        Assert.That(fault.IsHangingWall(new Vector3d(10d, 20d, 29d)), Is.True);
        Assert.That(fault.GetBlockDisplacement(new Vector3d(10d, 20d, 29d)),
            Is.EqualTo(new Vector3d(3d, -4d, 0d)));
        Assert.That(fault.GetBlockDisplacement(new Vector3d(10d, 20d, 31d)),
            Is.EqualTo(Vector3d.Zero));
    }

    [TestCase(FaultMechanism3d.Normal)]
    [TestCase(FaultMechanism3d.Reverse)]
    [TestCase(FaultMechanism3d.Thrust)]
    [TestCase(FaultMechanism3d.StrikeSlipLeftLateral)]
    [TestCase(FaultMechanism3d.StrikeSlipRightLateral)]
    [TestCase(FaultMechanism3d.ObliqueSlip)]
    public void Constructor_PreservesDeclaredFaultMechanism(FaultMechanism3d mechanism)
    {
        var fault = Create(mechanism);

        Assert.That(fault.Mechanism, Is.EqualTo(mechanism));
    }

    [Test]
    public void Constructor_RejectsNonPerpendicularStrikeAndDipBasis()
    {
        Assert.Throws<ArgumentException>(() => new FaultKinematics3d(
            "fault", "frame", "m", Vector3d.Zero,
            new Vector3d(1d, 0d, 0d), new Vector3d(1d, 1d, 0d),
            FaultHangingWallSide3d.PositiveNormalSide,
            FaultMechanism3d.Normal, 0d, 1d));
    }

    [Test]
    public void Constructor_RejectsInvalidIdentityAndSlipValues()
    {
        Assert.Throws<ArgumentException>(() => new FaultKinematics3d(
            " ", "frame", "m", Vector3d.Zero,
            new Vector3d(1d, 0d, 0d), new Vector3d(0d, 1d, 0d),
            FaultHangingWallSide3d.PositiveNormalSide,
            FaultMechanism3d.Normal, 0d, 1d));

        Assert.Throws<ArgumentOutOfRangeException>(() => new FaultKinematics3d(
            "fault", "frame", "m", Vector3d.Zero,
            new Vector3d(1d, 0d, 0d), new Vector3d(0d, 1d, 0d),
            FaultHangingWallSide3d.PositiveNormalSide,
            FaultMechanism3d.Normal, double.NaN, 1d));
    }
}
