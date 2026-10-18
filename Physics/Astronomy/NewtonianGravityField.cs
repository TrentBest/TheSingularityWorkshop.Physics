using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>A point-mass source for a Newtonian gravitational-field sample.</summary>
/// <param name="BodyId">Stable identity used to make accumulation order deterministic.</param>
/// <param name="MassKilograms">Positive source mass in kilograms.</param>
/// <param name="PositionMeters">Source position in a shared Cartesian frame, in metres.</param>
public sealed record GravitatingPointMass(
    string BodyId,
    double MassKilograms,
    Vector3d PositionMeters)
{
    /// <summary>Validates identity and mass and returns this source.</summary>
    public GravitatingPointMass EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(BodyId))
            throw new ArgumentException("A stable body ID is required.", nameof(BodyId));
        if (!double.IsFinite(MassKilograms) || MassKilograms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(MassKilograms), "Mass must be finite and positive.");

        return this;
    }
}

/// <summary>The Newtonian acceleration and gravitational potential at one sample point.</summary>
/// <param name="AccelerationMetersPerSecondSquared">Acceleration in metres per second squared.</param>
/// <param name="PotentialMetersSquaredPerSecondSquared">Specific gravitational potential in square metres per square second.</param>
/// <param name="ContributingBodyCount">Number of source bodies included in the field.</param>
public readonly record struct GravitationalFieldSample(
    Vector3d AccelerationMetersPerSecondSquared,
    double PotentialMetersSquaredPerSecondSquared,
    int ContributingBodyCount);

/// <summary>Evaluates a Newtonian point-mass field at a point in a shared Cartesian frame.</summary>
/// <remarks>
/// The field is the superposition of ideal point masses: finite-size bodies, general relativity,
/// orbital integration, collision handling, and impact physics are outside this evaluator. The
/// gravitational constant and all positions are explicit caller inputs. Samples exactly at a
/// source position are rejected because the point-mass field is singular there. Source accumulation
/// is sorted by stable body ID so caller enumeration order does not change the summation order.
/// </remarks>
public static class NewtonianGravityField
{
    /// <summary>Evaluates acceleration and specific potential at a point.</summary>
    /// <param name="sources">The finite set of point-mass sources.</param>
    /// <param name="samplePositionMeters">Sample position in metres in the same frame as each source.</param>
    /// <param name="gravitationalConstantM3PerKgSecondSquared">Explicit gravitational constant in m³/(kg·s²).</param>
    public static GravitationalFieldSample Sample(
        IEnumerable<GravitatingPointMass> sources,
        Vector3d samplePositionMeters,
        double gravitationalConstantM3PerKgSecondSquared)
    {
        ArgumentNullException.ThrowIfNull(sources);
        if (!double.IsFinite(gravitationalConstantM3PerKgSecondSquared) ||
            gravitationalConstantM3PerKgSecondSquared <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gravitationalConstantM3PerKgSecondSquared),
                "The gravitational constant must be finite and positive.");
        }

        var orderedSources = sources.Select(source =>
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.EnsureValid();
        }).OrderBy(source => source.BodyId, StringComparer.Ordinal).ToArray();

        for (var index = 1; index < orderedSources.Length; index++)
        {
            if (StringComparer.Ordinal.Equals(orderedSources[index - 1].BodyId, orderedSources[index].BodyId))
                throw new ArgumentException("Source body IDs must be unique.", nameof(sources));
        }

        var acceleration = Vector3d.Zero;
        var potential = 0d;
        foreach (var source in orderedSources)
        {
            var offset = source.PositionMeters - samplePositionMeters;
            var distance = offset.Magnitude;
            if (distance == 0d)
                throw new ArgumentException(
                    $"The sample point coincides with point-mass source '{source.BodyId}'.",
                    nameof(samplePositionMeters));

            var gm = gravitationalConstantM3PerKgSecondSquared * source.MassKilograms;
            var accelerationMagnitude = gm / (distance * distance);
            var contribution = offset.Normalized() * accelerationMagnitude;
            var potentialContribution = -gm / distance;
            if (!double.IsFinite(accelerationMagnitude) || !double.IsFinite(potentialContribution))
                throw new ArithmeticException("The gravitational field exceeds the finite numeric range.");

            acceleration += contribution;
            potential += potentialContribution;
            if (!double.IsFinite(potential))
                throw new ArithmeticException("Accumulating gravitational potential exceeded the finite numeric range.");
        }

        return new GravitationalFieldSample(acceleration, potential, orderedSources.Length);
    }
}
