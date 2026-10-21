# SeededPointSet2d — source companion

**Purpose:** Generate a bounded, deterministic set of two-dimensional points for procedural geometry, experiments, and reproducible visual demonstrations.

**Responsibility:** `TheSingularityWorkshop.Physics.Spatial.SeededPointSet2d` maps each stable point index to independent X/Y samples using `Squirrel3.Hash`. It returns the generated `Vector2d` values in index order through a read-only collection.

**Implementation source:** [`Physics/Spatial/SeededPointSet2d.cs`](../../Physics/Spatial/SeededPointSet2d.cs)  
**Class diagram:** [`SeededPointSet2d.svg`](SeededPointSet2d.svg)  
**Executable recipe:** [Seeded point-set exporter](../../samples/SeededPointSet2dExport/README.md)  
**Tests:** [`SeededPointSet2dTests.cs`](../../tests/TheSingularityWorkshop.Physics.Tests/Spatial/SeededPointSet2dTests.cs)

## Class card

```text
«static» SeededPointSet2d
  + MaximumPointCount : int = 65,536
  + Generate(count, minX, minY, maxX, maxY, seed)
      : IReadOnlyList<Vector2d>

Uses: Squirrel3.Hash(index, seed XOR axis-domain)
Creates: Vector2d
```

## Contract and constraints

- `count` must be between zero and 65,536 inclusive.
- Each lower bound must be finite; each exclusive upper bound must be finite and strictly greater than its lower bound.
- The range difference must also be finite.
- Coordinates are mapped into the half-open rectangle `[minX, maxX) × [minY, maxY)`; rounding at the upper edge is corrected with `Math.BitDecrement`.
- Per-index random access avoids mutable random-stream call-order dependence. X and Y use distinct domain constants.
- Results are unit-agnostic and may contain duplicate points. This is not a physical distribution, collision-free packing algorithm, or solver.
- The returned collection is read-only to callers; generated values preserve stable index order.

## Use case — reproduce a point cloud

1. Choose a seed, point count, and finite half-open rectangle.
2. Call `SeededPointSet2d.Generate(...)` with those inputs.
3. Consume the returned points in their stable order or pass them to the [exporter recipe](../../samples/SeededPointSet2dExport/README.md).
4. Re-run with the same inputs to reproduce the same indexed coordinates; change the seed to produce a different deterministic set.

## Evidence boundary

The class diagram describes source structure. The unit tests exercise the public sampler contract, while the exporter recipe checks that serialized output round-trips to the in-memory coordinates. Neither artifact claims GPU execution or a collision-free spatial packing guarantee.
