namespace TheSingularityWorkshop.Physics.Spatial;

/// <summary>Classifies a point relative to a closed 2D polygon boundary.</summary>
public enum PointPolygonRelation2d
{
    /// <summary>The point is outside the polygon.</summary>
    Outside,
    /// <summary>The point lies on a polygon edge or vertex.</summary>
    Boundary,
    /// <summary>The point is inside the polygon.</summary>
    Inside
}
