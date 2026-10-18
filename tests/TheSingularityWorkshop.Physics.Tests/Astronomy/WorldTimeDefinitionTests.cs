using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class WorldTimeDefinitionTests
{
    [Test]
    public void EarthReferenceConversion_IsIndependentOfLocalRotationPeriod()
    {
        var world = new WorldTimeDefinition("example-world", "epoch-0", 36000d);

        Assert.That(world.GetEarthReferenceDays(86400d), Is.EqualTo(1d));
        Assert.That(world.LocalRotationPeriodInEarthDays(), Is.EqualTo(36000d / 86400d));
        Assert.That(world.GetLocalRotationCycles(86400d), Is.EqualTo(2.4d));
    }

    [Test]
    public void RotationPhase_WrapsIntoUnitInterval()
    {
        var world = new WorldTimeDefinition("example-world", "epoch-0", 100d);

        Assert.That(world.GetLocalRotationPhase(250d), Is.EqualTo(0.5d));
    }

    [TestCase("")]
    [TestCase(" ")]
    public void InvalidWorldId_IsRejected(string worldId)
    {
        var world = new WorldTimeDefinition(worldId, "epoch-0", 100d);

        Assert.Throws<ArgumentException>(() => world.EnsureValid());
    }

    [TestCase(0d)]
    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void InvalidRotationPeriod_IsRejected(double period)
    {
        var world = new WorldTimeDefinition("example-world", "epoch-0", period);

        Assert.Throws<ArgumentException>(() => world.EnsureValid());
    }
}
