using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// Describes a finite-fault-plane intersection with one layer in a sampled stratigraphic column.
/// </summary>
/// <remarks>
/// This is a geometric cross-cutting observation only. It does not offset the layer, infer slip,
/// model deformation, or modify either input. A vertical column has a unique plane intersection
/// only when the fault plane has a non-negligible vertical normal component. Intersections on
/// layer boundaries are deliberately not assigned to either adjacent layer.
/// </remarks>
public sealed class FaultStratigraphicIntersection3d
{
    private FaultStratigraphicIntersection3d(
        string faultId,
        string columnId,
        string layerId,
        int materialId,
        string formationEventId,
        Vector3d intersectionPoint)
    {
        FaultId = faultId;
        ColumnId = columnId;
        LayerId = layerId;
        MaterialId = materialId;
        FormationEventId = formationEventId;
        IntersectionPoint = intersectionPoint;
    }

    /// <summary>Stable identity of the finite fault segment.</summary>
    public string FaultId { get; }

    /// <summary>Stable identity of the sampled stratigraphic column.</summary>
    public string ColumnId { get; }

    /// <summary>Identity of the layer cut by the fault plane.</summary>
    public string LayerId { get; }

    /// <summary>Caller-defined material catalogue identity of the intersected layer.</summary>
    public int MaterialId { get; }

    /// <summary>Formation/deposition event identity retained from the original layer.</summary>
    public string FormationEventId { get; }

    /// <summary>Unique geometric intersection point in the declared coordinate frame and unit.</summary>
    public Vector3d IntersectionPoint { get; }

    /// <summary>
    /// Attempts to find the unique intersection of a finite fault plane and a vertical column.
    /// </summary>
    /// <param name="fault">Finite fault segment whose plane and footprint are tested.</param>
    /// <param name="column">Vertical stratigraphic column to intersect.</param>
    /// <param name="intersection">The intersected layer record, or null when no unique layer intersection exists.</param>
    /// <param name="verticalNormalTolerance">Minimum absolute vertical component of the unit plane normal.</param>
    /// <returns>True only when the plane crosses the interior of one layer inside the finite segment footprint.</returns>
    /// <remarks>
    /// The column's WorldX/WorldY and layer elevations are interpreted in the fault's coordinate
    /// frame and linear unit, with increasing elevation aligned to the positive Z axis. This
    /// first contract does not support arbitrary vertical-axis orientations. Coordinate-frame
    /// and unit identities must match exactly. A plane
    /// parallel to or coincident with the vertical column has no unique point intersection and
    /// returns false. A point on a layer boundary, in a stratigraphic gap, or outside the finite
    /// fault footprint also returns false. This method records no displacement.
    /// </remarks>
    public static bool TryIntersectColumn(
        FiniteFaultSegment3d fault,
        StratigraphicColumn3d column,
        out FaultStratigraphicIntersection3d? intersection,
        double verticalNormalTolerance = 1e-12)
    {
        ArgumentNullException.ThrowIfNull(fault);
        ArgumentNullException.ThrowIfNull(column);
        if (!double.IsFinite(verticalNormalTolerance) || verticalNormalTolerance < 0d)
            throw new ArgumentOutOfRangeException(nameof(verticalNormalTolerance));

        var kinematics = fault.Kinematics;
        if (!string.Equals(kinematics.CoordinateFrameId, column.CoordinateFrameId, StringComparison.Ordinal))
            throw new ArgumentException("Fault and column coordinate frames must match.", nameof(column));
        if (!string.Equals(kinematics.LinearUnit, column.LinearUnit, StringComparison.Ordinal))
            throw new ArgumentException("Fault and column linear units must match.", nameof(column));

        var normalZ = kinematics.UnitNormal.Z;
        if (Math.Abs(normalZ) <= verticalNormalTolerance)
        {
            intersection = null;
            return false;
        }

        var elevation = kinematics.PlaneOrigin.Z -
            ((column.WorldX - kinematics.PlaneOrigin.X) * kinematics.UnitNormal.X +
             (column.WorldY - kinematics.PlaneOrigin.Y) * kinematics.UnitNormal.Y) / normalZ;
        if (!double.IsFinite(elevation))
        {
            intersection = null;
            return false;
        }

        var point = new Vector3d(column.WorldX, column.WorldY, elevation);
        if (!fault.ContainsProjection(point))
        {
            intersection = null;
            return false;
        }

        StratigraphicLayer3d? matchedLayer = null;
        foreach (var layer in column.Layers)
        {
            if (elevation < layer.TopElevation && elevation > layer.BottomElevation)
            {
                matchedLayer = layer;
                break;
            }
        }

        if (matchedLayer is null)
        {
            intersection = null;
            return false;
        }

        intersection = new FaultStratigraphicIntersection3d(
            kinematics.FaultId,
            column.ColumnId,
            matchedLayer.LayerId,
            matchedLayer.MaterialId,
            matchedLayer.ProvenanceEventId,
            point);
        return true;
    }
}
