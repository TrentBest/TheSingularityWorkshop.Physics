# Geometry Foundation: 2D Vectors

**Status:** finite 2D vector arithmetic, exact-sign orientation, closed-segment intersection classification, and an explicit absolute-distance tolerance are implemented. This is not a complete geometry or collision API.

## Contract

[`Vector2d`](../Physics/Spatial/Vector2d.cs) represents two finite, unit-agnostic components. It provides addition, subtraction, scalar scaling/division, squared length, length, dot product, signed 2D cross product, and squared distance. Focused tests live in [Vector2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Vector2dTests.cs).

For three points, [`Geometry2d.Orient`](../Physics/Spatial/Geometry2d.cs) returns `CounterClockwise`, `Clockwise`, or `Collinear`. A conservative floating-point filter uses exact integer arithmetic over represented binary64 coordinates when the filter cannot safely determine the sign. This is exact classification of supplied values, not a claim that measured inputs are physically exact. See [Geometry2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Geometry2dTests.cs).

[`Geometry2dSegments.ClassifyIntersection`](../Physics/Spatial/Geometry2dSegments.cs) classifies two closed segments as disjoint, proper intersection, endpoint touch, collinear overlap, or degenerate-point intersection. It deliberately returns a kind, not an intersection coordinate; coordinate calculation needs its own precision and failure contract. See [Geometry2dSegmentsTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Geometry2dSegmentsTests.cs).

[`GeometryTolerance2d`](../Physics/Spatial/GeometryTolerance2d.cs) makes a caller-selected absolute Euclidean-distance threshold explicit. Its value must be finite and non-negative and uses the same coordinate units as the points. `IsNear` performs a scaled distance comparison to avoid squared-distance overflow. There is no universal default: the appropriate tolerance depends on coordinate scale, units, source uncertainty, and the decision being made. See [GeometryTolerance2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/GeometryTolerance2dTests.cs).

## Deliberate limits

- Components have no built-in unit or coordinate-frame identity; mixing frames or units remains the caller's responsibility.
- Equality is exact component equality. A tolerance comparison is a separate explicit operation, not a replacement for equality or robust orientation.
- Tolerance-based proximity does not alter segment intersection classification. A near miss remains disjoint under the exact represented-coordinate classifier, even if a domain-specific solver later chooses to treat it as contact.
- The type rejects non-finite inputs and arithmetic results that overflow to non-finite values. Some finite squared lengths can still overflow; scaled norm helpers remain future work.
- Exact orientation fallback prioritizes correctness over throughput and is not a GPU kernel. The tolerance policy is scalar CPU code and does not represent measurement uncertainty unless the caller chooses a threshold from an explicit uncertainty model.

## Next geometry slices

1. Establish coordinate-frame identity/conversion conventions before combining geometry from separate spaces.
2. Specify intersection-coordinate computation with a numerical contract for parallel, collinear, endpoint-touching, degenerate, ill-conditioned, and out-of-range cases.
3. Reuse the same semantics in polygon tests, 2D collision, and living visual explanations.
4. Add batch/SoA equivalents only where they share scalar reference fixtures and declare whether they preserve exact or approximate predicate semantics.

These primitives establish a reusable geometry vocabulary; they are not evidence that a general geometry engine exists.
