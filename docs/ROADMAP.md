# Physics Roadmap

This roadmap separates architecture from completed functionality. Items move to “implemented” only with code, tests, and documented scope.

The scale and capacity contract is defined in [Physics Across Scales](PHYSICS_ACROSS_SCALES.md). It distinguishes local interaction, active scenes, regions, persistent worlds, and timeline-scale simulation, and sets evidence gates for capacity claims. It is a design contract, not an implementation checklist.

## Stage 0 — Establish the domain

- [x] Create a dedicated Physics repository.
- [x] Define core/host boundaries and deterministic execution principles.
- [x] Map physics capability classes to the Workshop ontology.
- [x] Establish theory, provenance, and living-documentation contracts.
- [x] Confirm initial target framework (net8.0) and repository/package identity.
- [x] Add project, solution, CI, and baseline tests.

## Stage 0.5 — Broad physical territory and shared primitives

- [x] Map the full planned domain space, its major couplings, first usable slices, guardrails, and implementation status ([Physics Domain Map](PHYSICS_DOMAIN_MAP.md)).
- [ ] Define reusable quantity/state conventions, model provenance, boundary conditions, conservation diagnostics, and observation contracts across domains.
- [ ] Establish a tested minimum vertical slice for thermodynamics, hydrodynamics, materials, reactions, fields, and waves before deepening only one domain.
- [ ] Use a bounded fire/combustion experiment as a cross-domain integration test after its prerequisite models exist; it is not a substitute for broader domain coverage.

The domain map is architecture, not implementation. A first bounded lumped thermal-state model is implemented on `development`; broader thermodynamics, heat transfer, hydrodynamics, material response, chemical reactions, and combustion still require their own models, tests, and scope.

## Stage 1 — First executable proof: mechanics

- [x] Implement an initial renderer-independent, SI-unit constant-acceleration kernel with input validation.
- [x] Define explicit logical timestep for the initial mechanics kernel.
- [x] Implement a bounded analytical case: one-dimensional constant acceleration.
- [x] Test analytical outcomes, repeatability, invalid inputs, and unit consistency.
- [x] Generate a versioned JSON run record from actual experiment samples, with explicit units and repeat-serialization tests.
- [x] Add an instructional free-fall experiment whose recorded states drive a living SVG lesson; CI checks the displayed state labels against actual C# output ([apple free-fall lesson](experiments/apple-free-fall.md)).
- [ ] Add reference-data provenance, uncertainty/residual reporting, and an independently checkable validation record.

## Stage 1.5 — Multi-rate physical execution

- [x] Add a bounded logical-time ledger that separates committed current tick from requested target tick and preserves catch-up debt ([logical time ledger](LOGICAL_TIME_LEDGER.md)).
- [ ] Define a logical simulation heartbeat distinct from render-frame timing.
- [ ] Add an FSM_API adapter with one scheduler FSM per capability and explicit domain processing groups.
- [ ] Define elapsed-time, catch-up, and event-triggered wake-up policies per model.
- [ ] Add separate background and local/high-detail work where relevance requires it.
- [ ] Test cadence boundaries, delayed events, coupled-domain ordering, and conservation across handoffs.
- [ ] Measure cost and physical error against a finer-step reference before selecting default rates.

The scheduling contract is documented in [Process-group scheduling](PROCESS_GROUP_SCHEDULING.md). The ledger accounts for requested versus committed ticks but does not execute physics or schedule groups; those integration tasks remain incomplete until code and tests exist.

## Stage 1.75 — Articulated mechanics integration

- [x] Define the proposed Physics-to-controller contract for achieved state, contact observations, constraint loads, structural failure, and topology ownership ([articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md)).
- [x] Add a bounded 1D compliant axial spring-damper and a first-crossing force-threshold monitor with step/time evidence ([spring-damper](AXIAL_SPRING_DAMPER.md), [threshold semantics](AXIAL_LINK_FAILURE.md)); this is an experiment, not a rigid articulated solver or material failure model.
- [x] Define finite `Vector3d`, normalized `Quaterniond`, metre-based `Pose3d`, and timestamped `RigidBodyState` with explicit coordinate-frame conventions.
- [x] Implement and test a bounded translational-only 3D distance spring-damper using body-origin separation and semi-implicit Euler ([spatial distance spring-damper](SPATIAL_DISTANCE_SPRING_DAMPER.md)); this is not a rigid joint or articulated solver.
- [x] Define a validated per-step `ContactObservation` contract with explicit optional force/impulse evidence ([contact observation](CONTACT_OBSERVATION.md)).
- [x] Detect bounded sphere-sphere contact geometry and resolve a single frictionless normal impulse ([sphere-sphere detection](SPHERE_SPHERE_CONTACT.md), [normal impulse response](CONTACT_IMPULSE_RESPONSE.md)).
- [x] Add bounded single-contact positional stabilization, preserving velocity/orientation and requiring callers to recompute contacts ([positional correction](CONTACT_POSITIONAL_CORRECTION.md)).
- [x] Add a bounded semi-implicit free rigid-body reference integrator with fixed-step, logical-time, orientation, and repeat-run tests ([reference integrator](FREE_RIGID_BODY_INTEGRATOR.md)); contacts and constraints remain separate.
- [x] Emit per-step contact observations with stable body/contact identities and logical step/time.
- [ ] Compute articulated constraint reaction loads with declared temporal semantics.
- [ ] Define threshold semantics for peak/sustained loads versus impulse before structural failure depends on them.
- [ ] Validate a structural failure scenario without conflating failure detection, body-topology mutation, and injury.
- [ ] Add deterministic/replay and cross-package contract tests showing Gestures can consume Physics observations without a Physics-to-Gestures dependency.

This is the first Physics milestone directly shaped by Gestures' Stateful IK requirements. The 1D compliant-link experiment and threshold-state monitor remain separate. Spatial value/state contracts, a translational-only 3D compliant link, a contact-observation record, sphere-sphere narrowphase, single-contact frictionless normal impulse, and bounded positional correction are implemented as separate narrow slices. They do not form a general collision or articulated solver. The bounded free rigid-body reference integrator is implemented and tested, but it does not apply forces, detect collisions, resolve contacts, model friction, or enforce constraints. Angular contact response, friction, simultaneous-contact solving, rigid-joint constraints/reaction loads, validated structural-failure behavior, and cross-package integration tests remain unimplemented. A bounded AABB sweep-and-prune broadphase exists and is tested; it returns candidate pairs and does not confirm collision.

## Stage 2 — Material identity and energy

- [ ] Define elements versus compounds, mixtures, and alloys.
- [ ] Add source/version/license provenance and valid property ranges.
- [x] Add a bounded lumped thermal-state contract with mass, absolute temperature, constant specific heat, reference temperature, and sensible-energy accounting ([first thermodynamics primitive](THERMODYNAMICS_FIRST_PRIMITIVE.md)).
- [x] Apply explicit net energy transfers for bounded heating/cooling while rejecting invalid properties, overflow, and states below absolute zero.
- [ ] Add temperature-dependent heat capacity, spatial gradients, transfer-rate models, and phase-change behavior.
- [ ] Add one supported phase-transition model and tests.

## Stage 3 — Coupled physical processes

- [x] Implement a deterministic, volume-accounted volcanic deposition primitive for a terrain height field ([geology model](GEOLOGY_VOLCANIC_TERRAIN.md)); tectonics, magma dynamics, cooling, and erosion remain future work.

- [ ] Add heat transfer with explicit boundary conditions.
- [x] Add bounded hydrodynamics references with explicit stability and limitation notes: a 1D linear wave model and 2D shallow-water models over flat/fixed terrain beds, including wet-dry shorelines ([hydrodynamics demonstrator](HYDRODYNAMICS_WORLD_DEMONSTRATOR.md), [shallow-water scope](HYDRODYNAMICS_SHALLOW_WATER_2D.md)). Systematic grid-convergence studies, rainfall/runoff, obstacles, and boat coupling remain future work.
- [ ] Connect mechanics, material response, and measurements.
- [ ] Add deformation, fracture, and causal deconstruction in increments.

## Stage 4 — Reactive phenomena

- [ ] Add reaction-model contracts and explicit species/stoichiometry.
- [ ] Couple combustion with heat transfer and material/fuel state.
- [ ] Add gas transport and buoyancy only with a declared model.
- [ ] Model electrical heating/arc effects through energy transfer and supported ionization approximations.
- [ ] Validate ignition, propagation, extinction, and conservation within the declared scope.

## Stage 5 — Persistent universe & multiscale worlds

- [x] Define stable finite-grid addressing and deterministic coordinate/seed derivation; versioned generation parameters remain future work.
- [ ] Define persistent cell/region state, checkpoints, pending events, and leave/revisit reconciliation.
- [ ] Define observer-driven resolution tiers without making observation the source of physical truth.
- [ ] Define coarse/fine model interfaces and conservative handoff requirements.
- [ ] Preserve aggregate state while distant processes continue evolving and account for elapsed logical time on wake-up.
- [x] Implement AABB intersection/union/expansion, linear swept bounds, and deterministic sweep-and-prune candidate pairs with tests.
- [ ] Add narrowphase contact confirmation and response; validate conservative bounds for supported motion models.
- [ ] Test approach/leave/revisit scenarios and transitions that occur while unobserved.
- [ ] Measure computational cost against physical error, not visual quality alone.

## Stage 5.5 — Historical timelines & parallel realities

- [x] Define immutable timeline lineage metadata, validated root-to-target ancestry, checkpoint eligibility across shared history, and historical fork points.
- [x] Document the checkpoint/replay/branch contract and separate reality, timeline, and checkpoint identities.
- [x] Define immutable checkpoint envelopes with state payload digest and model/schema identity, plus validated checkpoint-relative event-history segments.
- [x] Define a validated replay route that checks checkpoint versions and splits timeline-owned intervals at historical fork boundaries.
- [x] Document deterministic replay versus true inverse-update semantics and optional extension boundaries ([deterministic history & extensions](DETERMINISTIC_HISTORY_AND_EXTENSIONS.md)).
- [x] Implement a model-driven single-segment replay executor that restores caller-owned model state through a checkpoint decoder, applies ordered events, and advances to a requested tick within the validated interval ([executor](../Physics/Universe/TimelineReplayExecutor.cs), [tests](../tests/TheSingularityWorkshop.Physics.Tests/TimelineReplayExecutorTests.cs)).
- [ ] Implement durable versioned authoritative-state checkpoint storage and ordered event/input history.
- [ ] Implement a concrete physics-world/solver reconstruction adapter with compatibility checks and independently verifiable state fidelity.
- [ ] Implement persistent reality identities and isolated mutable state across parallel realities.
- [ ] Test that post-fork changes remain isolated and parent history remains unchanged.
- [ ] Define and test a trace-based reverse-step capability that refuses unsupported non-reversible operations.
- [ ] Implement opt-in prediction and rollback/resimulation on isolated speculative branches.
- [ ] Measure storage/replay trade-offs for shared history, checkpoint frequency, and branch count.
- [ ] Define a versioned, portable shareable-moment manifest with bounded scrub windows, provenance, compatibility, integrity, and privacy rules.
- [ ] Implement moment capture/preview/scrubbing over validated replay execution; distinguish exact reconstruction from approximate fallback.
- [ ] Implement creator-controlled sharing and remix lineage without mutating source timelines; validate access control, attribution, retention, and deletion.
- [ ] Prototype a lightweight hosted moment flow and measure package size, reconstruction latency, and first-time recipient success before scaling hosting.

The initial lineage type is metadata only. Time travel and multiverse simulation are not complete until persistence, replay, isolation, and validation are implemented.

## Stage 6 — Singularity Laboratory

- [ ] Connect experiments to MicroBundles and FSM_COS composition.
- [ ] Generate explainers from model/run metadata.
- [ ] Bind SVG diagrams, plots, and controls to real model state.
- [ ] Expand into the laboratory's experiment catalog with clear accuracy labels.

## Release gate

No package release or publication occurs from this roadmap without explicit authorization. A capability or demo must not be marked complete merely because documentation describes it.

---

## Resources & Support

[Physics home](../README.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) · [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="200" />
</p>

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
