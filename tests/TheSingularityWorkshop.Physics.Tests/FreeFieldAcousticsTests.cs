using NUnit.Framework;
using TheSingularityWorkshop.Physics.Acoustics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class FreeFieldAcousticsTests
{
    [Test]
    public void Observe_PropagationDelayMatchesDistanceAndConfiguredSoundSpeed()
    {
        var source = new AcousticSource("bell", new AcousticPosition3(343d, 0d, 0d), 94d);
        var listener = new AcousticListener("listener", new AcousticPosition3(0d, 0d, 0d));

        var observation = FreeFieldAcoustics.Observe(source, listener);

        Assert.That(observation.DistanceMeters, Is.EqualTo(343d).Within(1e-12));
        Assert.That(observation.PropagationDelaySeconds, Is.EqualTo(1d).Within(1e-12));
        Assert.That(observation.SourceId, Is.EqualTo("bell"));
        Assert.That(observation.ListenerId, Is.EqualTo("listener"));
    }

    [Test]
    public void Observe_FreeFieldPressureLevelFallsByApproximatelySixDecibelsPerDistanceDoubling()
    {
        var source = new AcousticSource("speaker", new AcousticPosition3(0d, 0d, 0d), 94d);
        var oneMetre = new AcousticListener("one", new AcousticPosition3(1d, 0d, 0d));
        var twoMetres = new AcousticListener("two", new AcousticPosition3(2d, 0d, 0d));
        var tenMetres = new AcousticListener("ten", new AcousticPosition3(10d, 0d, 0d));

        var atOneMetre = FreeFieldAcoustics.Observe(source, oneMetre);
        var atTwoMetres = FreeFieldAcoustics.Observe(source, twoMetres);
        var atTenMetres = FreeFieldAcoustics.Observe(source, tenMetres);

        Assert.That(atOneMetre.ReceivedLevelDbSpl, Is.EqualTo(94d).Within(1e-12));
        Assert.That(atTwoMetres.ReceivedLevelDbSpl, Is.EqualTo(94d - 20d * Math.Log10(2d)).Within(1e-12));
        Assert.That(atTenMetres.ReceivedLevelDbSpl, Is.EqualTo(74d).Within(1e-12));
    }

    [Test]
    public void Observe_ReturnsNormalizedDirectionFromListenerTowardSource()
    {
        var source = new AcousticSource("impact", new AcousticPosition3(3d, 4d, 0d), 80d);
        var listener = new AcousticListener("observer", new AcousticPosition3(0d, 0d, 0d));

        var observation = FreeFieldAcoustics.Observe(source, listener);

        Assert.That(observation.DirectionX, Is.EqualTo(0.6d).Within(1e-12));
        Assert.That(observation.DirectionY, Is.EqualTo(0.8d).Within(1e-12));
        Assert.That(observation.DirectionZ, Is.EqualTo(0d).Within(1e-12));
        Assert.That(observation.HasDirection, Is.True);
        Assert.That(Math.Sqrt(observation.DirectionX * observation.DirectionX +
                              observation.DirectionY * observation.DirectionY +
                              observation.DirectionZ * observation.DirectionZ), Is.EqualTo(1d).Within(1e-12));
    }

    [Test]
    public void Observe_CoincidentSourceAndListenerHaveZeroDelayAndNoDefinedDirection()
    {
        var position = new AcousticPosition3(2d, -1d, 4d);
        var source = new AcousticSource("near", position, 90d, referenceDistanceMeters: 0.5d);
        var listener = new AcousticListener("listener", position);

        var observation = FreeFieldAcoustics.Observe(source, listener);

        Assert.That(observation.DistanceMeters, Is.EqualTo(0d));
        Assert.That(observation.PropagationDelaySeconds, Is.EqualTo(0d));
        Assert.That(observation.ReceivedLevelDbSpl, Is.EqualTo(90d));
        Assert.That(observation.HasDirection, Is.False);
        Assert.That(observation.DirectionX, Is.EqualTo(0d));
        Assert.That(observation.DirectionY, Is.EqualTo(0d));
        Assert.That(observation.DirectionZ, Is.EqualTo(0d));
    }

    [Test]
    public void Observe_UsesConfiguredReferenceDistanceForNearFieldLevelClamp()
    {
        var source = new AcousticSource("source", new AcousticPosition3(0d, 0d, 0d), 72d, referenceDistanceMeters: 2d);
        var listener = new AcousticListener("listener", new AcousticPosition3(1d, 0d, 0d));

        var observation = FreeFieldAcoustics.Observe(source, listener);

        Assert.That(observation.ReceivedLevelDbSpl, Is.EqualTo(72d).Within(1e-12));
        Assert.That(observation.PropagationDelaySeconds, Is.EqualTo(1d / 343d).Within(1e-12));
    }

    [Test]
    public void Observe_RejectsInvalidSoundSpeed()
    {
        var source = new AcousticSource("source", new AcousticPosition3(0d, 0d, 0d), 80d);
        var listener = new AcousticListener("listener", new AcousticPosition3(1d, 0d, 0d));

        Assert.Throws<ArgumentOutOfRangeException>(() => FreeFieldAcoustics.Observe(source, listener, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => FreeFieldAcoustics.Observe(source, listener, double.NaN));
    }

    [Test]
    public void Constructors_RejectInvalidSourceMetadata()
    {
        Assert.Throws<ArgumentException>(() => new AcousticSource(" ", new AcousticPosition3(0d, 0d, 0d), 80d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcousticSource("source", new AcousticPosition3(0d, 0d, 0d), double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcousticSource("source", new AcousticPosition3(0d, 0d, 0d), 80d, 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcousticPosition3(double.NaN, 0d, 0d));
    }
}
