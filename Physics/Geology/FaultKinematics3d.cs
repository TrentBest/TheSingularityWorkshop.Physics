using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>Declared geological fault mechanism; this label does not infer a stress regime.</summary>
public enum FaultMechanism3d
{
    Unclassified = 0,
    Normal = 1,
    Reverse = 2,
    Thrust = 3,
    StrikeSlipLeftLateral = 4,
    StrikeSlipRightLateral = 5,
    ObliqueSlip = 6
}

/// <summary>Which signed side of the oriented fault plane is the hanging wall.</summary>
public enum FaultHangingWallSide3d
{
    PositiveNormalSide = 1,
    NegativeNormalSide = -1
}

/// <summary>
/// A prescribed, planar fault displacement expressed in a caller-defined 3D coordinate frame.
/// </summary>
/// <remarks>
/// This is a kinematic block-displacement contract, not a stress, friction, rupture, or earthquake
/// solver. The caller supplies an orthonormal strike/down-dip basis. Positive strike slip follows
/// <see cref="StrikeDirection"/>; positive dip slip follows <see cref="DownDipDirection"/>.
/// The hanging wall receives the resulting displacement and the footwall remains fixed.
/// The oriented plane normal is StrikeDirection × DownDipDirection. The caller explicitly chooses
/// which normal side is the hanging wall; no handedness or tectonic sense is inferred.
/// Points exactly on the mathematical plane receive zero displacement. The plane is infinite;
/// finite fault patches, segmented surfaces, and distributed near-fault deformation are future models.
/// </remarks>
public sealed class FaultKinematics3d
{
    private const double BasisTolerance = 1e-9;

    /// <summary>Creates a prescribed planar fault displacement.</summary>
    public FaultKinematics3d(
        string faultId,
        string coordinateFrameId,
        string linearUnit,
        Vector3d planeOrigin,
        Vector3d strikeDirection,
        Vector3d downDipDirection,
        FaultHangingWallSide3d hangingWallSide,
        FaultMechanism3d mechanism,
        double strikeSlip,
        double dipSlip)
    {
        RequireText(faultId, nameof(faultId));
        RequireText(coordinateFrameId, nameof(coordinateFrameId));
        RequireText(linearUnit, nameof(linearUnit));
        if (!Enum.IsDefined(hangingWallSide))
            throw new ArgumentOutOfRangeException(nameof(hangingWallSide));
        if (!Enum.IsDefined(mechanism))
            throw new ArgumentOutOfRangeException(nameof(mechanism));
        if (!double.IsFinite(strikeSlip))
            throw new ArgumentOutOfRangeException(nameof(strikeSlip));
        if (!double.IsFinite(dipSlip))
            throw new ArgumentOutOfRangeException(nameof(dipSlip));

        var strike = strikeDirection.Normalized();
        var downDip = downDipDirection.Normalized();
        if (Math.Abs(strike.Dot(downDip)) > BasisTolerance)
            throw new ArgumentException("Strike and down-dip directions must be perpendicular.", nameof(downDipDirection));

        var normal = strike.Cross(downDip).Normalized();

        FaultId = faultId;
        CoordinateFrameId = coordinateFrameId;
        LinearUnit = linearUnit;
        PlaneOrigin = planeOrigin;
        StrikeDirection = strike;
        DownDipDirection = downDip;
        UnitNormal = normal;
        HangingWallSide = hangingWallSide;
        Mechanism = mechanism;
        StrikeSlip = strikeSlip;
        DipSlip = dipSlip;
        HangingWallDisplacement = (strike * strikeSlip) + (downDip * dipSlip);
    }

    /// <summary>Stable identity of this fault segment.</summary>
    public string FaultId { get; }
    /// <summary>Caller-defined coordinate-frame identity.</summary>
    public string CoordinateFrameId { get; }
    /// <summary>Caller-defined unit shared by positions and slip components.</summary>
    public string LinearUnit { get; }
    /// <summary>A point on the mathematical fault plane.</summary>
    public Vector3d PlaneOrigin { get; }
    /// <summary>Unit vector defining positive strike-slip direction.</summary>
    public Vector3d StrikeDirection { get; }
    /// <summary>Unit vector defining positive down-dip slip direction.</summary>
    public Vector3d DownDipDirection { get; }
    /// <summary>Unit normal oriented as strike cross down-dip.</summary>
    public Vector3d UnitNormal { get; }
    /// <summary>Caller-declared side of the plane occupied by the hanging wall.</summary>
    public FaultHangingWallSide3d HangingWallSide { get; }
    /// <summary>Caller-declared mechanism; no stress regime is inferred.</summary>
    public FaultMechanism3d Mechanism { get; }
    /// <summary>Signed displacement component along strike, in the declared linear unit.</summary>
    public double StrikeSlip { get; }
    /// <summary>Signed displacement component down dip, in the declared linear unit.</summary>
    public double DipSlip { get; }
    /// <summary>Full hanging-wall displacement vector in the declared coordinate frame.</summary>
    public Vector3d HangingWallDisplacement { get; }

    /// <summary>Returns signed perpendicular distance from a point to the oriented fault plane.</summary>
    public double SignedDistanceToPlane(Vector3d point) => (point - PlaneOrigin).Dot(UnitNormal);

    /// <summary>Returns true when a point lies strictly on the caller-declared hanging-wall side.</summary>
    public bool IsHangingWall(Vector3d point)
    {
        var signedDistance = SignedDistanceToPlane(point);
        return HangingWallSide == FaultHangingWallSide3d.PositiveNormalSide
            ? signedDistance > 0d
            : signedDistance < 0d;
    }

    /// <summary>
    /// Returns the prescribed block displacement at a point: hanging wall moves, footwall stays fixed.
    /// Points exactly on the mathematical plane return zero because side ownership is undefined there.
    /// </summary>
    public Vector3d GetBlockDisplacement(Vector3d point) =>
        IsHangingWall(point) ? HangingWallDisplacement : Vector3d.Zero;

    private static void RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
    }
}
