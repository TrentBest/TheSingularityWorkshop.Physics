using System.Collections.ObjectModel;
using System.Text.Json;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Declares a repeatable three-dimensional point-projectile trajectory under constant
/// world-space acceleration. All numeric values use SI units.
/// </summary>
/// <param name="InitialState">Initial position (m), velocity (m/s), and elapsed time (s).</param>
/// <param name="Acceleration">Constant world-space acceleration (m/s^2).</param>
/// <param name="DeltaTime">Positive fixed logical timestep (s).</param>
/// <param name="StepCount">Positive number of steps to sample.</param>
/// <param name="CoordinateFrame">Human-readable coordinate-frame convention, such as "world, Z-up".</param>
public sealed record ProjectileTrajectoryExperimentDefinition(
    ProjectileState InitialState,
    Vector3d Acceleration,
    double DeltaTime,
    int StepCount,
    string CoordinateFrame)
{
    /// <summary>Validates all declared inputs before the experiment allocates or produces samples.</summary>
    public void Validate()
    {
        if (!InitialState.IsValid)
            throw new ArgumentException("The initial projectile state must have finite, non-negative elapsed time.", nameof(InitialState));
        if (!double.IsFinite(DeltaTime) || DeltaTime <= 0d)
            throw new ArgumentOutOfRangeException(nameof(DeltaTime), "The logical timestep must be positive and finite.");
        if (StepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(StepCount), "At least one logical step is required.");
        if (string.IsNullOrWhiteSpace(CoordinateFrame))
            throw new ArgumentException("A coordinate-frame convention must be declared.", nameof(CoordinateFrame));
    }
}

/// <summary>
/// Immutable output of a sampled projectile experiment. Samples include the initial state
/// at index zero and one state for every completed solver step.
/// </summary>
public sealed record ProjectileTrajectoryExperimentResult(
    ProjectileTrajectoryExperimentDefinition Definition,
    ReadOnlyCollection<ProjectileState> Samples)
{
    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final sampled projectile state.</summary>
    public ProjectileState FinalState => Samples[^1];

    /// <summary>Total simulated duration added by this experiment, in seconds.</summary>
    public double SimulatedDuration => CompletedSteps * Definition.DeltaTime;
}

/// <summary>
/// Samples the existing analytical projectile solver at a declared fixed timestep.
/// This experiment does not add drag, collision detection, launch mechanics, or rigid-body physics.
/// </summary>
public static class ProjectileTrajectoryExperiment
{
    /// <summary>Runs a declared trajectory and records the initial state plus every subsequent state.</summary>
    /// <exception cref="ArgumentNullException">The definition is null.</exception>
    /// <exception cref="ArgumentException">The initial state or coordinate-frame declaration is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timestep or step count is invalid.</exception>
    /// <exception cref="OverflowException">The sample count or a calculated state exceeds supported finite range.</exception>
    public static ProjectileTrajectoryExperimentResult Run(
        ProjectileTrajectoryExperimentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        var samples = new ProjectileState[checked(definition.StepCount + 1)];
        var state = definition.InitialState;
        samples[0] = state;

        for (var step = 1; step < samples.Length; step++)
        {
            state = ConstantAccelerationProjectileSolver.Advance(
                state,
                definition.Acceleration,
                definition.DeltaTime);
            samples[step] = state;
        }

        return new ProjectileTrajectoryExperimentResult(definition, Array.AsReadOnly(samples));
    }
}

/// <summary>
/// Initial versioned JSON record for a sampled 3D projectile trajectory. It records the
/// declared model inputs and actual solver samples; it is not a general provenance format.
/// </summary>
public sealed record ProjectileTrajectoryRunRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Stable identifier for the analytical model.</summary>
    public string ModelId { get; init; } = "kinematics.constant-acceleration-projectile";

    /// <summary>Version of the model contract, independent of package version.</summary>
    public string ModelVersion { get; init; } = "1";

    /// <summary>Version of this JSON record schema.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Explicit units for the recorded physical quantities.</summary>
    public IReadOnlyDictionary<string, string> Units { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["acceleration"] = "m/s^2",
            ["elapsedTime"] = "s",
            ["position"] = "m",
            ["velocity"] = "m/s"
        };

    /// <summary>Exact input definition used for the run.</summary>
    public required ProjectileTrajectoryExperimentDefinition Definition { get; init; }

    /// <summary>Actual recorded solver states, including the initial state.</summary>
    public required IReadOnlyList<ProjectileState> Samples { get; init; }

    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final state from the recorded samples.</summary>
    public ProjectileState FinalState => Samples[^1];

    /// <summary>Creates a record from the actual output of a trajectory experiment.</summary>
    public static ProjectileTrajectoryRunRecord FromResult(ProjectileTrajectoryExperimentResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ProjectileTrajectoryRunRecord
        {
            Definition = result.Definition,
            Samples = Array.AsReadOnly(result.Samples.ToArray())
        };
    }

    /// <summary>Serializes the record as human-readable JSON with stable property naming and unit ordering.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}
