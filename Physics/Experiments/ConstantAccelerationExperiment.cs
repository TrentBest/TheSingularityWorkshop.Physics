using System.Collections.ObjectModel;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Fully specified inputs for a repeatable one-dimensional constant-acceleration run.
/// Values use SI units and a fixed logical timestep.
/// </summary>
/// <param name="InitialState">Initial kinematic state snapshot.</param>
/// <param name="Acceleration">Constant acceleration in metres per second squared.</param>
/// <param name="DeltaTime">Positive logical timestep in seconds.</param>
/// <param name="StepCount">Number of solver steps to execute; must be positive.</param>
public sealed record ConstantAccelerationExperimentDefinition(
    KinematicState InitialState,
    double Acceleration,
    double DeltaTime,
    int StepCount)
{
    /// <summary>Validates experiment inputs before any samples are produced.</summary>
    public void Validate()
    {
        if (!InitialState.IsValid)
        {
            throw new ArgumentException(
                "The initial state must be finite and have non-negative elapsed time.",
                nameof(InitialState));
        }

        if (!double.IsFinite(Acceleration))
        {
            throw new ArgumentException("Acceleration must be finite.", nameof(Acceleration));
        }

        if (!double.IsFinite(DeltaTime) || DeltaTime <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(DeltaTime),
                "The logical timestep must be positive and finite.");
        }

        if (StepCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StepCount),
                "At least one logical step is required.");
        }
    }
}

/// <summary>
/// Immutable output of an experiment. Samples include the initial state at index zero,
/// followed by one state for each completed logical step.
/// </summary>
/// <param name="Definition">The exact input definition used for the run.</param>
/// <param name="Samples">State samples, including the initial state.</param>
public sealed record ConstantAccelerationExperimentResult(
    ConstantAccelerationExperimentDefinition Definition,
    ReadOnlyCollection<KinematicState> Samples)
{
    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final sampled state.</summary>
    public KinematicState FinalState => Samples[^1];
}

/// <summary>
/// Runs the constant-acceleration analytical model over a declared fixed-step schedule.
/// This experiment intentionally models only one-dimensional constant acceleration.
/// </summary>
public static class ConstantAccelerationExperiment
{
    /// <summary>Runs the declared experiment and records every state snapshot.</summary>
    /// <exception cref="ArgumentException">An initial state or acceleration is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The timestep or step count is invalid.</exception>
    /// <exception cref="OverflowException">A calculated state exceeds finite double range.</exception>
    public static ConstantAccelerationExperimentResult Run(
        ConstantAccelerationExperimentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        var samples = new KinematicState[checked(definition.StepCount + 1)];
        var state = definition.InitialState;
        samples[0] = state;

        for (var step = 1; step < samples.Length; step++)
        {
            state = ConstantAccelerationSolver.Advance(
                state,
                definition.Acceleration,
                definition.DeltaTime);
            samples[step] = state;
        }

        return new ConstantAccelerationExperimentResult(
            definition,
            Array.AsReadOnly(samples));
    }
}
