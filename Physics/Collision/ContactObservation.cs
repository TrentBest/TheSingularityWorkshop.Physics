using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Collision;

/// <summary>
/// Immutable evidence that a collision/contact model reported a contact candidate
/// between two bodies at a specific logical simulation step.
/// </summary>
/// <remarks>
/// The contact point is in world-frame metres, the normal is a unit vector pointing
/// from body A toward body B, and relative velocity is world-frame metres per second
/// (body B minus body A). Signed separation is in metres: negative means penetration,
/// zero means touching, and positive means a gap accepted by the reporting model's
/// contact tolerance. Optional force and impulse measurements remain null when the
/// producing model did not compute them. This record does not itself detect contacts,
/// solve friction, establish support, or mutate body state.
/// </remarks>
public readonly record struct ContactObservation
{
    private const double UnitNormalTolerance = 1e-9;

    /// <summary>Gets the stable contact identity assigned by the producing model.</summary>
    public string ContactId { get; }

    /// <summary>Gets the stable identity of body A.</summary>
    public string BodyAId { get; }

    /// <summary>Gets the stable identity of body B.</summary>
    public string BodyBId { get; }

    /// <summary>Gets the world-frame contact point in metres.</summary>
    public Vector3d Point { get; }

    /// <summary>Gets the unit normal from body A toward body B.</summary>
    public Vector3d NormalFromAToB { get; }

    /// <summary>Gets world-frame relative velocity (body B minus body A) in metres per second.</summary>
    public Vector3d RelativeVelocity { get; }

    /// <summary>Gets signed surface separation in metres; negative values indicate penetration.</summary>
    public double SignedSeparation { get; }

    /// <summary>Gets whether the producing contact model considers this contact active at this step.</summary>
    public bool IsActive { get; }

    /// <summary>Gets optional normal force magnitude in newtons, or null if not computed.</summary>
    public double? NormalForceNewtons { get; }

    /// <summary>Gets optional normal impulse magnitude in newton-seconds, or null if not computed.</summary>
    public double? NormalImpulseNewtonSeconds { get; }

    /// <summary>Gets the zero-based logical simulation step for this observation.</summary>
    public long StepIndex { get; }

    /// <summary>Gets elapsed logical simulation time in seconds.</summary>
    public double ElapsedTime { get; }

    /// <summary>Gets whether this observation contains valid identities, geometry, time, and optional load values.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(ContactId) &&
        !string.IsNullOrWhiteSpace(BodyAId) &&
        !string.IsNullOrWhiteSpace(BodyBId) &&
        !string.Equals(BodyAId, BodyBId, StringComparison.Ordinal) &&
        IsFinite(Point) &&
        IsFinite(NormalFromAToB) &&
        IsUnitNormal(NormalFromAToB) &&
        IsFinite(RelativeVelocity) &&
        double.IsFinite(SignedSeparation) &&
        IsOptionalNonNegativeFinite(NormalForceNewtons) &&
        IsOptionalNonNegativeFinite(NormalImpulseNewtonSeconds) &&
        StepIndex >= 0 &&
        double.IsFinite(ElapsedTime) &&
        ElapsedTime >= 0d;

    /// <summary>Creates a validated contact observation. A null load means the producing model did not compute it.</summary>
    public ContactObservation(
        string contactId,
        string bodyAId,
        string bodyBId,
        Vector3d point,
        Vector3d normalFromAToB,
        Vector3d relativeVelocity,
        double signedSeparation,
        bool isActive,
        long stepIndex,
        double elapsedTime,
        double? normalForceNewtons = null,
        double? normalImpulseNewtonSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(contactId))
            throw new ArgumentException("A stable contact identity is required.", nameof(contactId));
        if (string.IsNullOrWhiteSpace(bodyAId))
            throw new ArgumentException("A stable body A identity is required.", nameof(bodyAId));
        if (string.IsNullOrWhiteSpace(bodyBId))
            throw new ArgumentException("A stable body B identity is required.", nameof(bodyBId));
        if (string.Equals(bodyAId, bodyBId, StringComparison.Ordinal))
            throw new ArgumentException("A contact must identify two distinct bodies.", nameof(bodyBId));
        if (!IsUnitNormal(normalFromAToB))
            throw new ArgumentException("The contact normal must be a unit vector pointing from body A toward body B.", nameof(normalFromAToB));
        if (!double.IsFinite(signedSeparation))
            throw new ArgumentOutOfRangeException(nameof(signedSeparation), "Signed separation must be finite.");
        if (!IsOptionalNonNegativeFinite(normalForceNewtons))
            throw new ArgumentOutOfRangeException(nameof(normalForceNewtons), "Normal force must be finite and non-negative when supplied.");
        if (!IsOptionalNonNegativeFinite(normalImpulseNewtonSeconds))
            throw new ArgumentOutOfRangeException(nameof(normalImpulseNewtonSeconds), "Normal impulse must be finite and non-negative when supplied.");
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex), "The logical step must be non-negative.");
        if (!double.IsFinite(elapsedTime) || elapsedTime < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedTime), "Elapsed logical time must be finite and non-negative.");

        ContactId = contactId;
        BodyAId = bodyAId;
        BodyBId = bodyBId;
        Point = point;
        NormalFromAToB = normalFromAToB;
        RelativeVelocity = relativeVelocity;
        SignedSeparation = signedSeparation;
        IsActive = isActive;
        NormalForceNewtons = normalForceNewtons;
        NormalImpulseNewtonSeconds = normalImpulseNewtonSeconds;
        StepIndex = stepIndex;
        ElapsedTime = elapsedTime;
    }

    private static bool IsUnitNormal(Vector3d normal)
    {
        try
        {
            return Math.Abs(normal.Magnitude - 1d) <= UnitNormalTolerance;
        }
        catch (ArithmeticException)
        {
            return false;
        }
    }

    private static bool IsFinite(Vector3d value) =>
        double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);

    private static bool IsOptionalNonNegativeFinite(double? value) =>
        value is null || (double.IsFinite(value.Value) && value.Value >= 0d);
}
