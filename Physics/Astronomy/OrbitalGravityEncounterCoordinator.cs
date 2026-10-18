using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>Coordinates fixed-step Newtonian gravity integration with deterministic pairwise mergers.</summary>
/// <remarks>
/// Each trial step is integrated with velocity Verlet. Body positions at the step endpoints define
/// piecewise-linear chords; swept-sphere detection estimates contact time along those chords. On a
/// predicted contact, the gravity integrator advances to that time, the pair is projected to touching
/// separation while preserving their centre of mass, and the explicit perfectly inelastic merge policy
/// is applied. The step size bounds the chord interval but does not provide a rigorous curvature error
/// bound, so this is approximate event handling, not exact continuous collision detection.
/// </remarks>
public static class OrbitalGravityEncounterCoordinator
{
    /// <summary>
    /// Advances the system under Newtonian gravity, merging contacts estimated within bounded steps.
    /// </summary>
    /// <param name="bodies">Unique-identity spherical bodies in one inertial Cartesian frame.</param>
    /// <param name="intervalSeconds">Finite positive total simulated duration.</param>
    /// <param name="maximumStepSeconds">Finite positive upper bound for each trial integration step.</param>
    /// <param name="gravitationalConstantM3PerKgSecondSquared">Explicit Newtonian gravitational constant.</param>
    /// <returns>Final bodies and deterministic merger history for the full interval.</returns>
    public static OrbitalEncounterResult ResolveMergers(
        IEnumerable<OrbitalBodyState> bodies,
        double intervalSeconds,
        double maximumStepSeconds,
        double gravitationalConstantM3PerKgSecondSquared)
    {
        ArgumentNullException.ThrowIfNull(bodies);
        ValidatePositiveFinite(intervalSeconds, nameof(intervalSeconds));
        ValidatePositiveFinite(maximumStepSeconds, nameof(maximumStepSeconds));
        ValidatePositiveFinite(
            gravitationalConstantM3PerKgSecondSquared,
            nameof(gravitationalConstantM3PerKgSecondSquared));

        var current = bodies.Select(body =>
        {
            ArgumentNullException.ThrowIfNull(body);
            return body.EnsureValid();
        }).OrderBy(body => body.BodyId, StringComparer.Ordinal).ToList();

        for (var i = 1; i < current.Count; i++)
        {
            if (StringComparer.Ordinal.Equals(current[i - 1].BodyId, current[i].BodyId))
                throw new ArgumentException($"Duplicate body identity '{current[i].BodyId}'.", nameof(bodies));
        }

        var events = new List<OrbitalMergeEvent>();
        var remaining = intervalSeconds;

        while (remaining > 0d && current.Count > 0)
        {
            var stepDuration = Math.Min(maximumStepSeconds, remaining);
            var predicted = NewtonianNBodyStepper.Step(
                current, stepDuration, gravitationalConstantM3PerKgSecondSquared);
            var candidates = FindChordContacts(current, predicted, stepDuration);

            if (candidates.Count == 0)
            {
                current = predicted.ToList();
                remaining = SubtractElapsed(remaining, stepDuration);
                continue;
            }

            candidates.Sort(static (left, right) =>
            {
                var timeOrder = left.TimeOfImpactSeconds.CompareTo(right.TimeOfImpactSeconds);
                if (timeOrder != 0)
                    return timeOrder;
                var firstOrder = StringComparer.Ordinal.Compare(left.BodyAId, right.BodyAId);
                return firstOrder != 0
                    ? firstOrder
                    : StringComparer.Ordinal.Compare(left.BodyBId, right.BodyBId);
            });

            var contact = candidates[0];
            var eventTime = contact.TimeOfImpactSeconds;
            if (eventTime > 0d)
            {
                current = NewtonianNBodyStepper.Step(
                    current, eventTime, gravitationalConstantM3PerKgSecondSquared).ToList();
                remaining = SubtractElapsed(remaining, eventTime);
            }

            var indexA = current.FindIndex(body => StringComparer.Ordinal.Equals(body.BodyId, contact.BodyAId));
            var indexB = current.FindIndex(body => StringComparer.Ordinal.Equals(body.BodyId, contact.BodyBId));
            if (indexA < 0 || indexB < 0)
                throw new InvalidOperationException("A selected contact no longer refers to active bodies.");

            var touchingPair = ProjectToTouchingSeparation(current[indexA], current[indexB]);
            var merge = OrbitalMergeResolver.TryMerge(touchingPair.First, touchingPair.Second)
                ?? throw new InvalidOperationException("Projected contact was rejected by the merger policy.");

            current.RemoveAt(Math.Max(indexA, indexB));
            current.RemoveAt(Math.Min(indexA, indexB));
            merge = WithUniqueMergedIdentity(merge, current);
            current.Add(merge.MergedBody);
            current.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.BodyId, right.BodyId));
            events.Add(merge);
        }

        return new OrbitalEncounterResult(current, events, intervalSeconds);
    }

    private static List<OrbitalSweptCollision> FindChordContacts(
        IReadOnlyList<OrbitalBodyState> start,
        IReadOnlyList<OrbitalBodyState> end,
        double stepDuration)
    {
        var candidates = new List<OrbitalSweptCollision>();
        for (var i = 0; i < start.Count; i++)
        {
            var chordVelocityA = (end[i].PositionMeters - start[i].PositionMeters) / stepDuration;
            var chordA = start[i] with { VelocityMetersPerSecond = chordVelocityA };
            for (var j = i + 1; j < start.Count; j++)
            {
                var chordVelocityB = (end[j].PositionMeters - start[j].PositionMeters) / stepDuration;
                var chordB = start[j] with { VelocityMetersPerSecond = chordVelocityB };
                var contact = OrbitalSweptCollisionDetector.TryDetect(chordA, chordB, stepDuration);
                if (contact is not null)
                    candidates.Add(contact);
            }
        }

        return candidates;
    }

    private static (OrbitalBodyState First, OrbitalBodyState Second) ProjectToTouchingSeparation(
        OrbitalBodyState bodyA,
        OrbitalBodyState bodyB)
    {
        var offset = bodyB.PositionMeters - bodyA.PositionMeters;
        var distance = offset.Magnitude;
        var direction = distance > 0d ? offset / distance : new Vector3d(1d, 0d, 0d);
        var combinedRadius = bodyA.RadiusMeters + bodyB.RadiusMeters;
        if (!double.IsFinite(combinedRadius))
            throw new ArithmeticException("Combined body radius exceeded the finite numeric range.");

        var totalMass = bodyA.MassKilograms + bodyB.MassKilograms;
        if (!double.IsFinite(totalMass))
            throw new ArithmeticException("Combined body mass exceeded the finite numeric range.");

        var correction = combinedRadius - distance;
        var first = bodyA with
        {
            PositionMeters = bodyA.PositionMeters -
                             direction * (correction * bodyB.MassKilograms / totalMass)
        };
        var second = bodyB with
        {
            PositionMeters = bodyB.PositionMeters +
                             direction * (correction * bodyA.MassKilograms / totalMass)
        };
        return (first, second);
    }

    private static OrbitalMergeEvent WithUniqueMergedIdentity(
        OrbitalMergeEvent merge,
        IReadOnlyCollection<OrbitalBodyState> survivors)
    {
        var baseId = merge.MergedBody.BodyId;
        var candidateId = baseId;
        var suffix = 1;
        while (survivors.Any(body => StringComparer.Ordinal.Equals(body.BodyId, candidateId)))
            candidateId = $"{baseId}#{suffix++}";

        return StringComparer.Ordinal.Equals(candidateId, baseId)
            ? merge
            : merge with { MergedBody = merge.MergedBody with { BodyId = candidateId } };
    }

    private static double SubtractElapsed(double remaining, double elapsed)
    {
        var next = remaining - elapsed;
        return next <= 0d ? 0d : next;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0d)
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
    }
}
