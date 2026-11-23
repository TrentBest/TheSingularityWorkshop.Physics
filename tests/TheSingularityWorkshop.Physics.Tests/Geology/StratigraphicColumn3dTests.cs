using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class StratigraphicColumn3dTests
{
    private static StratigraphicLayer3d Layer(string id, int material, double top, double bottom,
        double startAge, double endAge, string eventId) =>
        new(id, material, top, bottom, startAge, endAge, eventId);

    [Test]
    public void Layer_PreservesMaterialGeometryAgeAndProvenance()
    {
        var layer = Layer("sandstone-01", 7, 120d, 100d, 90_000_000d, 85_000_000d, "event-deposition-01");
        Assert.Multiple(() =>
        {
            Assert.That(layer.MaterialId, Is.EqualTo(7));
            Assert.That(layer.Thickness, Is.EqualTo(20d));
            Assert.That(layer.FormationStartAgeYearsBeforePresent, Is.EqualTo(90_000_000d));
            Assert.That(layer.FormationEndAgeYearsBeforePresent, Is.EqualTo(85_000_000d));
            Assert.That(layer.ProvenanceEventId, Is.EqualTo("event-deposition-01"));
        });
    }

    [Test]
    public void Layer_RejectsInvalidMaterialThicknessAndAgeInterval()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Layer("x", 0, 10d, 0d, 10d, 5d, "event"));
        Assert.Throws<ArgumentException>(() => Layer("x", 1, 0d, 0d, 10d, 5d, "event"));
        Assert.Throws<ArgumentException>(() => Layer("x", 1, 10d, 0d, 5d, 10d, "event"));
        Assert.Throws<ArgumentException>(() => Layer("x", 1, 10d, 0d, 10d, 5d, " "));
    }

    [Test]
    public void Column_PreservesTopToBottomOrderAndAllowsHiatus()
    {
        var upper = Layer("upper", 2, 20d, 10d, 40d, 30d, "event-upper");
        var lower = Layer("lower", 1, 10d, -5d, 100d, 80d, "event-lower");
        var column = new StratigraphicColumn3d("column-01", 12d, 15d, "world-local", "sea-level", "m", "up",
            new[] { upper, lower });
        Assert.That(column.Layers[0], Is.SameAs(upper));
        Assert.That(column.Layers[1], Is.SameAs(lower));
        Assert.That(column.TopElevation, Is.EqualTo(20d));
        Assert.That(column.BottomElevation, Is.EqualTo(-5d));
        Assert.That(column.TotalLayerThickness, Is.EqualTo(25d));
    }

    [Test]
    public void Column_RejectsOverlapAndIncorrectStratigraphicAgeOrder()
    {
        var olderAbove = Layer("older-above", 1, 20d, 5d, 100d, 90d, "event-1");
        var youngerBelow = Layer("younger-below", 2, 4d, -5d, 80d, 70d, "event-2");
        Assert.Throws<ArgumentException>(() => new StratigraphicColumn3d("column", 0d, 0d, "frame", "datum", "m", "up",
            new[] { olderAbove, youngerBelow }));
        var upper = Layer("upper", 1, 20d, 0d, 40d, 30d, "event-1");
        var lower = Layer("lower", 2, 10d, -5d, 100d, 80d, "event-2");
        Assert.Throws<ArgumentException>(() => new StratigraphicColumn3d("column", 0d, 0d, "frame", "datum", "m", "up",
            new[] { upper, lower }));
    }

    [Test]
    public void Column_RejectsDuplicateLayerIdsAndInvalidCoordinates()
    {
        var first = Layer("same", 1, 20d, 10d, 40d, 30d, "event-1");
        var second = Layer("same", 2, 10d, 0d, 100d, 80d, "event-2");
        Assert.Throws<ArgumentException>(() => new StratigraphicColumn3d("column", 0d, 0d, "frame", "datum", "m", "up",
            new[] { first, second }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StratigraphicColumn3d("column", double.NaN, 0d, "frame", "datum", "m", "up",
            new[] { first }));
    }
}
