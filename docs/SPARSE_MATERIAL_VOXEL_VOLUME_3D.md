# Sparse material voxel volume snapshots (3D)

## Status

**Implemented:** immutable, deterministic read snapshots of sparse material voxel storage, plus a provenance-bearing spatial envelope that binds a snapshot to stable region/dataset/revision identities, coordinate-frame and axis declarations, linear units, vertical datum/direction, material-catalog identity, and source/model/algorithm/run provenance.

**Not yet implemented by this contract:** coordinate transforms, georeferencing, material definitions/properties, durable storage, edit journals, density/fractional occupancy, sparse streaming, and a City-side volumetric importer.

## Source

- `Spatial/SparseMaterialVoxelVolume3d.cs` — mutable sparse chunked material storage and `CaptureSnapshot()`.
- `Spatial/SparseMaterialVoxelVolumeSnapshot3d.cs` — immutable snapshot and occupied-voxel sample.
- `Spatial/SparseMaterialVoxelVolumeEnvelope3d.cs` — required spatial identity, unit, datum, material-catalog, and provenance metadata around a snapshot; strict versioned JSON serialization/deserialization.
- `docs/schemas/sparse-material-voxel-volume.v1.schema.json` — external JSON Schema for the v1 interchange payload.
- `tests/TheSingularityWorkshop.Physics.Tests/Spatial/SparseMaterialVoxelVolume3dTests.cs` — ordering, query, geometry, immutability, and source-isolation tests.
- `tests/TheSingularityWorkshop.Physics.Tests/Spatial/SparseMaterialVoxelVolumeEnvelope3dTests.cs` — metadata validation and snapshot/provenance preservation.

## Contract

A snapshot captures:

- the source grid origin (minimum corner of voxel `(0,0,0)`);
- cubic voxel edge length;
- chunk edge length;
- each occupied integer voxel coordinate and its positive caller-defined material ID.

Material ID zero remains reserved for empty space. Samples are sorted by voxel X, then Y, then Z, independent of dictionary/chunk visitation order. The snapshot owns a defensive copy and exposes a read-only collection. Later edits or clearing of the mutable source volume do not alter an existing snapshot.

The snapshot provides deterministic material lookup and voxel-center conversion using the captured grid geometry. It deliberately does not infer units or frame semantics from the numeric coordinates. A producer must supply those semantics in `SparseMaterialVoxelVolumeEnvelope3d` before a consumer can safely interpret the values as metres, georeferenced coordinates, elevation, or a particular axis convention. The envelope requires stable region, dataset, and revision IDs; frame ID; linear unit; X/Y/Z axis mapping; vertical datum and positive direction; material-catalog ID; source repository/revision; model and algorithm identities/versions; generation-parameter JSON; and source run ID. An optional deterministic seed is retained when applicable. The grid convention is fixed as a minimum-corner origin, cubic voxels, and integer X/Y/Z indices.

## Numerical and representation limits

The source remains a discrete, cubic, integer-indexed voxel volume. Material IDs are caller-defined identities, not material properties. The snapshot and envelope support a versioned JSON interchange payload with schema ID `singularity-physics.sparse-material-voxel-volume.v1`. `Serialize()` emits the canonical payload; `Deserialize()` rejects unknown properties, unsupported schema/grid conventions, non-finite geometry, invalid material IDs, duplicate voxel coordinates, and payloads above its configurable occupied-sample limit (default five million). The companion JSON Schema is published at `docs/schemas/sparse-material-voxel-volume.v1.schema.json`. This is not a durable checkpoint or transaction log, and producer-supplied IDs are not asserted to be globally unique. Consumers must preserve source samples and metadata; any transform, resampling, surface extraction, or mesh is a separately identified derived product. It does not smooth the block-face surface or add fractional occupancy.

## Next contract layer

The next terrain integration increment is a reproducible analytically checkable volumetric fixture and a read-only City consumer adapter. The importer must validate the versioned payload and must not invent missing transforms or material properties. Height fields should be exposed as specialized/derived terrain representations where appropriate; they do not replace volumetric terrain for caves, overhangs, tunnels, and underground spaces.
