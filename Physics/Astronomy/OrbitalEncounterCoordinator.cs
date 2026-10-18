namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>The final state and ordered merger history for a constant-velocity encounter interval.</summary>
/// <param name="FinalBodies">Remaining bodies, sorted by ordinal body identity.</param>
/// <param name="MergeEvents">Merger events in chronological, deterministic resolution order.</param>
/// <param name="ElapsedSeconds">The full interval simulated by the coordinator.</param>
public sealed record OrbitalEncounterResult(
    IReadOnlyList<OrbitalBodyState> FinalBodies,
    IReadOnlyList<OrbitalMergeEvent> MergeEvents,
    double ElapsedSeconds);

/// <summary>Coordinates swept contacts and pairwise merger policy for a constant-velocity interval.</summary>
/// <remarks>
/// This is an encounter coordinator, not a gravity integrator. It linearly drifts every body to the earliest
/// predicted contact, applies the selected perfectly inelastic merge policy, and then recomputes candidates
/// against the changed body set for the remaining interval. Simultaneous contacts are resolved one at a time
/// in stable body-ID order, with the candidate set rebuilt after each outcome. Gravity and acceleration are
/// not integrated during this interval; callers should use short intervals or coordinate this with a stepper.
/// </remarks>
public static class OrbitalEncounterCoordinator
{
    /// <summary>
    /// Resolves all swept contacts as perfectly inelastic mergers during a linear-drift interval.
    /// Inputs are not mutated. Final bodies and event history are deterministic for equivalent input states.
    /// </summary>
    /// <param name="bodies">The unique-identity bodies at the start of the interval.</param>
    /// <param name="intervalSeconds">Finite positive interval duration in seconds.</param>
    public static OrbitalEncounterResult ResolveLinearMergers(
        IEnumerable<OrbitalBodyState> bodies,
        double intervalSeconds)
    {
        ArgumentNullException.ThrowIfNull(bodies);
        if (!double.IsFinite(intervalSeconds) || intervalSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "The interval must be finite and positive.");

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

        while (current.Count > 1)
        {
            var candidates = new List<OrbitalSweptCollision>();
            for (var i = 0; i < current.Count; i++)
            {
                for (var j = i + 1; j < current.Count; j++)
                {
                    var candidate = OrbitalSweptCollisionDetector.TryDetect(current[i], current[j], remaining);
                    if (candidate is not null)
                        candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
            {
                current = Drift(current, remaining);
                remaining = 0d;
                break;
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

            var next = candidates[0];
            var eventTime = next.TimeOfImpactSeconds;
            current = Drift(current, eventTime);
            remaining = Math.Max(0d, remaining - eventTime);

            var bodyA = current.Single(body => StringComparer.Ordinal.Equals(body.BodyId, next.BodyAId));
            var bodyB = current.Single(body => StringComparer.Ordinal.Equals(body.BodyId, next.BodyBId));
            var merge = OrbitalMergeResolver.TryMerge(bodyA, bodyB);
            if (merge is null)
            {
                // The quadratic contact time can round to a separation infinitesimally beyond the
                // radii sum. Correct only this numerical contact geometry, preserving the pair COM.
                (bodyA, bodyB) = SnapPairToContact(bodyA, bodyB);
                merge = OrbitalMergeResolver.TryMerge(bodyA, bodyB);
                if (merge is null)
                    throw new ArithmeticException("A predicted swept contact could not be accepted by the merger policy.");
            }

            current.RemoveAll(body =>
                StringComparer.Ordinal.Equals(body.BodyId, next.BodyAId) ||
                StringComparer.Ordinal.Equals(body.BodyId, next.BodyBId));
            merge = WithUniqueMergedIdentity(merge, current);
            current.Add(merge.MergedBody);
            current.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.BodyId, right.BodyId));
            events.Add(merge);

            if (remaining <= 0d)
                break;
        }

        if (remaining > 0d)
            current = Drift(current, remaining);

        return new OrbitalEncounterResult(current.AsReadOnly(), events.AsReadOnly(), intervalSeconds);
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

    private static List<OrbitalBodyState> Drift(IEnumerable<OrbitalBodyState> bodies, double seconds)
    {
        if (seconds == 0d)
            return bodies.ToList();

        return bodies.Select(body =>
        {
            var position = body.PositionMeters + body.VelocityMetersPerSecond * seconds;
            if (!double.IsFinite(position.X) || !double.IsFinite(position.Y) || !double.IsFinite(position.Z))
                throw new ArithmeticException($"Linear drift produced a non-finite position for body '{body.BodyId}'.");
            return body with { PositionMeters = position };
        }).ToList();
    }

    private static (OrbitalBodyState First, OrbitalBodyState Second) SnapPairToContact(
        OrbitalBodyState bodyA,
        OrbitalBodyState bodyB)
    {
        var first = StringComparer.Ordinal.Compare(bodyA.BodyId, bodyB.BodyId) < 0 ? bodyA : bodyB;
        var second = ReferenceEquals(first, bodyA) ? bodyB : bodyA;
        var delta = second.PositionMeters - first.PositionMeters;
        var distance = delta.Magnitude;
        var radiusSum = first.RadiusMeters + second.RadiusMeters;
        if (!double.IsFinite(distance) || !double.IsFinite(radiusSum) || radiusSum <= 0d)
            throw new ArithmeticException("Unable to correct the predicted contact geometry.");

        var direction = distance > 0d
            ? delta / distance
            : new TheSingularityWorkshop.Physics.Spatial.Vector3d(1d, 0d, 0d);
        var totalMass = first.MassKilograms + second.MassKilograms;
        var totalRadius = radiusSum;
        var centreOfMass = first.PositionMeters * (first.MassKilograms / totalMass) +
                           second.PositionMeters * (second.MassKilograms / totalMass);
        var correctedFirstPosition = centreOfMass - direction * (second.MassKilograms / totalMass * totalRadius);
        var correctedSecondPosition = centreOfMass + direction * (first.MassKilograms / totalMass * totalRadius);
        return (
            first with { PositionMeters = correctedFirstPosition },
            second with { PositionMeters = correctedSecondPosition });
    }
}
