# Physics Roadmap

This roadmap separates architecture from completed functionality. Items move to “implemented” only with code, tests, and documented scope.

## Stage 0 — Establish the domain

- [x] Create a dedicated Physics repository.
- [x] Define core/host boundaries and deterministic execution principles.
- [x] Map physics capability classes to the Workshop ontology.
- [x] Establish theory, provenance, and living-documentation contracts.
- [x] Confirm initial target framework (net8.0) and repository/package identity.
- [x] Add project, solution, CI, and baseline tests.

## Stage 1 — First executable proof: mechanics

- [x] Implement an initial renderer-independent, SI-unit constant-acceleration kernel with input validation.
- [x] Define explicit logical timestep for the initial mechanics kernel.
- [x] Implement a bounded analytical case: one-dimensional constant acceleration.
- [x] Test analytical outcomes, repeatability, invalid inputs, and unit consistency.
- [x] Generate a versioned JSON run record from actual experiment samples, with explicit units and repeat-serialization tests.
- [ ] Add reference-data provenance, uncertainty/residual reporting, and an independently checkable validation record.

## Stage 2 — Material identity and energy

- [ ] Define elements versus compounds, mixtures, and alloys.
- [ ] Add source/version/license provenance and valid property ranges.
- [ ] Add energy and heat-capacity contracts.
- [ ] Model heating/cooling with declared assumptions.
- [ ] Add one supported phase-transition model and tests.

## Stage 3 — Coupled physical processes

- [ ] Add heat transfer with explicit boundary conditions.
- [ ] Add a bounded hydrodynamics scenario and convergence/limitation notes.
- [ ] Connect mechanics, material response, and measurements.
- [ ] Add deformation, fracture, and causal deconstruction in increments.

## Stage 4 — Reactive phenomena

- [ ] Add reaction-model contracts and explicit species/stoichiometry.
- [ ] Couple combustion with heat transfer and material/fuel state.
- [ ] Add gas transport and buoyancy only with a declared model.
- [ ] Model electrical heating/arc effects through energy transfer and supported ionization approximations.
- [ ] Validate ignition, propagation, extinction, and conservation within the declared scope.

## Stage 5 — Multiscale worlds

- [ ] Define coarse/fine model interfaces and conservative handoff requirements.
- [ ] Preserve aggregate state while distant processes continue evolving.
- [ ] Test approach/leave/revisit scenarios.
- [ ] Measure computational cost against physical error, not visual quality alone.

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
