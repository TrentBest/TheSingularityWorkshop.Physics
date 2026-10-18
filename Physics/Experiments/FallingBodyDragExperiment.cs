using System.Collections.ObjectModel;
using TheSingularityWorkshop.Physics.Aerodynamics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>Inputs for a deterministic one-dimensional falling-body drag experiment.</summary>
/// <param name="InitialHeight">Initial height above the ground in metres.</param>
/// <param name="InitialVelocity">Initial vertical velocity in metres per second; upward is positive.</param>
/// <param name="Mass">Body mass in kilograms, strictly positive.</param>
/// <param name="Gravity">Positive gravitational acceleration magnitude in metres per second squared.</param>
/// <param name="AirDensity">Air density in kilograms per cubic metre.</param>
/// <param name="DragCoefficient">Dimensionless, constant drag coefficient.</param>
/// <param name="ReferenceArea">Reference area in square metres.</param>
/// <param name="DeltaTime">Fixed integration step in seconds.</param>
/// <param name="StepCount">Number of steps; sample zero is the initial state.</param>
public sealed record FallingBodyDragDefinition(
    double InitialHeight,
    double InitialVelocity,
    double Mass,
    double Gravity,
    double AirDensity,
    double DragCoefficient,
    double ReferenceArea,
    double DeltaTime,
    int StepCount)
{
    /// <summary>Checks all experiment inputs before integration.</summary>
    public void Validate()
    {
        RequireFinite(InitialHeight, nameof(InitialHeight));
        RequireFinite(InitialVelocity, nameof(InitialVelocity));
        RequirePositive(Mass, nameof(Mass));
        RequirePositive(Gravity, nameof(Gravity));
        RequireNonNegative(AirDensity, nameof(AirDensity));
        RequireNonNegative(DragCoefficient, nameof(DragCoefficient));
        RequireNonNegative(ReferenceArea, nameof(ReferenceArea));
        RequirePositive(DeltaTime, nameof(DeltaTime));
        if (InitialHeight < 0d)
            throw new ArgumentOutOfRangeException(nameof(InitialHeight), "Initial height cannot be below ground.");
        if (StepCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(StepCount), "At least one integration step is required.");
    }

    private static void RequireFinite(double value, string name)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(name, "Value must be finite.");
    }

    private static void RequirePositive(double value, string name)
    {
        RequireFinite(value, name);
        if (value <= 0d)
            throw new ArgumentOutOfRangeException(name, "Value must be positive.");
    }

    private static void RequireNonNegative(double value, string name)
    {
        RequireFinite(value, name);
        if (value < 0d)
            throw new ArgumentOutOfRangeException(name, "Value must be non-negative.");
    }
}

/// <summary>A sampled vertical state; upward is positive.</summary>
public readonly record struct FallingBodyDragSample(double ElapsedTime, double Height, double Velocity);

/// <summary>Immutable output containing the inputs and every integrated state.</summary>
public sealed record FallingBodyDragResult(
    FallingBodyDragDefinition Definition,
    ReadOnlyCollection<FallingBodyDragSample> Samples)
{
    /// <summary>Final recorded state.</summary>
    public FallingBodyDragSample FinalState => Samples[^1];
}

/// <summary>
/// Runs a deterministic semi-implicit Euler falling-body experiment with optional quadratic drag.
/// Ground contact is not resolved; the integration continues below ground if the run is too long.
/// </summary>
public static class FallingBodyDragExperiment
{
    /// <summary>Runs and records the initial state plus every completed step.</summary>
    public static FallingBodyDragResult Run(FallingBodyDragDefinition definition, bool includeDrag = true)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();

        var samples = new FallingBodyDragSample[checked(definition.StepCount + 1)];
        var height = definition.InitialHeight;
        var velocity = definition.InitialVelocity;
        var time = 0d;
        samples[0] = new FallingBodyDragSample(time, height, velocity);

        for (var i = 1; i < samples.Length; i++)
        {
            var drag = includeDrag
                ? -0.5d * definition.AirDensity * definition.DragCoefficient *
                  definition.ReferenceArea * Math.Abs(velocity) * velocity
                : 0d;
            var acceleration = -definition.Gravity + drag / definition.Mass;
            velocity += acceleration * definition.DeltaTime;
            height += velocity * definition.DeltaTime;
            time += definition.DeltaTime;
            if (!double.IsFinite(height) || !double.IsFinite(velocity) || !double.IsFinite(time))
                throw new ArithmeticException("Falling-body integration exceeded the finite numeric range.");
            samples[i] = new FallingBodyDragSample(time, height, velocity);
        }

        return new FallingBodyDragResult(definition, Array.AsReadOnly(samples));
    }
}
