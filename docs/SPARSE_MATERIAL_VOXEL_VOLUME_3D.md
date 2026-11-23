# Sparse material voxel volume snapshots (3D)

## Status

**Implemented:** immutable, deterministic read snapshots of the current sparse material voxel storage.

**Not yet implemented by this contract:** world/region identity, coordinate-frame and axis metadata, physical length units, vertical datum, georeferencing, material definitions/properties, durable revision identity, generator provenance, edit journals, density/fractional occupancy, and cross-chunk streaming.

## Source

- `Spatial/SparseMaterialVoxelVolume3d.cs` — mutable sparse chunked material storage and `CaptureSnapshot()`.
- `Spatial/SparseMaterialVoxelVolumeSnapshot3d.cs` — immutable snapshot and occupied-voxel sample.
- `tests/TheSingularityWorkshop.Physics.Tests/Spatial/SparseMaterialVoxelVolume3dTests.cs` — ordering, query, geometry, immutability, and source-isolation tests.

## Contract

A snapshot captures:

- the source grid origin (minimum corner of voxel `(0,0,0)`);
- cubic voxel edge length;
- chunk edge length;
- each occupied integer voxel coordinate and its positive caller-defined material ID.

Material ID zero remains reserved for empty space. Samples are sorted by voxel X, then Y, then Z, independent of dictionary/chunk visitation order. The snapshot owns a defensive copy and exposes a read-only collection. Later edits or clearing of the mutable source volume do not alter an existing snapshot.

The snapshot provides deterministic material lookup and voxel-center conversion using the captured grid geometry. It deliberately does not infer units or frame semantics from the numeric coordinates. A producer must supply those semantics in a higher-level terrain/world contract before a consumer can safely interpret the values as metres, georeferenced coordinates, elevation, or a particular axis convention.

## Numerical and representation limits

The source remains a discrete, cubic, integer-indexed voxel volume. Material IDs are caller-defined identities, not material properties. The snapshot is an in-memory read view, not a serialized interchange format, durable checkpoint, transaction log, or claim of a globally unique revision. It does not smooth the block-face surface or add fractional occupancy.

## Next contract layer

The next terrain integration increment should add explicit producer-owned spatial metadata and stable world/region identity around this storage snapshot, then provide a reproducible analytically checkable fixture. Height fields should be exposed as specialized/derived terrain representations where appropriate; they do not replace volumetric terrain for caves, overhangs, tunnels, and underground spaces.
