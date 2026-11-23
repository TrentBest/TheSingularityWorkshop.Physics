# Stratigraphic column grids (3D)

## Status

**Implemented:** an immutable regular horizontal grid of vertical stratigraphic columns. The grid validates sample positions, spacing, row-major ordering, unique column identities, and shared coordinate/vertical conventions.

**Not implemented:** interpolation between columns, continuous layer surfaces, lateral correlation, folding, fault displacement, material-property resolution, process-derived deposition/erosion, or a continuous 3D stratigraphic volume.

## Contract

`StratigraphicColumnGrid3d` stores a rectangular sample lattice:

- `OriginX` and `OriginY` identify the horizontal position of column (0,0).
- `SpacingX` and `SpacingY` are finite positive distances in the declared linear unit.
- Column `(x,y)` must be located exactly at `(OriginX + x·SpacingX, OriginY + y·SpacingY)`.
- Columns are stored row-major with X varying fastest.
- Every column must share the grid's coordinate frame, vertical datum, linear unit, and positive vertical direction.
- Column IDs must be unique within the grid.
- The input collection is defensively copied; each column is already immutable.

The grid exposes integer-index lookup. It deliberately does not invent a nearest-column policy or interpolate a column at arbitrary world coordinates.

## Interpretation boundary

This is a **sampled 3D stratigraphic dataset**, not a continuous stratigraphic solid. Each sample remains a vertical stack with its own layer boundaries, material IDs, age intervals, and formation-event provenance. Between samples, layer continuity and geometry are unknown. A future continuous volume must explicitly define lateral correlation, surface reconstruction/interpolation, discontinuities, and fault offsets rather than silently assuming them.

The grid does not modify the existing sparse material voxel representation. That representation stores occupancy and material IDs but does not yet retain per-voxel stratigraphic layer identity or formation-event provenance. A conversion between the two must therefore be an explicit, versioned derivation with stated sampling and information-loss rules.

## Tests

`StratigraphicColumnGrid3dTests` covers row-major lookup, defensive copying, shape and coordinate validation, convention mismatch, duplicate identities, bounds, and invalid geometry.
