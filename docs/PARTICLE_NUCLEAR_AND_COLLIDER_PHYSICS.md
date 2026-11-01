![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Particle, Nuclear, and Collider Physics

**Status:** domain map and staged implementation direction. This chapter does not claim that particle transport, detector simulation, or a collider event generator is implemented.

**Purpose:** establish a rigorous path from relativistic mathematics and particle identity through collision kinematics, event records, detector response, reconstruction, and evidence-backed scientific experiences.

## 1. What this domain studies

Particle physics investigates the fundamental constituents of matter and their interactions. Nuclear physics studies atomic nuclei, their structure, transformations, and interactions. Collider science brings beams of particles into controlled collisions and uses detectors and statistical analysis to infer what occurred.

A collider event is not merely two dots colliding. It is a chain of distinct models and measurements:

1. Define incoming particles and beam states.
2. Transform their momenta into the relevant reference frame.
3. Determine kinematically allowed outcomes and interaction probabilities.
4. Generate or observe an event under a declared physical model.
5. Transport outgoing particles through a detector model or real apparatus.
6. Convert detector interactions into signals and reconstructed tracks/objects.
7. Apply selection and statistical analysis.
8. Compare with reference predictions and quantify uncertainty.

Each stage has its own assumptions, inputs, outputs, validation, and evidence status. A kinematic event display must not be described as a detector simulation. A simulated event must not be described as an experimental observation.

## 2. Relativistic kinematics: the mathematical foundation

Use a declared metric convention consistently. For the common high-energy-physics convention with metric signature ((+,-,-,-)), a four-momentum is

[
p^\mu = \left(\frac{E}{c}, p_x, p_y, p_z\right)
]

and its invariant mass satisfies

[
m^2c^2 = p_\mu p^\mu
= \frac{E^2}{c^2} - p_x^2-p_y^2-p_z^2.
]

Equivalently,

[
E^2 = (pc)^2 + (mc^2)^2.
]

When using natural units (c=1), the equations simplify, but the unit convention must be stated at every API and serialization boundary. The library should not silently mix SI and natural units.

### Invariant mass of a system

For a collection of particles, sum their four-momenta:

[
P^\mu = \sum_i p_i^\mu.
]

The invariant mass of the system follows from

[
M^2c^2 = P_\mu P^\mu.
]

This quantity is independent of the inertial frame. It is central to reconstructing resonances and describing the available center-of-mass energy.

### Lorentz transformations

A Lorentz transformation changes the components of a four-vector between inertial frames while preserving the Minkowski inner product. The implementation must document its boost direction and sign convention, test inverse transformations, test composition, and verify invariant preservation within scale-aware numerical tolerances.

### Two-body decay

For a parent particle with a known four-momentum decaying into two daughters, conservation requires

[
p_{\mathrm{parent}}^\mu =
p_1^\mu+p_2^\mu.
]

A first teaching model should solve a rest-frame two-body decay analytically, then apply a Lorentz boost to the daughters. This gives hand-checkable expected values and tests for energy-momentum conservation without pretending to model the underlying interaction dynamics.

## 3. Collision energy and event rates

For two incoming particles with four-momenta (p_1) and (p_2), the center-of-momentum invariant is derived from

[
s c^2 = (p_1+p_2)_\mu(p_1+p_2)^\mu,
]

where (sqrt{s}) is the total center-of-mass energy in the usual particle-physics convention.

Cross section, integrated luminosity, and event count are related in expectation by

[
N = \sigma \mathcal{L}_{\mathrm{int}},
]

subject to acceptance, efficiency, branching fractions, backgrounds, and the precise definition of the selected event sample. This is an expected-rate relationship, not a promise that a particular run produces an exact count.

A future implementation must keep physical cross sections, luminosity units, detector acceptance, reconstruction efficiency, and statistical uncertainty as distinct inputs/results.

## 4. Domain map

| Branch | Required knowledge and models | Evidence required before claiming implementation |
|---|---|---|
| Particle identity | Particle/antiparticle identity, mass, charge, spin, quantum numbers, lifetime and decay channels | Versioned sourced data, licensing, units, uncertainty and validity |
| Relativistic mathematics | Four-vectors, metric convention, Lorentz boosts, invariant mass, center-of-momentum frame | Independent identities, inverse/composition tests, limiting cases |
| Collision kinematics | Beam states, thresholds, scattering and decay kinematics | Hand-computable cases and four-momentum residuals |
| Nuclear physics | Isotopes, binding energy, decay, fission/fusion, reaction channels | Source-grounded nuclear data and model-specific validation |
| Interaction/event models | Declared interaction model, cross sections, branching fractions, stochastic sampling | Reproducible seeds, reference distributions, model validity and uncertainty |
| Detector geometry | Layered geometry, materials, magnetic field and sensitive volumes | Explicit geometry/material contracts and independent geometric tests |
| Detector response | Energy loss, scattering, ionization, calorimetry, timing and signal formation | Calibrated response models and reference comparisons |
| Reconstruction | Track fitting, vertexing, calorimeter clustering, particle identification | Resolution/efficiency studies and quantified uncertainty |
| Analysis | Event selection, histograms, background estimates, statistical inference | Reproducible datasets, uncertainty and provenance |
| Living event displays | Collision vertices, tracks, detector layers, energy deposits and reconstructed objects | Generated from the exact event record or detector output being shown |

## 5. Event record and provenance

A collision/decay event record should be immutable and versioned. At minimum it should identify:

- schema version and stable event ID;
- event classification and declared reference frame;
- particle IDs linked to a versioned property source;
- incoming and outgoing four-momenta with explicit units and metric convention;
- parent/daughter relationships and vertex positions where modeled;
- model/generator identity and version;
- random seed and sampling configuration where stochastic generation is used;
- detector model and conditions when detector simulation is included;
- source dataset, licensing, provenance and uncertainty where real data is involved;
- conservation residuals and validation status.

A missing field must not be filled with a plausible guess. Distinguish generated values, measured values, reconstructed values, and derived values.

## 6. Living pictures and scientific honesty

The first event visualization should show an analytically defined kinematic event: incoming four-momenta, the collision/decay vertex, outgoing directions, and conservation checks. Its geometry and labels must be generated from the recorded event, not separately authored to look convincing.

A later detector view may add layers, magnetic bending, tracking hits, calorimeter deposits, and reconstructed objects only when those outputs are produced by explicit detector-response and reconstruction models.

For every visual, expose:
- evidence label: conceptual, analytical, model-backed, detector-simulated, or real-data;
- model/event/schema version and run or dataset identity;
- axes, units, frame, and scale;
- assumptions, omissions, and uncertainty;
- links to theory, source, tests, and source data.

## 7. Staged implementation

The tracked implementation sequence is [issue #43](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/43):

1. Source-backed particle identity and property vocabulary.
2. Four-vector and Lorentz-transform primitives.
3. Versioned event records.
4. One analytically solvable two-body decay or scattering case.
5. Living SVG generated from the event record and checked by CI.
6. Detector geometry and response as separate capabilities.
7. Reconstruction, statistical analysis, and reference-data comparisons.

This sequence deliberately starts with mathematics and conservation laws. A polished event display is not a substitute for those foundations.

## 8. Architecture boundaries

The Physics package owns particle and nuclear quantities, physical models, mathematical operations, event state, numerical methods, diagnostics, and evidence records. It does not own a GUI, renderer, Unity integration, WebPage experience, or general workflow orchestration. Existing Workshop presentation packages may consume the generated records and images.

## Further reading

- [Physics and mathematics branch atlas](PHYSICS_BRANCH_ATLAS.md)
- [Mathematics domain map](MATHEMATICS_DOMAIN_MAP.md)
- [Equations, derivations, and scientific experiences](EQUATIONS_DERIVATIONS_AND_SCIENTIFIC_EXPERIENCES.md)
- [Numerical validation playbook](NUMERICAL_VALIDATION.md)
- [Particle, nuclear, and collider implementation issue](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/43)
- [Mathematics implementation issue](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/44)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
