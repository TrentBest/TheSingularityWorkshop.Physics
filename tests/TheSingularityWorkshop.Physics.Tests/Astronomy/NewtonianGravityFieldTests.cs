using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class NewtonianGravityFieldTests
{
    private const double G = 2d;

    [Test]
    public void Sample_SinglePointMass_ReturnsNewtonianAccelerationAndSpecificPotential()
    {
        var source = new GravitatingPointMass("star", 8d, Vector3d.Zero);

        var field = NewtonianGravityField.Sample(
            [source],
            new Vector3d(2d, 0d, 0d),
            G);

        Assert.That(field.AccelerationMetersPerSecondSquared.X, Is.EqualTo(-4d).Within(1e-14));
        Assert.That(field.AccelerationMetersPerSecondSquared.Y, Is.EqualTo(0d));
        Assert.That(field.AccelerationMetersPerSecondSquared.Z, Is.EqualTo(0d));
        Assert.That(field.PotentialMetersSquaredPerSecondSquared, Is.EqualTo(-8d).Within(1e-14));
        Assert.That(field.ContributingBodyCount, Is.EqualTo(1));
    }

    [Test]
    public void Sample_SuperposesSources_AndIsIndependentOfEnumerationOrder()
    {
        var left = new GravitatingPointMass("a", 1d, new Vector3d(-1d, 0d, 0d));
        var right = new GravitatingPointMass("b", 1d, new Vector3d(1d, 0d, 0d));
        var point = new Vector3d(0d, 2d, 0d);

        var forward = NewtonianGravityField.Sample([left, right], point, G);
        var reverse = NewtonianGravityField.Sample([right, left], point, G);

        Assert.That(forward.AccelerationMetersPerSecondSquared.X,
            Is.EqualTo(reverse.AccelerationMetersPerSecondSquared.X));
        Assert.That(forward.AccelerationMetersPerSecondSquared.Y,
            Is.EqualTo(reverse.AccelerationMetersPerSecondSquared.Y));
        Assert.That(forward.AccelerationMetersPerSecondSquared.Z,
            Is.EqualTo(reverse.AccelerationMetersPerSecondSquared.Z));
        Assert.That(forward.PotentialMetersSquaredPerSecondSquared,
            Is.EqualTo(reverse.PotentialMetersSquaredPerSecondSquared));
        Assert.That(forward.ContributingBodyCount, Is.EqualTo(2));
    }

    [Test]
    public void Sample_RejectsSingularityAndInvalidSourceData()
    {
        var source = new GravitatingPointMass("star", 1d, Vector3d.Zero);
        Assert.Throws<ArgumentException>(() =>
            NewtonianGravityField.Sample([source], Vector3d.Zero, G));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NewtonianGravityField.Sample(
                [new GravitatingPointMass("star", 0d, Vector3d.Zero)],
                new Vector3d(1d, 0d, 0d),
                G));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NewtonianGravityField.Sample([source], new Vector3d(1d, 0d, 0d), 0d));
    }

    [Test]
    public void Sample_RejectsDuplicateStableIds()
    {
        var first = new GravitatingPointMass("same", 1d, Vector3d.Zero);
        var second = new GravitatingPointMass("same", 2d, new Vector3d(2d, 0d, 0d));

        Assert.Throws<ArgumentException>(() =>
            NewtonianGravityField.Sample([first, second], new Vector3d(1d, 0d, 0d), G));
    }
}
