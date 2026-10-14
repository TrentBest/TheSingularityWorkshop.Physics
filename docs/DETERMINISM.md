# Determinism Contract

Determinism is an explicit property of a configured run, not a promise implied by the word “physics.”

## Replay inputs

A replay record identifies initial physical state and units; ordered external inputs/events; model, material-data, and schema versions; logical step size and step count; numerical solver/configuration; boundary conditions; any random seed and deterministic algorithm; and supported runtime/platform constraints.

Equivalent inputs under the same supported execution contract should produce equivalent state and measurements. If bitwise identity is supported, test and state that explicitly.

## Time

Physical time advances by declared logical steps. Rendering frames, browser timers, wall-clock time, and UI refresh rates must not silently determine integration. Pausing or changing presentation rate must not alter the result of a fixed sequence of logical steps.

## Ordering

Order-sensitive reductions, collisions, interactions, and event processing need stable ordering. Do not rely on incidental dictionary iteration, thread scheduling, race-winning writes, or unspecified GPU execution order. GPU execution is not itself a determinism guarantee.

## Randomness

Randomness is explicit input. A seeded generator is necessary but not sufficient if call order can change; streams should be scoped or indexed so unrelated work does not perturb the result unexpectedly.

## Numerical scope

Distinguish:
- deterministic replay on one supported configuration;
- numerically equivalent results within a declared tolerance;
- cross-platform bitwise reproducibility.

Floating-point behavior can differ by runtime, architecture, compiler, vectorization, and solver implementation. Do not promise the strongest level without evidence.

## Fidelity changes

A fidelity transition records the model change and preserves declared aggregate state. The same model at the same logical step must not be silently replaced with a reset or newly randomized approximation.

## Required tests

- identical inputs produce identical replay results within the stated contract;
- unit conversion preserves physical meaning within declared tolerance;
- event ordering is stable;
- pause/render cadence does not affect logical stepping;
- conservation/invariant checks run where applicable;
- coarse-to-fine transitions disclose and test their mapping.
