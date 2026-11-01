using System.Text.Json;
using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class InclinedPlaneExperimentTests
{
    [Test]
    public void Run_RetainsInputsAndComputedOutcome()
    {
        var definition = new InclinedPlaneExperimentDefinition(
            MassKilograms: 2d,
            AngleRadians: Math.PI / 6d,
            VerticalDropMeters: 1.5d);

        var result = InclinedPlaneExperiment.Run(definition);

        Assert.That(result.Definition, Is.EqualTo(definition));
        Assert.That(result.Outcome.PathLengthMeters, Is.EqualTo(3d).Within(1e-12));
        Assert.That(result.Outcome.FinalSpeedMetersPerSecond,
            Is.EqualTo(Math.Sqrt(2d * 9.80665d * 1.5d)).Within(1e-12));
    }

    [Test]
    public void RunRecord_SerializesVersionedInputsUnitsAndActualOutput()
    {
        var result = InclinedPlaneExperiment.Run(
            new InclinedPlaneExperimentDefinition(1d, Math.PI / 4d, 2d));
        var record = InclinedPlaneRunRecord.FromResult(result, "test-revision");

        using var document = JsonDocument.Parse(record.ToJson());
        var root = document.RootElement;

        Assert.That(root.GetProperty("modelId").GetString(),
            Is.EqualTo("mechanics.frictionless-inclined-plane"));
        Assert.That(root.GetProperty("modelVersion").GetString(), Is.EqualTo("1"));
        Assert.That(root.GetProperty("schemaVersion").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("definition").GetProperty("verticalDropMeters").GetDouble(),
            Is.EqualTo(2d));
        Assert.That(root.GetProperty("result").GetProperty("finalSpeedMetersPerSecond").GetDouble(),
            Is.EqualTo(Math.Sqrt(2d * 9.80665d * 2d)).Within(1e-12));
        Assert.That(root.GetProperty("units").GetProperty("angle").GetString(), Is.EqualTo("rad"));
    }

    [Test]
    public void RunRecord_SerializesDeterministically()
    {
        var result = InclinedPlaneExperiment.Run(
            new InclinedPlaneExperimentDefinition(1d, Math.PI / 6d, 2d));
        var record = InclinedPlaneRunRecord.FromResult(result);

        Assert.That(record.ToJson(), Is.EqualTo(record.ToJson()));
    }

    [Test]
    public void Run_RejectsInvalidDefinition()
    {
        var definition = new InclinedPlaneExperimentDefinition(
            MassKilograms: 1d,
            AngleRadians: 0d,
            VerticalDropMeters: 2d);

        Assert.Throws<ArgumentOutOfRangeException>(() => InclinedPlaneExperiment.Run(definition));
    }
}
