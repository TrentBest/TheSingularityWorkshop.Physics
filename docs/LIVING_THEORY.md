# Living Theory & Interactive Documentation

The Workshop should eventually generate physics explanations from the same versioned definitions, experiments, and measurements that drive simulations. This is the direction, not a claim that an automatic theory generator already exists.

## One source of truth

**Ontology identity → Model definition → Experiment configuration → Deterministic run → Measurements → Explanation artifact**

The explanation is a view over those records, not a second, hand-maintained physics implementation.

## What a living page can show

- **What:** the physical phenomenon and quantities involved.
- **Why:** governing relationships and assumptions.
- **How:** the actual model and numerical method.
- **Interact:** bounded changes to declared inputs.
- **Observe:** measurements, plots, and state transitions.
- **Validate:** analytical comparisons, residuals, invariants, and reference data.
- **Trace:** model/data versions, source provenance, and run identity.

## SVG as an explanatory instrument

SVG diagrams should be generated from or bound to real model values where possible. Axes, arrows, dimensions, labels, and color encodings need explicit units and accessible text. Interactivity exposes the value it changes and the state it observes.

GUI.SVG and other rendering technologies are optional consumers of physics contracts. No SVG/DOM types belong in the physics core.

## Reproducible evidence

Every generated artifact points to the run configuration and model versions used. A reader should be able to rerun a supported experiment and compare measurements. Illustrative or synthetic data must be labeled as such.

## Build in stages

1. Keep theory metadata alongside model contracts.
2. Give experiments structured inputs, outputs, measurements, and provenance.
3. Produce static Markdown/SVG summaries from real run records.
4. Add interactive controls connected to bounded experiment inputs.
5. Add version-aware comparison and replay.
6. Let the Workshop host combine those pages into the Singularity Laboratory's navigable research spaces.

## First concrete path: from an existing run record to a living experiment

The repository already has a narrow working example: `ConstantAccelerationExperiment` records the initial one-dimensional state and every completed fixed step, while `ConstantAccelerationRunRecord` serializes the declared inputs, SI units, model/schema versions, and actual samples. See the [experiment implementation](../Physics/Experiments/ConstantAccelerationExperiment.cs), [run-record implementation](../Physics/Experiments/ConstantAccelerationRunRecord.cs), and [theory/evidence contract](THEORY.md). This is a foundation, not yet a general experiment or provenance framework.

The 3D trajectory experiment now connects that same solver to recorded samples. `ProjectileTrajectoryExperiment` stores the declared inputs, coordinate-frame convention, initial state, and every state returned by the existing solver; `ProjectileTrajectoryRunRecord` preserves those samples with model/schema identity and SI units. See [the experiment and record implementation](../Physics/Experiments/ProjectileTrajectoryExperiment.cs) and [the trajectory acceptance tests](../tests/TheSingularityWorkshop.Physics.Tests/ProjectileTrajectoryExperimentTests.cs). This is a sampled analytical trajectory, not a drag, launch-mechanism, collision, or impact simulation.

### Implemented trajectory experiment contract

A first trajectory experiment should declare and preserve:

- **Identity:** stable model ID, model-contract version, and run-record schema version.
- **Initial state:** position and velocity vectors, elapsed logical time, and SI units.
- **Environment:** constant acceleration vector and the coordinate-frame/sign convention chosen by the caller.
- **Schedule:** positive finite fixed timestep and positive step count, with overflow/invalid-input behavior defined.
- **Samples:** the initial state plus each completed logical step, in stable order.
- **Derived observations:** final state and, when useful, per-sample logical time; derived values must be traceable to recorded samples.
- **Validation evidence:** analytical comparison for constant acceleration, a declared numerical tolerance, invalid-input tests, and a repeatability test.
- **Limitations:** explicit statement that the run is a point-mass kinematics experiment, not evidence for aerodynamic flight, real-world launch accuracy, target intersection, or impact physics.

Do not invent external measurement provenance for a purely analytical demonstration. If a later experiment uses measured gravity, wind, launch speed, or other external data, record the source and uncertainty separately from the model's own computed output.

### Evidence path for the first living page

1. Reuse the tested constant-acceleration solver; do not reimplement the equations in the renderer.
2. Define a small immutable experiment input and run record, keeping the coordinate frame explicit.
3. Record the initial state and each sampled state from actual solver calls.
4. Test each sample against the analytical solution, then test repeatability and invalid boundaries.
5. Generate a static Markdown/SVG page from the record, labeling axes and values with units and distinguishing analytical predictions from simulated samples.
6. Only after the static artifact is reproducible should interactive controls be connected to bounded inputs. A control must rerun or clearly identify the exact run it is displaying.

The first living page can therefore be narrow and honest: a declared model, equations, a real recorded run, plotted samples, validation results, source links, and a visible list of omitted physics. It need not wait for a universal experiment schema or a full physics engine.

---

## Resources & Support

[Physics home](../README.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) · [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="200" />
</p>

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
