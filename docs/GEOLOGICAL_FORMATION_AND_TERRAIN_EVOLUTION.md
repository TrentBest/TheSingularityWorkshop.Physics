[object Object]

## Atomic geological voxel-volume revisions

`GeologicalVoxelVolumeRevision3d` groups one immutable `SparseMaterialVoxelVolumeEnvelope3d` and its complete `GeologicalVoxelProvenance3d` map. Construction requires the provenance map to retain the exact same envelope object, preventing a mapping from an earlier snapshot being paired with a later voxel state. `GeologicalVoxelVolumeCandidate3d.Create` checks the event's source/result revision identities, requires a new result identity, preserves dataset and spatial-grid conventions, and requires complete provenance for the result envelope. Occupancy and material IDs may change only when the caller supplies a matching complete provenance map.

`GeologicalVoxelVolumeRevisionStore3d` commits the envelope and mapping together as one immutable revision under a lock. It checks exact source-revision identity, expected source ID, event source ID, and revision-ID reuse; competing stale candidates are rejected without changing the current revision. This closes the in-memory atomicity gap between voxel data and per-voxel geological provenance. It does not provide durable persistence, distributed transactions, multi-branch history, a process solver, or event replay. See [Geological voxel-volume revision transactions (3D)](GEOLOGICAL_VOXEL_VOLUME_REVISION_TRANSACTIONS_3D.md) and focused tests.
