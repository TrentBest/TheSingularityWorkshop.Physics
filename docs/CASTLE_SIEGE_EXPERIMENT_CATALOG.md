# Castle Siege Experiment Catalog

This catalog turns the castle/trebuchet attraction into a sequence of reusable, testable experiments. Each experiment should be runnable from the Physics domain and consumable by a hosted WebPage Experience. The initial visitor need not see every experiment at once; the Workshop Lab can expose the deeper controls.

**Status convention:** *Available* means the current code and tests demonstrate it; *next* means a concrete candidate for implementation; *later* means it depends on capabilities not yet implemented. Verify the current code and roadmap before changing any status.

Related: [Domain specification](CASTLE_SIEGE_PHYSICS_DOMAIN.md) · [Castle Siege overview](CASTLES_AND_CATAPULTS.md) · [Roadmap](ROADMAP.md).

## Experiment map

| ID | Question | Independent variable(s) | Primary observations | Gate |
|---|---|---|---|---|
| CS-00 | What does this scene contain? | camera/presentation choices | scene composition; explicit fixture status | scene-only |
| CS-01 | How does constant acceleration change a launch? | initial state, gravity, timestep | position/velocity by logical tick | current kernel foundation; adapt and verify |
| CS-02 | How do elevation and azimuth change the path? | launch speed, elevation, azimuth | sampled path, flight time | deterministic projectile |
| CS-03 | Does the path intersect the selected wall section? | target position/size/orientation | hit/miss/indeterminate, intersection | target geometry |
| CS-04 | What arrives at impact? | mass, launch state, gravity | impact velocity, momentum, kinetic energy | confirmed impact |
| CS-05 | What changes when one parameter changes? | one supported field per variant | overlay, exact input diff, measurement delta | run identity/comparison |
| CS-06 | When does a wall section register damage? | declared material/response parameters | damage state, threshold reason, event | bounded wall response |
| CS-07 | When does a section fail? | supported loads/thresholds, prior damage | failure decision and cause | structural response |
| CS-08 | What happens to the failed section? | failure event and body initialization | topology/debris bodies, accounting | topology/deconstruction |
| CS-09 | How does the trebuchet create launch velocity? | counterweight/tension, arm geometry, release | mechanism state, energy and launch state | mechanism model |
| CS-10 | What changes when drag/wind is enabled? | validated force-model parameters | path deviation, energy/work accounting | optional force model |
| CS-11 | What does a coarse distant simulation lose? | update cadence and resolution | error, conservation, cost | multi-rate world |
| CS-12 | Can a saved run be restored faithfully? | checkpoint interval, event sequence | state digest, divergence, replay status | authoritative history |

## Detailed experiment definitions

### CS-00 — The siege scene as an inspectable apparatus

**Question:** Can a visitor identify the apparatus, target and current capability without mistaking authored presentation for simulation?

- Setup: one trebuchet/catapult, one projectile, one castle silhouette, one selected wall section, commander camera.
- Controls: inspect scene; select target; open experiment panel; switch to Workshop Lab path when available.
- Output: scene identity, selected target identity, and a visible capability/status panel.
- Acceptance: no trajectory/impact/damage is represented as computed unless sourced from a Physics result. Illustrative fixtures carry an explicit label.
- Value: this is still a worthwhile hosted destination before solver integration.

### CS-01 — Constant acceleration as the first trusted result

**Question:** Does the implementation match the declared constant-acceleration model?

- Inputs: initial position and velocity, acceleration/gravity, logical timestep, step count.
- Outputs: samples at declared logical ticks, final state, run record and model identity.
- Reference: `x(t)=x₀+v₀t+½at²`, `v(t)=v₀+at`.
- Acceptance: reference agreement within a declared tolerance, repeatable serialization, input validation, exact sample count.
- Limitation: one-dimensional constant acceleration does not establish 3D collision, drag, rigid-body or mechanism support.

### CS-02 — Aim the ideal projectile

**Question:** How do launch speed, elevation and azimuth affect a trajectory under the chosen ideal model?

- Inputs: initial position, launch speed, elevation, azimuth, gravity, timestep and duration.
- Outputs: trajectory samples, apex (if defined), range/time diagnostics and declared model assumptions.
- Cross-check: derive components from the agreed coordinate convention; for equal launch/landing heights with no drag, compare range against `R=v² sin(2θ)/g`.
- Acceptance: same inputs reproduce the same result; near-vertical, zero-speed and invalid-input cases are covered.
- Limitation: the level-ground range formula is not valid as a general answer for a raised or uneven target.

### CS-03 — Hit or miss the selected wall section

**Question:** Does the computed path cross the target geometry?

- Inputs: CS-02 run plus target identity, frame, geometry and active time interval.
- Outputs: hit/miss/indeterminate; first intersection time/position where supported; target identity.
- Acceptance: tests cover target before/after the path, miss, interior hit, boundary/grazing, coordinate mismatch and degenerate geometry.
- Limitation: a trajectory line crossing a rendered mesh is not sufficient evidence unless it is the actual declared target geometry and contract.

### CS-04 — Account for impact

**Question:** What measurable motion reaches the target?

- Inputs: confirmed impact, projectile mass and pre-impact velocity.
- Outputs: momentum `p=mv`, kinetic energy `E=½m|v|²`, and any supported normal/tangential decomposition.
- Acceptance: units and vector frame are explicit; values are tied to the run and impact event.
- Limitation: incoming energy/momentum do not alone specify deformation, fracture, penetration or wall collapse.

### CS-05 — Baseline versus one-factor variant

**Question:** What exactly changed when one parameter changed?

- Preserve an immutable baseline definition/result.
- Create a separate variant with a new run ID, parent reference and exact changed-field list.
- Change one supported input at a time for interpretable comparisons. Multi-factor sweeps are a separate experiment type and must declare their design.
- Compare trajectories and only those outcome metrics the enabled capabilities actually provide.
- Acceptance: the baseline cannot be mutated through variant editing; serialized variants retain units, model/schema version and assumptions.

### CS-06 — First bounded wall response

**Question:** Under a small, explicit teaching model, what condition causes a wall section to change state?

- Inputs: confirmed impact observation, target section identity, declared material/structural parameter set and model version.
- Outputs: response decision, prior/new state, threshold comparison, cause and limitations.
- Acceptance: tests cover below/at/above threshold, repeated hits if supported, duplicate-event protection and stable event order.
- Model requirement: parameter values must be labeled as sourced, measured, assumed or illustrative. Do not present invented stone properties as authoritative.
- Limitation: this experiment is not general masonry fracture or a validated structural engineering tool.

### CS-07 — Failure decision is not debris rendering

**Question:** Did the supported structural rule declare a section failed, and why?

- Inputs: wall state and the loads/thresholds supported by the chosen model.
- Outputs: failure/no-failure, cause, affected section, tick and model identity.
- Acceptance: failure criteria and peak/sustained/impulse semantics are explicit; tests verify the boundary and event ordering.
- Limitation: no general structural claim from an animation or visual crack decal.

### CS-08 — From failure to physical fragments

**Question:** Can a failure event create consistent new physical bodies?

- Separate failure detection, topology mutation, body creation/initial conditions and presentation.
- Record mass distribution and any momentum/energy approximation or loss; state where exact conservation is not claimed.
- Acceptance: stable body IDs, deterministic ordering, no orphaned references, repeatable state and bounded body count.
- Limitation: visual particles alone are effects, not debris bodies.

### CS-09 — Trebuchet mechanism

**Question:** How does stored energy and mechanism geometry generate the launch state?

- Candidate parameters: arm length/inertia, counterweight mass/height or tension, pivot constraints, release angle, sling/release geometry, losses and projectile mass.
- Outputs: mechanism state by tick, energy ledger within model scope, release event, launch position and velocity.
- Acceptance: state assumptions and mechanism type; validate against a simpler reference model or controlled calibration; test limiting cases and energy accounting.
- Gate: do not expose tension/draw-back as a physical control until this mapping exists. Until then use direct launch-state inputs or label the control visual-only.

### CS-10 — Optional aerodynamic forces

**Question:** How do drag and wind alter the trajectory?

- Enable only after a named force model with units, parameters and validity range is implemented.
- Record work/energy implications and environmental assumptions.
- Compare against a reference or convergence study; no hidden wind or default drag.
- Acceptance: zero-force limit returns the baseline model within tolerance.

### CS-11 — Multi-rate simulation

**Question:** Can the same physical state be advanced at different cadences without unacceptable divergence?

- Compare a finer-step reference against candidate coarse rates for projectile flight, wall response, distant scenery and optional secondary effects.
- Record error, wall-clock cost, event latency and any conserved aggregates.
- Acceptance: define error budgets per capability; wake-up reconciles elapsed logical time and pending events.
- Limitation: visually smooth interpolation is not evidence of correct physical evolution.

### CS-12 — Restore, replay and branch

**Question:** Can a run be restored from a checkpoint and reproduced with its ordered inputs/events?

- Persist model/schema identity, initial state, canonical inputs, ordered events, checkpoints and integrity digests.
- Verify compatible restoration and state fidelity before exposing a replay control.
- Variants can be compared now as independent run snapshots; that is not authoritative replay.
- Acceptance: incompatible versions fail explicitly; parent history is immutable; branch changes do not leak into baseline state.

## UI/teaching sequence

1. **Watch:** authored siege venue; status says what is and is not computed.
2. **Aim:** manipulate elevation/azimuth and see validated path/target intersection when available.
3. **Measure:** inspect time, velocity, momentum and energy with units and model assumptions.
4. **Compare:** lock a baseline, change one supported variable, and overlay results.
5. **Explain:** select a measurement or event to reveal the governing model, assumptions and test/reference.
6. **Damage:** enable only when a real bounded wall-response capability exists.
7. **Build a mechanism:** explore counterweight/arm/release physics when the mechanism model exists.
8. **Investigate history:** expose replay only after authoritative reconstruction passes its gate.

## Suggested first regression suite

- same canonical definition yields same sample sequence and run record;
- analytical reference agreement at every declared sample;
- invalid/non-finite input and invalid timestep rejected;
- angle and coordinate convention tests;
- hit, miss, grazing and target-frame mismatch tests;
- immutable baseline and exact variant diff;
- capability false means corresponding control/output cannot claim a computed result;
- impact precedes damage; damage precedes failure; failure precedes debris creation;
- duplicate event does not apply damage twice unless repeated impact is explicitly a new event;
- serialization round-trip preserves schema/model identity, units, inputs and sample order.

The first public demo can stop at any gate and still be meaningful. It must not skip the evidence required by the next gate.
