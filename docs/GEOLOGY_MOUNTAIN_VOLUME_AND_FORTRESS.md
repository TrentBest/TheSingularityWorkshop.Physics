# Mountain volumes and the Dwarven Fortress: terrain-to-world contract

**Status:** cross-domain design brief. The current Physics terrain implementation is a 2.5D height field; this document defines the next capability required for a mountain that can contain a genuinely explorable underground fortress. It does not claim that a 3D terrain-volume solver already exists.

## Why a height field is not enough

The existing `TerrainHeightField2D` is useful for exterior relief, drainage catchments, slopes, ridgelines, and mountain silhouettes. It stores one elevation for each horizontal sample. By construction it cannot represent:

- caves and chambers beneath the surface;
- overhangs, arches, cliffs, and vertical faces;
- multiple disconnected rock bodies or internal voids;
- a tunnel that passes beneath another tunnel;
- a fortress hall with open air above its floor but solid mountain around it.

We should preserve the height field as an efficient exterior terrain representation. We must not stretch it into a cave/fortress volume by inventing underground elevations.

## Ownership

| Concern | Owner |
|---|---|
| Exterior terrain elevation, slopes, drainage and physical terrain processes | Physics |
| 3D geological/solid volume representation and validated physical changes to it | Physics |
| Material identity and geology-specific physical properties | Physics, with domain-specific material models |
| Underground regions, chambers, passages, boundaries, excavations, and environmental topology | FSM_Dungeon |
| Dwarven identity, capabilities, roles, needs, motivations, craft, culture, organizations and behavior | SURPS |
| Excavation and construction history as world events and authored intent | FSM_Dungeon records the underground history; Physics evaluates physical consequences |
| Rendering fidelity, camera, pixels, lighting presentation | Renderer / Visualization |
| Capability composition and authoritative state transitions | Host / FSM_COS / FSM_Foundry |

The same generated mountain should remain usable without SURPS. A fortress is one composition placed into an environment, not a new mountain physics mode.

## Proposed physical representation

The first useful 3D capability should be a bounded, deterministic solid-volume field with explicit spatial conventions. Candidate representations include a signed-distance field, a regular voxel occupancy/material field, or a hybrid. Choose based on measured memory, sampling, update, and boundary costs—not on rendering convenience.

The contract must make these semantics explicit:

- coordinate frame and handedness;
- origin, axis directions, and linear units;
- cell dimensions and sample convention (cell-centred or node-centred);
- volume bounds and out-of-bounds behavior;
- sign/occupancy convention for solid rock, air, and unknown;
- material identity per solid sample or region;
- geological layer/formation provenance;
- resolution and uncertainty;
- deterministic seed and generator/model version;
- immutable snapshots or explicit transactional updates;
- validation and conservation evidence for material removal/addition.

A signed-distance field can represent smooth boundaries and query distance to rock, but it is not by itself a complete material inventory or fracture solver. A voxel material field can preserve categorical material and occupancy but may expose resolution artifacts. A hybrid can be justified later. The first increment should choose one authoritative representation and provide adapters only where they preserve semantics.

## Mountain generation: staged construction

### Stage 1 — exterior envelope

Use the existing height-field capability to generate and inspect a mountain silhouette. A first authored profile can combine a broad massif, ridges, saddles, valleys, and a declared base plane. Keep deterministic seed, dimensions, cell spacing, coordinate frame, vertical datum, and source provenance in a run record.

This stage can provide a real exterior mountain surface, but not an internal mountain volume.

### Stage 2 — solid volume

Generate a bounded 3D solid beneath the exterior envelope. Start with a deliberately simple, deterministic geological mass and a small number of declared material regions. Validate:

- every interior sample lies below the exterior surface where the mountain is intended to be solid;
- samples outside the envelope are air;
- the base boundary is explicit;
- material-region labels are stable and queryable;
- repeated generation with identical inputs is identical;
- the volume and sample counts are bounded and reported.

Do not imply tectonic realism. The first volume can be a controllable geometric mountain mass with named material zones.

### Stage 3 — geology and physical constraints

Add geological structure only when each process has a defined model and tests: strata, faults, joints, fractures, weathering, water-bearing zones, geothermal gradients, and material variation. Physical stability is not inferred from a rock label or a shader. Strength, stress, support, fracture, and collapse require their own declared physical models and evidence.

### Stage 4 — excavation as a physical change

An excavation proposal identifies a region to remove, its volume, material inventory, method, and intended geometry. A successful operation must produce a new volume snapshot and a material-removal ledger. Invalid or unsupported operations leave the source state unchanged.

Do not automatically call every tunnel physically safe. The first stage may perform geometric collision/clearance and material accounting only. Stress redistribution, support loads, rock bursts, progressive failure, and collapse are separate later capabilities.

### Stage 5 — environmental coupling

Once the mountain and void geometry are authoritative, compose water, heat, air, and other environmental models where supported. A water-bearing fracture or underground stream should be located from world state and boundary conditions, not painted into the renderer.

## The Dwarven Fortress as a proving scenario

The mountain fortress is the first rich consumer, not a special-case terrain generator. It should force the interfaces to answer practical questions:

- Where is the mountain's exterior and how thick is the overburden above each hall?
- Which geological material is present at the proposed excavation?
- What is the excavated volume and where does spoil go?
- Which chambers and passages are connected in 3D?
- Does a shaft intersect a hall, aquifer, fault, or another passage?
- Which routes are accessible to a particular entity, given clearance, slope, vertical transitions, hazards, and equipment?
- Where can water accumulate and what downstream regions can it reach?
- Which walls, pillars, arches, and supports are constructed, and what physical capabilities are available to assess them?

Dwarven engineering, architecture, and settlement logic belong to the scenario and SURPS content. The Physics model must not encode "dwarven" as a physical material or solver.

## Validation gates

1. **Geometry:** known analytic shapes (sphere, ellipsoid, capped ridge) produce expected inside/outside and boundary queries.
2. **Determinism:** fixed definition and seed produce identical serialized volume samples and provenance.
3. **Bounds:** memory and query cost are measured for declared dimensions/resolution; oversized requests fail safely.
4. **Material accounting:** excavation reports removed volume by material within the declared discretization tolerance.
5. **Transactional behavior:** invalid excavation never partially mutates the source snapshot.
6. **Cross-section agreement:** sampled surface of the 3D volume agrees with its declared exterior height field within the stated tolerance.
7. **Dungeon handoff:** Dungeon can consume a stable mountain-volume snapshot without referencing Renderer or SURPS.
8. **Evidence:** generator, versioned run record, tests, and a Pages inspector all refer to the same canonical output.

## Pages experience

The Physics Pages terrain experience already inspects a generated height field and its samples. Extend the learning path rather than replacing it:

1. **Mountain surface:** orbit/rotate a generated terrain surface; inspect elevation, slope, and coordinate sample.
2. **Cutaway:** reveal a vertical section through the mountain and inspect solid/material regions.
3. **Excavation preview:** select a proposed chamber or tunnel and show removed volume/material totals; do not label it structurally safe.
4. **Fortress handoff:** open the same volume in the Dungeon experience, where chambers, passages, gates, water, and constructed spaces are overlaid as domain topology.
5. **Dwarven life:** inspect settlement spaces and inhabitants through SURPS, with role and behavior claims backed by the relevant rulebooks.

The browser should inspect canonical generated data or a recorded model result. It must not become an independent terrain generator. A visualization of a target stage must be labeled as a mock-up until it is backed by the owning Physics/Dungeon output.

## Current evidence and limits

- Implemented: `TerrainHeightField2D`, deterministic plane/ramp/bowl generation, bilinear sampling, surface normals, smooth radial edits, and a generated 17 × 17 Pages run record.
- Implemented separately: volume-accounted idealized volcanic deposition over a 2D height field.
- Implemented on `development`: `MountainSolidVolume3d.Generate` extrudes a declared height-field exterior down to an explicit base using a bounded sparse material-voxel volume. Horizontal voxel centres align with height-field samples; vertical occupancy is decided at voxel centres; the maximum top-boundary quantization error is half a voxel. It emits the existing versioned spatial-volume envelope with generator parameters and provenance.
- The first mountain generator is deterministic geometric mass with one caller-supplied material ID. It does not implement geological stratigraphy, cave carving, excavation ledgers, material property models, rock mechanics, structural safety, or an explorable mountain fortress. The height field remains the exterior authority; the 3D solid is the occupancy/material representation beneath it.

References:
- [Terrain height-field implementation](../Physics/Geology/TerrainHeightField2D.cs)
- [Terrain run record](experiments/terrain-height-field.run.json)
- [Volcanic terrain capability and limits](GEOLOGY_VOLCANIC_TERRAIN.md)
- [Physics Pages terrain experience](PHYSICS_PAGES_EXPERIENCE.md)
- [Terrain interaction architecture](TERRAIN_INTERACTIONS_AND_MICROBUNDLES.md)
- [FSM_Dungeon living underground worlds](https://github.com/TrentBest/FSM_Dungeon/blob/development/docs/WORLD_SIMULATION.md)
- [SURPS Dungeons and Underground sourcebook](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/books/dungeons-and-underground.md)
