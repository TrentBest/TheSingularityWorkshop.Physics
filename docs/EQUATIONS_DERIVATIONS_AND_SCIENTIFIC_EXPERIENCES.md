# Equations, Derivations, and Scientific Experiences

**Status:** living catalog and content contract. The equations below are introductory reference material, not a claim that every listed relationship has a solver implementation in this package.

**Purpose:** make an equation a navigable, verifiable entry point into its mathematics, physical meaning, derivation, history, evidence, implementation, and teachable experience. The catalog should grow across physics and mathematics without pretending one solver or one document can do everything.

## The central idea

An equation is a compact statement about relationships. It is not, by itself, a complete physical model. A usable entry must also say what its symbols mean, what units apply, what assumptions are made, where the relationship is valid, how it can be derived or motivated, what evidence supports it, and which implementations (if any) have been tested.

Every entry is a composable knowledge object. The same equation can be used by a textbook experience, a simulation, a classroom exercise, a historical biography, or an engineering tool without duplicating the underlying source material.

## A knowledge graph, not a flat formula sheet

A typical chain looks like this:

`Scientist ↔ Work ↔ Claim / Equation ↔ Definitions ↔ Derivation ↔ Assumptions ↔ Model ↔ Solver ↔ Experiment ↔ Observation ↔ Explanation`

These links are many-to-many. Newton did not invent every idea associated with classical mechanics alone; historical development involved predecessors, collaborators, contemporaries, disputes, and later reinterpretations. Attribute specific claims carefully and distinguish historical context from modern notation.

The intended packaging distinction is:

- **Physics NuGet:** host-independent physical quantities, equations, model contracts, bounded implementations, experiment records, validation, and provenance.
- **MicroBundles:** distributable compositions of content and capabilities for a particular lesson, person, experiment, or interactive experience.
- **Experience/presentation layer:** the interface that helps a person explore, change inputs, ask questions, and inspect evidence. It must not become the source of physical truth.
- **Solver:** a replaceable implementation that resolves a well-specified mathematical problem. An equation entry must not claim that a solver exists merely because the formula is documented.

## Equation entry contract

Each equation should eventually have a stable identifier and machine-readable metadata. Human-readable pages can be generated from the same source records so the formula, explanation, tests, and links do not silently drift apart.

Required fields:

1. **Identity:** stable ID, title, version, aliases, domain, and relationships to other entries.
2. **Statement:** canonical expression plus equivalent forms where meaningful.
3. **Symbol table:** every symbol, its meaning, quantity kind, SI unit, sign convention, coordinate/frame convention, and whether it is scalar, vector, tensor, or function.
4. **Scope:** assumptions, boundary/initial conditions, valid regime, exclusions, and known failure modes.
5. **Derivation:** numbered steps with each transformation justified; identify axioms, definitions, empirical laws, approximations, and mathematical theorems separately.
6. **Interpretation:** plain-language explanation, dimensional analysis, limiting cases, and physical intuition.
7. **History:** people, works, dates where substantiated, attribution nuance, primary-source links, and reputable secondary references.
8. **Evidence:** experiment or observation, data source, provenance, uncertainty, comparison method, and evidence level.
9. **Executable links:** model ID, solver capability, experiment definition, test suite, output schema, and implementation status. Omit links that do not yet exist.
10. **Experience links:** lessons, visual explanations, exercises, biography, simulations, translations, and reusable MicroBundles.

Suggested evidence labels: **conceptual**, **analytical**, **model-backed**, **reference-compared**, and **validated within stated scope**. Never promote a diagram or illustration to solver output. Never label a model “validated” without naming the comparison and its limits.

## Worked entry: Newton's second law

### Canonical relationship

For a body of constant mass in an inertial reference frame, the familiar introductory form is:

$$
\mathbf{F}_{\text{net}} = m\mathbf{a}
$$

where:

- \(\mathbf{F}_{\text{net}}\) is the vector sum of external forces, measured in newtons (N).
- \(m\) is the body's mass, measured in kilograms (kg).
- \(\mathbf{a}\) is acceleration, measured in metres per second squared (m/s²).

The units are consistent because \(1\,\mathrm{N}=1\,\mathrm{kg}\cdot\mathrm{m}/\mathrm{s}^2\).

This is a useful form for many classical-mechanics problems. It is not the most general way to state momentum change: for systems with changing mass or in contexts where momentum is the natural quantity, use a carefully defined momentum balance.

### Momentum form and a derivation path

A common modern statement of the law is:

$$
\mathbf{F}_{\text{net}} = \frac{d\mathbf{p}}{dt}
$$

For a body with constant mass, with \(\mathbf{p}=m\mathbf{v}\), and with mass constant during the differentiation:

$$
\frac{d\mathbf{p}}{dt}
= \frac{d(m\mathbf{v})}{dt}
= m\frac{d\mathbf{v}}{dt}
= m\mathbf{a}
$$

Therefore, under those stated conditions:

$$
\mathbf{F}_{\text{net}}=m\mathbf{a}
$$

This derivation depends on the definitions of momentum and acceleration and on the constant-mass assumption. It does not mean that every problem involving variable-mass systems can be solved by blindly substituting a changing mass into \(F=ma\); the system boundary and momentum flux must be accounted for.

### Solve for different unknowns

When the model assumptions hold and mass is nonzero:

$$
\mathbf{a}=\frac{\mathbf{F}_{\text{net}}}{m}
\qquad
m=\frac{\lVert\mathbf{F}_{\text{net}}\rVert}{\lVert\mathbf{a}\rVert}
\]

The second rearrangement is only useful when the acceleration is nonzero and force and acceleration are aligned as expected in this model. In general, the vector equation and all known constraints must be respected; component-wise division is not a universal operation.

### A thought experiment

Hold mass constant and double the net force: the predicted acceleration doubles. Hold net force constant and double mass: the predicted acceleration halves. If the net force is zero, acceleration is zero in this model, meaning velocity remains constant—not necessarily that the body is at rest.

These are predictions from the stated model, not experimental measurements. An experience should make that distinction visible.

### A minimal computational model

A bounded implementation can calculate acceleration from a declared net force and positive mass. It should validate finite inputs, preserve units/conventions at API boundaries, and expose its assumptions. A time integrator that advances position and velocity is a separate capability with its own step-size behavior and tests. Neither implementation should silently imply friction, drag, gravity, or contact forces: those enter only when explicitly included in the net force or coupled model.

**Implementation status:** this page documents the relationship and derivation. Consult the [capability catalog](PHYSICS_CAPABILITY_CATALOG.md) and linked source/tests before claiming a particular solver or experiment implements it.

## The scientist is a connected experience

For a Newton experience, the scientist record should link out to a broader body of material rather than attempting to cram an entire biography into the equation page:

- **Person record:** identity, chronology, major works, scientific context, and carefully sourced biographical claims.
- **Work record:** a particular text or edition, publication context, original language where relevant, and a stable access link.
- **Contribution record:** the specific claim, equation, method, or concept associated with the work, including nuance and prior contributions.
- **Equation record:** the mathematical statement, derivation, assumptions, and present-day interpretation.
- **Experiment record:** the question, apparatus/model, independent and dependent variables, controls, observations, and uncertainty.
- **Experience bundle:** a selected path through the person, work, equation, experiment, and related concepts, with interactive choices and appropriate reading level.

Possible starting points for research (source availability and individual pages should be checked when creating a specific record):

- [The Newton Project, University of Oxford](https://www.newtonproject.ox.ac.uk/) — editions and scholarly material relating to Newton's writings.
- [MacTutor History of Mathematics Archive](https://mathshistory.st-andrews.ac.uk/) — historical context for mathematicians and mathematical ideas.
- [Encyclopaedia Britannica: Isaac Newton](https://www.britannica.com/biography/Isaac-Newton) — a secondary biographical overview.

These links are starting points, not substitutes for citing the specific work or passage that supports a historical claim. Preserve title, author/editor, edition, publication date where known, URL or persistent identifier, access date where useful, and license/usage restrictions. Do not assume all historical texts or scans are public domain merely because they are online.

## From one equation to a learning path

A well-formed experience could let a learner:

1. Meet Newton in a sourced historical context.
2. Read a plain-language statement and inspect every symbol and unit.
3. Expand the derivation one justified step at a time.
4. Change force and mass in a controlled experiment.
5. See predicted acceleration, the assumptions used, and any numerical integration error separately.
6. Compare with a documented observation or reference dataset when one is available.
7. Follow links to momentum, inertial frames, gravity, friction, energy, and other connected entries.
8. Save/share the lesson as a composition of independently versioned MicroBundles.

The learner should always be able to ask: “What did I change? Which model produced this result? Which assumptions apply? Is this a derivation, a simulation, measured data, or an illustration?”

## Extending the catalog across mathematics and physics

The catalog is intended to cover much more than mechanics. A domain list is an index to build, not a claim that all its equations or solvers already exist.

- **Mathematical foundations:** arithmetic, algebra, functions, geometry, trigonometry, vectors, matrices, complex numbers, calculus, differential equations, linear algebra, probability, statistics, numerical analysis, optimization, dimensional analysis, and uncertainty.
- **Mechanics and motion:** kinematics, Newtonian dynamics, momentum, angular momentum, work, energy, rotation, oscillation, elasticity, contact, friction, and continuum mechanics.
- **Thermodynamics and heat transfer:** equations of state, first and second laws, entropy, heat capacity, conduction, convection, radiation, phase equilibrium, latent heat, and transport.
- **Fluids and hydrodynamics:** continuity, momentum balance, hydrostatics, shallow-water approximations, viscosity, wave propagation, turbulence models, buoyancy, and fluid-structure interaction.
- **Electricity and magnetism:** charge, current, voltage, circuits, electric and magnetic fields, induction, semiconductor junctions, and Maxwell's equations.
- **Waves, acoustics, and optics:** wave equations, interference, resonance, sound propagation, reflection, refraction, diffraction, and geometric optics.
- **Materials, chemistry, and reactive systems:** constitutive laws, diffusion, species balance, reaction kinetics, thermochemistry, combustion, and material failure.
- **Further fields:** quantum, statistical, nuclear, particle, astrophysical, geophysical, and biological models—each with their own regimes, evidence, and implementation boundaries.

Each subject should grow from a navigable concept map and equation inventory into individually sourced entries, derivations, bounded models, experiments, tests, and teaching experiences. Do not paste a large unreviewed equation dump and call the domain complete.

## Source and correctness policy

- Prefer primary sources for original claims and authoritative technical references for modern formulations.
- Attribute discoveries carefully; a formula's modern notation may postdate the person associated with it.
- Keep original historical wording distinct from modern explanatory paraphrase.
- Record units, assumptions, sign conventions, and domains of validity.
- Distinguish exact identities, idealized models, empirical fits, approximations, numerical methods, and hypotheses.
- Attach provenance to datasets and material properties; cached content and model knowledge can help discover leads but do not establish evidence.
- Make derivations reviewable. A citation to a whole book is not enough when a particular page, theorem, or passage is available.
- Every executable claim should point to code and tests. A formula documented here is not automatically implemented, numerically stable, or validated.

## Next increments

1. Define a versioned, machine-readable equation entry schema with unit-aware symbol metadata and source records.
2. Link existing Physics experiments and models to equation IDs without misrepresenting their scope.
3. Build the first small equation set for kinematics, momentum, work/energy, and thermal energy accounting.
4. Generate an accessible living diagram from structured relationships, with conceptual/model-backed/validated status visibly distinguished.
5. Add sourced person and work records as separate reusable content bundles.
6. Let experiences compose those records into lessons; keep the core NuGet free of GUI, web, and host-specific dependencies.

The long-term aim is not a static encyclopedia. It is a body of knowledge where **the equation, its reasoning, its history, its executable model, and its teaching experience remain connected while staying independently reusable**.
