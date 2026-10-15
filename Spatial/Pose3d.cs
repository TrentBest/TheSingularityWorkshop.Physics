namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>
/// A rigid pose expressed in a right-handed world frame.
/// </summary>
/// <remarks>
/// Position is measured in metres in world coordinates. Orientation is a normalized
/// active rotation mapping body-local vectors into world-frame vectors. This type
/// describes a pose only; it does not imply that the pose was physically achieved
/// by a solver.
/// </remarks>
public readonly record struct Pose3d
{
    /// <summary>Gets the body origin position in world-frame metres.</summary>
    public Vector3d Position { get; }

    /// <summary>Gets the normalized body-local-to-world orientation.</summary>
    public Quaterniond Orientation { get; }

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
