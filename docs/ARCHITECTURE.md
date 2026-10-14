![Physics capability architecture](images/physics-architecture.svg)

# Physics Architecture

**Status:** architecture contract; implementation is incremental. This document describes intended boundaries, not claims that every solver already exists.

## Responsibility boundary

| Layer | Responsibility | Must not become |
|---|---|---|
| Physical domain | Units, quantities, state, model contracts, numerical methods | A renderer or application |
| Physics capability | A bounded model such as mechanics, heat transfer, or fluid flow | A monolithic all-physics switch |
| FSM_API | Lifecycle, discrete transitions, deferred work, state-machine coordination | A substitute for numerical integration |
| MicroBundleDomain | Bundle identity, metadata, dependencies, configuration contracts | The numerical solver |
| FSM_COS | Resolve and compose capabilities needed by an experience | A physics engine or application loop |
| Host / renderer | Input, visualization, presentation, interaction | The authority on physical truth |
| Living theory | Explain assumptions, equations, measurements, provenance | Claims disconnected from runtime data |

The dependency direction remains downward. The physics core must not depend on WebPage, GUI.SVG, Renderer, Unity, or FSM_COS. Integration adapters can live above the core.

## Physics is causal state, not appearance

A model receives declared state and inputs, advances according to a specified method, and produces new state plus observations. Presentation consumes those outputs.

- Electrical energy can heat a conductor; a sufficiently specified model may then calculate ionization, expansion, light, and cooling.
- Combustion depends on fuel, oxidizer, temperature, reaction assumptions, and transport—not merely a flame animation.
- Fracture changes material topology and transfers energy and momentum; it is not only a pre-authored destruction effect.
- A material label is not a complete material model. Composition, phase, temperature, pressure, and model scope matter.

Every capability must state which links in these chains it actually models.

## Ontology-aligned composition

Physics classes are capability domains, not a fixed number of ontological layers. Ontology addressing and indexing can evolve independently of this taxonomy. A model may participate in several domains, and a MicroBundle may request a small subset.

A bounded fire experiment might compose units and time, material/fuel data, heat transfer, reaction/combustion, optional gas flow and buoyancy, and measurements. A simpler flame demonstration may intentionally omit gas chemistry and disclose that scope.

## First executable experiment

The initial implemented experiment is intentionally narrow: one-dimensional constant acceleration. Its definition captures the initial kinematic state, constant acceleration, positive logical timestep, and step count. The result retains the exact definition and a state sample for the initial condition plus every completed step.

A run can be converted into a versioned JSON record containing model/schema identifiers, explicit units, the declared definition, all actual samples, completed-step count, and final observed state. Repeated serialization of the same record is tested for stable output. This is a first record format, not yet a general provenance system: source-data citations, uncertainty analysis, external reference comparisons, and full model-build provenance remain future work.

## Execution contract

A complete future run record should declare model/data versions, initial state, boundary conditions, external inputs, logical timestep, solver configuration, deterministic ordering, measurements, stopping conditions, provenance, and validation references.

The numerical kernel must be callable independently of host frame rate. FSM_API can coordinate discrete modes such as ready, advancing, paused, completed, or invalid. Fixed-step numerical integration remains explicit in the physics layer. The current scalar experiment runner and JSON record are pure .NET code; an FSM_API adapter and MicroBundle/FSM_COS composition are not yet implemented in this repository.

## Process-group scheduling

Physical capabilities should be scheduled independently through FSM_API processing groups. A domain FSM may invoke its domain group when its process-rate policy says it is due; a canonical `TickAll` group can provide the fixed logical heartbeat from which those rates are counted. The heartbeat is simulation time, not implicitly a render frame.

Initial cadence hypotheses differ by domain: bulk electricity may update around once per logical second, bulk heat around every two seconds, mechanics according to stability/contact requirements, and impact consequences in response to detected events. These are design examples, not implemented rates. Local interactions, faults, threshold crossings, and other causally important events can wake a model earlier than its background cadence.

The [process-group scheduling contract](PROCESS_GROUP_SCHEDULING.md) defines time accounting, catch-up, cross-domain exchange, relevance tiers, and the validation data required before optimizing. A slower schedule must not silently lose elapsed time or skip an event that changes the physical outcome.

## State and measurements

Prefer explicit state over hidden global state. Events record transitions; they are not a second source of truth. Measurements identify quantity/unit, logical time or step, source model/state, method, and uncertainty or numerical residual where known.

Checkpointing and replay capture the physical inputs needed to reconstruct a run. Presentation-only data must not change physical outcomes.

## Fidelity and scale

Different distances or relevance levels may use different computational detail, but a cheaper representation must not reset history. A coarse model preserves declared aggregate state; refinement begins from that state and discloses approximation/error. Do not silently create energy, mass, momentum, fuel, or elapsed time.

Where a conservative handoff cannot be justified, reject, constrain, or label the transition approximate.

## Validation gates

A capability is not complete because it looks plausible. Evidence grows through:
1. dimensional/unit checks and invariants;
2. analytical or limiting cases;
3. deterministic repeat tests;
4. traceable reference-data comparisons;
5. timestep/grid convergence where relevant;
6. coupled-system tests;
7. regression scenarios and declared accuracy bounds.

Cross-platform bitwise identity is distinct from deterministic replay on a supported runtime.

## Core constraints

- No Unity types or engine dependency in the core.
- No renderer-specific vectors or coordinate systems in domain contracts.
- No wall-clock time or render-frame delta as a hidden simulation input.
- No unseeded randomness in deterministic paths.
- No silent assumptions about units, material data, or boundary conditions.
- No claim of atomistic, quantum, plasma, combustion, or turbulent-flow fidelity unless the model supports it.
- No generated explanation that exceeds the evidence of the model that produced it.

---

## Resources & Support

[Physics home](../README.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) · [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="200" />
</p>

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
