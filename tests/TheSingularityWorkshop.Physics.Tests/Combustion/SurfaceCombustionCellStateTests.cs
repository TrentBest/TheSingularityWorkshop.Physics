using NUnit.Framework;
using TheSingularityWorkshop.Physics.Combustion;

namespace TheSingularityWorkshop.Physics.Tests.Combustion;

[TestFixture]
public sealed class SurfaceCombustionCellStateTests
{
    private static SurfaceCombustionCellState ValidCell() => new(
        "cell-0", "material:wax", 0.0001d, 0.001d, 0.0002d, 15d,
        0d, 0d, 1d);

    [Test]
    public void ValidCellWithUnitNormalIsAccepted()
    {
        var cell = ValidCell();

        Assert.That(cell.IsValid, Is.True);
        Assert.DoesNotThrow(cell.EnsureValid);
    }

    [Test]
    public void NormalMustBeUnitLength()
    {
        var cell = ValidCell() with { NormalX = 0d, NormalY = 0d, NormalZ = 2d };

        Assert.That(cell.IsValid, Is.False);
        Assert.Throws<ArgumentException>(cell.EnsureValid);
    }

    [Test]
    public void InventoriesCannotBeNegative()
    {
        var cell = ValidCell() with { OxygenMassKilograms = -0.001d };

        Assert.That(cell.IsValid, Is.False);
    }

    [Test]
    public void AreaMustBePositiveAndFinite()
    {
        Assert.That((ValidCell() with { AreaSquareMeters = 0d }).IsValid, Is.False);
        Assert.That((ValidCell() with { AreaSquareMeters = double.PositiveInfinity }).IsValid, Is.False);
    }

    [Test]
    public void CellRequiresStableIdentityAndMaterialIdentity()
    {
        Assert.That((ValidCell() with { CellId = "" }).IsValid, Is.False);
        Assert.That((ValidCell() with { MaterialId = "" }).IsValid, Is.False);
    }
}
