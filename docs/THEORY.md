# Theory: Explain What the Model Can Prove

The Workshop's physics documentation should help readers move between a physical question, its mathematical model, an experiment, measured results, and the limits of the conclusion. This is an evidence contract, not a claim that theory generation is already implemented.

## A theory record

Every model-backed explanation should identify:
- **Question:** what is being investigated?
- **Model:** which governing relationship or algorithm is used?
- **Assumptions:** what is held constant, idealized, omitted, or approximated?
- **Domain:** supported units, ranges, geometry, phases, and boundary conditions.
- **Inputs:** initial state and external conditions.
- **Prediction:** expected outcome and derivation.
- **Observation:** actual output of the recorded run.
- **Residual:** difference between prediction, reference, and computed result.
- **Evidence:** source, edition/version, section/page or dataset identifier, license, and retrieval details.
- **Limitations:** what the result does not establish.

## Types of statement

Keep these distinct:
1. **Definition** — a convention chosen by the system.
2. **Established relationship** — a recognized law/model, with conditions and provenance.
3. **Measured datum** — a reported measurement with source and uncertainty where available.
4. **Derived result** — computed from declared inputs and equations.
5. **Simulation output** — produced by a particular numerical method/configuration.
6. **Assumption** — a simplification introduced by the experiment.
7. **Hypothesis** — a claim the experiment is designed to test.
8. **Interpretation** — an explanation beyond direct measurement, labeled accordingly.

A visually persuasive result is not, by itself, a validated physical result.

## First implemented evidence link

The constant-acceleration experiment runner records the initial state and each resulting state sample. Its JSON run record includes the model identifier/version, record-schema version, input definition, explicit SI units, all samples, completed step count, and final observed state. Tests verify the record comes from actual experiment output and that serializing the same record twice yields identical text.

This first format is intentionally narrow. It does not yet include external reference sources, source-data licenses, uncertainty estimates, model build hashes, independent reference comparisons, or a generated narrative with citations. Those are future additions, not implied capabilities.

## Current executable example: 3D projectile state step

The repository now contains a small analytical projectile-state step, implemented in [`ProjectileState.cs`](../Physics/Kinematics/ProjectileState.cs) and exercised by [`ConstantAccelerationProjectileSolverTests.cs`](../tests/TheSingularityWorkshop.Physics.Tests/ConstantAccelerationProjectileSolverTests.cs). It advances a point state under constant acceleration; it does not yet produce a sampled trajectory record.

For initial position `x₀`, velocity `v₀`, constant acceleration `a`, and positive timestep `Δt`, the model is:

\[
\mathbf{x}_{1}=\mathbf{x}_{0}+\mathbf{v}_{0}\Delta t+\frac{1}{2}\mathbf{a}(\Delta t)^2
\]

\[
\mathbf{v}_{1}=\mathbf{v}_{0}+\mathbf{a}\Delta t
\]

The source uses `Vector3d` for position, velocity, and acceleration. The state documents position in metres, velocity in metres per second, acceleration in metres per second squared, and elapsed logical time in seconds. The caller owns the coordinate-frame convention; one test chooses Z-up and applies gravity as `(0, 0, -9.81) m/s²`.

The focused tests establish the following bounded claims:

- One three-dimensional example matches hand-computed analytical component values within `1e-12`.
- One hundred fixed 0.01-second steps agree with a single 1-second step within the test's tolerance for the checked state components.
- Invalid elapsed time, invalid timestep values, and elapsed-time overflow are rejected.

These checks do **not** validate the chosen gravity value against a measurement, a launch mechanism, aerodynamic drag, collision, target intersection, impact, or structural damage. The next evidence-bearing increment is a versioned trajectory experiment that records inputs and sampled states, with its own validation contract.

## Generated explanations

Living theory should be generated from versioned model metadata and actual experiment records, not from visual appearance alone. An explanation can show equations and symbol definitions, labeled diagrams with units, interactive parameter changes, expected/observed curves, conservation checks, numerical residuals, references, licenses, and explicit “not modeled” disclosures.

When an experiment changes model version, make the change visible and retain the lineage of prior runs.

## Reference discipline

Record enough provenance to identify the source used. Do not copy restricted textbook prose, diagrams, or datasets without permission. Summaries, independently authored diagrams, and properly licensed/open data should preserve attribution and terms.

## The living document loop

**Model → Run → Measure → Compare → Explain → Review → Version**

Generated documentation can make evidence inspectable; it cannot convert weak assumptions into strong science.

---

## Resources & Support

[Physics home](../README.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [Workshop ontology](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) · [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="200" />
</p>

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
