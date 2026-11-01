using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Declared inputs for one repeatable frictionless inclined-plane comparison run.
/// All quantities use SI units; the angle is measured above the horizontal in radians.
/// </summary>
public sealed record InclinedPlaneExperimentDefinition(
    double MassKilograms,
    double AngleRadians,
    double VerticalDropMeters,
    double GravitationalAccelerationMetersPerSecondSquared = 9.80665d);

/// <summary>Result of evaluating one declared inclined-plane experiment.</summary>
public sealed record InclinedPlaneExperimentResult(
    InclinedPlaneExperimentDefinition Definition,
    InclinedPlaneResult Outcome);

/// <summary>Executes the analytical frictionless inclined-plane model for one input set.</summary>
public static class InclinedPlaneExperiment
{
    /// <summary>Evaluates the model and retains the exact inputs alongside its output.</summary>
    public static InclinedPlaneExperimentResult Run(
        InclinedPlaneExperimentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var outcome = InclinedPlaneSolver.Calculate(
            definition.MassKilograms,
            definition.AngleRadians,
            definition.VerticalDropMeters,
            definition.GravitationalAccelerationMetersPerSecondSquared);

        return new InclinedPlaneExperimentResult(definition, outcome);
    }
}
