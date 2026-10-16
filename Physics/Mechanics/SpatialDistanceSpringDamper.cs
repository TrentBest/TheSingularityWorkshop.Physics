using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>
/// A translational-only, three-dimensional compliant link between two body origins.
/// </summary>
/// <remarks>
/// The link applies equal-and-opposite axial spring/damper forces along the line
/// between body origins, then advances their world-frame linear motion with
/// semi-implicit Euler. Position is in metres, velocity in metres per second,
/// mass in kilograms, stiffness in newtons per metre, damping in newton-seconds
/// per metre, and force in newtons. Body orientation and angular velocity are
/// preserved but do not affect this model. This is not a rigid joint, contact
/// model, rotational rigid-body solver, or general articulated solver.
/// </remarks>
public static class SpatialDistanceSpringDamper
{
    /// <summary>Parameters for a compliant link between two distinct bodies.</summary>
    public sealed record Definition(
        string LinkId,
        double RestLength,
        double Stiffness,
        double Damping)
    {
        /// <summary>Validates link identity and finite non-negative parameters.</summary>
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

    /// <summary>Computed axial load and achieved body snapshots for one logical step.</summary>
    public sealed record StepResult(
        string LinkId,
        long StepIndex,
        double ElapsedTime,
        Vector3d ForceOnBodyA,
        Vector3d ForceOnBodyB,
        RigidBodyState BodyA,
        RigidBodyState BodyB);

    /// <summary>
    /// Computes one semi-implicit Euler step from the pre-step axial spring/damper load.
    /// The reported force is not an impulse or a rigid-joint reaction.
    /// </summary>
    public static StepResult Advance(
        Definition definition,
        RigidBodyState bodyA,
        double massA,
        RigidBodyState bodyB,
        double massB,
        long stepIndex,
        double elapsedTime,
        double deltaTime)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.Validate();
        ValidateBody(bodyA, massA, nameof(bodyA), nameof(massA));
        ValidateBody(bodyB, massB, nameof(bodyB), nameof(massB));
        if (string.Equals(bodyA.BodyId, bodyB.BodyId, StringComparison.Ordinal))
            throw new ArgumentException("The link must connect two distinct body identities.", nameof(bodyB));
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex), "Step index must be non-negative.");
        if (!double.IsFinite(elapsedTime) || elapsedTime < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedTime), "Elapsed time must be finite and non-negative.");
        if (!double.IsFinite(deltaTime) || deltaTime <= 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Timestep must be positive and finite.");

        var displacement = bodyB.Pose.Position - bodyA.Pose.Position;
        var distance = displacement.Magnitude;
        var nextElapsedTime = elapsedTime + deltaTime;
        if (!double.IsFinite(nextElapsedTime))
            throw new OverflowException("The logical time step exceeded the finite double range.");

        // Coincident origins have no geometrically defined axial direction. In that
        // singular case, this central-force model reports no axial force for the step.
        var forceOnA = Vector3d.Zero;
        if (distance > 0d)
        {
            var direction = displacement / distance;
            var relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;
            var radialVelocity = relativeVelocity.Dot(direction);
            var extension = distance - definition.RestLength;
            var axialForce = (definition.Stiffness * extension) + (definition.Damping * radialVelocity);
            if (!double.IsFinite(axialForce))
                throw new OverflowException("The spring-damper force exceeded the finite double range.");
            forceOnA = direction * axialForce;
        }

        var accelerationA = forceOnA / massA;
        var accelerationB = -forceOnA / massB;
        var velocityA = bodyA.LinearVelocity + (accelerationA * deltaTime);
        var velocityB = bodyB.LinearVelocity + (accelerationB * deltaTime);
        var positionA = bodyA.Pose.Position + (velocityA * deltaTime);
        var positionB = bodyB.Pose.Position + (velocityB * deltaTime);

        var achievedA = new RigidBodyState(
            bodyA.BodyId,
            new Pose3d(positionA, bodyA.Pose.Orientation),
            velocityA,
            bodyA.AngularVelocity,
            stepIndex,
            nextElapsedTime);
        var achievedB = new RigidBodyState(
            bodyB.BodyId,
            new Pose3d(positionB, bodyB.Pose.Orientation),
            velocityB,
            bodyB.AngularVelocity,
            stepIndex,
            nextElapsedTime);

        return new StepResult(
            definition.LinkId,
            stepIndex,
            nextElapsedTime,
            forceOnA,
            -forceOnA,
            achievedA,
            achievedB);
    }

    private static void ValidateBody(
        RigidBodyState body,
        double mass,
        string bodyParameter,
        string massParameter)
    {
        if (!body.IsValid)
            throw new ArgumentException("A valid body state with stable identity and finite logical time is required.", bodyParameter);
        if (!double.IsFinite(mass) || mass <= 0d)
            throw new ArgumentOutOfRangeException(massParameter, "Mass must be positive and finite.");
        if (!IsFinite(body.LinearVelocity) || !IsFinite(body.AngularVelocity))
            throw new ArgumentException("Body velocities must be finite.", bodyParameter);
    }

    private static bool IsFinite(Vector3d value) =>
        double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
}
