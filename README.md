<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/docs/images/avatar.png" alt="The Singularity Workshop" width="200" />
</p>

# The Singularity Workshop · Physics

**Physics is the study of what changes, why it changes, and what must follow.** This package is the Workshop's foundation for deterministic, composable physical models—so experiments, simulations, and eventually entire worlds share explainable rules rather than disconnected visual effects.

<p align="center"><strong>What → Why → How → Observe → Explain</strong></p>

---

## 00 ✳ What

TheSingularityWorkshop.Physics is the physics domain for the Singularity Workshop. It will provide small, composable physical capabilities that can be selected for an experiment or world: mechanics, energy, thermodynamics, fluids, materials, electromagnetism, reactions, combustion, waves, and more.

Physics state is authoritative. Rendering and interaction observe that state; they do not invent the underlying result.

> **Status:** foundation and architecture in development. A capability is considered implemented only when its model, assumptions, units, deterministic behavior, and validation are represented in code and tests.

## 01 ◇ Why

A spark should be more than a sprite. A fire should change its fuel, temperature, gases, and surroundings. A copper object should not behave like iron merely because both have a metallic shader. A distant fire may use a cheaper representation, but it must continue to evolve and remain coherent when approached again.

The Workshop needs one causal foundation that can explain its results, support repeatable experiments, and scale the detail of computation without silently rewriting history.

## 02 △ How

- **FSM_API** coordinates discrete lifecycle and state transitions; numerical solvers calculate physical changes.
- **Ontology-aligned capabilities** keep mechanics, thermodynamics, hydrodynamics, materials, and other domains independently composable.
- **MicroBundles** describe the smallest useful combination of models and data for a particular experiment.
- **Deterministic inputs and explicit time** make runs replayable and comparable.
- **Measurements, provenance, and validation** connect every result to its model and evidence.
- **Living theory** grows from the same model definitions and observations used by the simulation.

Physics has no dependency on Unity, a renderer, or a particular host application.

## 03 ▷ See it in a minute

The first executable proof will be a small, reproducible mechanics experiment: predict the motion, state the assumptions, advance fixed logical steps, and compare the measured result with an analytical reference.

This repository is currently establishing the contracts and ontology map. The experiment is not advertised as implemented until the solver and tests exist.

## 04 ○ Documentation & theory

- [Architecture](docs/ARCHITECTURE.md) — boundaries, determinism, and execution model
- [Physics classes & ontology](docs/PHYSICS_CLASSES.md) — capability domains and their relationships
- [Theory and evidence](docs/THEORY.md) — how explanations connect to models and results
- [Determinism contract](docs/DETERMINISM.md) — replay, time, ordering, and numerical limits
- [Roadmap](docs/ROADMAP.md) — staged path from mechanics to coupled phenomena
- [Living documentation design](docs/LIVING_THEORY.md) — future generated explainers, measurements, and SVG diagrams

The long-term goal is not a library of static pages about physics. It is living theory: explanations that can show the governing model, expose assumptions, run an experiment, display measurements, and trace each conclusion to its source.

---

## Resources & Support

- [The Singularity Workshop](https://github.com/TrentBest)
- [FSM_API](https://github.com/TrentBest/FSM_API) · [NuGet](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API/)
- [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology)
- [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain)
- [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS)
- [Report an issue](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)
- [GitHub profile](https://github.com/TrentBest) · [Coder Legion](https://coderlegion.com/)
- [Support the Workshop on Patreon](https://www.patreon.com/)
- [Support the Workshop on PayPal](https://www.paypal.com/)

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
