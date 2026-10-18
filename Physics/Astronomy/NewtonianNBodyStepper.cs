using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>A deterministic fixed-step Newtonian N-body integrator using velocity Verlet.</summary>
/// <remarks>
/// This is a non-relativistic point-mass gravity integrator. Radius is carried as physical state but
/// does not soften gravity or resolve contact. A step that places two bodies at the same position is
/// rejected as singular. Caller owns the fixed timestep and must select it based on the scenario;
/// this class does not adapt time or schedule process groups. Results are returned in ordinal body-ID
/// order. This first version favors clarity and reproducibility over large-N performance.
/// </remarks>
public static class NewtonianNBodyStepper
{
    /// <summary>Advances a complete N-body state by one positive fixed timestep.</summary>
    /// <param name="bodies">Current body states in a shared inertial Cartesian frame.</param>
    /// <param name="deltaTimeSeconds">Positive fixed timestep in seconds.</param>
    /// <param name="gravitationalConstantM3PerKgSecondSquared">Explicit G in m³/(kg·s²).</param>
    /// <returns>The advanced states, ordered by stable body ID.</returns>
    public static OrbitalBodyState[] Step(
        IEnumerable<OrbitalBodyState> bodies,
        double deltaTimeSeconds,
        double gravitationalConstantM3PerKgSecondSquared)
    {
        ArgumentNullException.ThrowIfNull(bodies);
        if (!double.IsFinite(deltaTimeSeconds) || deltaTimeSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds), "Timestep must be finite and positive.");
        if (!double.IsFinite(gravitationalConstantM3PerKgSecondSquared) ||
            gravitationalConstantM3PerKgSecondSquared <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gravitationalConstantM3PerKgSecondSquared),
                "The gravitational constant must be finite and positive.");
        }

        var current = bodies.Select(body =>
        {
            ArgumentNullException.ThrowIfNull(body);
            return body.EnsureValid();
        }).OrderBy(body => body.BodyId, StringComparer.Ordinal).ToArray();

        for (var index = 1; index < current.Length; index++)
        {
            if (StringComparer.Ordinal.Equals(current[index - 1].BodyId, current[index].BodyId))
                throw new ArgumentException("Body IDs must be unique.", nameof(bodies));
        }

        if (current.Length == 0)
            return [];

        var initialAccelerations = CalculateAccelerations(current, gravitationalConstantM3PerKgSecondSquared);
        var halfDeltaTimeSquared = 0.5d * deltaTimeSeconds * deltaTimeSeconds;
        var halfDeltaTime = 0.5d * deltaTimeSeconds;
        var predicted = new OrbitalBodyState[current.Length];

        for (var i = 0; i < current.Length; i++)
        {
            var body = current[i];
            var nextPosition = body.PositionMeters +
                               body.VelocityMetersPerSecond * deltaTimeSeconds +
                               initialAccelerations[i] * halfDeltaTimeSquared;
            predicted[i] = body with { PositionMeters = nextPosition };
        }

        var nextAccelerations = CalculateAccelerations(predicted, gravitationalConstantM3PerKgSecondSquared);
        var result = new OrbitalBodyState[current.Length];
        for (var i = 0; i < current.Length; i++)
        {
            var nextVelocity = current[i].VelocityMetersPerSecond +
                               (initialAccelerations[i] + nextAccelerations[i]) * halfDeltaTime;
            result[i] = predicted[i] with { VelocityMetersPerSecond = nextVelocity };
        }

        return result;
    }

    private static Vector3d[] CalculateAccelerations(
        IReadOnlyList<OrbitalBodyState> bodies,
        double gravitationalConstantM3PerKgSecondSquared)
    {
        var accelerations = new Vector3d[bodies.Count];
        for (var i = 0; i < bodies.Count; i++)
        {
            var acceleration = Vector3d.Zero;
            for (var j = 0; j < bodies.Count; j++)
            {
                if (i == j)
                    continue;

                var offset = bodies[j].PositionMeters - bodies[i].PositionMeters;
                var distance = offset.Magnitude;
                if (distance == 0d)
                {
                    throw new ArgumentException(
                        $"Bodies '{bodies[i].BodyId}' and '{bodies[j].BodyId}' occupy the same point; point-mass gravity is singular.");
                }

                var distanceCubed = distance * distance * distance;
                var scale = gravitationalConstantM3PerKgSecondSquared *
                            bodies[j].MassKilograms / distanceCubed;
                if (!double.IsFinite(scale))
                    throw new ArithmeticException("Calculating N-body acceleration exceeded the finite numeric range.");

                acceleration += offset * scale;
            }

            accelerations[i] = acceleration;
        }

        return accelerations;
    }
}
