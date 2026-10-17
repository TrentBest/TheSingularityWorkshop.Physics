using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Math;

[TestFixture]
public sealed class Vector3dBatchTests
{
    [Test]
    public void Add_ComputesStructureOfArrays()
    {
        double[] ax = [1d, 2d], ay = [3d, 4d], az = [5d, 6d];
        double[] bx = [10d, 20d], by = [30d, 40d], bz = [50d, 60d];
        double[] ox = new double[2], oy = new double[2], oz = new double[2];

        Vector3dBatch.Add(ax, ay, az, bx, by, bz, ox, oy, oz);

        Assert.That(ox, Is.EqualTo(new[] { 11d, 22d }));
        Assert.That(oy, Is.EqualTo(new[] { 33d, 44d }));
        Assert.That(oz, Is.EqualTo(new[] { 55d, 66d }));
    }

    [Test]
    public void Scale_AllowsInPlaceSameIndexOperation()
    {
        double[] x = [1d, -2d], y = [3d, -4d], z = [5d, -6d];

        Vector3dBatch.Scale(x, y, z, 2d, x, y, z);

        Assert.That(x, Is.EqualTo(new[] { 2d, -4d }));
        Assert.That(y, Is.EqualTo(new[] { 6d, -8d }));
        Assert.That(z, Is.EqualTo(new[] { 10d, -12d }));
    }

    [Test]
    public void Dot_ComputesOneScalarPerPair()
    {
        double[] ax = [1d, 0d], ay = [0d, 1d], az = [0d, 0d];
        double[] bx = [1d, 1d], by = [0d, 2d], bz = [0d, 0d];
        double[] output = new double[2];

        Vector3dBatch.Dot(ax, ay, az, bx, by, bz, output);

        Assert.That(output, Is.EqualTo(new[] { 1d, 2d }));
    }

    [Test]
    public void Operations_RejectMismatchedSpanLengths()
    {
        Assert.Throws<ArgumentException>(() =>
            Vector3dBatch.Add(new double[1], new double[1], new double[1],
                new double[1], new double[1], new double[1],
                new double[2], new double[2], new double[2]));
    }

    [Test]
    public void Scale_RejectsNonFiniteScalar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Vector3dBatch.Scale(new double[0], new double[0], new double[0],
                double.NaN, new double[0], new double[0], new double[0]));
    }
}
