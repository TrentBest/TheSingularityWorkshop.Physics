using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class Bounds3dTests
{
    [Test]
    public void Constructor_RejectsInvertedAxes()
    {
        Assert.Throws<ArgumentException>(() => new Bounds3d(
            new Vector3d(2d, 0d, 0d),
            new Vector3d(1d, 1d, 1d)));
    }

    [Test]
    public void SizeCenterAndVolume_AreCalculatedFromCorners()
    {
        var bounds = new Bounds3d(
            new Vector3d(-1d, 2d, 0d),
            new Vector3d(3d, 6d, 5d));

        Assert.That(bounds.Size, Is.EqualTo(new Vector3d(4d, 4d, 5d)));
        Assert.That(bounds.Center, Is.EqualTo(new Vector3d(1d, 4d, 2.5d)));
        Assert.That(bounds.Volume, Is.EqualTo(80d));
    }

    [Test]
    public void Contains_IncludesFacesEdgesAndCorners()
    {
        var bounds = new Bounds3d(Vector3d.Zero, new Vector3d(1d, 1d, 1d));

        Assert.That(bounds.Contains(Vector3d.Zero), Is.True);
        Assert.That(bounds.Contains(new Vector3d(1d, 0.5d, 1d)), Is.True);
        Assert.That(bounds.Contains(new Vector3d(1.01d, 0.5d, 0.5d)), Is.False);
    }

    [Test]
    public void Intersects_TreatsTouchingFacesAsIntersection()
    {
        var left = new Bounds3d(Vector3d.Zero, new Vector3d(1d, 1d, 1d));
        var touching = new Bounds3d(new Vector3d(1d, 0d, 0d), new Vector3d(2d, 1d, 1d));
        var separated = new Bounds3d(new Vector3d(1.01d, 0d, 0d), new Vector3d(2d, 1d, 1d));

        Assert.That(left.Intersects(touching), Is.True);
        Assert.That(left.Intersects(separated), Is.False);
    }

    [Test]
    public void Union_ContainsBothInputs()
    {
        var left = new Bounds3d(Vector3d.Zero, new Vector3d(1d, 2d, 3d));
        var right = new Bounds3d(new Vector3d(-2d, 1d, 2d), new Vector3d(4d, 5d, 6d));

        var union = left.Union(right);

        Assert.That(union.Min, Is.EqualTo(new Vector3d(-2d, 0d, 0d)));
        Assert.That(union.Max, Is.EqualTo(new Vector3d(4d, 5d, 6d)));
        Assert.That(union.Contains(left.Min), Is.True);
        Assert.That(union.Contains(right.Max), Is.True);
    }

    [Test]
    public void Volume_ThrowsWhenNotRepresentable()
    {
        var bounds = new Bounds3d(
            Vector3d.Zero,
            new Vector3d(double.MaxValue, 2d, 2d));

        Assert.Throws<ArithmeticException>(() => _ = bounds.Volume);
    }
}
