# Procedural Geometry and Fractal Data Contract

**Status:** design proposal. This document defines a candidate boundary and validation plan; it does not claim that a fractal renderer, procedural-mud solver, terrain generator, or general procedural-geometry API is implemented.

## Decision

Mathematics remains inside **TheSingularityWorkshop.Physics**. We are not creating a separate Math library or package. Geometry and numerical primitives are useful because they support physical models, but they belong to the Physics repository's mathematical foundations.

The intended flow is:

`declared model + parameters + seed`
→ `deterministic mathematical evaluation`
→ `renderer-independent geometry/data`
→ `Physics validation or simulation`
→ `GUI.SVG / Renderer / Visualize manifestation`

A visualization adapter may expose parameters, show intermediate results, and help inspect data. It must not become the authoritative owner of the mathematical result.

## Keep three different ideas separate

### 1. Fractal mathematics

A fractal construction or fractal-like process defines a rule across scales. Examples include escape-time sets, recursive branching, and selected forms of noise. These are not interchangeable algorithms: their outputs and statistical properties differ.

The Mandelbrot set is an excellent interactive browser experiment for iteration, escape-time classification, and zoom-dependent sampling. It is **not**, by itself, a physical model of mud, erosion, debris, or material cohesion.

### 2. Procedural geometry

A procedural geometry method converts declared inputs into a finite, inspectable result, such as points, segments, polygon contours, triangles, occupancy samples, or a scalar field. Its contract must define coordinate convention, bounds, resolution, topology, numeric limits, and how failure is reported.

Potential families include seeded point distributions, recursive branching, fractional-noise fields, Voronoi-derived partitions, and geometry generated from physical erosion/deposition rules. Each family needs its own rationale and tests; no one family should be described as universally “natural.”

### 3. Physical material behavior

Mud splatter and cohesive debris are not only shapes. A credible model may need material properties, impact conditions, moisture or cohesion assumptions, conservation/accounting rules, and a declared approximation level. A visually convincing contour is not evidence that those dynamics have been simulated.

Start with useful geometry and explicit limits; add physical coupling only when its governing assumptions and validation cases can be stated.

## Renderer-independent output contract

Any future generator should make these details explicit:

- **Identity:** algorithm name and version, parameter set, seed, and stable purpose/stream identifiers.
- **Coordinates:** dimension, units where applicable, axis convention, and coordinate-frame identity.
- **Bounds and resolution:** requested domain, sample spacing or refinement policy, maximum output count, and work limits.
- **Output meaning:** whether values are points, a boundary, a mesh, a scalar field, or an occupancy sample; do not treat these representations as equivalent.
- **Determinism:** same supported inputs produce the same logical output under the documented runtime/numeric contract. Do not promise cross-platform bitwise identity until it is tested.
- **Validation:** finite values, valid indices/topology, in-bounds output where required, and explicit handling of degenerate or exhausted results.
- **Cost:** generated element count, work/iteration limit, and enough metadata to measure time and memory externally.
- **Provenance:** retain inputs and algorithm identity in experiment records so a result can be reproduced and compared.
- **No presentation dependency:** core output must not require SVG, HTML, Blazor, WPF, Unity, or a GPU.

Use the existing deterministic Squirrel3 facilities when a procedural model needs reproducible sampling. Randomness is a sampling source, not a physical distribution or a substitute for declaring the model.

## First proof sequence

1. **Mandelbrot browser experiment:** expose the complex-plane window, iteration cap, pixel/sample resolution, and zoom history. Label the output as a mathematical visualization, not a physics simulation. Record the exact inputs.
2. **Deterministic data proof:** generate a bounded, seeded set of geometry samples in Physics and test replayability, bounds, limits, and edge cases without referencing a renderer.
3. **Manifestation proof:** render that same generated result through GUI.SVG and verify that the displayed sample count, bounds, and identifiers agree with the Physics output. SVG is a view of the data, not the source of truth.
4. **Geometry usefulness proof:** turn a selected procedural result into a well-defined contour or triangle set only when the chosen algorithm supports that topology. Test validity and degeneracy; do not silently repair invalid geometry.
5. **Physical coupling proof:** choose one narrowly scoped use case—such as impact-driven particle/debris placement or a bounded deposition field—and compare it with a declared reference case. Report model assumptions and error; do not label it “mud physics” based only on appearance.
6. **Cost/fidelity proof:** compare several explicit resolution levels, tracking output size, generation work, geometric error or a suitable proxy, and downstream cost.

Each stage must be independently useful and may be stopped if evidence does not justify the next stage.

## Numerical and safety limits

- Bound recursion depth, iteration count, field dimensions, and output element count. User-controlled zoom or resolution must not permit unbounded work or allocation.
- A deeper Mandelbrot zoom can expose floating-point precision limits; more iterations do not solve loss of coordinate precision. The browser demonstration should state its numeric range and avoid claiming unlimited deep zoom.
- A fractal boundary may have arbitrarily fine detail in the mathematical ideal while every implementation has finite precision and finite output. The output contract must say what was sampled or approximated.
- Noise-based patterns can look rough without representing erosion, cohesion, or any other physical mechanism. Label such outputs as procedural geometry unless validated physical rules actually generate them.
- A compact generator can still be expensive to evaluate. Measure the actual work and output; do not infer performance from a short formula.

## Acceptance gates

A first implementation should not be considered complete until focused tests establish:

- deterministic replay for fixed inputs and seed;
- changed seeds or parameters affect only the intended aspects of output;
- invalid, non-finite, degenerate, and out-of-range inputs fail explicitly;
- configured work/output limits are enforced;
- generated geometry obeys its declared coordinate and bounds contract;
- output is consumable without a rendering package;
- any SVG/Renderer manifestation matches the same generated data;
- the documentation distinguishes implemented behavior, analytical demonstration, and future physical modeling.

## Current status

The repository already contains reusable foundations including finite 2D geometry predicates, polygon operations, vector/matrix primitives, lookup-table support, online statistics, and deterministic Squirrel3 sampling. Their existence makes this direction plausible; it does **not** mean the procedural generator described here exists. The first implementation should be selected after reviewing these existing primitives and choosing one bounded, testable output contract.
