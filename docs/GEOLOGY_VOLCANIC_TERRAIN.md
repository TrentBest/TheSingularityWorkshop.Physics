# Geology: volcanic terrain source (first increment)

This increment introduces a small, renderer-independent terrain primitive that can change a world through a physically accountable event: volcanic material is added to a cell-centered elevation field. It is designed as a first geology capability and as a future input to the hydrodynamics terrain/shoreline work.

## Implemented model

- A row-major 2D terrain height field with metre-based elevation and horizontal cell dimensions.
- A volcanic deposition event defined by vent cell, footprint radius, and erupted volume.
- A discrete cone-shaped deposition profile that tapers linearly to zero at the radius.
- Per-cell deposition weights are normalized over the actual grid footprint, so the sum of elevation change multiplied by cell area equals the requested deposited volume, within floating-point tolerance.
- Candidate terrain is computed before committing; invalid requests and non-finite candidate state leave the terrain unchanged.
- Repeated events accumulate and are deterministic for the same inputs.

## Physical interpretation and limits

The deposited volume is a prescribed input, not a prediction of eruption magnitude. The cone is an idealized geometry kernel, not a claim that real lava always forms a cone. This primitive does not yet model magma pressure, melt fraction, temperature, viscosity, vent dynamics, lava transport, cooling/solidification, ash, tectonic plate motion, crustal deformation, or erosion. It does not remove material from the terrain.

The terrain field is separate from rendering. A renderer can visualize the actual elevation samples, but cannot create a volcano that the physical state does not contain.

## Next coupling steps

1. Define terrain and bed elevation as explicit hydrodynamics inputs rather than querying renderer meshes.
2. Add still-water equilibrium over non-flat bed geometry, then wet/dry shoreline policy and flow/volume tests.
3. Introduce a declared erosion law that transfers sediment between terrain and water or a sediment ledger; test solid-volume accounting and sensitivity to flow.
4. Add more geology primitives only as bounded models: deposition, slope failure, tectonic uplift/subsidence, and eventually magma/thermal coupling.

## Terrain semantics: physics first, expert-domain recognition second

The physical solver should produce measurable fields such as elevation, slope, material, temperature, stress, and erosion/deposition history. A terrain-feature recognizer then interprets those fields under an explicit expert-domain definition: for example, a cliff candidate may require a sustained steep slope, a minimum relief, and a minimum spatial extent. Such a recognizer is a semantic filter over physical evidence—not a replacement for the physical process and not an arbitrary visual label. Thresholds and uncertainty should be declared and testable.

A feature label should retain its evidence and confidence. “Cliff” is not a primitive force or a magic terrain brush; it is a domain interpretation of generated terrain.

## Validation covered

- Positive finite eruption inputs and valid vent coordinates.
- Deterministic, localized terrain changes.
- Discrete deposited-volume accounting over rectangular cells.
- Accumulation across repeated eruptions.
- Defensive snapshots and unchanged state after invalid requests.
