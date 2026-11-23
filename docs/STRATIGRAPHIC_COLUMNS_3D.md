# Stratigraphic columns (3D)

## Status

**Implemented:** immutable stratigraphic layer and vertical-column contracts with material identity, vertical bounds, formation-age intervals, and event provenance.

**Not implemented:** material-property catalogue, lateral layer correlation, depositional or erosional process solver, folding, fault displacement of strata, intrusions, compaction, porosity, or a complete 3D stratigraphic volume.

## Contract

- `StratigraphicLayer3d` identifies a positive caller-defined material ID, finite top and bottom elevations, an older-to-younger formation-age interval in years before present, and the event ID that formed or deposited the layer.
- `StratigraphicColumn3d` identifies one horizontal sample location and explicit coordinate frame, vertical datum, linear unit, and positive vertical direction.
- Layers are supplied from highest to lowest elevation. They may touch but cannot overlap.
- An upper layer cannot be older than the layer beneath it. Age gaps are allowed and represent unresolved hiatuses.
- Layer identities are unique within a column. Input collections are defensively copied and exposed read-only.

## Deliberate limits

A material ID is a catalogue key, not density, porosity, strength, thermal properties, or composition. The age and event fields preserve provenance but do not prove the event's physical validity. The column is a local vertical stack sampled at (WorldX, WorldY); it does not imply lateral continuity or interpolate neighboring columns.

The current fault model prescribes block displacement in 3D but does not deform these columns. The next integration must record cross-cutting relationships and apply a declared displacement to layer geometry without losing original layer identity or formation provenance. It must not imply stress, rupture, or plate dynamics that are not implemented.

## Tests

`tests/TheSingularityWorkshop.Physics.Tests/Geology/StratigraphicColumn3dTests.cs` covers layer identity/material/provenance, finite positive thickness, geological age ordering, stack order, non-overlap, allowed hiatuses, duplicate IDs, and invalid coordinates.
