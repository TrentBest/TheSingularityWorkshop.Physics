using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class SeededPointSet2dTests
{
    [Test]
    public void Generate_RepeatsExactlyForSameSeedAndInputs()
    {
        var first = SeededPointSet2d.Generate(512, -12d, -4d, 8d, 9d, 123u);
        var second = SeededPointSet2d.Generate(512, -12d, -4d, 8d, 9d, 123u);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Generate_ChangesSamplesForDifferentSeeds()
    {
        var first = SeededPointSet2d.Generate(64, 0d, 0d, 1d, 1d, 1u);
        var second = SeededPointSet2d.Generate(64, 0d, 0d, 1d, 1d, 2u);

        Assert.That(second, Is.Not.EqualTo(first));
    }

    [Test]
    public void Generate_RespectsHalfOpenBounds()
    {
        const double minX = -3.5d;
        const double minY = 10d;
        const double maxX = 7.25d;
        const double maxY = 12.5d;

        var points = SeededPointSet2d.Generate(2048, minX, minY, maxX, maxY, 42u);

        Assert.That(points, Has.Count.EqualTo(2048));
        foreach (var point in points)
        {
            Assert.That(point.X, Is.GreaterThanOrEqualTo(minX).And.LessThan(maxX));
            Assert.That(point.Y, Is.GreaterThanOrEqualTo(minY).And.LessThan(maxY));
        }
    }

    [Test]
    public void Generate_EmptyResultIsValid()
    {
        var points = SeededPointSet2d.Generate(0, -1d, -1d, 1d, 1d, 0u);

        Assert.That(points, Is.Empty);
    }

    [Test]
    public void Generate_ResultDoesNotExposeMutableArray()
    {
        var points = SeededPointSet2d.Generate(4, 0d, 0d, 1d, 1d, 7u);

        Assert.That(points, Is.Not.InstanceOf<Vector2d[]>());
        Assert.Throws<NotSupportedException>(() => ((IList<Vector2d>)points)[0] = new Vector2d(0d, 0d));
    }

    [Test]
    public void Generate_RejectsCountOutsideConfiguredLimit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeededPointSet2d.Generate(-1, 0d, 0d, 1d, 1d, 0u));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeededPointSet2d.Generate(SeededPointSet2d.MaximumPointCount + 1, 0d, 0d, 1d, 1d, 0u));
    }

    [Test]
    public void Generate_RejectsInvalidOrNonFiniteBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeededPointSet2d.Generate(1, 1d, 0d, 1d, 1d, 0u));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeededPointSet2d.Generate(1, 0d, 0d, double.PositiveInfinity, 1d, 0u));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeededPointSet2d.Generate(1, double.NaN, 0d, 1d, 1d, 0u));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeededPointSet2d.Generate(1, -double.MaxValue, 0d, double.MaxValue, 1d, 0u));
    }

    [Test]
    public void Generate_HandlesAdjacentRepresentableBoundsWithoutReturningExclusiveEndpoint()
    {
        var minimum = 1d;
        var maximum = Math.BitIncrement(minimum);

        var points = SeededPointSet2d.Generate(128, minimum, minimum, maximum, maximum, 3u);

        Assert.That(points, Is.All.Matches<Vector2d>(p => p.X == minimum && p.Y == minimum));
    }
}
