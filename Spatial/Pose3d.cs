namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// A rigid pose expressed in a right-handed world frame.
/// </summary>
/// <remarks>
/// Position is measured in metres in world coordinates. Orientation is a normalized
/// active rotation mapping body-local vectors into world-frame vectors. This type
/// describes a pose only; it does not imply that the pose was physically achieved
/// by a solver. Since value types can be default-initialized, check <see cref="IsValid"/>
/// before consuming a pose from an untrusted or uninitialized source.
/// </remarks>
public readonly record struct Pose3d
{
    /// <summary>Gets the body origin position in world-frame metres.</summary>
    public Vector3d Position { get; }

    /// <summary>Gets the normalized body-local-to-world orientation.</summary>
    public Quaterniond Orientation { get; }

    /// <summary>Gets whether this pose contains a non-zero orientation.</summary>
    public bool IsValid =>
        Orientation.W != 0d || Orientation.X != 0d ||
        Orientation.Y != 0d || Orientation.Z != 0d;

    /// <summary>Creates a pose with a finite position and a valid orientation.</summary>
    /// <exception cref="InvalidOperationException">The orientation is the zero quaternion.</exception>
    public Pose3d(Vector3d position, Quaterniond orientation)
    {
        Position = position;
        Orientation = orientation.Normalized();
    }

    /// <summary>Transforms a body-local direction into the world frame.</summary>
    public Vector3d TransformDirectionToWorld(Vector3d bodyLocalDirection) =>
        Orientation.Rotate(bodyLocalDirection);

    /// <summary>Transforms a world-frame direction into the body-local frame.</summary>
    public Vector3d TransformDirectionToLocal(Vector3d worldDirection) =>
        Orientation.Conjugate().Normalized().Rotate(worldDirection);
}
