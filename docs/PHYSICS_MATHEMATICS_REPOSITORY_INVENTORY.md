# Physics and Mathematics Repository Inventory

**Snapshot:** 2026-11-22 UTC  
**Branch inspected:** development  
**Purpose:** establish a factual starting point for [the Physics + Mathematics MicroBundle program](PHYSICS_AND_MATHEMATICS_MICROBUNDLE_ARCHITECTURE.md) tracked by [issue #56](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/56).

This is a structural inventory, not a scientific audit. File presence does not establish correctness, completeness, or validated capability. Counts are a snapshot of the recursive Git tree at the time of inspection and will change as work proceeds.

## Repository structure snapshot

The recursive tree contains:

| Area | Observed inventory |
|---|---:|
| Files beneath Physics/ | 116 |
| Files beneath Physics/Math/ | 10 |
| Files beneath Physics/Spatial/ | 13 |
| Files beneath tests/ | 114 |
| Markdown files beneath docs/ | 105 |
| Files beneath docs/experiments/ | 41 |
| GitHub workflow files | 5 |
| Project/solution files | 25 |

The source tree includes these Physics namespaces:

- Acoustics
- Aerodynamics
- Astronomy
- Chemistry
- Collision
- Combustion
- Dynamics
- Electricity
- Experiments
- Geology
- Hydrodynamics
- Kinematics
- Materials
- Math
- Mechanics
- MicroBundles
- Spatial
- Thermodynamics
- Timing
- Universe
- World

There is also a root Spatial namespace with shared spatial primitives. Its ownership and dependency relationship to Physics/Spatial and Physics/Math should be explicitly documented in the architecture and project boundaries.

## Projects and execution infrastructure

Observed project groups include:

- Physics core project and solution.
- Separate Physics MicroBundles adapter project.
- Core test project.
- MicroBundle adapter test project.
- Test-only Gestures integration project.
- Benchmark project.
- Two procedural geometry sample/export projects.
- Multiple focused run-record generator projects under tools/.
- Five GitHub workflows: build, benchmark, Gestures contract, Physics Pages, and Sith policy.

The adapter project currently references the Physics core and MicroBundleDomain. The core scientific model must remain independent of host composition. The adapter assembly is the appropriate boundary for host-facing MicroBundle interfaces.

## Current MicroBundle adapter inventory

Three adapter implementations are present in the current source:

| Bundle ID | Provider ID | Capability | Manifest |
|---:|---|---|---|
| 5201 | physics.contact.sphere-sphere.v1 | Sphere-sphere narrowphase contact evidence | [Manifest](../microbundles/SphereSphereContact/microbundle.json) |
| 5202 | physics.contact.normal-impulse.v1 | Frictionless translational normal-impulse response | [Manifest](../microbundles/NormalImpulseResponse/microbundle.json) |
| 5203 | physics.kinematics.prescribed-motion.v1 | Prescribed constant-velocity and quintic smooth-step kinematics, analytical ceiling validation, and achieved-state commit validation | [Manifest](../microbundles/PrescribedKinematicMotion/microbundle.json) |

The manifest for bundle 5203 was missing from the manifest tree even though its adapter and README entry existed. It has now been added. The MicroBundles README now links all three manifests.

The three manifests have since been expanded under the versioned Physics MicroBundle manifest schema. They now declare ontology placement, capability inputs/outputs, model assumptions and exclusions, units/frame/time semantics where known, execution responsibilities, evidence status, and educational references. The schema and contract are documented in [the manifest contract](PHYSICS_MICROBUNDLE_MANIFEST_CONTRACT.md). CI validates schema correctness, unique bundle/provider IDs, capability-provider inclusion, referenced-file existence, and parity between each manifest and its matching runtime adapter: bundle ID, descriptor version, and provider-ID set. The source-to-adapter check currently uses a constrained C# regex rather than a full parser. This remains descriptive metadata: it does not prove scientific validity or that a runtime provider was loaded.

The current adapters do not constitute a general physics simulation loop. Existing documentation correctly distinguishes contact evidence, a supplied-contact impulse response, and prescribed motion from a full collision/rigid-body/world solver.

## Existing mathematical foundations

The source tree already contains mathematical implementations in:

- Physics/Math: derivative estimates and derivation records, quadrature, online statistics, matrix operations, batch vector operations, deterministic Squirrel3 random streams, keyed sampling, lookup tables, and Galilean frame transforms.
- Physics/Spatial: 2D vectors, coordinate frames, points, rigid transforms, geometric predicates, segment intersections, polygon operations, seeded point sets, and bounded Mandelbrot escape-time generation.
- Root Spatial: 3D vectors, poses, quaternions, bounds, and sparse voxel volume.

The Mathematics Domain Map describes a much wider target than these current implementations. The first source-to-capability audit is recorded in [the coverage matrix](PHYSICS_MATHEMATICS_SOURCE_CAPABILITY_MATRIX.md). It identifies candidate capability families and the current evidence chain without equating file presence with validated coverage.

## Existing physical-domain foundations

The current Physics namespaces already span mechanics/kinematics, collision, rigid-body dynamics, gravity/orbital mechanics, materials, thermodynamics/heat transfer, hydrodynamics, aerodynamics, acoustics, electricity, chemistry, combustion, geology, timing, and universe/timeline concepts.

This breadth is a starting inventory only. Each domain requires a capability-by-capability audit:
- What physical question is supported?
- Which model and equations are actually implemented?
- What assumptions, boundary conditions, and valid ranges apply?
- Which outputs are observations versus derived diagnostics?
- What tests and independent analytical/reference checks exist?
- What theory chapter explains and derives the model?
- Is there a typed MicroBundle provider and manifest?
- Is the capability composable without leaking UI/host dependencies?

## Documentation and learning assets

The repository already contains substantial domain maps, architecture documents, theory/evidence standards, numerical validation guidance, run-record contracts, living SVGs, experiment descriptions, JSON run records, and focused calculus chapters.

The next documentation audit should classify each resource as one of:
- textbook theory chapter;
- capability/API reference;
- architecture/contract;
- experiment guide;
- generated evidence/run record;
- conceptual diagram;
- implementation roadmap.

A README or experiment description is not automatically a textbook chapter. A run record is evidence for a particular run, not proof that the model is universally valid.

## Immediate gaps and next actions

1. **Manifest schema:** design a versioned, machine-validated schema and compatibility rules before multiplying manifests.
2. **Identity and ontology:** map bundle IDs, provider IDs, ontology concepts, model IDs, experiment IDs, and chapter IDs without conflating them.
3. **Complete capability matrix:** source → contract → adapter → manifest → tests → theory → experiments → validation evidence.
4. **Project boundary audit:** clarify root Spatial versus Physics/Spatial and the intended ownership of shared mathematical primitives. The first audit found both folders currently contribute to the same `TheSingularityWorkshop.Physics.Spatial` namespace; no files have been moved.
5. **Mathematics gap audit:** map each Mathematics subdomain and numerical method to implemented source, tests, error bounds, and theory.
6. **Physics gap audit:** do the same for every physical subdomain, including supported scales and coupling limits.
7. **Textbook adoption system:** establish chapter metadata, citation/provenance, instructor notes, exercises, accessibility, and generated index.
8. **Laboratory composition:** define an experiment manifest that selects bundle versions and binds controls/observations to the same model run.
9. **Validation gates:** automate consistency checks so a bundle cannot be marked implemented merely because a manifest or document exists.

## Current status

The architecture chapter and umbrella issue are established. The three existing adapters have corresponding expanded manifests, a versioned schema, and a CI validation gate. The first source-to-capability matrix now records the three current vertical slices, candidate Mathematics/Mechanics/Spatial families, and the unresolved shared Spatial ownership question. Full domain coverage, textbook metadata, and the ontology-driven laboratory catalog remain future work. The latest verified Physics workflow passed the expanded manifest-to-adapter identity checks and the complete experiment/build/test/package pipeline.

No master promotion, NuGet publication, Pages deployment, or branch deletion was performed.
