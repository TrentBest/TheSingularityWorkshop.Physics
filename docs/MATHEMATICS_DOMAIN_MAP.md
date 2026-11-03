<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# Mathematics Domain Map

**Status:** domain architecture and implementation inventory. The breadth below is the target vocabulary, not a claim that every branch has an implementation or solver.

Mathematics is not a separate product or package in this architecture; it lives inside TheSingularityWorkshop.Physics as the shared language for physical laws, geometry, uncertainty, numerical algorithms, optimization, measurements, and computational cost. Other domains may consume a suitable contract without requiring a separate Math library.

## Mathematical domains

| Domain | Core concepts | First useful reusable capabilities | Status in this repository |
|---|---|---|---|
| Arithmetic and algebra | scalars, expressions, equations, inequalities, polynomials, roots, systems | stable scalar operations; explicit domains; equation records and symbolic identities | Partial: scalar arithmetic is used throughout; no symbolic algebra engine |
| Geometry | points, distances, lines, planes, intersections, regions, coordinate frames | 2D/3D primitives, predicates, transforms, tolerances and degeneracy policies | Partial: [Vector2d](../Physics/Spatial/Vector2d.cs) supplies finite frame-agnostic 2D vector arithmetic; [CoordinateFrame2d](../Physics/Spatial/CoordinateFrame2d.cs) supplies caller-defined frame identity and [Point2d](../Physics/Spatial/Point2d.cs) tags coordinates so cross-frame comparisons can be rejected; [RigidTransform2d](../Physics/Spatial/RigidTransform2d.cs) implements explicit source/destination-frame rotation and translation, inverse, direction transform, and ordered composition; [Geometry2d.Orient](../Physics/Spatial/Geometry2d.cs) robustly classifies represented binary64 point triples; [`Geometry2dSegments.ClassifyIntersection`](../Physics/Spatial/Geometry2dSegments.cs) classifies closed-segment intersections; and [`GeometryTolerance2d`](../Physics/Spatial/GeometryTolerance2d.cs) supplies an explicit caller-selected absolute Euclidean-distance comparison; [`Geometry2dSegments.Intersect`](../Physics/Spatial/SegmentIntersection2d.cs) constructs bounded unique-point intersections; [`Geometry2dPolygon.LocatePoint`](../Physics/Spatial/Geometry2dPolygon.cs) classifies points as inside, outside, or boundary for simple polygons. Focused tests cover these slices. Transform graphs and unit conversion remain gaps |
| Procedural geometry and fractal data | bounded seeded samples, escape-time classification, recursive constructions, scalar fields, contours, meshes, scale-dependent detail | renderer-independent generation contracts, deterministic replay, bounded work/output, topology and error validation | Partial: [SeededPointSet2d](../Physics/Spatial/SeededPointSet2d.cs) implements bounded seeded 2D point sampling; [MandelbrotEscapeTime2d](../Physics/Spatial/MandelbrotEscapeTime2d.cs) implements a bounded, tested escape-time grid with explicit sample, iteration, and work caps. Both have focused tests and UML coverage in docs/UML. General scalar fields, contours/meshes, and fractal-to-physics coupling remain unimplemented; see [design contract](PROCEDURAL_GEOMETRY_AND_FRACTAL_DATA.md) |
| Linear algebra | vectors, matrices, bases, transforms, determinants, eigenvalues, decompositions | matrix types, solve/invert policies, conditioning diagnostics, batch operations | Partial: [Matrix3x3d](../Physics/Math/Matrix3x3d.cs) supports finite 3×3 construction, determinant, transpose, matrix/vector products; [SoA Vector3d batch operations](../Physics/Math/Vector3dBatch.cs) and [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/Math/Matrix3x3dTests.cs) exist. General matrix dimensions, solves, inverses and decompositions remain gaps |
| Calculus | limits, derivatives, integrals, gradients, divergence, curl, differential equations | finite differences with error policies; quadrature; derivative/gradient contracts; ODE integration | Partial: [`FiniteDifferences.CentralDifference`](../Physics/Math/FiniteDifferences.cs) provides a validated scalar first-derivative estimate with a caller-selected symmetric step; [`CentralDifferenceWithErrorEstimate`](../Physics/Math/FiniteDifferences.cs) applies second-order Richardson extrapolation and reports its correction magnitude as an asymptotic error indicator (not a rigorous bound). [`DerivativeDerivationRecord`](../Physics/Math/DerivativeDerivationRecord.cs) records the actual four function samples, coarse/fine secant slopes, named mathematical rules, assumptions, caller-declared unit labels, extrapolation correction, and estimate in a renderer-independent JSON contract. A deterministic C# generator and CI checks validate the contract against the analytical derivative of a quadratic. Focused polynomial, trigonometric, invalid-input, overflow, refinement, and derivation-contract tests exist. A reproducible twelve-step sine sweep is generated by `tools/DerivativeStepSizeSweepGenerator`, serialized at [derivative-step-size-sweep.run.json](experiments/derivative-step-size-sweep.run.json), and visualized in [a living logarithmic error plot](experiments/derivative-step-size-sweep.svg). It compares actual error against the analytical cosine derivative and plots the Richardson correction indicator separately (not a rigorous bound); CI validates the C# output and renderer reproducibility. [Quadrature.CompositeSimpson](../Physics/Math/Quadrature.cs) implements bounded composite Simpson integration with a positive even interval count and a one-million refined-grid work cap; [CompositeSimpsonWithErrorEstimate](../Physics/Math/Quadrature.cs) doubles the grid and reports the Richardson correction magnitude using the fourth-order factor 15 (an asymptotic indicator, not a rigorous bound). Focused tests cover polynomial and trigonometric analytical references, reversed and zero-width bounds, invalid inputs, non-finite samples, overflow, refinement, repeated grid coordinates, and evaluation counts. A deterministic exponential-decay integration record and living area-accumulation SVG are generated from actual C# samples and checked in CI ([record](experiments/quadrature-area.run.json), [visualization](experiments/quadrature-area.svg)). The theory chapter is [Calculus II — Composite Simpson Quadrature](CALCULUS_II_COMPOSITE_SIMPSON_QUADRATURE.md). Multivariable calculus and ODE toolkit remain gaps; see [Calculus I — Central Finite Differences](CALCULUS_I_CENTRAL_FINITE_DIFFERENCES.md) |
| Discrete mathematics | sets, relations, graphs, combinatorics, recurrence, logic | stable graph/identity primitives, graph traversal, combinatorial counts | Partial: timeline and grid concepts exist; not a general discrete math library |
| Probability | events, distributions, conditional probability, sampling | distribution contracts separated from deterministic pseudorandom source; reproducible samples | Partial: [Squirrel3 stream](../Physics/Math/Squirrel3Random.cs) and [keyed sampler](../Physics/Math/Squirrel3Keyed.cs) exist; distribution library is a gap |
| Statistics | samples, estimators, moments, regression, confidence intervals, hypothesis tests | online mean/variance, covariance, quantiles, fit diagnostics, interval assumptions | Partial: [single-pass Welford mean and population/sample variance](../Physics/Math/OnlineStatistics.cs) is implemented with [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/Math/OnlineStatisticsTests.cs); covariance, quantiles, regression, and inference remain gaps |
| Numerical analysis | floating-point error, stability, convergence, interpolation, root finding | tolerances, residuals, error estimates, condition numbers, deterministic reference tests | Partial: [bounded 1D LUT](../Physics/Math/LookupTable1D.cs) with linear interpolation exists; general toolkit is a gap |
| Optimization | objective/constraint models, local/global search, least squares | objective contracts, stopping criteria, residuals, reproducible initial samples | Gap: no general optimization API claimed |
| Information and signal mathematics | Fourier transforms, convolution, filtering, spectral analysis | signal/sample contracts, windowing conventions, reference cases | Gap: acoustic models exist, but no general signal-processing suite is claimed |
| Graph and tensor methods | sparse/dense graphs, tensors, tensor contractions, automatic differentiation | explicit shape/layout contracts and backend-neutral operations | Gap: no tensor or autodiff runtime claimed |
| Number theory and exact arithmetic | integer domains, modular arithmetic, rational values, primality | exact rational/unit-conversion support where useful; overflow policy | Gap: not a general exact arithmetic package |

## Mathematical structures and methods that physics depends on

The domain rows above are a starting inventory, not an exhaustive boundary. The following subjects must remain visible in the atlas even when they are not yet represented by reusable code. They are distinct mathematical disciplines or method families; they should not be collapsed into a generic “advanced math” bucket.

| Subject | Core mathematical territory | Physical consumers and why it matters | Repository status |
|---|---|---|---|
| Dimensional analysis and metrology | quantities, dimensions, unit systems, scale analysis, uncertainty budgets, significant precision, measurement models | Every measured or simulated physical quantity; model validation, similarity laws, experimental comparison | Partial: unit labels and SI conventions occur in models and records; shared quantity/dimension and uncertainty contracts remain gaps |
| Complex analysis | complex variables, analytic functions, residues, contour integration, conformal maps | AC circuits, waves, oscillations, quantum mechanics, potential flow, Green-function methods | Gap: complex-number support is not yet a general complex-analysis toolkit |
| Functional analysis | normed/inner-product spaces, Hilbert and Banach spaces, linear functionals, convergence of functions, weak formulations | Quantum state spaces, PDE existence/weak solutions, continuum mechanics, variational methods | Gap |
| Operator theory and spectral theory | linear operators, adjoints, spectra, eigenfunctions, self-adjointness, resolvents, spectral decompositions | Quantum observables, normal modes, stability, wave equations, diffusion and field equations | Gap beyond small finite-dimensional matrix operations |
| Differential geometry | manifolds, tangent/cotangent spaces, metrics, connections, geodesics, curvature, coordinate charts | General relativity, curved configuration spaces, continuum kinematics, robotics-like physical constraints | Gap beyond Euclidean 2D/3D primitives and explicit rigid transforms |
| Tensor calculus | covariant/contravariant tensors, index notation, tensor products, contractions, metric raising/lowering, covariant derivatives | Stress/strain, inertia, constitutive laws, electromagnetism, relativity, anisotropic materials | Gap: vectors and matrices exist; no general tensor calculus is claimed |
| Differential forms and exterior calculus | forms, wedge product, exterior derivative, pullback, Stokes' theorem, integration on manifolds | Coordinate-independent electromagnetism, flux/circulation, conservation laws, continuum field theory | Gap |
| Topology | continuity, connectedness, compactness, homotopy, winding/linking, topological invariants | Defects, vortices, phase transitions, configuration spaces, field topology and robust geometric classification | Gap beyond limited polygon/segment classifications |
| Calculus of variations | functionals, Euler–Lagrange equations, constraints, boundary terms, optimal paths, stationary action | Lagrangian mechanics, geodesics, elasticity, optics, field equations, optimal control | Gap |
| Hamiltonian and symplectic mathematics | phase spaces, canonical coordinates, symplectic forms, Poisson brackets, Hamiltonian flows, canonical transforms | Classical mechanics, orbital dynamics, molecular dynamics, geometric integration and quantum foundations | Gap |
| Dynamical systems and bifurcation theory | flows/maps, equilibria, linearization, stability, invariant manifolds, attractors, bifurcations, chaos | Nonlinear mechanics, fluid instabilities, climate, circuits, reaction systems and coupled physical models | Gap as a reusable analysis domain |
| PDE theory and continuum mathematics | elliptic/parabolic/hyperbolic equations, weak solutions, conservation laws, characteristics, shocks, boundary layers | Heat, waves, elasticity, fluids, electromagnetism, diffusion, reaction-transport and fields | Gap as a general PDE framework; selected domain-specific solvers exist |
| Numerical discretization and computational mathematics | finite difference/volume/element, spectral and meshless methods, weak forms, stability, consistency, convergence, preconditioning | Turning continuous physical laws into bounded, validated computation | Partial: finite differences and quadrature exist; broad solver families and error analysis remain gaps |
| Harmonic and asymptotic methods | Fourier series/transforms, Laplace transforms, Green functions, distributions, perturbation, multiple scales, WKB and asymptotic expansions | Linear response, waves, acoustics, quantum mechanics, transport, boundary layers and approximate solutions | Gap beyond bounded interpolation and current calculus slices |
| Probability, stochastic calculus and statistical mechanics | random variables/processes, Markov chains, Brownian motion, stochastic differential equations, ensembles, partition functions, entropy and fluctuations | Thermal physics, kinetic theory, diffusion, noise, uncertainty, particle events and molecular systems | Partial: deterministic random streams and online variance exist; distributions and stochastic-process models remain gaps |
| Information and inference mathematics | information/entropy, Bayesian inference, likelihood, inverse problems, identifiability, regularization, model selection | Experimental physics, parameter estimation, tomography, detector reconstruction and uncertainty-aware model calibration | Gap as a general reusable toolkit |
| Control and estimation theory | state-space systems, controllability, observability, feedback, Kalman/Bayesian filters, parameter/state estimation | Stabilization, physical instrumentation, navigation, robotics, experimental apparatus and coupled systems | Gap |
| Discrete, combinatorial and computational structures | graph algorithms, networks, combinatorics, cellular/agent systems, automata, logic and algorithms | Lattice models, reaction networks, meshes, molecular graphs, causal/event networks and computational complexity | Partial: selected grid/timeline and seeded-sampling concepts exist; general graph and combinatoric methods remain gaps |
| Relativistic and representation mathematics | Lorentz and Poincaré groups, Lie groups/algebras, representations, spinors, invariants and relativistic geometry | Special/general relativity, particle physics, quantum angular momentum and field symmetries | Gap beyond basic spatial/quaternion primitives |
| Number theory and exact/symbolic methods | integers, rationals, modular arithmetic, exact algebra, symbolic manipulation, computer algebra and proof-oriented identities | Exact conservation identities, lattice/quantized models, reproducible derivations and verification of formula transformations | Gap: no general symbolic or computer-algebra engine is claimed |

### Mathematical dependency map by physical discipline

This is a cross-reference, not a requirement that every physical model depend on every mathematical subject.

| Physical discipline | Mathematical foundations that must be represented |
|---|---|
| Classical mechanics and rigid bodies | Euclidean/analytic geometry, vectors and matrices, ODEs, linear algebra, variational calculus, Hamiltonian/symplectic methods, constrained optimization, numerical integration |
| Gravitation, orbital mechanics and relativity | Differential geometry, tensor calculus, ODE/PDE, variational principles, Lie groups, Lorentz geometry, numerical integration and stability |
| Thermodynamics and statistical mechanics | Multivariable calculus, differential equations, probability, statistics, optimization, convexity, information/entropy, asymptotics and dimensional analysis |
| Fluids, waves and continuum mechanics | Vector/tensor calculus, PDEs, conservation laws, functional analysis, numerical discretization, stability, turbulence/statistical methods and geometry/topology |
| Heat transfer, materials and reactions | PDEs, transport equations, constitutive tensors, thermodynamics, kinetics, nonlinear dynamics, numerical methods and uncertainty propagation |
| Electromagnetism, circuits and optics | Vector calculus, differential forms, complex analysis, Fourier/Laplace methods, PDEs, Green functions, linear systems and spectral methods |
| Acoustics and signal-bearing physical systems | Harmonic/Fourier analysis, complex analysis, convolution, stochastic processes, PDEs, spectral estimation and inverse problems |
| Quantum, atomic and molecular physics | Complex linear algebra, Hilbert spaces, operator/spectral theory, differential equations, group representations, tensor products, probability and variational/perturbation methods |
| Nuclear, particle and collider physics | Relativistic geometry, Lie groups/representations, four-vector algebra, probability/statistics, stochastic event models, numerical integration, inference and uncertainty analysis |
| Geophysics, planetary science and astrophysics | Differential geometry, tensor calculus, PDEs, dynamical systems, continuum methods, inverse problems, statistics, numerical linear algebra and multiscale methods |
| Experimental and computational physics | Metrology, dimensional analysis, statistics, Bayesian inference, signal processing, inverse problems, optimization, numerical error analysis and reproducibility |

### Coverage and maturity rules

Track each subject independently through these states:

1. **Mapped** — its scope, neighboring subjects and physical consumers are identified.
2. **Explained** — theory, notation, assumptions, derivations and references are documented.
3. **Specified** — reusable contracts, units, conventions, failure modes and numerical expectations are defined.
4. **Implemented** — source code exists for the declared scope.
5. **Validated** — focused tests and independent analytical/reference evidence establish the declared behavior.
6. **Applied** — at least one physical domain consumes the capability in a reproducible experiment.
7. **Cross-validated** — independent methods, refinement studies or external reference data establish confidence across a declared range.

A subject may be mapped and explained while remaining unimplemented. Never promote a whole discipline's status because one narrow primitive exists. Record the implemented slice and its limits.

## Cross-cutting rules

1. **Name the domain and its assumptions.** “Solve this equation” must distinguish an algebraic solution from a numerical approximation, a physical model, or a fitted statistical relationship.
2. **Units are semantic.** Dimensional analysis constrains physical equations; it does not replace numerical conditioning or domain validation.
3. **Geometry includes degeneracy.** Document tolerances, coordinate frames, orientation conventions, collinear/coplanar cases, and invalid intersections.
4. **Statistics describes evidence, not truth by declaration.** Every estimator or interval must name its assumptions, sample definition, missing-data policy, and interpretation.
5. **Random source and probability model are different objects.** Use the repository's Squirrel3 implementation as the random source. A uniform sample is not a Gaussian, Poisson, or physically justified distribution without an explicit transformation/model.
6. **Approximation exposes error.** Numerical routines should document convergence, tolerances, conditioning, and failure modes rather than returning plausible-looking numbers silently.
7. **GPU compatibility is a contract.** Shape, strides/layout, precision, reductions, determinism, bounds, and unsupported operations must be explicit before selecting a compute backend.

## Recommended build order

1. Strengthen the [2D geometry foundation](GEOMETRY_FOUNDATION_2D.md) with adversarial intersection-coordinate tests and uncertainty-aware policies; explicit pairwise rigid transforms, bounded unique-point intersection construction, and simple-polygon point location are implemented, while transform graphs, polygon validation/area/triangulation, and unit conversion still need their own contracts.
2. Add general matrix and small linear-system primitives with reference tests.
3. Add quantities/dimensions and dimensional-analysis helpers without making every low-level type allocation-heavy.
4. Extend scalar numerical analysis: root finding, adaptive quadrature, residuals and defensible error bounds. Composite Simpson quadrature and finite differences now provide bounded starting primitives with explicit limitations.
5. Add probability distributions parameterized by Squirrel3, then statistical accumulators and regression with known-answer fixtures.
6. Add batch/SoA interfaces and GPU contract tests before introducing hardware-specific execution.
7. Add sparse/large linear algebra, FFT/signal operations, optimization, tensor operations and optional autodiff only when their test/reference infrastructure is in place.

## Evidence standard

A row moves from gap to implemented only when it has source, focused tests, documented numerical/domain limits, and an observable reference example. A conceptual SVG may teach the idea, but it cannot be presented as output from an absent solver.

---

<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="320" /></p>
