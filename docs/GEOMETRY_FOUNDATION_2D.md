# Geometry Foundation: 2D Vectors

**Status:** implemented 2D vector arithmetic and a robust orientation predicate; not a complete geometry or collision API.

## Contract

[`Vector2d`](../Physics/Spatial/Vector2d.cs) represents two finite, unit-agnostic components. It provides addition, subtraction, scalar scaling/division, squared length, length, dot product, signed 2D cross product, and squared distance. Focused tests live in [Vector2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Vector2dTests.cs).

The cross product returns the scalar z-component (x_1 y_2-y_1 x_2). Its sign indicates orientation for vector pairs, but computing it directly is not a robust point-orientation test. For three points, [`Geometry2d.Orient`](../Physics/Spatial/Geometry2d.cs) returns `CounterClockwise`, `Clockwise`, or `Collinear`. It uses a conservative floating-point filter and exact integer arithmetic over the supplied binary64 coordinates when the filter cannot safely determine the sign. Focused cases live in [Geometry2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Geometry2dTests.cs).

## Deliberate limits

- Components have no built-in unit or coordinate-frame identity.
- Equality is exact component equality, not tolerance-based geometric equality.
- The type rejects non-finite inputs and arithmetic results that overflow to non-finite values.
- It does not normalize vectors, infer tolerances, classify segment intersections, or decide whether measured coordinates are physically accurate. `Geometry2d.Orient` classifies the exact represented binary64 values; it is not a measurement-uncertainty policy. The exact fallback prioritizes correctness over throughput and is not a GPU kernel.
- Squared lengths can overflow even when components are finite; callers needing extreme dynamic range require scaled norms.

## Next geometry slices

1. Define coordinate-frame and tolerance policies separately from exact predicates; measurements may need uncertainty-aware decisions.
2. Add segment/line intersection results with explicit parallel, collinear, endpoint-touching, and degenerate cases, built on `Geometry2d.Orient`.
3. Reuse the same semantics in polygon tests, 2D collision, and living visual explanations.
4. Add batch/SoA equivalents only where they share scalar reference fixtures and declare whether they preserve exact or approximate predicate semantics.

This primitive is the beginning of a reusable geometry vocabulary, not evidence that a general geometry engine exists.
