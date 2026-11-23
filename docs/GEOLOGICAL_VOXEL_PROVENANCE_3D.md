# Geological voxel provenance (3D)

## Purpose

`GeologicalVoxelProvenance3d` binds stratigraphic layer and formation-event identity to every occupied voxel in one immutable `SparseMaterialVoxelVolumeEnvelope3d` revision. It closes the gap between the existing material-only voxel storage and the geological requirement that subsurface material retain the layer and event from which it originated.

## Reuse existing spatial storage

The mapping deliberately does not introduce another voxel grid or duplicate occupancy/material arrays. The existing envelope remains authoritative for:

- voxel coordinates, origin, voxel size, and chunk convention;
- occupied/empty state and per-voxel material IDs;
- dataset, revision, coordinate frame, linear unit, axis convention, vertical datum, and positive vertical direction;
- material-catalogue identity and generation provenance.

The geological mapping adds only per-occupied-voxel `LayerId` and `FormationEventId` annotations, while retaining the voxel coordinate and matching material ID.

## Validation contract

Construction requires a complete one-to-one mapping:

1. The annotation count equals the snapshot's occupied voxel count.
2. Every voxel coordinate appears exactly once.
3. Every annotation addresses an occupied voxel in the bound snapshot.
4. The annotation material ID exactly matches the snapshot material ID.
5. Layer and formation-event identities are non-empty.
6. The resulting samples are immutable to callers and ordered lexicographically by voxel X, then Y, then Z.

The mapping retains the exact envelope object, thereby binding it to its dataset and revision identities. It does not perform coordinate conversion, material-catalogue lookup, stratigraphic reconstruction, or event validation against an external event ledger.

## Explicit limits

This is provenance annotation, not a continuous geological volume solver. It does not infer surfaces between samples, assign voxels to layers automatically, determine whether a voxel's geometry is physically consistent with a layer, model folding or fault offsets, or simulate deposition/erosion. A producer must derive and validate the annotations from its own authoritative geological state before constructing the mapping.

The first implementation is an immutable in-memory mapping. Persistence, serialization schema, transactional co-commit with voxel edits, and efficient chunk-aligned annotation storage remain future work. The source envelope and geological mapping must be versioned together by their shared dataset/revision identity; consumers must reject stale or mismatched revisions rather than silently reusing annotations.

## API

- `GeologicalVoxelProvenanceSample3d`: one voxel, layer identity, material ID, and formation-event identity.
- `GeologicalVoxelProvenance3d`: validates complete coverage against one immutable volume envelope and provides `TryGet` lookup by voxel coordinate.

Focused tests cover complete coverage, duplicate/missing annotations, empty voxels, material mismatch, identity validation, deterministic ordering, and defensive copying.
