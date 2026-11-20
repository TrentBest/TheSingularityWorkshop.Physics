using System.Collections.ObjectModel;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.World;

/// <summary>Immutable sphere-only body model consumed by the deterministic CPU reference stepper.</summary>
public sealed record CpuWorldBodyDefinition
{
    public string BodyId { get; }
    public double MassKilograms { get; }
    public double SphereRadiusMetres { get; }
    public Vector3d ConstantAccelerationMetresPerSecondSquared { get; }

    public CpuWorldBodyDefinition(
        string bodyId,
        double massKilograms,
        double sphereRadiusMetres,
        Vector3d constantAccelerationMetresPerSecondSquared)
    {
        if (string.IsNullOrWhiteSpace(bodyId))
            throw new ArgumentException("A stable body identity is required.", nameof(bodyId));
        if (!double.IsFinite(massKilograms) || massKilograms <= 0d)
            throw new ArgumentOutOfRangeException(nameof(massKilograms), "Mass must be finite and positive.");
        if (!double.IsFinite(sphereRadiusMetres) || sphereRadiusMetres <= 0d)
            throw new ArgumentOutOfRangeException(nameof(sphereRadiusMetres), "Sphere radius must be finite and positive.");

        BodyId = bodyId;
        MassKilograms = massKilograms;
        SphereRadiusMetres = sphereRadiusMetres;
        ConstantAccelerationMetresPerSecondSquared = constantAccelerationMetresPerSecondSquared;
    }
}

/// <summary>A body definition paired with its immutable achieved input state.</summary>
public sealed record CpuWorldBody(CpuWorldBodyDefinition Definition, RigidBodyState State);

/// <summary>Fixed model identity and numerical tolerances for one CPU world-step configuration.</summary>
public sealed record CpuWorldConfiguration
{
    public string ModelId { get; }
    public double DeltaTimeSeconds { get; }
    public double Restitution { get; }
    public double ContactToleranceMetres { get; }
    public double PenetrationSlopMetres { get; }
    public double CorrectionFraction { get; }
    public double MaximumCorrectionMetres { get; }

    public CpuWorldConfiguration(
        string modelId,
        double deltaTimeSeconds,
        double restitution = 0d,
        double contactToleranceMetres = 0d,
        double penetrationSlopMetres = 0.001d,
        double correctionFraction = 0.8d,
        double maximumCorrectionMetres = 0.2d)
    {
        if (string.IsNullOrWhiteSpace(modelId))
            throw new ArgumentException("A stable model/configuration identity is required.", nameof(modelId));
        if (!double.IsFinite(deltaTimeSeconds) || deltaTimeSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds), "The fixed timestep must be finite and positive.");
        if (!double.IsFinite(restitution) || restitution < 0d || restitution > 1d)
            throw new ArgumentOutOfRangeException(nameof(restitution), "Restitution must be within [0, 1].");
        if (!double.IsFinite(contactToleranceMetres) || contactToleranceMetres < 0d)
            throw new ArgumentOutOfRangeException(nameof(contactToleranceMetres));
        if (!double.IsFinite(penetrationSlopMetres) || penetrationSlopMetres < 0d)
            throw new ArgumentOutOfRangeException(nameof(penetrationSlopMetres));
        if (!double.IsFinite(correctionFraction) || correctionFraction <= 0d || correctionFraction > 1d)
            throw new ArgumentOutOfRangeException(nameof(correctionFraction));
        if (!double.IsFinite(maximumCorrectionMetres) || maximumCorrectionMetres <= 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumCorrectionMetres));

        ModelId = modelId;
        DeltaTimeSeconds = deltaTimeSeconds;
        Restitution = restitution;
        ContactToleranceMetres = contactToleranceMetres;
        PenetrationSlopMetres = penetrationSlopMetres;
        CorrectionFraction = correctionFraction;
        MaximumCorrectionMetres = maximumCorrectionMetres;
    }
}

/// <summary>Explicit limitations reported by the first deterministic CPU world-step slice.</summary>
public enum CpuWorldStepLimitation
{
    DiscreteEndpointNarrowphase,
    LinearSweptBoundsOnly,
    FrictionlessTranslationalResponse,
    NoAngularContactResponse,
    NoPersistentContacts,
    NoSimultaneousMultiContactSolver
}

/// <summary>Immutable result of one successfully completed deterministic CPU world step.</summary>
public sealed class CpuWorldStepResult
{
    public string ModelId { get; }
    public long StepIndex { get; }
    public double ElapsedTimeSeconds { get; }
    public IReadOnlyList<RigidBodyState> Bodies { get; }
    public IReadOnlyList<ContactObservation> Contacts { get; }
    public IReadOnlyList<CpuWorldStepLimitation> Limitations { get; }

    internal CpuWorldStepResult(
        string modelId,
        long stepIndex,
        double elapsedTimeSeconds,
        IEnumerable<RigidBodyState> bodies,
        IEnumerable<ContactObservation> contacts)
    {
        ModelId = modelId;
        StepIndex = stepIndex;
        ElapsedTimeSeconds = elapsedTimeSeconds;
        Bodies = new ReadOnlyCollection<RigidBodyState>(bodies.ToArray());
        Contacts = new ReadOnlyCollection<ContactObservation>(contacts.ToArray());
        Limitations = Array.AsReadOnly(Enum.GetValues<CpuWorldStepLimitation>());
    }
}

/// <summary>
/// Deterministic CPU reference composition of free-body integration, swept AABB candidate
/// generation, endpoint sphere contact detection, isolated single-contact response, and
/// post-correction contact recomputation.
/// </summary>
/// <remarks>
/// Bodies are ordered by ordinal stable ID. A step is atomic: all validation and response
/// occurs in local immutable snapshots, and a result is returned only after every stage
/// succeeds. The broadphase sweep is linear endpoint enclosure, not continuous collision
/// detection; endpoint narrowphase can miss a pair that crosses and separates within a step.
/// Any body participating in more than one active contact causes the step to be rejected.
/// </remarks>
public static class DeterministicCpuWorldStepper
{
    /// <summary>Advances a sphere-only world by exactly one fixed logical timestep.</summary>
    public static CpuWorldStepResult Step(
        CpuWorldConfiguration configuration,
        IEnumerable<CpuWorldBody> bodies)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(bodies);

        var ordered = bodies.OrderBy(body => body?.Definition?.BodyId, StringComparer.Ordinal).ToArray();
        if (ordered.Length == 0)
            throw new ArgumentException("At least one body is required.", nameof(bodies));

        var ids = new HashSet<string>(StringComparer.Ordinal);
        long inputStep = -1;
        double inputTime = 0d;
        for (var i = 0; i < ordered.Length; i++)
        {
            var body = ordered[i] ?? throw new ArgumentException("World body entries cannot be null.", nameof(bodies));
            if (body.Definition is null)
                throw new ArgumentException("Every world body requires a definition and state.", nameof(bodies));
            if (!ids.Add(body.Definition.BodyId))
                throw new ArgumentException($"Duplicate body identity: {body.Definition.BodyId}.", nameof(bodies));
            if (!body.State.IsValid || !string.Equals(body.Definition.BodyId, body.State.BodyId, StringComparison.Ordinal))
                throw new ArgumentException("Each body definition must match a valid state identity.", nameof(bodies));
            if (!IsFinite(body.State.LinearVelocity) || !IsFinite(body.State.AngularVelocity))
                throw new ArgumentException("Body velocities must be finite.", nameof(bodies));

            if (i == 0)
            {
                inputStep = body.State.StepIndex;
                inputTime = body.State.ElapsedTime;
            }
            else if (body.State.StepIndex != inputStep || body.State.ElapsedTime != inputTime)
            {
                throw new ArgumentException("All world bodies must share exactly one input logical step and time.", nameof(bodies));
            }
        }

        var nextStep = checked(inputStep + 1);
        var nextTime = inputTime + configuration.DeltaTimeSeconds;
        if (!double.IsFinite(nextTime))
            throw new ArithmeticException("The next world time is outside the finite numeric range.");

        var integrated = new Dictionary<string, RigidBodyState>(StringComparer.Ordinal);
        var definitions = new Dictionary<string, CpuWorldBodyDefinition>(StringComparer.Ordinal);
        var proxies = new List<BroadphaseProxy>(ordered.Length);
        for (var i = 0; i < ordered.Length; i++)
        {
            var body = ordered[i];
            var definition = body.Definition;
            var state = body.State;
            var advanced = RigidBodyIntegrator.Advance(
                state, definition.ConstantAccelerationMetresPerSecondSquared, configuration.DeltaTimeSeconds);
            if (advanced.StepIndex != nextStep || advanced.ElapsedTime != nextTime)
                throw new InvalidOperationException("The free-body integrator did not preserve the world clock contract.");

            integrated.Add(definition.BodyId, advanced);
            definitions.Add(definition.BodyId, definition);

            var radius = definition.SphereRadiusMetres;
            var start = state.Pose.Position;
            var end = advanced.Pose.Position;
            var startBounds = new AxisAlignedBounds(
                start.X - radius, start.Y - radius, start.Z - radius,
                start.X + radius, start.Y + radius, start.Z + radius);
            var swept = startBounds.SweptBy(end.X - start.X, end.Y - start.Y, end.Z - start.Z);
            // IDs are deterministic within this sorted snapshot; no runtime hash is used.
            proxies.Add(new BroadphaseProxy(i + 1L, swept));
        }

        var candidates = SweepAndPruneBroadphase.FindCandidates(proxies);
        var activeContacts = new List<ContactObservation>();
        foreach (var pair in candidates)
        {
            var bodyA = ordered[checked((int)pair.FirstId - 1)];
            var bodyB = ordered[checked((int)pair.SecondId - 1)];
            var stateA = integrated[bodyA.Definition.BodyId];
            var stateB = integrated[bodyB.Definition.BodyId];
            var contact = SphereSphereContactDetector.Detect(
                ContactId(bodyA.Definition.BodyId, bodyB.Definition.BodyId),
                stateA, bodyA.Definition.SphereRadiusMetres,
                stateB, bodyB.Definition.SphereRadiusMetres,
                nextStep, nextTime, configuration.ContactToleranceMetres);

            if (contact is null)
            {
                var delta = stateB.Pose.Position - stateA.Pose.Position;
                if (delta == Vector3d.Zero &&
                    bodyA.Definition.SphereRadiusMetres + bodyB.Definition.SphereRadiusMetres > 0d)
                    throw new NotSupportedException("Coincident sphere centres have no defined contact normal.");
                continue;
            }

            activeContacts.Add(contact.Value);
        }

        var contactCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var contact in activeContacts)
        {
            contactCounts[contact.BodyAId] = contactCounts.GetValueOrDefault(contact.BodyAId) + 1;
            contactCounts[contact.BodyBId] = contactCounts.GetValueOrDefault(contact.BodyBId) + 1;
        }
        var multiplyContacting = contactCounts.Where(pair => pair.Value > 1).Select(pair => pair.Key).ToArray();
        if (multiplyContacting.Length > 0)
            throw new NotSupportedException(
                $"Simultaneous multi-contact response is unsupported; affected bodies: {string.Join(", ", multiplyContacting)}.");

        var finalStates = new Dictionary<string, RigidBodyState>(integrated, StringComparer.Ordinal);
        var finalContacts = new List<ContactObservation>(activeContacts.Count);
        foreach (var contact in activeContacts)
        {
            var definitionA = definitions[contact.BodyAId];
            var definitionB = definitions[contact.BodyBId];
            var response = ContactImpulseResolver.Resolve(
                contact,
                finalStates[contact.BodyAId], definitionA.MassKilograms,
                finalStates[contact.BodyBId], definitionB.MassKilograms,
                configuration.Restitution);

            var corrected = ContactPositionCorrector.Correct(
                response.Contact,
                response.BodyA, definitionA.MassKilograms,
                response.BodyB, definitionB.MassKilograms,
                configuration.PenetrationSlopMetres,
                configuration.CorrectionFraction,
                configuration.MaximumCorrectionMetres);

            finalStates[contact.BodyAId] = corrected.BodyA;
            finalStates[contact.BodyBId] = corrected.BodyB;

            var finalContact = SphereSphereContactDetector.Detect(
                contact.ContactId,
                corrected.BodyA, definitionA.SphereRadiusMetres,
                corrected.BodyB, definitionB.SphereRadiusMetres,
                nextStep, nextTime, configuration.ContactToleranceMetres);
            if (finalContact is not null)
            {
                var observation = finalContact.Value;
                finalContacts.Add(new ContactObservation(
                    observation.ContactId, observation.BodyAId, observation.BodyBId,
                    observation.Point, observation.NormalFromAToB, observation.RelativeVelocity,
                    observation.SignedSeparation, observation.IsActive, observation.StepIndex,
                    observation.ElapsedTime, response.Contact.NormalForceNewtons,
                    response.Contact.NormalImpulseNewtonSeconds));
            }
        }

        return new CpuWorldStepResult(
            configuration.ModelId, nextStep, nextTime,
            ordered.Select(body => finalStates[body.Definition.BodyId]),
            finalContacts.OrderBy(contact => contact.BodyAId, StringComparer.Ordinal)
                .ThenBy(contact => contact.BodyBId, StringComparer.Ordinal));
    }

    private static string ContactId(string bodyA, string bodyB) => $"{bodyA.Length}:{bodyA}{bodyB.Length}:{bodyB}";

    private static bool IsFinite(Vector3d value) =>
        double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
}
