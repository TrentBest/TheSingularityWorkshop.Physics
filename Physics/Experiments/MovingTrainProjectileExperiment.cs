using System.Collections.ObjectModel;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Numerics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Inputs for the same constant-acceleration projectile viewed from a train and
/// a parallel-axis ground frame. The train has constant world velocity.
/// </summary>
/// <param name="TrainFrameInitialState">Projectile state relative to the train at its epoch.</param>
/// <param name="TrainFrameAcceleration">Projectile acceleration in the train frame (m/s^2).</param>
/// <param name="TrainVelocityInWorld">Train velocity relative to the ground (m/s).</param>
/// <param name="TrainOriginAtEpoch">Train-frame origin in ground coordinates at t = 0 (m).</param>
/// <param name="DeltaTime">Fixed logical timestep (s).</param>
/// <param name="StepCount">Number of steps to sample.</param>
public sealed record MovingTrainProjectileExperimentDefinition(
    ProjectileState TrainFrameInitialState,
    Vector3d TrainFrameAcceleration,
    Vector3d TrainVelocityInWorld,
    Vector3d TrainOriginAtEpoch,
    double DeltaTime,
    int StepCount)
{
    /// <summary>Validates the state and scalar integration settings.</summary>
    public void Validate()
    {
        if (!TrainFrameInitialState.IsValid)
            throw new ArgumentException("The train-frame initial state must be finite with non-negative elapsed time.", nameof(TrainFrameInitialState));
        if (!double.IsFinite(DeltaTime) || DeltaTime <= 0d)
            throw new ArgumentOutOfRangeException(nameof(DeltaTime), "The timestep must be positive and finite.");
        if (StepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(StepCount), "At least one step is required.");
    }
}

/// <summary>A train-frame sample paired with its transformed ground-frame state at the same time.</summary>
public sealed record MovingTrainProjectileSample(
    ProjectileState TrainFrameState,
    ProjectileState GroundFrameState);

/// <summary>Immutable paired output from a moving-train projectile experiment.</summary>
public sealed record MovingTrainProjectileExperimentResult(
    MovingTrainProjectileExperimentDefinition Definition,
    ReadOnlyCollection<MovingTrainProjectileSample> Samples)
{
    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final paired states at the same logical time.</summary>
    public MovingTrainProjectileSample FinalSample => Samples[^1];
}

/// <summary>
/// Runs one constant-acceleration trajectory in train coordinates and derives the ground
/// view using a Galilean transform. No second trajectory solver or independently sampled
/// ground trajectory is used, so the pair shares logical timestamps by construction.
/// </summary>
public static class MovingTrainProjectileExperiment
{
    /// <summary>Runs the paired experiment using fixed logical steps.</summary>
    public static MovingTrainProjectileExperimentResult Run(
        MovingTrainProjectileExperimentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        var trainDefinition = new ProjectileTrajectoryExperimentDefinition(
            definition.TrainFrameInitialState,
            definition.TrainFrameAcceleration,
            definition.DeltaTime,
            definition.StepCount,
            "train frame, parallel axes, Z-up");

        var trainResult = ProjectileTrajectoryExperiment.Run(trainDefinition);
        var paired = new MovingTrainProjectileSample[trainResult.Samples.Count];

        for (var i = 0; i < trainResult.Samples.Count; i++)
        {
            var trainState = trainResult.Samples[i];
            var groundState = GalileanFrameTransform.ToWorld(
                trainState,
                definition.TrainVelocityInWorld,
                definition.TrainOriginAtEpoch);
            paired[i] = new MovingTrainProjectileSample(trainState, groundState);
        }

        return new MovingTrainProjectileExperimentResult(
            definition,
            Array.AsReadOnly(paired));
    }
}
