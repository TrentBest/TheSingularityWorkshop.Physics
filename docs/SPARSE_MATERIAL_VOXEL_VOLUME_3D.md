# Sparse material voxel volume snapshots (3D)

## Status

**Implemented:** immutable, deterministic read snapshots of sparse material voxel storage; a provenance-bearing spatial envelope with strict versioned JSON interchange; and a bounded analytical ellipsoid generator that produces a deterministic voxel-center approximation of a known continuous solid.

**Not yet implemented by this contract:** coordinate transforms, georeferencing, material definitions/properties, durable storage, edit journals, density/fractional occupancy, sparse streaming, and a City-side volumetric importer.

## Source

- `Spatial/SparseMaterialVoxelVolume3d.cs` — mutable sparse chunked material storage and `CaptureSnapshot()`.
- `Spatial/SparseMaterialVoxelVolumeSnapshot3d.cs` — immutable snapshot and occupied-voxel sample.
- `Spatial/SparseMaterialVoxelVolumeEnvelope3d.cs` — required spatial identity, unit, datum, material-catalog, and provenance metadata around a snapshot; strict versioned JSON serialization/deserialization.
- `Spatial/AnalyticalSolidVolume3d.cs` — bounded analytic ellipsoid generation using voxel-center inclusion and a configurable candidate-work limit.
- `docs/schemas/sparse-material-voxel-volume.v1.schema.json` — external JSON Schema for the v1 interchange payload.
- `docs/records/sparse-material-voxel-volume-example.json` — synthetic 2×2×2 interchange fixture; it validates serialization mechanics only.
- `docs/records/analytical-ellipsoid-volume-example.json` — versioned 4×4×4 analytic ellipsoid output with 32 occupied samples, generated from the Physics ellipsoid algorithm and loaded by a producer-side test. It is synthetic geometric evidence, not geological evidence.
- `tests/TheSingularityWorkshop.Physics.Tests/Spatial/SparseMaterialVoxelVolume3dTests.cs` — ordering, query, geometry, immutability, and source-isolation tests.
- `tests/TheSingularityWorkshop.Physics.Tests/Spatial/SparseMaterialVoxelVolumeEnvelope3dTests.cs` — metadata validation and snapshot/provenance preservation.
- `tests/TheSingularityWorkshop.Physics.Tests/Spatial/AnalyticalSolidVolume3dTests.cs` — analytic inside/outside, determinism, bounds, and fixture-to-generator equivalence tests.

## Contract

A snapshot captures:

- the source grid origin (minimum corner of voxel `(0,0,0)`);
- cubic voxel edge length;
- chunk edge length;
- each occupied integer voxel coordinate and its positive caller-defined material ID.

Material ID zero remains reserved for empty space. Samples are sorted by voxel X, then Y, then Z, independent of dictionary/chunk visitation order. The snapshot owns a defensive copy and exposes a read-only collection. Later edits or clearing of the mutable source volume do not alter an existing snapshot.

The snapshot provides deterministic material lookup and voxel-center conversion using the captured grid geometry. It deliberately does not infer units or frame semantics from the numeric coordinates. A producer must supply those semantics in `SparseMaterialVoxelVolumeEnvelope3d` before a consumer can safely interpret the values as metres, georeferenced coordinates, elevation, or a particular axis convention. The envelope requires stable region, dataset, and revision IDs; frame ID; linear unit; X/Y/Z axis mapping; vertical datum and positive direction; material-catalog ID; source repository/revision; model and algorithm identities/versions; generation-parameter JSON; and source run ID. An optional deterministic seed is retained when applicable. The grid convention is fixed as a minimum-corner origin, cubic voxels, and integer X/Y/Z indices.

## Analytical reference geometry

`AnalyticalSolidVolume3d.CreateEllipsoid` evaluates each voxel center in a finite candidate grid against:

```
((x - cx) / rx)^2 + ((y - cy) / ry)^2 + ((z - cz) / rz)^2 <= 1
```

where `(cx, cy, cz)` is the world-space center and `(rx, ry, rz)` are positive semi-axis lengths. The result is a discrete occupancy approximation whose boundary depends on voxel size and center-inclusion convention. Candidate work is bounded before allocation. The generator does not assign geological meaning, infer material properties, or claim that a synthetic ellipsoid is a mountain.

The published 4×4×4 fixture uses a 2 m semi-axis in each direction, 1 m cubic voxels, and material ID 1. It contains 32 occupied voxel centers. Tests regenerate the fixture from its declared grid and analytic parameters and compare the exact ordered samples with the versioned interchange record.

## Numerical and representation limits

The source remains a discrete, cubic, integer-indexed voxel volume. Material IDs are caller-defined identities, not material properties. The snapshot and envelope support a versioned JSON interchange payload with schema ID `singularity-physics.sparse-material-voxel-volume.v1`. `Serialize()` emits the canonical payload; `Deserialize()` rejects unknown properties, unsupported schema/grid conventions, non-finite geometry, invalid material IDs, duplicate voxel coordinates, and payloads above its configurable occupied-sample limit (default five million). The companion JSON Schema is published at `docs/schemas/sparse-material-voxel-volume.v1.schema.json`. This is not a durable checkpoint or transaction log, and producer-supplied IDs are not asserted to be globally unique. Consumers must preserve source samples and metadata; any transform, resampling, surface extraction, or mesh is a separately identified derived product. It does not smooth the block-face surface or add fractional occupancy.

## Next contract layer

The next terrain integration increment is to generate an analytic mountain-shaped volume whose sampled exterior agrees with a declared Physics height-field profile, then provide a read-only City consumer adapter. The ellipsoid fixture proves a deterministic 3D analytic solid and versioned handoff record; it does not yet prove terrain elevation queries, geological realism, surface extraction, material properties, or mountain/height-field agreement. Height fields remain specialized/derived terrain representations; they do not replace volumetric terrain for caves, overhangs, tunnels, and underground spaces.
