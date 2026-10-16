# Lookup Tables for Physics, Mathematics, and Visual Resolution

**Purpose:** define where lookup tables (LUTs) belong in the Workshop physics stack, how they complement analytical mathematics, and how Castle Siege can use them without turning approximations into hidden truth.

**Status:** architecture/design contract. This document does not claim a general LUT runtime or a Math MicroBundle has been located or implemented. The Math MicroBundle's exact repository/package remains to be identified from the installed ecosystem before any dependency is added.

Related: [Castle Siege Physics Domain](CASTLE_SIEGE_PHYSICS_DOMAIN.md) · [Experiment Catalog](CASTLE_SIEGE_EXPERIMENT_CATALOG.md) · [Physics classes](PHYSICS_CLASSES.md) · [Living theory](LIVING_THEORY.md).

## 1. The relationship: mathematics describes, physics instantiates

Mathematics provides the relationships, representations, solvers, interpolation rules, and error bounds. Physics defines which quantities exist in a model, how they interact, what state changes, and which observations count as evidence. They are distinct but mutually informative:

- A falling apple motivates a mathematical model of acceleration.
- The model predicts velocity and position.
- A wall impact asks for a model of momentum transfer, deformation, fracture, or another response.
- Observations and controlled experiments can expose where the current mathematical model is inadequate.
- The revised model becomes testable again.

A LUT is one way to represent a relationship that is expensive, complex, empirically measured, or deliberately approximated. It is not a substitute for defining the relationship or its domain of validity.

## 2. When a LUT is appropriate

Use a LUT when a bounded input domain and an explicit error budget make table evaluation valuable. Candidate Castle Siege uses include:

- trigonometric values for bounded angle ranges where a measured approximation is acceptable;
- drag or aerodynamic coefficients as a function of declared dimensionless/physical parameters, once the model and data source are specified;
- material response curves based on a named, versioned, measured or explicitly illustrative source;
- deformation or breakup response approximations for authored payload classes;
- visual debris/splatter appearance parameters derived from physical event magnitude, while keeping the presentation mapping non-authoritative;
- LOD/resolution selection, where a table maps distance, relative scale, speed, or observer relevance to an update/render policy;
- expensive repeated transforms or mechanism response mappings after profiling demonstrates the cost.

Do not introduce a LUT merely because it sounds faster. The analytical formula may already be cheap, the domain may be too wide, interpolation may cost more than evaluation, or the approximation error may be unacceptable.

## 3. Four distinct table families

Keep these table types separate in both documentation and serialized metadata.

| Family | Contents | Authority | Example |
|---|---|---|---|
| Mathematical/numerical | sampled function values or coefficients | model-specific numerical approximation | sine/cosine table over a bounded angle interval |
| Physical constitutive/empirical | measured or calibrated material/force response | authoritative only inside declared model validity | response curve for a named material model |
| Content/ontology profile | asset parameters and supported physical model choices | input description, not a solver | pumpkin mass/size range and supported response profile |
| Presentation/LOD | rendering, effect, or update-resolution selection | never physical truth | choose a debris visual tier by energy band and viewing distance |

Do not let a presentation LUT determine hit/miss, mass, momentum, structural failure, or persistent world state. A visual table can choose how to depict an already-established event; it cannot create the event.

## 4. Required LUT envelope

Every LUT used in a physical result should be wrapped in a versioned descriptor. Field names here are a proposal, not existing API declarations.

- **Identity:** stable table ID, schema version, model/version and content digest.
- **Purpose:** what relationship is approximated and which model consumes it.
- **Inputs:** named variables, SI units, dimensions, coordinate convention and valid ranges.
- **Sampling:** axis values, uniform/nonuniform spacing, stored values and any coefficient representation.
- **Evaluation:** interpolation method, boundary behavior, extrapolation policy and treatment of invalid inputs.
- **Evidence:** source/provenance, calibration dataset or analytical reference, generation method and licensing where applicable.
- **Error:** maximum/typical error, measurement uncertainty where known, tolerance and validation method.
- **Determinism:** stable table bytes/version, defined interpolation order and supported floating-point expectations.
- **Fallback:** reject, clamp, use a declared analytical fallback, or return an explicit unsupported status. Never silently extrapolate outside a physical table's validated range.
- **Ownership:** Physics owns authoritative physical LUTs; Math may own general numerical primitives and table-evaluation algorithms; ontology/content bundles describe asset profiles; WebPage/Renderer may own presentation-only tables.

A table update changes the effective model. It must change a version or content digest so experiment runs can identify the exact table that produced them.

## 5. Interpolation and numerical policy

The implementation must select interpolation based on the represented relationship, not a global convenience default.

- Nearest-neighbor is appropriate only for discrete categories or deliberate quantization.
- Linear interpolation is simple and transparent for sufficiently smooth, bounded relationships.
- Higher-order interpolation may improve smoothness but can overshoot, violate monotonicity, or create nonphysical negative values.
- Monotone interpolation may be preferable for calibrated curves that must remain monotonic.
- Multidimensional tables need declared axis ordering, dimensionality, memory layout, and a tested interpolation strategy.
- Extrapolation should default to rejection for physical response tables unless the model explicitly defines another safe behavior.
- Quantized output should declare quantization error; deterministic evaluation does not by itself imply accurate evaluation.

Tests should compare table results against analytic or trusted reference samples, probe endpoints and between-sample locations, cover invalid/out-of-range inputs, verify deterministic repeated reads, and measure error across the supported domain. Store golden vectors where they help catch accidental table regeneration changes.

## 6. Math MicroBundle boundary

The Math MicroBundle should be considered a candidate provider of reusable mathematical capabilities, not an assumed package dependency. The exact bundle identity and contract must be verified first.

Potential responsibilities for a general Math capability bundle:
- stable vector/matrix/quaternion and coordinate transforms where these are not already owned by the core Physics contract;
- bounded interpolation and LUT construction/evaluation;
- numerical integration and root/optimization methods where justified;
- error measurement, dimensional checks and reference comparisons;
- deterministic sampling/curve tools;
- generated or supplied tables with versioned provenance.

Physics should own physical meanings and model policy: gravity, impact, contact, materials, fragmentation, fluid/air interactions and their validity limits. It can consume generic Math primitives without outsourcing the definition of physics to a generic utility bundle.

Before connecting them, inventory actual types and packages to avoid duplicating existing primitives. Preserve the current NuGet physics kernel's host independence. A runtime MicroBundle relationship must follow the actual MicroBundleDomain/FSM_COS contract; do not add a package reference just to express a conceptual relationship.

## 7. Castle Siege application: from a launch to many consequences

A LUT may assist a model, but the event chain remains explicit:

1. A validated payload profile supplies mass, dimensions, shape and supported physical/material model choices.
2. A launch mechanism or explicit launch-state experiment supplies position and velocity.
3. The trajectory model computes state and samples.
4. Target intersection confirms whether and where contact occurs.
5. Impact accounting computes supported quantities such as incoming momentum and kinetic energy.
6. A payload response model decides whether the payload rebounds, deforms, breaks, bursts, disperses, or has an unsupported response.
7. A wall response model independently decides how the target changes.
8. Fragment/particle/body initialization creates new physical entities only when supported.
9. Settling and contact simulation advance those entities, including small secondary movements or a bounded rubble/fragment avalanche.
10. Presentation LUTs choose effects for the computed event; they do not decide the physical outcome.

This separation is essential for an apple, pumpkin, stone, clay object, water-filled vessel, or future user-defined asset to share the same launch/trajectory machinery while having different impact and breakup responses.

## 8. Resolution, cost, and error budgets

A LUT is only one possible optimization. Compare it with analytical evaluation and a reference implementation before adopting it.

For each candidate, record:
- table size and memory footprint;
- construction/loading cost;
- evaluation throughput and end-to-end experiment cost;
- interpolation error over the declared input domain;
- determinism/repeatability;
- error accumulation across repeated timesteps or coupled events;
- effect on conservation or physical invariants where relevant;
- behavior at the limits of the table's valid range.

The LOD system must remain separate from physical truth. A distant fragment may be represented by a coarser aggregate or advanced at a lower cadence only if the handoff has a defined error budget and preserves the state needed on re-observation. Rendering LOD can simplify geometry without changing the simulation. Physical LOD needs a model and reconciliation test.

## 9. Acceptance gates

- [ ] Exact Math MicroBundle repository/package and current contract identified before any dependency.
- [ ] Each proposed table classified as mathematical, physical/empirical, ontology/content, or presentation/LOD.
- [ ] Every physical table has provenance, units, validity bounds, interpolation policy, version/digest, and error evidence.
- [ ] Out-of-range behavior is explicit; no silent extrapolation.
- [ ] Tests compare LUT results with a trusted reference and probe endpoints/interior samples.
- [ ] Table version/digest is captured in experiment/run provenance.
- [ ] Presentation tables cannot change authoritative physical outcomes.
- [ ] Profiling demonstrates why the LUT is worth its storage and error cost.
- [ ] Physics remains renderer- and host-independent.
- [ ] Any MicroBundle composition uses actual domain/runtime contracts, not a conceptual package reference.

## 10. Recommended first LUT experiment

Start with a small bounded numerical table whose analytic reference is known. Compare direct evaluation with LUT evaluation at sample points and randomized in-range points; measure error, throughput, and memory. Then test one empirical/material-response curve only after a defensible source or explicitly illustrative profile is chosen. Do not start by placing every equation into a table.
