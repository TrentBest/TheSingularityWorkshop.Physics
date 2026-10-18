# Marble Run: Stunt Scoring and Evolving Marble Contract

**Status:** experience design and Physics capability requirements; not an implemented scoring system or solver feature.

Marble Run should reward what the simulation demonstrates, then let a player's marble evolve in response. The Physics package reports measured physical events and advances physical state. The Experiences package owns stunt definitions, score values, player progression, presentation, and the rules for contested routes.

## The play loop

1. The player nudges their marble left or right.
2. The marble moves through the authored track under the same physical rules as every other marble.
3. Physics produces time-stamped state and contact/trajectory observations.
4. The Experience evaluates those observations against stunt rules, awards score, and records the reason for each award.
5. Score progression may gradually alter the player's marble's size and selected physical parameters.
6. The changed marble meets new opportunities and constraints; the next run remains a consequence of simulation, not a scripted animation.

## Stunts as verifiable claims

| Stunt | Evidence required | Possible score dimensions |
|---|---|---|
| Ramp launch | Marble enters a ramp launch region and leaves its supported surface with upward/outward motion | launch quality, airtime, horizontal distance, landing |
| Ring traversal | Marble trajectory crosses the ring's opening volume in the valid direction without intersecting the ring | clean passage, speed, difficulty |
| Spiral loop | Marble completes a full defined loop path while satisfying track-contact and direction requirements | completion, speed, smoothness, optional clean exit |
| Route denial | A marble's legitimate position or contact changes another marble's access to a contested route | successful contest, timing, difficulty; avoid awarding arbitrary collisions |
| Near miss / precision | A trajectory passes a hazard or target within an authored tolerance without invalid contact | proximity, speed, repeatability |

These are illustrative rules, not existing Physics event APIs. A region crossing alone cannot prove a loop was completed; loop recognition needs a track-defined path/phase or equivalent geometric evidence. A route-denial award is a game rule that should be grounded in a causal contest, not merely another marble falling or stopping nearby.

## Score is not physics

Physics must not contain Marble Run point values or player progression rules. The experience can consume stable, timestamped observations such as:

- body identity and logical step/time;
- pose and velocity at an observation;
- confirmed contact pair and contact geometry;
- entry/exit or directed crossing of a defined region, when such a detector is implemented;
- track-support/contact continuity, when the supported contact model can establish it.

Every award should carry a reason, involved body IDs, event time/step, rule ID/version, and score delta. This allows replays, debugging, score auditing, and deterministic comparison. Avoid awarding points repeatedly for the same crossing while a marble remains inside a region; use explicit enter/exit state, one-shot event identity, or cooldown rules.

## Marble growth changes the actual model

A score-to-growth curve should be monotonic but bounded. For example, a designer might choose a smooth diminishing-return scale curve rather than adding a fixed radius every time points are awarded. The formula and caps belong to the Experience's versioned progression configuration.

Changing radius is a physical change, not a visual-only effect:

- For a homogeneous sphere at constant density, mass scales with radius cubed: \(m \propto r^3\).
- Its solid-sphere moment of inertia is \(I = \frac{2}{5}mr^2\), so at constant density it scales with radius to the fifth power.
- A larger radius changes collision geometry, fit through openings, contact timing, and ramp/loop clearance.
- If the design instead keeps mass fixed while radius changes, density and inertia assumptions must be explicitly recalculated.

Physics should receive a validated, versioned physical-property change at a defined simulation boundary. It should not silently resize a rendered mesh while retaining stale collision shapes, mass, or inertia. The change must define what happens to existing momentum: preserving linear velocity, preserving linear momentum, or applying another explicit game rule produce different outcomes. The Experience chooses the intended rule; Physics applies the requested state change consistently.

Use bounded changes and clear feedback so growth remains an interesting adaptation rather than a numerical runaway. Consider caps on radius, mass, speed, and per-award property change. A change should never teleport the marble through nearby geometry; overlap resolution or a safe resize policy is needed.

## Fairness and multiplayer interactions

- Other marbles obey the same collision and contact rules; the player does not get hidden physics advantages.
- Award contested-route bonuses only when the configured event rule is satisfied.
- Make scoring idempotent under replay: the same event history and rule version should not award points twice.
- Keep score/progression state separate from authoritative physical state, but record both in the run history so a replay can reconstruct the same model configuration.
- Use stable body identities and deterministic ordering for simultaneous or near-simultaneous events.

## Physics implementation gates

This concept does **not** claim that the following capabilities already exist. The implementation path is:

1. Validate the free-body reference integrator against analytical free-fall trajectories and timestep refinement.
2. Establish a deterministic world-step order for state advancement, broadphase, narrowphase, contact response, and positional correction.
3. Add and test sphere-versus-plane/track contact and the frictional response needed for rolling and ramp support.
4. Define geometry-region crossing and supported-track observations with stable IDs and no duplicate event emission.
5. Add sphere mass/inertia and radius-change semantics, including collision-shape rebuild and safe overlap handling.
6. Validate a single marble's ramp, ring, and loop traversal before scaling to many marbles.
7. Benchmark the same seeded scene at increasing body counts and report physical error, contact workload, time per step, and missed/duplicate events.

## Ownership boundary

- **Physics:** numerical state advancement, mass/inertia, collision geometry, contact/rolling response, stable physical observations, deterministic step semantics.
- **Experiences / Marble Run:** track and stunt definitions, scoring rules, progression curve, player input, camera, UI, run history presentation, and reward balance.
- **Renderer / Visualize:** display the resulting authoritative state; visual size must agree with collision and physical size unless an explicitly cosmetic effect says otherwise.

Related: [Free Rigid-Body Reference Integrator](FREE_RIGID_BODY_INTEGRATOR.md) · [Physics Roadmap](ROADMAP.md)
