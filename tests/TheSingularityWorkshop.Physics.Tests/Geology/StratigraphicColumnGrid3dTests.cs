using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class StratigraphicColumnGrid3dTests
{
    private static StratigraphicColumn3d Column(string id, double x, double y,
        string frame = "world", string datum = "datum", string unit = "m", string direction = "up") =>
        new(id, x, y, frame, datum, unit, direction,
            new[] { new StratigraphicLayer3d($"{id}-layer", 1, 10d, 0d, 20d, 10d, $"{id}-event") });

    [Test]
    public void Constructor_PreservesRowMajorColumnsAndGridGeometry()
    {
        var columns = new[]
        {
            Column("c00", 100d, 200d), Column("c10", 110d, 200d),
            Column("c01", 100d, 220d), Column("c11", 110d, 220d)
        };

        var grid = new StratigraphicColumnGrid3d(
            2, 2, 10d, 20d, 100d, 200d, "world", "sea-level", "m", "up", columns);

        Assert.Multiple(() =>
        {
            Assert.That(grid.Columns.Count, Is.EqualTo(4));
            Assert.That(grid.GetColumn(0, 0), Is.SameAs(columns[0]));
            Assert.That(grid.GetColumn(1, 0), Is.SameAs(columns[1]));
            Assert.That(grid.GetColumn(0, 1), Is.SameAs(columns[2]));
            Assert.That(grid.GetColumn(1, 1), Is.SameAs(columns[3]));
            Assert.That(grid.TryGetColumn(1, 1, out var found), Is.True);
            Assert.That(found, Is.SameAs(columns[3]));
            Assert.That(grid.SpacingX, Is.EqualTo(10d));
            Assert.That(grid.SpacingY, Is.EqualTo(20d));
        });
    }

    [Test]
    public void Constructor_DefensivelyCopiesColumnCollection()
    {
        var source = new[] { Column("c0", 0d, 0d), Column("c1", 1d, 0d) };
        var grid = new StratigraphicColumnGrid3d(2, 1, 1d, 1d, 0d, 0d,
            "world", "datum", "m", "up", source);

        source[0] = Column("replacement", 0d, 0d);

        Assert.That(grid.GetColumn(0, 0).ColumnId, Is.EqualTo("c0"));
    }

    [Test]
    public void Constructor_RejectsIncorrectGridShapeAndSampleCoordinates()
    {
        Assert.Throws<ArgumentException>(() =>
            new StratigraphicColumnGrid3d(2, 1, 1d, 1d, 0d, 0d,
                "world", "datum", "m", "up", new[] { Column("c0", 0d, 0d) }));

        Assert.Throws<ArgumentException>(() =>
            new StratigraphicColumnGrid3d(2, 1, 1d, 1d, 0d, 0d,
                "world", "datum", "m", "up",
                new[] { Column("c0", 0d, 0d), Column("c1", 2d, 0d) }));
    }

    [Test]
    public void Constructor_RejectsConventionMismatchAndDuplicateColumnIds()
    {
        Assert.Throws<ArgumentException>(() =>
            new StratigraphicColumnGrid3d(2, 1, 1d, 1d, 0d, 0d,
                "world", "datum", "m", "up",
                new[] { Column("c0", 0d, 0d), Column("c1", 1d, 0d, frame: "other") }));

        Assert.Throws<ArgumentException>(() =>
            new StratigraphicColumnGrid3d(2, 1, 1d, 1d, 0d, 0d,
                "world", "datum", "m", "up",
                new[] { Column("same", 0d, 0d), Column("same", 1d, 0d) }));
    }

    [Test]
    public void TryGetColumn_ReturnsFalseOutsideGridAndGetColumnThrows()
    {
        var grid = new StratigraphicColumnGrid3d(1, 1, 1d, 1d, 0d, 0d,
            "world", "datum", "m", "up", new[] { Column("c0", 0d, 0d) });

        Assert.That(grid.TryGetColumn(-1, 0, out var missing), Is.False);
        Assert.That(missing, Is.Null);
        Assert.That(grid.TryGetColumn(0, 1, out missing), Is.False);
        Assert.That(missing, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetColumn(1, 0));
    }

    [Test]
    public void Constructor_RejectsInvalidDimensionsSpacingAndNonFiniteBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StratigraphicColumnGrid3d(0, 1, 1d, 1d, 0d, 0d,
                "world", "datum", "m", "up", Array.Empty<StratigraphicColumn3d>()));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StratigraphicColumnGrid3d(1, 1, double.NaN, 1d, 0d, 0d,
                "world", "datum", "m", "up", new[] { Column("c0", 0d, 0d) }));
        Assert.Throws<ArgumentException>(() =>
            new StratigraphicColumnGrid3d(2, 1, double.MaxValue, 1d, double.MaxValue, 0d,
                "world", "datum", "m", "up",
                new[] { Column("c0", double.MaxValue, 0d), Column("c1", 0d, 0d) }));
    }
}
