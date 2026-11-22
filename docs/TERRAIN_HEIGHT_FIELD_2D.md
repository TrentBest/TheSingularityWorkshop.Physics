# Terrain height field 2D (first authoring slice)

## Scope

`TerrainHeightField2D` is an immutable, cell-centred elevation snapshot for bounded regions that can be represented by one elevation per horizontal sample. It is the first general-purpose authoring/query contract alongside the existing `VolcanicTerrain2D` deposition model; it does not replace that model or claim to be a complete terrain workbench.

- Samples are row-major, with X varying fastest.
- The world-coordinate origin is the centre of sample (0,0).
- X/Y spacing is explicit and positive; horizontal and vertical values share the caller-declared linear unit.
- Coordinate-frame identity, vertical datum, and positive vertical direction are required caller inputs. No implicit transform, datum conversion, or unit conversion occurs.
- Elevations are finite doubles. Snapshots are defensive; brush edits return a new field.
- Bilinear elevation sampling is available only inside the sample bounds. Outside queries return false rather than silently clamping or extrapolating.
- Surface normals use finite differences: centred differences in the interior and one-sided differences at boundaries. The result is an upward-facing Physics `Vector3d`.

## Implemented authoring operations

- Plane: `baseElevation + slopeX * localX + slopeY * localY`, where local coordinates are measured from sample (0,0).
- Ramp: a linear X-axis transition between declared start and end elevations.
- Bowl: a bounded parabolic depression with declared centre, radius, rim elevation, and depth.
- Radial brush: deterministic smoothstep-weighted additive or subtractive elevation change. Positive delta raises; negative delta lowers. The source snapshot is unchanged if an invalid/non-finite candidate is encountered.

These are geometric authoring operations, not claims about geological formation. In particular, a bowl is not an erosion simulation, and a ramp is not a tectonic process.

## Deliberate limits

This is a 2.5D height field. It cannot encode caves, tunnels, overhangs, vertical faces, or multiple surface layers; those require the existing sparse volumetric representation and a future explicit height-field/volume bridge. The first brush is a radial elevation edit, not a material-aware tool, transaction log, undo system, chunked editor, or GPU implementation. Stable operation IDs, provenance history, dirty-region reports, material fields, chunk seams, and deterministic seeded regional generators remain future increments.

The existing `VolcanicTerrain2D` remains the current volume-accounted prescribed-deposition model. It is not silently coupled to this new authoring field, and the fixed-bed shallow-water solver remains unchanged.

## Validation

Focused tests cover explicit origin and spacing, analytical plane sampling, ramp endpoints, bowl centre/rim, deterministic and localized brush edits, immutable source state, invalid sample arrays, and atomic failure on overflow. These tests validate this bounded API; they do not establish geological realism or large-world scalability.
