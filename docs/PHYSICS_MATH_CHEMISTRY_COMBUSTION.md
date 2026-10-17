# Physics + Math Domain Expansion — Math, Chemistry, Combustion, and Fluids

**Status:** design contract and implementation queue. This records intended breadth; it does not claim the listed solvers already exist.

**Authority:** Physics owns physical state transitions and evidence-backed models. Math owns reusable numerical primitives only when a stable, separately useful contract has emerged. Presentation may visualize results but cannot invent physical state.

Related: [Physics capability catalog](PHYSICS_CAPABILITY_CATALOG.md) · [Physics/Math LUT policy](PHYSICS_MATH_LUTS.md) · [Architecture](ARCHITECTURE.md) · [Living theory](LIVING_THEORY.md).

## 1. Governing principle

The goal is not a pile of effects; it is an inspectable domain of physical relationships. Each capability exposes its state, inputs, units, model assumptions, validity limits, deterministic stepping contract, observations, and tests. Build the smallest correct slice, then couple it to neighboring domains through explicit energy, mass, momentum, and information transfers.

"Good physics" means a stated model whose behavior can be tested against analytic solutions, conservation checks, measured reference data, or clearly labelled illustrative assumptions. Visual plausibility alone is not validation.

## 2. Math: a real capability, not an assumed dependency

Math is currently a proposed domain boundary, not a verified package or MicroBundle. Before adding a package reference, inspect Workshop repositories and MicroBundle manifests for an existing Math provider. Do not duplicate existing Spatial types or introduce an upward dependency.

Candidate reusable primitives:
- scalar/vector operations with explicit dimensions or units where needed;
- interpolation (linear, monotone, cubic only where appropriate), curve sampling, and bounded root finding;
- numerical integration with explicit timestep, method, stability conditions, and error measurements;
- coordinate transforms, rotations, and robust geometric predicates respecting existing Physics spatial contracts;
- LUT generation/evaluation, multidimensional axis layout, deterministic interpolation, error comparison, and content digests;
- deterministic sampling and distributions built over an explicitly selected RNG;
- reference functions and numerical validation helpers used by tests, not hidden inside physical solvers.

Split Math into a standalone package only when the public API is coherent, reusable beyond Physics, and does not merely re-export Physics implementation details. Until then, keep narrowly scoped internal numerical primitives in Physics and document the extraction seam.

## 3. RNG: deterministic, ergonomic, explicit

Evaluate Squirrel3 as a candidate small deterministic integer hash/counter-based generator. First establish its algorithm, integer widths, seed/key semantics, published test vectors, licensing, and suitability for required statistical uses. Do not call a hash cryptographically secure or imply statistical quality beyond tested evidence. Physics randomness is for reproducible simulation and procedural variation, not secrets.

Desired helper experience:
- seeded integer, bounded integer, boolean, float/double in [0,1), and ranged float/double;
- vectors, directions, points in a box/sphere/disk, and weighted choices;
- reproducible per-entity/per-event streams keyed by world seed + stable identity + logical event/counter, so adding an unrelated entity does not reshuffle every later result;
- normal/Poisson/etc. sampling only when distributions and edge cases are implemented and tested;
- deterministic shuffle and selection without replacement;
- serialization/replay of algorithm ID, version, seed/key, counter/state, and any sampling method affecting a run.

Requirements: avoid modulo bias for bounded integers; define endpoint behavior; reject invalid bounds; add golden vectors, distribution sanity tests (not presented as proof), repeatability tests, and tests showing independence from call ordering where the keyed API promises it. Deterministic reproducibility is not the same as physical uncertainty.

## 4. LUT-first where justified, analytical truth where affordable

Use the existing [LUT policy](PHYSICS_MATH_LUTS.md). Every authoritative physical table declares identity/version/digest, input axes and SI units, validity range, source/provenance, interpolation and boundary policy, uncertainty/error budget, deterministic evaluation, and a safe out-of-domain result.

Prefer analytic equations when cheap, stable, and clearer. Use LUTs when profiling or data provenance justifies them. Tables are promising for molecular potential curves, calibrated material properties, temperature-dependent reaction rates, combustion coefficients, spectral emission approximations, and presentation mappings. Keep presentation LUTs non-authoritative.

## 5. Molecular structure and bonding — start with hydrogen

The first chemistry demonstrator should make a hydrogen atom pair inspectable in 3D: nuclei, electron probability/bond representation, inter-nuclear distance, energy, and the distinction between an explanatory diagram and a quantum-mechanical result.

Milestones:
1. **Geometry/ontology:** atoms, isotopes, electrons as modelled entities/representations, bonds as model outputs or authored hypotheses; explicit units and stable identity.
2. **Educational reference:** visualize two H atoms and their separation; show a covalent bond only when the selected model says it is bonded. Explain shared electron density rather than depicting electrons as tiny classical planets on fixed tracks.
3. **Potential-energy curve:** a versioned H₂ potential-energy LUT over internuclear distance, sourced from a named trusted dataset or computed reference. Record units, equilibrium region, dissociation limit, uncertainty/accuracy and interpolation error. No fabricated table values.
4. **Bounded molecular model:** evaluate energy/force from the declared potential; test force direction against the energy gradient, equilibrium behavior, and table-boundary behavior.
5. **Electrostatic/orbital extensions:** only after choosing a defensible model level (classical electrostatics, orbital approximation, or quantum-chemistry data). Do not market a simple spring as quantum chemistry.

The visual should make assumptions visible. Fixed orbital paths may be shown as historical/classroom metaphors only when labelled as such, not as literal electron trajectories.

## 6. Combustion — Burning Engine concept

The proposed Burning Engine is a coupled surface/volume model, not merely a flame shader. Inputs include surface locations and normals, material/fuel profile, exposed area, and environmental conditions. Fuel and oxygen availability constrain reaction; heat drives ignition, propagation, gas production, and visible emission.

The three-texture idea is a viable GPU-oriented representation, with explicit semantics:
- **Fuel field:** remaining fuel or fuel mass per surface/voxel cell; consumption is a conservative decrement tied to reaction extent.
- **Thermal field:** temperature or thermal energy with declared units and a capacity model. Prefer energy as the conserved quantity where practical; derive temperature from heat capacity and state assumptions.
- **Oxidizer field:** oxygen amount/concentration or mass per cell, not an unqualified normalized colour. Reaction consumption is stoichiometrically linked to fuel consumed.

Additional state may be required for gas products, soot/smoke, moisture, and transport. Do not force these into three fields if that breaks mass/energy accounting. GPU textures may pack the fields, but logical fields and units remain defined independently of pixel channels.

Minimum model sequence:
1. **Single-cell reaction accounting:** declared fuel species/reaction, stoichiometry, fuel/oxygen limiting reagent, reaction rate, heat release, and mass/energy bookkeeping. Use an explicitly sourced reaction profile.
2. **Surface ignition and conduction:** normals determine surface orientation/exposure and sampling geometry, but normals alone do not determine burning. Include heat transfer, ignition threshold, fuel/oxygen availability, and a stated approximation for flame-to-surface feedback.
3. **Neighbor transport:** transport heat, oxygen, and gaseous products across cells using a stable, bounded method. Track timestep and grid spacing; test positivity, conservation, and stability limits.
4. **Flame volume/emission:** derive flame geometry and visibility from solved state. Visible flame is a presentation criterion based on temperature/reaction/emission range, not authoritative combustion state. Hot gas can exist without visible flame; visible flame must not create heat or consume fuel by itself.
5. **Coupled effects:** smoke, soot, buoyancy, expansion, and sound consume explicit reaction/transport outputs. Avoid arbitrary energy creation.

Intended data flow: material + surface geometry + environment → reaction/transport state → energy and mass changes → gas/thermal plume → presentation. GPU implementation is a performance backend, not a different set of physics rules. Begin with a CPU reference model for tiny grids and compare any GPU path against it.

## 7. Gas filling a volume

Treat gas as a field/state in a bounded volume, not as a colour that spreads. Start with a scoped model level:
- well-mixed compartment model for concentration/partial pressure and mass balance; then
- grid-based advection/diffusion for spatial distribution; optionally
- a low-Mach/approximate buoyancy model for thermal gas motion.

Specify volume geometry, boundary conditions, source/sink rates, temperature, pressure assumptions, and whether gas is treated as ideal. For multiple species, track each species separately and validate non-negative concentration, conservation under closed boundaries, mixing behavior, and timestep/grid sensitivity. Oxygen from this domain can feed Burning Engine; combustion products can feed the gas domain. Coupling must prevent double-counting mass or heat.

## 8. Mixing liquids and surface water

Build upward from existing shallow-water models; do not call shallow-water a full 3D fluid solver. First extensions should focus on measurable coupling:
- multiple liquid species or tracers with concentration and conservative mixing;
- temperature/thermal energy exchange;
- sources, sinks, impermeable/absorbing boundaries, and container geometry;
- buoyancy/density stratification only when the chosen model supports it;
- boat/body coupling through explicit displacement, momentum exchange, and wave/contact observations;
- terrain and buildings as geometry/boundary inputs, not decorative masks.

Test closed-system mass conservation, tracer conservation, positivity, equilibrium/mixing expectations, and stability as timestep/grid spacing changes. A bathtub with boats, waves, terrain, and buildings is an integration demonstration assembled from smaller validated capabilities, not the first solver.

## 9. Domain coupling and simulation schedule

Capabilities may advance at different logical rates, but each exchange must define its interval and aggregation rule. Faster collision/contact events must not be silently lost when thermal or fluid processes tick more slowly. Exchange quantities over elapsed logical time, preserve event ordering, and record the coupling schedule in experiment provenance. Never equate render frames with physical timesteps.

Examples:
- collision/impact → heat, deformation, sound or fracture through an event contract;
- electricity → Joule heat using electrical energy actually transferred;
- combustion → fuel/oxygen/product mass changes + reaction heat;
- hot gas → buoyancy/transport and thermal exchange;
- liquid flow → pressure/force on a boat, then reaction impulse back into fluid;
- molecular potential → bond force/energy only in the declared chemistry model.

## 10. Validation gates

Every implemented slice needs:
- SI units and explicit conventions;
- defined state and input validity;
- deterministic repeatability under a declared RNG/solver contract;
- analytic/reference comparisons with tolerance or uncertainty;
- conservation/positivity checks where applicable;
- boundary and invalid-input tests;
- timestep/grid refinement or stability evidence for numerical PDEs;
- serialized run identity with model, data/LUT digest, RNG algorithm/seed, parameters, timestep, and assumptions;
- a living experiment/SVG generated from actual model output, clearly labelled as analytical, measured, illustrative, or presentation-only.

A document or diagram is not evidence of an implemented solver. Update the capability catalog only when source and focused tests substantiate the status.

## 11. Suggested implementation order

1. Inspect current code/tests and preserve Operation Sith branch discipline: development is the integration branch; do not create another work branch.
2. Inventory math, LUT, random, thermodynamics, electricity, materials, and hydrodynamics primitives; reuse rather than duplicate.
3. Deliver a tiny deterministic RNG contract and helper tests, after verifying Squirrel3 suitability and exact algorithm semantics.
4. Add a Math/LUT reference experiment with golden vectors and explicit error measurements.
5. Prototype a sourced H₂ potential-energy curve and molecular visualization data contract.
6. Implement single-cell combustion mass/energy accounting before surface textures or 3D flames.
7. Implement a well-mixed gas compartment and couple it to oxygen consumption/combustion products.
8. Extend liquid mixing conservatively on top of existing hydrodynamics.
9. Only then optimize hot loops or move selected fields to GPU textures, with CPU-reference comparisons and measured profiling.

No merge to master, package publication, release, deployment, or unrelated repository edits are authorized by this document.
