using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ConstantAccelerationRunRecordTests
{
    [Test]
    public void FromResult_RecordsDefinitionSamplesAndFinalMeasurement()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(0d, 0d, 0d),
            Acceleration: 2d,
            DeltaTime: 0.5d,
            StepCount: 4);
        var result = ConstantAccelerationExperiment.Run(definition);

        var record = ConstantAccelerationRunRecord.FromResult(result);

        Assert.That(record.ModelId, Is.EqualTo("kinematics.constant-acceleration"));
        Assert.That(record.ModelVersion, Is.EqualTo("1"));
        Assert.That(record.SchemaVersion, Is.EqualTo(1));
        Assert.That(record.Definition, Is.EqualTo(definition));
        Assert.That(record.Samples, Is.EqualTo(result.Samples));
        Assert.That(record.CompletedSteps, Is.EqualTo(4));
        Assert.That(record.FinalState, Is.EqualTo(result.FinalState));
        Assert.That(record.Units["position"], Is.EqualTo("m"));
        Assert.That(record.Units["acceleration"], Is.EqualTo("m/s^2"));
    }

    [Test]
    public void ToJson_RepeatedRecordSerializationIsStableAndContainsMeasurements()
    {
        var definition = new ConstantAccelerationExperimentDefinition(
            new KinematicState(1d, 2d, 0d),
            Acceleration: 3d,
            DeltaTime: 0.25d,
            StepCount: 2);
        var result = ConstantAccelerationExperiment.Run(definition);
        var record = ConstantAccelerationRunRecord.FromResult(result);

        var firstJson = record.ToJson();
        var secondJson = record.ToJson();

        Assert.That(secondJson, Is.EqualTo(firstJson));
        Assert.That(firstJson, Does.Contain("\"modelId\": \"kinematics.constant-acceleration\""));
        Assert.That(firstJson, Does.Contain("\"samples\""));
        Assert.That(firstJson, Does.Contain("\"finalState\"").Or.Contain("\"FinalState\"").Or.Contain("\"finalState\""));
        Assert.That(firstJson, Does.Contain("\"position\": 2.09375"));
    }
}
