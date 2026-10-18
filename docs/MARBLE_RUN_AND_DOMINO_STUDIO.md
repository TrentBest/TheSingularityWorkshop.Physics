# Marble Run and Domino Studio: Physics-Driven Experiences

**Status:** experience architecture and physics acceptance contract. This document does not claim that a high-count rolling solver, general rigid-body solver, image-to-domino converter, or complete domino chain-reaction engine is implemented.

## Ownership boundary

Marble Run and Domino Studio are **experiences that consume Physics**, not Physics MicroBundles. They own construction, input interpretation, scene layout, editing, camera, presentation, run controls, and experience-specific persistence. Physics owns reusable physical state, model contracts, solvers, contact observations, deterministic stepping, and measurable run outputs.

- **Marble Run:** builds terrain and track geometry, places/releases marbles, tunes supported parameters, and presents trajectories and results.
- **Domino Studio:** imports an image, proposes a domino arrangement, exposes correction tools, and launches a physical chain reaction.
- **Physics:** provides the shared physical substrate. It must not depend on either experience, an image editor, UI framework, renderer, Unity, or MicroBundle orchestration.
- **Composition layer:** discovers and composes optional providers/models where needed. These experiences are not themselves domain physics providers merely because they use Physics.

An experience may package a particular model/data profile for convenience, but that does not move experience semantics into a Physics MicroBundle.

## 1. Marble Run: many bodies over constructed terrain

### User experience

The player creates or edits an environment from continuous terrain and track features: slopes, ridges, bowls, funnels, rails, ramps, loops, drops, bridges, tunnels, split paths, and collection zones. The player releases one marble or a large batch and observes motion emerge from the simulation. A useful demonstration shows physical diagnostics as well as visuals: speed, collision count, energy changes, route choice, elapsed simulation time, and where or why a marble stopped or escaped.

The target is many thousands of bodies, but throughput is an acceptance goal to measure—not a performance claim. Define benchmark hardware, scene geometry, marble count, contact density, timestep, and acceptable error before stating capacity.

### Geometry contract

Represent the authored environment in two layers:

1. **Experience geometry:** editable terrain, track pieces, joins, rails, triggers, and annotations.
2. **Physics collision representation:** versioned, validated shapes derived from authored geometry. These may use height fields, analytic primitives, triangle meshes, or simplified collision proxies according to the supported solver.

The render mesh must not silently become the authoritative collision model. Every proxy declares transform, units, approximation, revision, and collision policy. Terrain seams, thin rails, narrow passages, sharp corners, and gaps require validation because they can cause tunnelling, snagging, or marbles leaving the intended track.

For smooth terrain, define a stable world frame, gravity direction/magnitude, length scale, and collision-surface normal convention. Geometry generation should be deterministic from saved seeds and authored parameters where procedural variation is used.

### Marble state and behavior

A marble needs stable identity and at least position, linear velocity, radius/shape, mass or density-derived mass, contact parameters, and active/sleeping/out-of-world status. Orientation and angular velocity are needed when rotational behavior is supported. The model should distinguish:

- gravity and free motion;
- normal contact and collision response;
- rolling versus sliding;
- static/kinetic friction and rolling resistance where supported;
- restitution/bounce and damping under declared assumptions;
- spin and angular momentum only when actually modeled;
- track constraints/rails and escape or fall detection;
- sleeping/wake-up behavior that does not lose relevant impacts or incoming energy.

A sphere can be approximated as a point for broadphase screening, but final contact response must use its radius and the selected contact model. Do not label a frictionless normal impulse solver as a complete rolling-marble solver. The repository has bounded contact primitives, but the full frictional, angular, persistent-contact, many-body integration required here remains a separate milestone.

### Scale strategy

Thousands of bodies do not require thousands of bespoke solvers. Use one shared simulation world, stable IDs, broadphase candidate generation, narrowphase contact observations, and a batched stepping pipeline. Spatial partitioning and sleeping can reduce work, but must not omit real interactions. Multiple update cadences are acceptable only if coupling, accumulated elapsed time, and energy/momentum handoff are explicit.

Establish a simple CPU reference first. Profile realistic collision distributions, not just isolated falling marbles. State determinism requirements precisely: fixed inputs/configuration and fixed stepping should reproduce results under the supported runtime contract. Do not promise bit-identical results across arbitrary CPU architectures or GPU implementations unless tested.

GPU acceleration is a later optimization, not a substitute for correct collision and energy accounting. Keep authoritative state typed and inspectable before choosing structure-of-arrays or GPU texture/storage encoding. If textures are used, specify units, encoding, precision, update cadence, overflow behavior, and diagnostics/readback strategy.

## 2. Domino Studio: image to editable 3D construction

### Conversion is a proposal, not magic physical inference

An image does not uniquely specify a 3D domino layout. It may be a top-down pattern, side elevation, perspective photograph, silhouette, or decorative target. Conversion must therefore begin with an explicit import interpretation and expose ambiguity rather than pretending one image determines one correct scene.

Suggested stages:

1. **Import and inspect:** retain the original image, dimensions, orientation, color space, and user-selected interpretation.
2. **Extract a layout signal:** derive a mask, edge map, skeleton, centerline, grid, or other selected representation. Version the extraction algorithm and parameters.
3. **Generate candidate placements:** create candidate centers, spacing, orientations, and heights from the extracted signal and user-selected scale. A top-down image can suggest XY placement; depth/height generally requires user input or a separately declared inference rule.
4. **Resolve physical feasibility:** check overlap, spacing, support surface, center of mass, initial lean, reachability of the next domino, and collision-proxy validity under the chosen model.
5. **Preview and edit:** allow moving/deleting/adding dominoes, adjusting local spacing and orientation, changing scale/material, and rerunning validation.
6. **Commit a versioned layout:** save image digest, conversion algorithm/version, parameters, user edits, units, coordinate frame, and generated placement list.
7. **Run the simulation:** the first push is an initial physical impulse or an explicitly modeled hand/finger interaction; subsequent motion comes from Physics contacts and gravity, not a scripted sequence.

A confidence score describes the conversion process, not certainty about subsequent physics. Every generated placement must be editable. If the input lacks depth information, show that ambiguity plainly.

### Domino geometry and state

A domino is a finite 3D body with dimensions, mass/inertia or a declared homogeneous-shape approximation, material/contact properties, orientation, linear/angular velocity, and contact state. It needs a support surface and gravity. Initial state may include realistic or deliberately stylized deviations only when they are explicit saved parameters.

The chain reaction emerges from geometry and dynamics: one body rotates about a contact/support region, impacts another body, transfers impulse and energy, and may or may not topple it. Spacing, alignment, mass, dimensions, friction, restitution, tilt, support, and solver settings affect propagation. Avoid hard-coded “topple next domino” logic, authored delay chains, or a Boolean fallen state that bypasses physical response.

A first domino can fail to transfer the chain. That is valid output. The experience should help the user understand why, inspect contact events and timing, modify the layout, and replay from a saved checkpoint. It should not secretly force the intended pattern to succeed.

### Image-derived art versus physical truth

Image colors, brightness, and decorative pattern can influence appearance or candidate density through explicit conversion settings. They are not material properties, mass, friction, or collision geometry unless the user deliberately maps them through a documented rule. Keep image classification and 3D layout generation outside the Physics core; Physics receives the resulting versioned geometry and material inputs.

## 3. Shared reusable Physics capabilities

The experiences should reuse rather than fork:

- declared world frame, gravity, and logical time;
- stable body/material/shape identity;
- shape transforms and collision proxies;
- broadphase candidate screening and shape-specific narrowphase;
- contact observations separate from contact response;
- rigid-body translational and rotational state;
- normal impulse, frictional impulse, positional/constraint correction, and resting-contact behavior as supported;
- deterministic stepping and reproducible run records;
- contact, energy, momentum, and solver diagnostics;
- checkpoints/replay with explicit limits;
- versioned geometry and parameter provenance.

These are not all implemented today. Current collision primitives are bounded, and Physics does not yet claim a general frictional/angular many-body solver. Use the capability catalog as the source of truth; update capability status only when source and focused tests prove behavior.

## 4. Validation and benchmark gates

### Marble Run

1. Analytic free fall and sphere-on-incline cases with declared assumptions.
2. Sphere contact against a plane and sphere-sphere collision with momentum/energy residual diagnostics appropriate to the selected restitution/friction model.
3. Rolling/sliding transition on an incline, including static friction limits if supported.
4. Track joins, curved surfaces, narrow rails, tunnelling checks, and out-of-track handling.
5. Multi-marble collision clusters and deterministic repeatability.
6. A measured scaling sweep (for example 100, 1,000, 10,000 and higher counts if feasible), recording hardware, timestep, contact density, wall-clock cost, solver iterations, and error metrics. Counts are test targets, not promises.
7. Stress tests for extreme geometry, invalid material data, high velocity, long duration, and sleeping/waking.

### Domino Studio

1. Image import/conversion fixtures for top-down, sparse, dense, curved, and ambiguous patterns.
2. Stable conversion output for identical source digest, algorithm version, and parameters.
3. Placement validation for overlap, unsupported placement, invalid scale, and malformed geometry.
4. Single domino falling under gravity with a supported rigid-body/contact model.
5. Two-domino impact and transfer test with declared initial conditions and diagnostic contact events.
6. A small chain where propagation succeeds, and a near-boundary case where propagation legitimately fails.
7. Replay test proving identical saved layout and initial impulse reproduce the declared deterministic contract.
8. Edit one placement and prove only the intended saved layout inputs changed.

Use analytical or trusted reference cases when available; otherwise label synthetic fixtures illustrative. Test convergence/sensitivity as the model allows. A visually convincing domino wave is not proof of a physically valid contact solver.

## 5. Reproducibility and user trust

Every run should record world/layout revision, body count, model/schema versions, physical parameters, coordinate frame, gravity, logical timestep, seed if used, initial intervention, and reduced-order or cinematic overrides. Capture observations such as trajectories, contacts, failed transfers, energy/momentum residuals, and termination reason.

The user should be able to save, replay, branch, compare, and inspect a run. Rendering may smooth presentation between authoritative steps, but must not rewrite physical state. Any cinematic boost or exaggerated response is a named override with declared scope, distinguished from baseline results.

## 6. Recommended implementation order

1. Audit and reuse existing rigid-body state, collision, contact, and deterministic math types; list missing contracts before adding duplicates.
2. Define shape/geometry revision and body identity contracts needed by both experiences.
3. Implement the smallest CPU rigid-body/contact vertical slice for a sphere and a standing box/domino, with focused analytical tests and energy/momentum diagnostics.
4. Add friction and resting contact as separately validated increments; do not bundle every rigid-body feature into one untestable milestone.
5. Build a modest marble-track fixture and two-domino transfer fixture using only the tested Physics API.
6. Add deterministic many-body benchmarking and spatial acceleration while preserving CPU reference behavior.
7. Define a renderer-independent image-layout interchange format and conversion pipeline in the Domino experience/consumer layer.
8. Add editable image-derived layouts, then run the same canonical Physics simulation used by other consumers.
9. Consider GPU storage/compute only after CPU results, error tolerances, and reproducibility requirements are explicit.
10. Produce living demonstrations from actual run records. Mark each image as model-backed, converted layout, or conceptual artwork.

## Non-goals and guardrails

- Do not add Marble Run or Domino Studio experience logic as Physics MicroBundles.
- Do not introduce UI, renderer, image-processing, Unity, or WebPage dependencies into Physics.
- Do not claim thousands-of-marble performance until benchmarked.
- Do not turn image pixels directly into unvalidated authoritative physical properties.
- Do not hard-code domino toppling order or marble trajectories to make a demo look successful.
- Do not treat a collision candidate as confirmed contact or an impact diagnostic as complete response.
- Do not claim a full rigid-body solver while friction, rotation, persistent contacts, or multi-contact behavior are missing.
- No package publication, release, or deployment is authorized by this design contract.
