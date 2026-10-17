namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>A rigid 2D coordinate transform with explicit source and destination frames.</summary>
/// <remarks>
/// Angles are in radians. Positive angles rotate counter-clockwise in the usual right-handed 2D
/// convention. The transform applies column-vector coordinates as destination = R(angle) * source
/// + translation, where translation is expressed in the destination frame. This is a floating-point
/// transform, not an exact geometric predicate; round trips can accumulate rounding error.
/// </remarks>
public readonly struct RigidTransform2d
{
    /// <summary>Gets the source coordinate frame.</summary>
    public CoordinateFrame2d SourceFrame { get; }
    /// <summary>Gets the destination coordinate frame.</summary>
    public CoordinateFrame2d DestinationFrame { get; }
    /// <summary>Gets the counter-clockwise rotation angle in radians.</summary>
    public double AngleRadians { get; }
    /// <summary>Gets the translation expressed in the destination frame.</summary>
    public Vector2d Translation { get; }

    /// <summary>Creates a rigid transform between two explicit frames.</summary>
    public RigidTransform2d(
        CoordinateFrame2d sourceFrame,
        CoordinateFrame2d destinationFrame,
        double angleRadians,
        Vector2d translation)
    {
        if (string.IsNullOrWhiteSpace(sourceFrame.Id))
            throw new ArgumentException("A valid source frame is required.", nameof(sourceFrame));
        if (string.IsNullOrWhiteSpace(destinationFrame.Id))
            throw new ArgumentException("A valid destination frame is required.", nameof(destinationFrame));
        if (!double.IsFinite(angleRadians))
            throw new ArgumentOutOfRangeException(nameof(angleRadians), "Angle must be finite and expressed in radians.");

        SourceFrame = sourceFrame;
        DestinationFrame = destinationFrame;
        AngleRadians = angleRadians;
        Translation = translation;
    }

    /// <summary>Transforms a point, rejecting points not expressed in the source frame.</summary>
    public Point2d Transform(Point2d point)
    {
        if (point.Frame != SourceFrame)
            throw new InvalidOperationException(
                $"Transform expects frame '{SourceFrame.Id}', but point uses '{point.Frame.Id}'.");

        return new Point2d(DestinationFrame, Rotate(point.Coordinates, AngleRadians) + Translation);
    }

    /// <summary>Transforms a direction vector without applying translation.</summary>
    public Vector2d TransformDirection(Vector2d direction) => Rotate(direction, AngleRadians);

    /// <summary>Returns the transform that reverses this transform, subject to floating-point rounding.</summary>
    public RigidTransform2d Inverse()
    {
        double inverseAngle = -AngleRadians;
        Vector2d inverseTranslation = Rotate(Translation * -1d, inverseAngle);
        return new RigidTransform2d(DestinationFrame, SourceFrame, inverseAngle, inverseTranslation);
    }

    /// <summary>
    /// Composes this transform with a following transform.
    /// The returned transform applies this transform first, then <paramref name="next"/>.
    /// </summary>
    public RigidTransform2d Then(RigidTransform2d next)
    {
        if (DestinationFrame != next.SourceFrame)
            throw new InvalidOperationException(
                $"Cannot compose transform ending in '{DestinationFrame.Id}' with one starting in '{next.SourceFrame.Id}'.");

        double angle = AngleRadians + next.AngleRadians;
        if (!double.IsFinite(angle))
            throw new ArithmeticException("RigidTransform2d composition produced a non-finite angle.");

        Vector2d translation = Rotate(Translation, next.AngleRadians) + next.Translation;
        return new RigidTransform2d(SourceFrame, next.DestinationFrame, angle, translation);
    }

    private static Vector2d Rotate(Vector2d vector, double angleRadians)
    {
        double cosine = System.Math.Cos(angleRadians);
        double sine = System.Math.Sin(angleRadians);
        return new Vector2d(
            (cosine * vector.X) - (sine * vector.Y),
            (sine * vector.X) + (cosine * vector.Y));
    }
}
