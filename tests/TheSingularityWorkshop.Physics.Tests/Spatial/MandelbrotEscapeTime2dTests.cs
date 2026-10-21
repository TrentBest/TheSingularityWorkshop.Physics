using NUnit.Framework;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Spatial;

[TestFixture]
public sealed class MandelbrotEscapeTime2dTests
{
    [Test]
    public void Generate_UsesStableRowMajorCellCenters()
    {
        var samples = MandelbrotEscapeTime2d.Generate(2, 2, -2d, -1d, 2d, 1d, 8);

        Assert.That(samples, Has.Count.EqualTo(4));
        Assert.That(samples.Select(s => (s.Column, s.Row)),
            Is.EqualTo(new[] { (0, 0), (1, 0), (0, 1), (1, 1) }));
        Assert.That(samples[0].Real, Is.EqualTo(-1d));
        Assert.That(samples[0].Imaginary, Is.EqualTo(-0.5d));
        Assert.That(samples[3].Real, Is.EqualTo(1d));
        Assert.That(samples[3].Imaginary, Is.EqualTo(0.5d));
    }

    [Test]
    public void Generate_ClassifiesKnownInteriorAndEscapingSamples()
    {
        var interior = MandelbrotEscapeTime2d.Generate(1, 1, -0.5d, -0.5d, 0.5d, 0.5d, 32);
        var escaping = MandelbrotEscapeTime2d.Generate(1, 1, 2.5d, -0.5d, 3.5d, 0.5d, 32);

        Assert.That(interior[0].Escaped, Is.False);
        Assert.That(interior[0].Iterations, Is.EqualTo(32));
        Assert.That(escaping[0].Escaped, Is.True);
        Assert.That(escaping[0].Iterations, Is.EqualTo(1));
    }

    [Test]
    public void Generate_RepeatsExactlyForSameInputs()
    {
        var first = MandelbrotEscapeTime2d.Generate(24, 16, -2.5d, -1.5d, 1d, 1.5d, 80);
        var second = MandelbrotEscapeTime2d.Generate(24, 16, -2.5d, -1.5d, 1d, 1.5d, 80);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Generate_ReturnsReadOnlyResults()
    {
        var samples = MandelbrotEscapeTime2d.Generate(2, 2, -2d, -1d, 2d, 1d, 8);

        Assert.That(samples, Is.Not.InstanceOf<MandelbrotSample2d[]>());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<MandelbrotSample2d>)samples)[0] = default);
    }

    [Test]
    public void Generate_RejectsResolutionAndWorkOverLimits()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(0, 1, -2d, -1d, 1d, 1d, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(513, 512, -2d, -1d, 1d, 1d, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(128, 128, -2d, -1d, 1d, 1d, 4096));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(1, 1, -2d, -1d, 1d, 1d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(1, 1, -2d, -1d, 1d, 1d, 4097));
    }

    [Test]
    public void Generate_RejectsInvalidOrNonFiniteWindowBounds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(1, 1, 1d, -1d, 1d, 1d, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(1, 1, double.NaN, -1d, 1d, 1d, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MandelbrotEscapeTime2d.Generate(1, 1, -double.MaxValue, -1d, double.MaxValue, 1d, 10));
    }
}
