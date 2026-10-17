# Geometry Foundation: 2D Vectors

**Status:** a small implemented mathematical primitive; not a complete geometry or collision API.

## Contract

[`Vector2d`](../Physics/Spatial/Vector2d.cs) represents two finite, unit-agnostic components. It provides addition, subtraction, scalar scaling/division, squared length, length, dot product, signed 2D cross product, and squared distance. Focused tests live in [Vector2dTests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Vector2dTests.cs).

The cross product returns the scalar z-component (x_1 y_2-y_1 x_2). Its sign indicates orientation: positive is counter-clockwise, negative clockwise, and zero collinear for the represented floating-point values. A zero result alone is not a robust near-collinearity decision.

## Deliberate limits

- Components have no built-in unit or coordinate-frame identity.
- Equality is exact component equality, not tolerance-based geometric equality.
- The type rejects non-finite inputs and arithmetic results that overflow to non-finite values.
- It does not normalize vectors, infer tolerances, classify segment intersections, or promise numerically robust orientation for extreme-scale or nearly-collinear inputs.
- Squared lengths can overflow even when components are finite; callers needing extreme dynamic range require scaled norms.

## Next geometry slices

1. Define coordinate-frame and tolerance policies before adding geometric predicates.
2. Add robust orientation and segment/line intersection results with explicit degenerate cases.
3. Reuse the same semantics in polygon tests, 2D collision, and visual explanations.
4. Add batch/SoA equivalents only where they share scalar reference fixtures.

This primitive is the beginning of a reusable geometry vocabulary, not evidence that a general geometry engine exists.
