using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class OrbitalTidePotentialBakerTests
{
    private static OrbitalTideForcingInput ExampleInput(
        double initialSubBodyLongitude = 0d,
        double observerLongitude = 0d) =>
        new(
            WorldId: "illustrative-world",
            EpochId: "illustrative-epoch",
            PerturberMassKg: 7.35e22,
            OrbitalDistanceMeters: 3.84e8,
            WorldRadiusMeters: 6.37e6,
            GravitationalConstantM3PerKgSecondSquared: 6.67430e-11,
            OrbitalPeriodSeconds: 27.3d * 86400d,
            WorldRotationPeriodSeconds: 24d * 3600d,
            InitialSubBodyLongitudeRadians: initialSubBodyLongitude,
            ObserverLongitudeRadians: observerLongitude);

    [Test]
    public void Bake_IsDeterministic_AndIncludesBothEndpoints()
    {
        var options = new TidalTableBakeOptions(1800d, 49);
        var first = OrbitalTidePotentialBaker.Bake(ExampleInput(), options);
        var second = OrbitalTidePotentialBaker.Bake(ExampleInput(), options);

        Assert.That(first.Count, Is.EqualTo(49));
        Assert.That(first.EndElapsedSeconds, Is.EqualTo(86400d));
        Assert.That(first.CopySamples(), Is.EqualTo(second.CopySamples()));
        Assert.That(first.GetSample(0).ElapsedSeconds, Is.EqualTo(0d));
        Assert.That(first.GetSample(48).ElapsedSeconds, Is.EqualTo(86400d));
    }

    [Test]
    public void SampleAt_InterpolatesInsideHorizon_AndRejectsOutside()
    {
        var table = OrbitalTidePotentialBaker.Bake(ExampleInput(), new TidalTableBakeOptions(10d, 3));

        var middle = table.SampleAt(5d);

        Assert.That(middle.ElapsedSeconds, Is.EqualTo(5d));
        Assert.That(middle.PotentialM2PerS2, Is.EqualTo(
            (table.GetSample(0).PotentialM2PerS2 + table.GetSample(1).PotentialM2PerS2) / 2d).Within(1e-12));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.SampleAt(-1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => table.SampleAt(20.1d));
    }

    [Test]
    public void CopySamples_DoesNotExposeMutableInternalStorage()
    {
        var table = OrbitalTidePotentialBaker.Bake(ExampleInput(), new TidalTableBakeOptions(10d, 3));
        var copy = table.CopySamples();
        copy[0] = new TidalTableSample(0d, 0d, 0d, TidalForcingState.TurningPoint);

        Assert.That(table.GetSample(0).PotentialM2PerS2, Is.Not.EqualTo(0d));
    }

    [Test]
    public void InitialAlignment_ProducesStationaryMaximumPotential()
    {
        var table = OrbitalTidePotentialBaker.Bake(
            ExampleInput(initialSubBodyLongitude: 0d, observerLongitude: 0d),
            new TidalTableBakeOptions(60d, 2));

        Assert.That(table.GetSample(0).State, Is.EqualTo(TidalForcingState.TurningPoint));
        Assert.That(table.GetSample(0).RateOfChangeM2PerS3, Is.EqualTo(0d).Within(1e-20));
    }

    [Test]
    public void InvalidInputs_AreRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            OrbitalTidePotentialBaker.Bake(
                ExampleInput() with { OrbitalDistanceMeters = 1d },
                new TidalTableBakeOptions(1d, 2)));
        Assert.Throws<ArgumentException>(() =>
            OrbitalTidePotentialBaker.Bake(ExampleInput(), new TidalTableBakeOptions(0d, 2)));
    }
}
