# Composable Physics: Models, Solvers, and Experiences

**Purpose:** make physical knowledge reusable without confusing the phenomenon being described with the numerical method used to calculate it, the interface used to author it, or the experience that teaches or applies it.

This is an architecture and implementation contract, not a claim that the package already implements every domain below. The ambition is broad coverage of physics; implementation status remains evidence-based and bounded by source, tests, and validated scope.

## The central separation

A developer, instructor, student, or experience author should be able to describe **what is present, what is happening, and what question they want answered** without having to hand-author the numerical procedure that resolves it.

Keep these responsibilities separate:

1. **Physical vocabulary and data** — quantities, units, material identity, state, geometry, fields, species, boundaries, initial conditions, and observations.
2. **Model** — the governing relationships and assumptions that describe a physical question: for example, constant acceleration, Fourier conduction, a shallow-water approximation, or a constitutive material law.
3. **Solver** — an implementation that resolves a model: closed-form evaluation, numerical integration, finite-volume method, root finder, iterative constraint solver, or another suitable method.
4. **Experiment** — a reproducible declaration of inputs, conditions, requested outputs, model and solver versions, and validation checks.
5. **Experience** — a lesson, laboratory, simulation, design tool, game system, or GUI that helps a person select the question and inspect the result.
6. **Presentation** — diagrams, plots, animation, sound, and UI. These communicate evidence; they do not decide authoritative physical outcomes.

A model must not silently prescribe one solver. A solver must not silently change the model's assumptions. A GUI must not require a student to know which integration algorithm to choose before they can ask a meaningful physical question.

## The desired authoring experience

The human-facing workflow begins with **what**, not with solver implementation details:

- Choose the phenomenon or question.
- Declare the objects/materials/fields involved and the relevant initial and boundary conditions.
- Set the quantities that matter, with units and visible ranges.
- Choose the fidelity or accuracy goal when alternatives are available.
- Ask for an observation: trajectory, temperature history, pressure field, energy balance, phase fraction, current, wave height, or another defined output.
- Let a compatible solver resolve the model, or explain which required capability is missing.
- Inspect the result alongside assumptions, measurements, residuals, uncertainty, and model limitations.
- Change a parameter, compare runs, reproduce the result, and share the experiment.

A future GUI can make these choices tangible with diagrams, constrained fields, sliders, tables, and interactive plots. That GUI is an authoring/observation client, not a dependency of the Physics NuGet package. The same experiment must remain expressible and executable in code and serializable without a GUI.

## Solver selection is a contract, not magic

A solver can be selected automatically only when compatibility and acceptance criteria are explicit. Compatibility includes model family, dimensionality, state variables, boundary conditions, material law, supported regimes, stability constraints, and required outputs. The selection policy should prefer a solver that satisfies declared accuracy, conservation, determinism, and cost requirements.

If multiple solvers are compatible, an experience may expose the trade-off: speed, numerical error, resolution, memory, determinism, or supported regime. If none is compatible, return an explicit unsupported-capability result; do not fabricate a plausible-looking answer.

Examples of solver families that can be acquired independently:
- analytical/closed-form evaluation;
- ordinary differential equation integrators;
- algebraic, root-finding, and optimization methods;
- finite-difference, finite-volume, and finite-element methods;
- particle and molecular methods;
- rigid-body/contact/constraint solvers;
- stochastic or uncertainty-propagation methods;
- field and wave solvers;
- reduced-order, lookup-table, and surrogate models with explicit validity/error envelopes.

These are families, not a promise that each is already present or that one solver can resolve every problem. A solver provider should declare the model contracts it supports, its valid range, numerical method, stability requirements, error evidence, and known omissions.

## Package boundary and composability

The Physics NuGet package should be the discoverable, host-independent source for physical vocabulary, model contracts, reusable physical models, solver interfaces and implementations that genuinely belong in the core, experiment definitions, observations, and validation primitives.

Keep it free of GUI, Unity, renderer, and application dependencies. A third-party developer should be able to reference the package and build a simulation, lab, analysis tool, or alternative interface without adopting Workshop infrastructure.

Workshop-internal MicroBundles can package a selected composition of model definitions, data, solver providers, validation rules, and lesson/experience resources. MicroBundles are an acquisition and composition mechanism for internal consumers; they must not be required for an external developer to use the public NuGet APIs. An experience can request capabilities by stable identity and declared contract rather than depending on a monolithic “everything” object.

Do not put every domain into one giant solver or force all capabilities to load together. Keep domains independently usable and connect them through explicit exchanges of typed quantities, state, energy, momentum, mass, species, fields, and observations. Coupling must declare temporal semantics and validate conservation/residuals across the boundary.

## Composition-level material discovery and physics mapping

Material discovery must climb with the MicroBundle composition hierarchy. This higher-level application behavior is **MicroBundle arbitration**, not a Physics runtime or a new standalone physics solver. The arbitration layer discovers bundle contributions and coordinates the composition; it can call Physics domain evaluators to interpret the supplied material evidence and map it to exact parts.

The intended chain is:

1. **Discover providers and catalogs.** Find material identities, property evidence, model providers, and solver capabilities by stable IDs and declared contracts. A MicroBundle can contribute any of these without owning the whole world.
2. **Declare physical composition.** An assembly names its parts and explicit parent relationships. A sword can contain a blade, guard, pommel, and grip; each part remains independently addressable. Do not infer a part hierarchy from a display name.
3. **Arbitrate identity per part.** Resolve each part's material from evidence and provenance. Strong explicit references outrank weaker clues; equally strong conflicts stay ambiguous. Unknown explicit material IDs must not be overwritten by a weaker guess.
4. **Map material evidence.** For a resolved `material:steel` blade, find the steel property records and their units, validity ranges, and evidence status. A copper part maps to copper records, not steel records. An unresolved or ambiguous part receives no material properties by default. Assess declared numeric validity ranges against observed conditions explicitly; exact quantity/unit mismatches are incomparable until a trusted conversion policy exists.
5. **Find compatible physics.** Index models by required material properties, state variables, conditions, units, geometry, and supported regime. A numeric range check is necessary evidence when relevant, not proof that free-text conditions or all model assumptions hold. For example, density can support a mass calculation only when volume and the appropriate density validity conditions are available; thermal conductivity can support a conduction model only when its required assumptions and boundary/state inputs are satisfied.
6. **Bind and validate at the higher composition level.** Assemble the compatible model/solver providers into the requested experience, retaining which part, material record, model version, source, and validity conditions justified each binding. Report missing properties, conflicts, incompatible regimes, or absent solvers instead of filling gaps with guesses.

This is a MicroBundle arbitration pipeline, not a single all-knowing physics component. The MicroBundle layer owns provider discovery, bundle-set composition, orchestration, lifecycle invalidation, and the final arbitration result. Physics-owned evaluators answer narrower domain questions on supplied data: **what material is this part declared to be?**, **what property evidence matches that material?**, and **which declared prerequisites are present with compatible units?** Full condition validation, model compatibility, and solver selection remain distinct stages. Keep these questions separate so each layer can improve without invalidating the others.

### Implemented boundary today

The Physics repository currently provides deterministic, independently testable domain primitives: provenance-aware `MaterialPropertyDefinition` records, explicit `PhysicalAssemblyDescription` / part declarations, per-part `MaterialIdentityArbitrator` resolution over caller-supplied evidence, `PhysicalAssemblyMaterialPropertyMapper`, `MaterialPropertyRangeEvaluator`, and `MaterialModelCapabilityMatcher`. The latter reports whether declared property prerequisites are present with exact-unit compatibility. These primitives do not perform MicroBundle provider discovery, manage bundle installation/removal, coordinate cross-bundle arbitration, or publish a composed application result. Those are responsibilities of the MicroBundle composition/arbitration layer, which may invoke these Physics evaluators. The evaluators also do not convert units, interpret free-text conditions, establish empirical quality, select constitutive laws, or run a solver.

## Broad physical territory to capture

This is the target knowledge map. Every topic progresses through clear status levels: **mapped → explained → specified → implemented → validated → coupled**. A conceptual image or a page of equations is not equivalent to an implementation.

### Foundations shared by domains
- Quantities, dimensions, SI units, coordinate frames, reference systems, uncertainty, significant precision, and dimensional analysis.
- State, identity, geometry, topology, initial conditions, boundary conditions, constraints, logical time, sampling, and observation.
- Conservation/accounting of mass, momentum, angular momentum, charge, and energy where applicable.
- Measurement, experimental design, model assumptions, approximation error, reproducibility, provenance, and falsification tests.
- Mathematical methods: algebra, calculus, differential equations, linear algebra, tensors where needed, numerical integration, interpolation, optimization, statistics, and uncertainty propagation.

### Motion, forces, and matter
- Kinematics; Newtonian mechanics; momentum and impulse; work, energy, and power; gravity and orbital motion.
- Rigid bodies; contact, friction, rolling, joints and constraints; mechanisms; elasticity and plasticity; stress, strain, buckling, fatigue, fracture, and deconstruction.
- Continuum mechanics; elasticity and viscoelasticity; granular matter; solids and structural response.
- Relativity and quantum mechanics should be separate, explicitly scoped model families—not assumed extensions of a classical solver.

### Thermodynamics and transfer
- Temperature, internal energy, heat capacity, work, entropy and equilibrium under declared assumptions.
- Conduction, convection, and radiation as distinct transfer mechanisms.
- Phase diagrams and transitions: melting, freezing, boiling, condensation, sublimation, crystallization, latent heat, and phase-dependent properties.
- Equations of state, mixtures, thermal expansion, diffusion, and coupled thermal/material response.
- Refrigeration, engines, heat pumps, and cycles as compositions of thermodynamic processes, not special-case temperature tricks.

### Fluids and transport
- Hydrostatics; buoyancy; fluid statics and dynamics; mass and momentum transport.
- Shallow-water and free-surface models; waves, channels, rivers, lakes, oceans, dams, spillways, and wet/dry boundaries.
- Viscosity, incompressible and compressible flow, laminar and turbulent regimes, boundary layers, drag, lift, cavitation, and multiphase flow.
- Heat/species transport in fluids; diffusion and mixing; porous media; sediment, erosion, deposition, and fluid/solid coupling.
- Every fluid model must state its regime and dimensional reduction; a shallow-water solver is not a general Navier–Stokes solver.

### Electricity, magnetism, optics, and waves
- Charge, voltage, current, resistance, capacitance, inductance, circuit laws, energy, and power.
- Semiconductor behavior and junctions, diodes, BJT/MOSFET models, switching, transients, fields, motors, generators, transformers, and power grids.
- Electric and magnetic fields, induction, electromagnetic waves, radiation, and explicitly scoped plasma/ionization models.
- Oscillations, wave propagation, resonance, acoustics, optics, reflection, refraction, diffraction, interference, absorption, and emission.
- Diagrams must distinguish conventional current from electron motion, and distinguish a conceptual schematic from a validated device or field solver.

### Materials, chemistry, and planetary systems
- Elements, compounds, mixtures, alloys, crystal structures, defects, phase-dependent properties, and provenance-aware property data.
- Chemical species, stoichiometry, reaction kinetics, equilibrium, diffusion, reaction enthalpy, combustion, ignition, extinction, and reactive flow.
- Geology, tectonics, volcanism, erosion, atmosphere, weather and climate processes, planetary energy balance, stellar processes, and orbital systems.
- Biological or ecological extensions should be layered as additional models with their own evidence and validity criteria, not presented as basic mechanics.

This map is intentionally broad. It is a research and implementation agenda, not an assertion that the package contains all of these models. Each topic needs a bounded first slice, a reliable reference case, tests, and an honest capability entry before being advertised as implemented.

## Environmental interaction scales and role-constrained actions

The water/weather interaction contract and the cross-package gesture boundary are expanded in [Environmental Interaction Scales and Gestures](ENVIRONMENTAL_INTERACTION_SCALES_AND_GESTURES.md). It distinguishes puddles, pools, lakes, rivers, and coastal/ocean regimes by geometry, boundary conditions, forcing, and scale—not merely by object name—and treats rain, snow, and wind as time-varying inputs that can couple to water, terrain, materials, and participants.

It also defines the intended Experience-level selection chain from task and actor qualification through platform capabilities, environment, candidate gesture prerequisites, and observed physical feedback. Physics owns physical state and constraints; Gestures owns motion patterns; neither package becomes dependent on the other. Carrier operations, aircraft-specific controls, and command roles are examples of the target architecture, not validated operational simulations.

## Data and scientific provenance

Physical property data and reference observations should carry as much of the following as applicable:

- stable identity, schema/version, units, composition and conditions;
- source, author/publisher, retrieval or publication date, license/usage terms, and source locator;
- whether values are measured, published reference values, analytically derived, fitted, illustrative, or synthetic;
- valid temperature/pressure/composition/regime range and known exclusions;
- uncertainty, significant precision, calibration method, and transformation history;
- content digest or immutable version so a run can identify the exact data it consumed.

Cached public data and general training knowledge are useful for discovering candidate concepts and sources, but neither alone proves that a numerical value is authoritative. Verify important constants and property tables against attributable references; retain the citation/provenance with the data and test against independent reference cases where feasible. Never present invented values as measured facts.

## Evidence levels and living explanation

Use explicit evidence labels everywhere, including documentation and SVGs:

- **Conceptual:** communicates an idea; positions and shapes are illustrative.
- **Analytical:** follows a stated closed-form relationship or hand-checkable calculation.
- **Model-backed:** generated from the repository's actual model execution or recorded run.
- **Reference-compared:** checked against an independently identified analytical, experimental, or published reference.
- **Validated within scope:** acceptance criteria, error limits, and tested regime are declared and met.

A useful living lesson connects the question, governing relationships, variable meanings and units, model assumptions, solver/method, actual observations, validation, and next experiment. It lets learners pause, change inputs, compare predictions, and see where the model ceases to apply. SVGs and GUI controls should expose state and cause rather than decorate an unexplained output.

## A capability acceptance checklist

A new capability is not complete until the appropriate items are satisfied:

- [ ] Physical question and model boundary are stated.
- [ ] State, quantities, units, sign conventions, coordinate frames, and valid ranges are explicit.
- [ ] Initial/boundary conditions and material assumptions are represented.
- [ ] Governing relationships are documented with a path from equation to intuition and observation.
- [ ] Model is separate from solver policy where more than one resolution method may apply.
- [ ] Invalid, unsupported, and out-of-range inputs have defined behavior.
- [ ] Deterministic repeatability is tested where promised.
- [ ] Analytical/reference tests and numerical stability/error tests cover the declared regime.
- [ ] Relevant conservation laws and residuals are measured.
- [ ] Data and model versions are captured in run provenance.
- [ ] An accessible living explanation is adjacent to the theory and clearly labeled by evidence level.
- [ ] Coupling tests verify both sides of the interface, including energy/mass/momentum accounting as relevant.
- [ ] The capability catalog lists implemented scope and exclusions.
- [ ] External NuGet consumers can use the API without GUI or MicroBundle dependencies.

## Recommended implementation order

1. **Make the shared language reliable:** quantities/units conventions, model identity, boundary conditions, observation/run metadata, and evidence labels. Reuse existing contracts; do not build a huge abstraction layer before proving it.
2. **Close small causal loops:** continue electrical power → thermal energy → heat transfer; then material properties → thermal response/phase change; then fluid state → force on a body; then mechanical impact → material/structural response. Each bridge needs a conservation and validation story.
3. **Add solver contracts only where needed:** establish a common capability description and solver compatibility handshake, but keep specialized solver APIs clear and efficient.
4. **Expand domain breadth with bounded references:** a small validated representative model for each major domain before deepening any one domain indefinitely.
5. **Build GUI authoring against stable contracts:** GUI edits the model/experiment declaration, previews units and constraints, invokes a compatible solver, and inspects recorded results. Keep the GUI outside core Physics.
6. **Compose reusable experiences:** internal MicroBundles can assemble a lesson or simulation from existing capabilities, data, and solver providers. Publish external package APIs and documentation independently.
7. **Scale fidelity responsibly:** use profiling, declared error budgets, and reference comparisons to decide when a fine solver can be replaced by a coarse model, LUT, aggregate, or GPU implementation.

The north star is not to hard-code an answer for every imaginable problem. It is to capture physical meaning and evidence in reusable contracts, then let compatible solvers resolve well-defined questions and let many different experiences make those answers understandable.
