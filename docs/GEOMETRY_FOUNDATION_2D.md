# Geometry Foundation: 2D Vectors

**Status:** finite 2D vector arithmetic, exact-sign orientation, closed-segment intersection classification, unique-point intersection construction, explicit absolute-distance tolerance, frame-tagged point identity, and frame-aware rigid 2D transforms are implemented. A complete geometry/collision API is not implemented.

## Contract

[`Vector2d`](../Physics/Spatial/Vector2d.cs) represents two finite, unit-agnostic components. It provides addition, subtraction, scalar scaling/division, squared length, length, dot product, signed 2D cross product, and squared distance. Focused tests live in [Vector2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Vector2dTests.cs).

[`CoordinateFrame2d`](../Physics/Spatial/CoordinateFrame2d.cs) provides a stable, non-empty, ordinal/case-sensitive identifier for a caller-defined frame. The owner must document its origin, axes, orientation, and coordinate units; the identifier does not define a transform or prove compatible conventions. [`Point2d`](../Physics/Spatial/Point2d.cs) pairs coordinates with that identity, includes the frame in exact equality, and exposes `RequireSameFrame` to reject comparisons across different frame IDs. [`RigidTransform2d`](../Physics/Spatial/RigidTransform2d.cs) maps points between explicit source and destination frames using counter-clockwise radians and destination-frame translation. It supports direction-only rotation, inverse, and ordered composition (`first.Then(next)` means apply first, then next); point application and composition reject frame mismatches. Focused tests are in [CoordinateFrame2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/CoordinateFrame2dTests.cs) and [RigidTransform2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/RigidTransform2dTests.cs).

For three points, [`Geometry2d.Orient`](../Physics/Spatial/Geometry2d.cs) returns `CounterClockwise`, `Clockwise`, or `Collinear`. A conservative floating-point filter uses exact integer arithmetic over represented binary64 coordinates when the filter cannot safely determine the sign. This is exact classification of supplied values, not a claim that measured inputs are physically exact. See [Geometry2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Geometry2dTests.cs).

[`Geometry2dSegments.ClassifyIntersection`](../Physics/Spatial/Geometry2dSegments.cs) classifies two closed segments as disjoint, proper intersection, endpoint touch, collinear overlap, or degenerate-point intersection. [`Geometry2dSegments.Intersect`](../Physics/Spatial/SegmentIntersection2d.cs) returns that classification plus a coordinate only when there is a unique point; disjoint segments and collinear overlaps have no unique point. Endpoint and degenerate intersections reuse an input coordinate. Proper-crossing construction normalizes coordinates to reduce intermediate overflow, but remains a floating-point construction and can fail for ill-conditioned cases. See [Geometry2dSegmentsTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Geometry2dSegmentsTests.cs) and [SegmentIntersection2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/SegmentIntersection2dTests.cs).

[`GeometryTolerance2d`](../Physics/Spatial/GeometryTolerance2d.cs) makes a caller-selected absolute Euclidean-distance threshold explicit. Its value must be finite and non-negative and uses the same coordinate units as the points. `IsNear` performs a scaled distance comparison to avoid squared-distance overflow. There is no universal default: the appropriate tolerance depends on coordinate scale, units, source uncertainty, and the decision being made. See [GeometryTolerance2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/GeometryTolerance2dTests.cs).

## Deliberate limits

- Frame identifiers are labels, not a unit system or transform graph. Reusing an identifier for inconsistent conventions is a caller error. `RigidTransform2d` provides explicit pairwise conversion, not a graph, automatic path finding, unit conversion, or convention discovery.
- Raw `Vector2d` operations remain frame-agnostic; use `Point2d` where frame identity matters.
- Equality is exact component equality. A tolerance comparison is a separate explicit operation, not a replacement for equality or robust orientation.
- Tolerance-based proximity does not alter segment intersection classification. A near miss remains disjoint under the exact represented-coordinate classifier, even if a domain-specific solver later chooses to treat it as contact.
- The type rejects non-finite inputs and arithmetic results that overflow to non-finite values. Some finite squared lengths can still overflow; scaled norm helpers remain future work.
- Exact orientation fallback prioritizes correctness over throughput and is not a GPU kernel. The tolerance policy is scalar CPU code and does not represent measurement uncertainty unless the caller chooses a threshold from an explicit uncertainty model.

## Next geometry slices

1. Strengthen intersection-coordinate construction with adversarial numerical tests and an explicit policy for ill-conditioned cases; the first bounded API is now implemented.
2. Add transform graph/path policy only when a consumer needs more than explicit pairwise transforms; define cycle and consistency behavior.
3. Reuse the same semantics in polygon tests, 2D collision, and living visual explanations.
4. Add batch/SoA equivalents only where they share scalar reference fixtures and declare whether they preserve exact or approximate predicate semantics.

These primitives establish a reusable geometry vocabulary; they are not evidence that a general geometry engine exists.
