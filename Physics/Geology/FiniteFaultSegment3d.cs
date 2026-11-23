using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// A finite rectangular footprint for a prescribed planar fault block displacement.
/// </summary>
/// <remarks>
/// The rectangle is centred on the underlying fault plane origin. Its long axis follows strike
/// and its short axis follows down-dip. The supplied <see cref="FaultKinematics3d"/> still
/// determines the hanging-wall side and displacement vector. Points on the footwall, outside
/// the finite footprint, or exactly on the fault plane receive zero displacement.
///
/// This is a bounded kinematic block-offset model, not an elastic dislocation solution.
/// Displacement changes discontinuously at the rectangular footprint boundary; the type does
/// not model stress transfer, rupture propagation, tip effects, or distributed deformation.
/// </remarks>
public sealed class FiniteFaultSegment3d
{
    /// <summary>Creates a finite rectangular segment around a prescribed fault plane origin.</summary>
    /// <param name="kinematics">The declared plane orientation, hanging-wall side, and slip vector.</param>
    /// <param name="halfStrikeLength">Half the segment length on either side of the plane origin, in the fault unit.</param>
    /// <param name="halfDownDipWidth">Half the segment width on either side of the plane origin, in the fault unit.</param>
    public FiniteFaultSegment3d(
        FaultKinematics3d kinematics,
        double halfStrikeLength,
        double halfDownDipWidth)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        if (!double.IsFinite(halfStrikeLength) || halfStrikeLength <= 0d)
            throw new ArgumentOutOfRangeException(nameof(halfStrikeLength));
        if (!double.IsFinite(halfDownDipWidth) || halfDownDipWidth <= 0d)
            throw new ArgumentOutOfRangeException(nameof(halfDownDipWidth));

        Kinematics = kinematics;
        HalfStrikeLength = halfStrikeLength;
        HalfDownDipWidth = halfDownDipWidth;
    }

    /// <summary>The prescribed planar kinematics shared by this finite segment.</summary>
    public FaultKinematics3d Kinematics { get; }

    /// <summary>Half the finite segment length along strike.</summary>
    public double HalfStrikeLength { get; }

    /// <summary>Half the finite segment width down dip.</summary>
    public double HalfDownDipWidth { get; }

    /// <summary>Returns true when a point projects inside or on the rectangular segment footprint.</summary>
    public bool ContainsProjection(Vector3d point)
    {
        var relative = point - Kinematics.PlaneOrigin;
        var alongStrike = relative.Dot(Kinematics.StrikeDirection);
        var alongDownDip = relative.Dot(Kinematics.DownDipDirection);

        return Math.Abs(alongStrike) <= HalfStrikeLength &&
               Math.Abs(alongDownDip) <= HalfDownDipWidth;
    }

    /// <summary>
    /// Returns the prescribed block displacement only within the finite segment footprint
    /// and on its declared hanging-wall side.
    /// </summary>
    public Vector3d GetBlockDisplacement(Vector3d point) =>
        ContainsProjection(point) ? Kinematics.GetBlockDisplacement(point) : Vector3d.Zero;
}
