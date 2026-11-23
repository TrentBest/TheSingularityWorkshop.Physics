using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class FaultStratigraphicIntersection3dTests
{
    private static FiniteFaultSegment3d CreateFault(
        Vector3d? strike = null,
        Vector3d? downDip = null,
        double halfStrike = 20d,
        double halfDownDip = 20d,
        Vector3d? planeOrigin = null,
        string frame = "world",
        string unit = "m")
    {
        var kinematics = new FaultKinematics3d(
            "fault-01", frame, unit, planeOrigin ?? Vector3d.Zero,
            strike ?? new Vector3d(1d, 0d, 0d),
            downDip ?? new Vector3d(0d, 1d, 0d),
            FaultHangingWallSide3d.PositiveNormalSide,
            FaultMechanism3d.Normal, 0d, 2d);
        return new FiniteFaultSegment3d(kinematics, halfStrike, halfDownDip);
    }

    private static StratigraphicColumn3d CreateColumn(
        double x = 0d, double y = 0d, string frame = "world", string unit = "m",
        StratigraphicLayer3d[]? layers = null) =>
        new("column-01", x, y, frame, "datum", unit, "up",
            layers ?? new[]
            {
                new StratigraphicLayer3d("upper", 4, 10d, 0d, 20d, 10d, "deposit-upper"),
                new StratigraphicLayer3d("lower", 7, 0d, -10d, 50d, 30d, "deposit-lower")
            });

    [Test]
    public void TryIntersectColumn_IdentifiesCutLayerAndPreservesItsProvenance()
    {
        var fault = CreateFault(
            strike: new Vector3d(1d, 0d, 0d),
            downDip: new Vector3d(0d, 1d, 1d),
            planeOrigin: new Vector3d(0d, 0d, 5d));
        var column = CreateColumn();

        var found = FaultStratigraphicIntersection3d.TryIntersectColumn(fault, column, out var result);

        Assert.That(found, Is.True);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.FaultId, Is.EqualTo("fault-01"));
            Assert.That(result.ColumnId, Is.EqualTo("column-01"));
            Assert.That(result.LayerId, Is.EqualTo("upper"));
            Assert.That(result.MaterialId, Is.EqualTo(4));
            Assert.That(result.FormationEventId, Is.EqualTo("deposit-upper"));
            Assert.That(result.IntersectionPoint, Is.EqualTo(new Vector3d(0d, 0d, 5d)));
        });
    }

    [Test]
    public void TryIntersectColumn_RejectsLayerBoundaryAndStratigraphicGap()
    {
        var fault = CreateFault(
            strike: new Vector3d(1d, 0d, 0d),
            downDip: new Vector3d(0d, 1d, 1d));
        var boundaryColumn = CreateColumn(layers: new[]
        {
            new StratigraphicLayer3d("upper", 1, 10d, 0d, 20d, 10d, "deposit-upper"),
            new StratigraphicLayer3d("lower", 2, 0d, -10d, 50d, 30d, "deposit-lower")
        });
        var gapColumn = CreateColumn(layers: new[]
        {
            new StratigraphicLayer3d("upper", 1, 10d, 2d, 20d, 10d, "deposit-upper"),
            new StratigraphicLayer3d("lower", 2, -2d, -10d, 50d, 30d, "deposit-lower")
        });

        Assert.That(FaultStratigraphicIntersection3d.TryIntersectColumn(fault, boundaryColumn, out var boundary), Is.False);
        Assert.That(boundary, Is.Null);
        Assert.That(FaultStratigraphicIntersection3d.TryIntersectColumn(fault, gapColumn, out var gap), Is.False);
        Assert.That(gap, Is.Null);
    }

    [Test]
    public void TryIntersectColumn_RespectsFiniteFaultFootprint()
    {
        var fault = CreateFault(
            strike: new Vector3d(1d, 0d, 0d),
            downDip: new Vector3d(0d, 1d, 1d),
            halfStrike: 2d,
            halfDownDip: 2d);
        var outsideColumn = CreateColumn(x: 3d);

        Assert.That(FaultStratigraphicIntersection3d.TryIntersectColumn(fault, outsideColumn, out var result), Is.False);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryIntersectColumn_IncludesFiniteSegmentFootprintEdge()
    {
        var fault = CreateFault(
            strike: new Vector3d(1d, 0d, 0d),
            downDip: new Vector3d(0d, 1d, 1d),
            halfStrike: 2d,
            halfDownDip: 2d,
            planeOrigin: new Vector3d(0d, 0d, 5d));
        var edgeColumn = CreateColumn(x: 2d);

        Assert.That(FaultStratigraphicIntersection3d.TryIntersectColumn(fault, edgeColumn, out var result), Is.True);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.LayerId, Is.EqualTo("upper"));
        Assert.That(result.IntersectionPoint, Is.EqualTo(new Vector3d(2d, 0d, 5d)));
    }

    [Test]
    public void TryIntersectColumn_ReturnsFalseForVerticalPlaneWithoutUniqueColumnIntersection()
    {
        var verticalFault = CreateFault(
            strike: new Vector3d(0d, 0d, 1d),
            downDip: new Vector3d(0d, 1d, 0d));
        Assert.That(FaultStratigraphicIntersection3d.TryIntersectColumn(verticalFault, CreateColumn(), out var result), Is.False);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryIntersectColumn_RejectsCoordinateFrameAndUnitMismatch()
    {
        var fault = CreateFault();
        Assert.Throws<ArgumentException>(() =>
            FaultStratigraphicIntersection3d.TryIntersectColumn(fault, CreateColumn(frame: "other"), out _));
        Assert.Throws<ArgumentException>(() =>
            FaultStratigraphicIntersection3d.TryIntersectColumn(fault, CreateColumn(unit: "ft"), out _));
    }

    [Test]
    public void TryIntersectColumn_RejectsInvalidToleranceAndNullInputs()
    {
        var column = CreateColumn();
        Assert.Throws<ArgumentNullException>(() =>
            FaultStratigraphicIntersection3d.TryIntersectColumn(null!, column, out _));
        Assert.Throws<ArgumentNullException>(() =>
            FaultStratigraphicIntersection3d.TryIntersectColumn(CreateFault(), null!, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FaultStratigraphicIntersection3d.TryIntersectColumn(CreateFault(), column, out _, double.NaN));
    }
}
