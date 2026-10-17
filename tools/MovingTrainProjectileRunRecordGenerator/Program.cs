using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: MovingTrainProjectileRunRecordGenerator <output.json>");
    return 2;
}

var definition = new MovingTrainProjectileExperimentDefinition(
    TrainFrameInitialState: new ProjectileState(
        Vector3d.Zero,
        new Vector3d(0d, 0d, 8d),
        0d),
    TrainFrameAcceleration: new Vector3d(0d, 0d, -9.81d),
    TrainVelocityInWorld: new Vector3d(20d, 0d, 0d),
    TrainOriginAtEpoch: Vector3d.Zero,
    DeltaTime: 0.25d,
    StepCount: 8);

var result = MovingTrainProjectileExperiment.Run(definition);
var record = MovingTrainProjectileRunRecord.FromResult(result);
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, record.ToJson() + Environment.NewLine);
Console.WriteLine($"Wrote {record.Samples.Count} paired train/ground states to {outputPath}");
return 0;
