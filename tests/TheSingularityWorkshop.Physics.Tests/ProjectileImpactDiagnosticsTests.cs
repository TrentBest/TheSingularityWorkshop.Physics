using NUnit.Framework;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ProjectileImpactDiagnosticsTests
{
    [Test]
    public void Compute_ConfirmedImpact_ReportsMomentumAndKineticEnergy()
    {
        var experiment = ProjectileExperiment.Run(new ProjectileExperimentDefinition(
            "impact-run",
            Vector3d.Zero,
            10d,
            0d,
            0d,
            Vector3d.Zero,
            0.1d,
            20));
        var target = new AxisAlignedBounds(4.5d, -1d, -1d, 5.5d, 1d, 1d);
        var intersection = ProjectileTargetIntersection.FindFirst(experiment, "gate", target);

        var result = ProjectileImpactDiagnostics.Compute(experiment, intersection, 2d);

        Assert.That(result.RunId, Is.EqualTo("impact-run"));
        Assert.That(result.TargetId, Is.EqualTo("gate"));
        Assert.That(result.TimeSeconds, Is.EqualTo(0.45d).Within(1e-10));
        Assert.That(result.MassKilograms, Is.EqualTo(2d));
        Assert.That(result.MomentumKilogramMetresPerSecond.X, Is.EqualTo(20d).Within(1e-10));
        Assert.That(result.MomentumKilogramMetresPerSecond.Y, Is.EqualTo(0d).Within(1e-10));
        Assert.That(result.MomentumKilogramMetresPerSecond.Z, Is.EqualTo(0d).Within(1e-10));
        Assert.That(result.KineticEnergyJoules, Is.EqualTo(100d).Within(1e-10));
    }

    [Test]
    public void Compute_RejectsMissAndRunMismatch()
    {
        var first = ProjectileExperiment.Run(new ProjectileExperimentDefinition(
            "first-run", Vector3d.Zero, 10d, 0d, 0d, Vector3d.Zero, 0.1d, 5));
        var second = ProjectileExperiment.Run(new ProjectileExperimentDefinition(
            "second-run", Vector3d.Zero, 10d, 0d, 0d, Vector3d.Zero, 0.1d, 5));
        var target = new AxisAlignedBounds(6d, -1d, -1d, 7d, 1d, 1d);
        var miss = ProjectileTargetIntersection.FindFirst(first, "far-wall", target);

        Assert.Throws<ArgumentException>(() => ProjectileImpactDiagnostics.Compute(first, miss, 1d));

        var hit = ProjectileTargetIntersection.FindFirst(
            first, "near-wall", new AxisAlignedBounds(1d, -1d, -1d, 2d, 1d, 1d));
        Assert.Throws<ArgumentException>(() => ProjectileImpactDiagnostics.Compute(second, hit, 1d));
    }

    [TestCase(0d)]
    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void Compute_RejectsInvalidMass(double mass)
    {
        var experiment = ProjectileExperiment.Run(new ProjectileExperimentDefinition(
            "mass-run", Vector3d.Zero, 10d, 0d, 0d, Vector3d.Zero, 0.1d, 20));
        var hit = ProjectileTargetIntersection.FindFirst(
            experiment, "target", new AxisAlignedBounds(1d, -1d, -1d, 2d, 1d, 1d));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProjectileImpactDiagnostics.Compute(experiment, hit, mass));
    }
}
