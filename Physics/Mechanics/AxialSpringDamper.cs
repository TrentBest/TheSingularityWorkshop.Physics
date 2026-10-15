namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>
/// One-dimensional two-body compliant link with a linear spring and linear damper.
/// This bounded model reports the link's computed axial force; it is not a rigid
/// joint, general constraint solver, collision model, or structural-failure model.
/// Positions are metres, velocities metres per second, masses kilograms,
/// stiffness newtons per metre, damping newton-seconds per metre, and force newtons.
/// The positive axis points from body A toward body B in the supplied coordinate line.
/// </summary>
public static class AxialSpringDamper
{
    /// <summary>Immutable one-dimensional body snapshot.</summary>
    public readonly record struct BodyState(
        string BodyId,
        double Mass,
        double Position,
        double Velocity)
    {
        /// <summary>True when identity is present, mass is positive, and state is finite.</summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(BodyId) &&
            double.IsFinite(Mass) && Mass > 0d &&
            double.IsFinite(Position) &&
            double.IsFinite(Velocity);
    }

    /// <summary>Parameters describing one compliant link between two distinct bodies.</summary>
    public sealed record Definition(
        string LinkId,
        double RestLength,
        double Stiffness,
        double Damping)
    {
        /// <summary>Validates identity and finite, non-negative link parameters.</summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(LinkId))
                throw new ArgumentException("Link identity is required.", nameof(LinkId));
            if (!double.IsFinite(RestLength) || RestLength < 0d)
                throw new ArgumentOutOfRangeException(nameof(RestLength), "Rest length must be finite and non-negative.");
            if (!double.IsFinite(Stiffness) || Stiffness < 0d)
                throw new ArgumentOutOfRangeException(nameof(Stiffness), "Stiffness must be finite and non-negative.");
            if (!double.IsFinite(Damping) || Damping < 0d)
                throw new ArgumentOutOfRangeException(nameof(Damping), "Damping must be finite and non-negative.");
        }
    }

    /// <summary>Computed load and resulting body snapshots for one logical step.</summary>
    public sealed record StepResult(
        string LinkId,
        long StepIndex,
        double ElapsedTime,
        double AxialForceOnBodyA,
        BodyState BodyA,
        BodyState BodyB);

    /// <summary>
    /// Computes one semi-implicit Euler step. Positive axial force acts on body A
    /// toward positive coordinates; the equal-and-opposite force acts on body B.
    /// The reported force is the spring-damper force evaluated from the pre-step
    /// separation and relative velocity, not an impulse or a rigid-joint reaction.
    /// </summary>
    public static StepResult Advance(
        Definition definition,
        BodyState bodyA,
        BodyState bodyB,
        long stepIndex,
        double elapsedTime,
        double deltaTime)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();
        if (!bodyA.IsValid)
            throw new ArgumentException("Body A must have a non-empty identity and finite state with positive mass.", nameof(bodyA));
        if (!bodyB.IsValid)
            throw new ArgumentException("Body B must have a non-empty identity and finite state with positive mass.", nameof(bodyB));
        if (string.Equals(bodyA.BodyId, bodyB.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("The link must connect two distinct body identities.", nameof(bodyB));
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex), "Step index must be non-negative.");
        if (!double.IsFinite(elapsedTime) || elapsedTime < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedTime), "Elapsed time must be finite and non-negative.");
        if (!double.IsFinite(deltaTime) || deltaTime <= 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Timestep must be positive and finite.");

        var separation = bodyB.Position - bodyA.Position;
        var relativeVelocity = bodyB.Velocity - bodyA.Velocity;
        var extension = separation - definition.RestLength;
        var forceOnA = (definition.Stiffness * extension) + (definition.Damping * relativeVelocity);
        var accelerationA = forceOnA / bodyA.Mass;
        var accelerationB = -forceOnA / bodyB.Mass;
        var velocityA = bodyA.Velocity + (accelerationA * deltaTime);
        var velocityB = bodyB.Velocity + (accelerationB * deltaTime);
        var positionA = bodyA.Position + (velocityA * deltaTime);
        var positionB = bodyB.Position + (velocityB * deltaTime);
        var nextElapsedTime = elapsedTime + deltaTime;

        if (!double.IsFinite(separation) || !double.IsFinite(relativeVelocity) ||
            !double.IsFinite(extension) || !double.IsFinite(forceOnA) ||
            !double.IsFinite(accelerationA) || !double.IsFinite(accelerationB) ||
            !double.IsFinite(velocityA) || !double.IsFinite(velocityB) ||
            !double.IsFinite(positionA) || !double.IsFinite(positionB) ||
            !double.IsFinite(nextElapsedTime))
            throw new OverflowException("The spring-damper step produced a value outside finite double range.");

        return new StepResult(
            definition.LinkId,
            stepIndex,
            nextElapsedTime,
            forceOnA,
            bodyA with { Position = positionA, Velocity = velocityA },
            bodyB with { Position = positionB, Velocity = velocityB });
    }
}
