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
| Calculus | limits, derivatives, integrals, gradients, divergence, curl, differential equations | finite differences with error policies; quadrature; derivative/gradient contracts; ODE integration | Partial: model equations and time stepping exist in narrow domains; no general calculus toolkit |
| Discrete mathematics | sets, relations, graphs, combinatorics, recurrence, logic | stable graph/identity primitives, graph traversal, combinatorial counts | Partial: timeline and grid concepts exist; not a general discrete math library |
| Probability | events, distributions, conditional probability, sampling | distribution contracts separated from deterministic pseudorandom source; reproducible samples | Partial: [Squirrel3 stream](../Physics/Math/Squirrel3Random.cs) and [keyed sampler](../Physics/Math/Squirrel3Keyed.cs) exist; distribution library is a gap |
| Statistics | samples, estimators, moments, regression, confidence intervals, hypothesis tests | online mean/variance, covariance, quantiles, fit diagnostics, interval assumptions | Partial: [single-pass Welford mean and population/sample variance](../Physics/Math/OnlineStatistics.cs) is implemented with [focused tests](../tests/TheSingularityWorkshop.Physics.Tests/Math/OnlineStatisticsTests.cs); covariance, quantiles, regression, and inference remain gaps |
| Numerical analysis | floating-point error, stability, convergence, interpolation, root finding | tolerances, residuals, error estimates, condition numbers, deterministic reference tests | Partial: [bounded 1D LUT](../Physics/Math/LookupTable1D.cs) with linear interpolation exists; general toolkit is a gap |
| Optimization | objective/constraint models, local/global search, least squares | objective contracts, stopping criteria, residuals, reproducible initial samples | Gap: no general optimization API claimed |
| Information and signal mathematics | Fourier transforms, convolution, filtering, spectral analysis | signal/sample contracts, windowing conventions, reference cases | Gap: acoustic models exist, but no general signal-processing suite is claimed |
| Graph and tensor methods | sparse/dense graphs, tensors, tensor contractions, automatic differentiation | explicit shape/layout contracts and backend-neutral operations | Gap: no tensor or autodiff runtime claimed |
| Number theory and exact arithmetic | integer domains, modular arithmetic, rational values, primality | exact rational/unit-conversion support where useful; overflow policy | Gap: not a general exact arithmetic package |

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
4. Add scalar numerical analysis: root finding, quadrature, finite differences, residuals and error bounds.
5. Add probability distributions parameterized by Squirrel3, then statistical accumulators and regression with known-answer fixtures.
6. Add batch/SoA interfaces and GPU contract tests before introducing hardware-specific execution.
7. Add sparse/large linear algebra, FFT/signal operations, optimization, tensor operations and optional autodiff only when their test/reference infrastructure is in place.

## Evidence standard

A row moves from gap to implemented only when it has source, focused tests, documented numerical/domain limits, and an observable reference example. A conceptual SVG may teach the idea, but it cannot be presented as output from an absent solver.
