using NUnit.Framework;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ProjectileTargetIntersectionTests
{
    private static ProjectileExperimentResult CreateRun(
        string runId,
        Vector3d initialPosition,
        double speed,
        double elevation,
        double azimuth,
        Vector3d gravity,
        double deltaTime = 0.1d,
        int steps = 100) =>
        ProjectileExperiment.Run(new ProjectileExperimentDefinition(
            runId, initialPosition, speed, elevation, azimuth, gravity, deltaTime, steps));

    [Test]
    public void FindFirst_TargetCrossedBetweenSamples_ReportsAnalyticalIntersection()
    {
        var run = CreateRun(
            "straight-line",
            Vector3d.Zero,
            10d,
            0d,
            0d,
            Vector3d.Zero,
            deltaTime: 1d,
            steps: 2);
        var target = new AxisAlignedBounds(4.5d, -1d, -1d, 5.5d, 1d, 1d);

        var result = ProjectileTargetIntersection.FindFirst(run, "gate-west", target);

        Assert.That(result.Hit, Is.True);
        Assert.That(result.RunId, Is.EqualTo("straight-line"));
        Assert.That(result.TargetId, Is.EqualTo("gate-west"));
        Assert.That(result.FirstIntersectionTime, Is.EqualTo(0.45d).Within(1e-10));
        Assert.That(result.IntersectionState!.Value.Position.X, Is.EqualTo(4.5d).Within(1e-9));
    }

    [Test]
    public void FindFirst_ParabolicTrajectory_HitsElevatedTarget()
    {
        var run = CreateRun(
            "arc",
            Vector3d.Zero,
            10d,
            90d,
            0d,
            new Vector3d(0d, 0d, -9.81d),
            deltaTime: 0.5d,
            steps: 4);
        var target = new AxisAlignedBounds(-0.1d, -0.1d, 4.9d, 0.1d, 0.1d, 5.1d);

        var result = ProjectileTargetIntersection.FindFirst(run, "upper-target", target);

        Assert.That(result.Hit, Is.True);
        Assert.That(result.FirstIntersectionTime, Is.EqualTo((10d - Math.Sqrt(100d - (2d * 9.81d * 4.9d))) / 9.81d).Within(1e-8));
        Assert.That(result.IntersectionState!.Value.Position.Z, Is.EqualTo(4.9d).Within(1e-8));
    }

    [Test]
    public void FindFirst_StartInsideTarget_IsHitAtTimeZero()
    {
        var run = CreateRun("inside", new Vector3d(5d, 0d, 0d), 10d, 0d, 0d, Vector3d.Zero);
        var target = new AxisAlignedBounds(4d, -1d, -1d, 6d, 1d, 1d);

        var result = ProjectileTargetIntersection.FindFirst(run, "starting-cell", target);

        Assert.That(result.Hit, Is.True);
        Assert.That(result.FirstIntersectionTime, Is.EqualTo(0d));
    }

    [Test]
    public void FindFirst_TargetOutsideTrajectoryDuration_IsMiss()
    {
        var run = CreateRun("short-run", Vector3d.Zero, 10d, 0d, 0d, Vector3d.Zero, 0.1d, 5);
        var target = new AxisAlignedBounds(6d, -1d, -1d, 7d, 1d, 1d);

        var result = ProjectileTargetIntersection.FindFirst(run, "far-wall", target);

        Assert.That(result.Hit, Is.False);
        Assert.That(result.FirstIntersectionTime, Is.Null);
        Assert.That(result.IntersectionState, Is.Null);
    }

    [Test]
    public void FindFirst_RejectsEmptyTargetIdentity()
    {
        var run = CreateRun("valid-run", Vector3d.Zero, 10d, 0d, 0d, Vector3d.Zero);
        var target = new AxisAlignedBounds(1d, -1d, -1d, 2d, 1d, 1d);

        Assert.Throws<ArgumentException>(() =>
            ProjectileTargetIntersection.FindFirst(run, " ", target));
    }
}
