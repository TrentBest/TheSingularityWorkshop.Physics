# Numerical Validation Playbook

**Purpose:** make physical claims auditable. A solver that produces a plausible picture has not, by that fact alone, demonstrated that its equations, units, numerical method, or limits are correct.

This is a contributor contract for the current Physics library and future models. It does not claim that every validation method below has already been implemented.

## Evidence ladder

Use the strongest feasible evidence for each model and label what the evidence actually establishes.

1. **Input contract:** reject non-finite values, invalid dimensions, impossible ranges, and unsupported configurations before state mutation.
2. **Algebraic checks:** test exact identities, sign conventions, dimensional relationships, and boundary behavior.
3. **Analytical comparison:** compare with a known closed-form solution where one exists.
4. **Invariant or balance checks:** measure mass, momentum, energy, volume, or another quantity that should be conserved or accounted for under the declared assumptions.
5. **Refinement study:** vary timestep, grid spacing, iteration count, or another numerical control and report how the result changes.
6. **Independent reference:** compare against a trusted published result, independent implementation, or measured dataset with provenance and uncertainty.
7. **Coupled-system validation:** test that independently validated components exchange quantities exactly once, in the right units, at the right logical time.

A model may have useful evidence at one level and gaps at another. For example, an analytical free-field acoustics calculation can be tested against its equations without proving that a room-acoustics model exists.

## Required experiment declaration

Every executable experiment should make the following information discoverable, in its definition, run record, or accompanying document:

- Physical question and intended use.
- Model identifier and model/schema version.
- SI units, coordinate frame, sign convention, and reference values.
- Initial state, external inputs, and boundary conditions.
- Numerical method, timestep or time-advance rule, and termination condition.
- Valid input ranges and simplifying assumptions.
- Expected analytical/reference result or applicable invariant.
- Error metric and acceptance tolerance, including why the tolerance is appropriate.
- Deterministic inputs and event ordering requirements.
- Output samples and derived observations, with provenance for external data.

The existing constant-acceleration run record is an initial example of capturing a definition and actual samples; it is not yet the repository-wide provenance format.

## Test patterns

### Input and state validity

Test valid ordinary values, lower and upper boundaries, invalid signs, zero where forbidden, NaN, positive/negative infinity, and overflow paths where relevant. If a rejected operation must not partially change state, assert that the original state remains unchanged.

### Analytical limits

Prefer small cases with known answers before large scenarios. Examples:

- Constant acceleration: compare sampled position and velocity with the analytical equations at each logical time, not only at the final time.
- Sphere contact: cover separated, touching, overlapping, coincident-center, and degenerate-input cases under the documented contract.
- Spring-damper: check extension sign, equal-and-opposite force conventions where applicable, damping direction, and zero-extension behavior.
- Lumped thermal state: check reference-temperature energy, positive/negative energy transfers, and the absolute-zero boundary.
- Shallow-water and wave models: check still-water or zero-amplitude states, boundary handling, finite outputs, and the documented stability restrictions.
- Volumetric terrain deposition: verify that requested deposited volume is accounted for by the height-field change within the model's stated discretization.
- Logical time: ensure planning does not commit time, stale batches are rejected, and pending ticks are not silently discarded.

These are suggested validation targets, not assertions that every listed edge case is currently covered. Consult the focused tests to see the exact implemented coverage.

### Conservation and residuals

For each balance quantity (Q), define a residual with a sign convention, for example:

`residual = initial + inflow - outflow - final`

Adapt the equation to the quantity and model; do not use a generic balance formula blindly. Report both absolute and relative residuals when useful. Relative error is ill-conditioned when the reference magnitude is near zero, so state how the denominator or zero-reference case is handled.

For a discrete fluid model, distinguish physical conservation error from boundary flux and from intentional sources/sinks. For thermal state, distinguish energy stored from energy transferred. For a contact solver, distinguish impulse applied by the model from forces supplied by other constraints.

## Numerical error and stability

- State whether a method is analytical, explicit, implicit, semi-implicit, or another clearly named method.
- Record the units and meaning of timestep and grid spacing.
- Document known stability conditions or a conservative valid range when one is known.
- Test more than one resolution; a single successful run cannot establish convergence.
- Separate truncation/discretization error from floating-point roundoff, solver tolerance, and model-form error.
- Do not call a method “accurate” without naming the quantity, reference, range, and error measure.

A result can be numerically converged yet physically inaccurate because the governing model omits an important mechanism. Conversely, a sound model can produce poor results when stepped outside its numerical stability range. Document these as separate failure modes.

## Determinism and replay

A deterministic test should repeat the same declared inputs and compare all relevant observations. Choose one contract explicitly:

- exact serialized equality on the supported configuration;
- bitwise equality for a named runtime/platform set; or
- numerical equality within a declared tolerance.

Stable seeds alone are insufficient if event order or random-number consumption changes with execution order. Record the model/data versions and logical-step schedule. See [Determinism Contract](DETERMINISM.md) and [Deterministic History & Extensions](DETERMINISTIC_HISTORY_AND_EXTENSIONS.md).

## Validation of coupled capabilities

Coupling tests should verify:

1. The producer emits a named quantity with units, sign, timestamp/logical step, and source identity.
2. The consumer accepts only the supported quantity and range.
3. The exchange is applied once, not once per observer or render frame.
4. Processing order is explicit or the coupling algorithm is deliberately staged/iterated.
5. A delayed or missed update has a documented policy.
6. The combined result agrees with a reference case or balance check.
7. Failure in one capability does not silently commit incomplete time or partially publish an invalid result.

Do not use a coupled showcase as the only test of its component solvers. Keep small focused tests and then add integration tests.

## Documentation and release gate

Before describing a model as implemented, ensure that:

- the source entry point is linked;
- assumptions, units, valid range, and omissions are documented;
- focused tests exist and pass in CI;
- at least one meaningful analytical, invariant, or independent-reference check is present where feasible;
- any known limitation is stated plainly;
- visual output is identified as an observation or illustration, not independent validation.

If the evidence is incomplete, say **prototype**, **reference model**, or **design contract** as appropriate. Honest scope is a feature: it lets future work extend a model without confusing a narrow success with general physical truth.

---

[Capability catalog](PHYSICS_CAPABILITY_CATALOG.md) · [Theory and evidence](THEORY.md) · [Determinism](DETERMINISM.md) · [Physics home](../README.md)

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
