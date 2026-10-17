using System.Collections.ObjectModel;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Fully specified inputs for a repeatable ideal 3D projectile experiment.
/// Position is metres, speed is metres per second, gravity is metres per second squared,
/// angles are degrees, and the timestep is seconds.
/// </summary>
public sealed record ProjectileExperimentDefinition(
    string RunId,
    Vector3d InitialPosition,
    double LaunchSpeed,
    double ElevationDegrees,
    double AzimuthDegrees,
    Vector3d Gravity,
    double DeltaTime,
    int StepCount)
{
    /// <summary>
    /// Validates all scalar experiment inputs before samples are produced.
    /// Elevation is measured from the x-y plane and is restricted to [-90, 90] degrees.
    /// Azimuth is measured in the x-y plane from +X toward +Y; any finite degree value is accepted.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RunId))
            throw new ArgumentException("A non-empty run identifier is required.", nameof(RunId));

        if (!double.IsFinite(LaunchSpeed) || LaunchSpeed < 0d)
            throw new ArgumentOutOfRangeException(nameof(LaunchSpeed), "Launch speed must be finite and non-negative.");

        if (!double.IsFinite(ElevationDegrees) || ElevationDegrees < -90d || ElevationDegrees > 90d)
            throw new ArgumentOutOfRangeException(nameof(ElevationDegrees), "Elevation must be finite and between -90 and 90 degrees.");

        if (!double.IsFinite(AzimuthDegrees))
            throw new ArgumentOutOfRangeException(nameof(AzimuthDegrees), "Azimuth must be finite.");

        if (!double.IsFinite(DeltaTime) || DeltaTime <= 0d)
            throw new ArgumentOutOfRangeException(nameof(DeltaTime), "The logical timestep must be positive and finite.");

        if (StepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(StepCount), "At least one logical step is required.");
    }

    /// <summary>
    /// Converts the declared launch speed and angles into a world-space initial velocity.
    /// The coordinate convention is right-handed and z-up.
    /// </summary>
    public Vector3d CreateInitialVelocity()
    {
        Validate();

        var elevation = ElevationDegrees * (Math.PI / 180d);
        var azimuth = AzimuthDegrees * (Math.PI / 180d);
        var horizontalSpeed = LaunchSpeed * Math.Cos(elevation);

        return new Vector3d(
            horizontalSpeed * Math.Cos(azimuth),
            horizontalSpeed * Math.Sin(azimuth),
            LaunchSpeed * Math.Sin(elevation));
    }
}

/// <summary>
/// Immutable output of a 3D ideal projectile experiment. Samples include the initial
/// state at index zero, followed by one state for each completed logical step.
/// </summary>
public sealed record ProjectileExperimentResult(
    ProjectileExperimentDefinition Definition,
    ReadOnlyCollection<ProjectileState> Samples)
{
    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final sampled state.</summary>
    public ProjectileState FinalState => Samples[^1];

    /// <summary>Total logical duration represented by the completed steps.</summary>
    public double Duration => CompletedSteps * Definition.DeltaTime;
}

/// <summary>
/// Runs an ideal point-projectile experiment under constant world-space acceleration.
/// This does not model drag, spin, projectile shape, collision, impact, or a siege mechanism.
/// </summary>
public static class ProjectileExperiment
{
    /// <summary>Executes the declared experiment and retains every logical-step sample.</summary>
    public static ProjectileExperimentResult Run(ProjectileExperimentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        var samples = new ProjectileState[checked(definition.StepCount + 1)];
        var state = new ProjectileState(
            definition.InitialPosition,
            definition.CreateInitialVelocity(),
            0d);
        samples[0] = state;

        for (var step = 1; step < samples.Length; step++)
        {
            state = ConstantAccelerationProjectileSolver.Advance(
                state,
                definition.Gravity,
                definition.DeltaTime);
            samples[step] = state;
        }

        return new ProjectileExperimentResult(
            definition,
            Array.AsReadOnly(samples));
    }
}
