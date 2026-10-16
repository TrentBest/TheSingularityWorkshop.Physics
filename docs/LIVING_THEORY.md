# Living Theory & Interactive Documentation

The Workshop should generate physics explanations from the same versioned definitions, experiments, and measurements that drive simulations. This is the direction, not a claim that a universal automatic theory generator already exists.

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

## First concrete path: from a run record to a living experiment

The repository has a narrow working example: `ConstantAccelerationExperiment` records the initial one-dimensional state and every completed fixed step, while `ConstantAccelerationRunRecord` serializes inputs, SI units, model/schema versions, and actual samples. See the [experiment implementation](../Physics/Experiments/ConstantAccelerationExperiment.cs), [run-record implementation](../Physics/Experiments/ConstantAccelerationRunRecord.cs), and [theory/evidence contract](THEORY.md). This is a foundation, not a general experiment or provenance framework.

The 3D trajectory experiment connects the existing analytical solver to recorded samples. `ProjectileTrajectoryExperiment` stores declared inputs, coordinate-frame convention, initial state, and each solver state; `ProjectileTrajectoryRunRecord` preserves those samples with model/schema identity and SI units. See the [experiment and record implementation](../Physics/Experiments/ProjectileTrajectoryExperiment.cs) and [trajectory acceptance tests](../tests/TheSingularityWorkshop.Physics.Tests/ProjectileTrajectoryExperimentTests.cs).

The X/Z trajectory SVG and the marked “Declared experiment” Markdown summary are generated from recorded run data by tools outside the Physics assembly. CI executes the C# trajectory experiment, serializes its actual run record to a temporary file, then verifies that both the committed SVG and the committed Markdown summary match that actual output. CI separately checks the SVG against the checked-in, reviewable JSON fixture. The C# model therefore guards both visual and tabular evidence; neither renderer duplicates the solver, and the Markdown tool changes only its explicitly marked section. [The page](experiments/projectile-trajectory.md), [serialized fixture](experiments/projectile-trajectory.run.json), [C# record generator](../tools/ProjectileRunRecordGenerator/Program.cs), [SVG renderer](../tools/render_projectile_trajectory.py), and [Markdown renderer](../tools/render_projectile_trajectory_markdown.py) are linked. This is one narrow reproducible path, not yet a universal theory generator.

### Implemented trajectory experiment contract

- **Identity:** stable model ID, model-contract version, and run-record schema version.
- **Initial state:** position and velocity vectors, elapsed logical time, and SI units.
- **Environment:** constant acceleration vector and caller-declared coordinate-frame/sign convention.
- **Schedule:** positive finite fixed timestep and positive step count, with invalid-input and overflow behavior defined.
- **Samples:** initial state plus each completed logical step, in stable order.
- **Validation:** analytical comparison, repeatability, invalid-input checks, and serialization tests.
- **Limitations:** a point-projectile kinematics experiment, not aerodynamic flight, launch accuracy, target intersection, or impact physics.

Do not invent external measurement provenance for a purely analytical demonstration. If a later experiment uses measured gravity, wind, launch speed, or other external data, record the source and uncertainty separately from computed output.

## Next steps

1. Add bounded input controls that rerun an experiment or clearly identify the exact run being displayed.
2. Extend the provenance contract before mixing measured data, model versions, and derived observations.
3. Generalize the evidence pipeline only after another physical model demonstrates the same end-to-end contract.

---

## Resources & Support

[Physics home](../README.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) · [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="200" />
</p>

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
