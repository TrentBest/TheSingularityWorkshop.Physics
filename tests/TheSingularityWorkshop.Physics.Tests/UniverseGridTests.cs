using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class UniverseGridTests
{
    [Test]
    public void TenByTenByTenGrid_MapsLinearIndexFortyTwoDeterministically()
    {
        var grid = new UniverseGrid(10, 10, 10);

        var coordinates = grid.GetCoordinates(42);

        Assert.That(coordinates, Is.EqualTo((2, 4, 0)));
        Assert.That(grid.GetLinearIndex(coordinates.X, coordinates.Y, coordinates.Z), Is.EqualTo(42));
    }

    [Test]
    public void Grid_RoundTripsEveryCell()
    {
        var grid = new UniverseGrid(10, 10, 10);

        for (var index = 0; index < grid.CellCount; index++)
        {
            var coordinates = grid.GetCoordinates(index);
            Assert.That(grid.GetLinearIndex(coordinates.X, coordinates.Y, coordinates.Z), Is.EqualTo(index));
        }
    }

    [Test]
    public void Grid_RejectsInvalidDimensionsAndCoordinates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UniverseGrid(0, 10, 10));
        var grid = new UniverseGrid(10, 10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetLinearIndex(10, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetCoordinates(1000));
    }
}

[TestFixture]
public sealed class DeterministicCellGeneratorTests
{
    [Test]
    public void SeedDerivation_IsStableAndIndependentOfCallOrder()
    {
        var first = DeterministicCellGenerator.DeriveSeed(1234UL, 2, 4, 0);
        _ = DeterministicCellGenerator.DeriveSeed(1234UL, 9, 9, 9);
        var second = DeterministicCellGenerator.DeriveSeed(1234UL, 2, 4, 0);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void SeedDerivation_DistinguishesCoordinatesAndUniverseSeeds()
    {
        var baseline = DeterministicCellGenerator.DeriveSeed(1234UL, 2, 4, 0);

        Assert.That(DeterministicCellGenerator.DeriveSeed(1234UL, 3, 4, 0), Is.Not.EqualTo(baseline));
        Assert.That(DeterministicCellGenerator.DeriveSeed(4321UL, 2, 4, 0), Is.Not.EqualTo(baseline));
    }

    [Test]
    public void UnitIntervalValue_IsInHalfOpenUnitInterval()
    {
        var value = DeterministicCellGenerator.DeriveUnitInterval(42UL, 2, 4, 0);

        Assert.That(value, Is.GreaterThanOrEqualTo(0d));
        Assert.That(value, Is.LessThan(1d));
    }
}
