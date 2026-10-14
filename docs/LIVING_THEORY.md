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

The first useful living document is one small experiment whose assumptions, equations, run, measurements, and explanation stay connected.
