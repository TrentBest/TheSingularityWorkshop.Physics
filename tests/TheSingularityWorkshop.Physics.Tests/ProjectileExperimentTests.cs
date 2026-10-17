using System.Text.Json;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ProjectileExperimentTests
{
    [Test]
    public void Run_RecordsInitialAndEveryCompletedStep()
    {
        var definition = new ProjectileExperimentDefinition(
            "castle-baseline-001",
            new Vector3d(1d, 2d, 3d),
            10d,
            30d,
            0d,
            new Vector3d(0d, 0d, -9.81d),
            0.1d,
            20);

        var result = ProjectileExperiment.Run(definition);

        Assert.That(result.Samples, Has.Count.EqualTo(21));
        Assert.That(result.CompletedSteps, Is.EqualTo(20));
        Assert.That(result.Duration, Is.EqualTo(2d).Within(1e-12));
        Assert.That(result.Samples[0].Position, Is.EqualTo(definition.InitialPosition));
        Assert.That(result.Samples[0].Velocity.X, Is.EqualTo(10d * Math.Cos(Math.PI / 6d)).Within(1e-12));
        Assert.That(result.Samples[0].Velocity.Y, Is.EqualTo(0d).Within(1e-12));
        Assert.That(result.Samples[0].Velocity.Z, Is.EqualTo(5d).Within(1e-12));
        Assert.That(result.FinalState.ElapsedTime, Is.EqualTo(2d).Within(1e-12));
    }

    [Test]
    public void Run_SameDefinition_ProducesSameSamples()
    {
        var definition = new ProjectileExperimentDefinition(
            "repeatable-run",
            Vector3d.Zero,
            25d,
            42d,
            17d,
            new Vector3d(0d, 0d, -9.81d),
            0.02d,
            250);

        var first = ProjectileExperiment.Run(definition);
        var second = ProjectileExperiment.Run(definition);

        Assert.That(second.Samples, Is.EqualTo(first.Samples));
    }

    [Test]
    public void RunRecord_PreservesRunInputsAndCoordinateConvention()
    {
        var result = ProjectileExperiment.Run(new ProjectileExperimentDefinition(
            "recorded-run",
            new Vector3d(4d, 5d, 6d),
            12d,
            15d,
            90d,
            new Vector3d(0d, 0d, -9.81d),
            0.05d,
            10));

        var record = ProjectileExperimentRunRecord.FromResult(result);
        var json = record.ToJson();
        using var document = JsonDocument.Parse(json);

        Assert.That(record.Definition.RunId, Is.EqualTo("recorded-run"));
        Assert.That(record.Samples, Has.Count.EqualTo(11));
        Assert.That(record.ModelId, Is.EqualTo("kinematics.constant-acceleration-projectile"));
        Assert.That(record.CoordinateConvention, Does.Contain("z-up"));
        Assert.That(document.RootElement.GetProperty("schemaVersion").GetInt32(), Is.EqualTo(1));
        Assert.That(document.RootElement.GetProperty("samples").GetArrayLength(), Is.EqualTo(11));
        Assert.That(json, Is.EqualTo(ProjectileExperimentRunRecord.FromResult(result).ToJson()));
    }

    [Test]
    public void Run_RejectsInvalidDefinitionBeforeSampling()
    {
        var invalidRunId = new ProjectileExperimentDefinition(
            " ",
            Vector3d.Zero,
            10d,
            30d,
            0d,
            new Vector3d(0d, 0d, -9.81d),
            0.1d,
            10);
        Assert.Throws<ArgumentException>(() => ProjectileExperiment.Run(invalidRunId));

        var invalidSpeed = invalidRunId with { RunId = "valid-id", LaunchSpeed = -1d };
        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectileExperiment.Run(invalidSpeed));

        var invalidElevation = invalidRunId with { RunId = "valid-id", LaunchSpeed = 1d, ElevationDegrees = 91d };
        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectileExperiment.Run(invalidElevation));

        var invalidSteps = invalidRunId with { RunId = "valid-id", StepCount = 0 };
        Assert.Throws<ArgumentOutOfRangeException>(() => ProjectileExperiment.Run(invalidSteps));
    }
}
