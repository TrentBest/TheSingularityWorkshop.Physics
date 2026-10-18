using NUnit.Framework;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests.Astronomy;

[TestFixture]
public sealed class OrbitalEncounterCoordinatorTests
{
    [Test]
    public void ResolveLinearMergers_RechecksContactsAfterEarlierMerger()
    {
        var a = Body("a", 1d, 0.5d, new Vector3d(-4d, 0d, 0d), new Vector3d(1d, 0d, 0d));
        var b = Body("b", 1d, 0.5d, Vector3d.Zero, Vector3d.Zero);
        var c = Body("c", 1d, 0.5d, new Vector3d(1.5d, 0d, 0d), Vector3d.Zero);

        var result = OrbitalEncounterCoordinator.ResolveLinearMergers(new[] { c, b, a }, 5d);

        Assert.That(result.ElapsedSeconds, Is.EqualTo(5d));
        Assert.That(result.MergeEvents, Has.Count.EqualTo(2));
        Assert.That(result.MergeEvents[0].BodyAId, Is.EqualTo("a"));
        Assert.That(result.MergeEvents[0].BodyBId, Is.EqualTo("b"));
        Assert.That(result.MergeEvents[1].BodyAId, Does.StartWith("merge["));
        Assert.That(result.MergeEvents[1].BodyBId, Is.EqualTo("c"));
        Assert.That(result.FinalBodies, Has.Count.EqualTo(1));
        Assert.That(result.FinalBodies[0].MassKilograms, Is.EqualTo(3d));
    }

    [Test]
    public void ResolveLinearMergers_SimultaneousContactsUseStableIdentityOrder()
    {
        var a = Body("a", 1d, 0.5d, Vector3d.Zero, Vector3d.Zero);
        var b = Body("b", 1d, 0.5d, new Vector3d(1d, 0d, 0d), Vector3d.Zero);
        var c = Body("c", 1d, 0.5d, new Vector3d(0d, 1d, 0d), Vector3d.Zero);

        var result = OrbitalEncounterCoordinator.ResolveLinearMergers(new[] { c, b, a }, 1d);

        Assert.That(result.MergeEvents, Has.Count.EqualTo(2));
        Assert.That(result.MergeEvents[0].BodyAId, Is.EqualTo("a"));
        Assert.That(result.MergeEvents[0].BodyBId, Is.EqualTo("b"));
        Assert.That(result.FinalBodies, Has.Count.EqualTo(1));
    }

    [Test]
    public void ResolveLinearMergers_DriftsBodiesWhenNoContactOccurs_WithoutMutatingInputs()
    {
        var a = Body("a", 1d, 0.5d, Vector3d.Zero, new Vector3d(2d, 0d, 0d));
        var b = Body("b", 1d, 0.5d, new Vector3d(10d, 0d, 0d), Vector3d.Zero);

        var result = OrbitalEncounterCoordinator.ResolveLinearMergers(new[] { b, a }, 3d);

        Assert.That(result.MergeEvents, Is.Empty);
        Assert.That(result.FinalBodies[0].BodyId, Is.EqualTo("a"));
        Assert.That(result.FinalBodies[0].PositionMeters.X, Is.EqualTo(6d));
        Assert.That(a.PositionMeters.X, Is.Zero);
        Assert.That(result.FinalBodies[1].PositionMeters.X, Is.EqualTo(10d));
    }

    [Test]
    public void ResolveLinearMergers_RejectsDuplicateBodyIdentities()
    {
        var a = Body("same", 1d, 0.5d, Vector3d.Zero, Vector3d.Zero);
        var b = Body("same", 1d, 0.5d, new Vector3d(1d, 0d, 0d), Vector3d.Zero);

        Assert.Throws<ArgumentException>(
            () => OrbitalEncounterCoordinator.ResolveLinearMergers(new[] { a, b }, 1d));
    }

    [Test]
    public void ResolveLinearMergers_RejectsInvalidInterval(double intervalSeconds)
    {
        var body = Body("a", 1d, 0.5d, Vector3d.Zero, Vector3d.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OrbitalEncounterCoordinator.ResolveLinearMergers(new[] { body }, intervalSeconds));
    }

    private static OrbitalBodyState Body(
        string id, double mass, double radius, Vector3d position, Vector3d velocity) =>
        new OrbitalBodyState(id, mass, radius, position, velocity).EnsureValid();
}
