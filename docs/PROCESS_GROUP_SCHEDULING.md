# Physics Process-Group Scheduling

**Status:** design contract for the next implementation stage. The cadence examples below are proposed defaults, not implemented behavior or measured performance.

## The idea

Each physical capability owns its model state and update work. FSM_API coordinates when that work is eligible to run; the physics capability calculates the physical result. The renderer observes the result and does not set the physical truth.

A domain FSM can act as a small scheduler: its update callback invokes the matching physics processing group with `FSM_API.Interaction.Update(groupName)`. Its FSM_API process rate determines how often that callback is evaluated **relative to invocations of its assigned processing group**.

![Multi-rate physics process-group scheduling](images/physics-process-group-scheduling.svg)

![Living scale-factor illustration: changing a declared property produces an immediate visible change](images/scale-factor-living.svg)

This is the visual-documentation pattern to repeat for cadence controls too: a future cadence-specific living image should expose the selected logical rate and the resulting schedule. The scale demo is a general illustration, not evidence of scheduler behavior.

Use a dedicated, fixed logical simulation heartbeat (for example, `TickAll`) as the parent group for cadence controllers. Do not silently equate a rendered frame with a unit of simulated time. If a host uses a 60 Hz fixed heartbeat, a process rate of 60 means every 60 heartbeat invocations; it approximates one second only while that heartbeat really represents 1/60 second of logical time. Variable-rate hosts must drive the simulation clock deliberately.

## Proposed capability groups

| Capability | Example initial cadence | Typical work | Important exceptions |
|---|---:|---|---|
| Electricity / power networks | ~1 logical second | Generation, aggregate demand, network loads, slow control response | Switching, faults, protection trips, and nearby interactive equipment may need immediate or finer updates |
| Thermodynamics / bulk heat | ~2 logical seconds | Heat exchange, temperature evolution, slow material response | Ignition, phase thresholds, rapid heating, and safety limits can demand earlier evaluation |
| Mechanics | Fixed-step or adaptive | Motion, forces, constraints, contact candidates | Fast motion and contact-critical objects need sufficiently frequent detection |
| Impact / damage consequences | Event-driven where possible | Impulse resolution, damage, fracture, debris and secondary effects | Collision detection still has to run often enough to avoid missed contacts; event-driven consequences do not mean event-blind detection |
| Measurement / diagnostics | Configurable | Sampling, residuals, conservation checks, experiment records | Validation sampling must not alter the simulated state |

These are starting hypotheses. Choose actual rates from model stability, event sensitivity, error bounds, and measured cost—not from a universal rule that one domain is always slow.

## Execution pattern

1. The host advances an explicit logical simulation clock and invokes the canonical `TickAll` heartbeat at the declared cadence.
2. Capability scheduler FSMs assigned to `TickAll` use their configured FSM_API process rates to decide when to run.
3. An eligible scheduler invokes its own domain processing group. That group updates only the contexts and work assigned to that capability.
4. The domain records its last update time, the elapsed logical interval, and any events it consumed or emitted.
5. Measurements and renderers read the resulting state without becoming authorities over it.

The core physics project must remain independent of FSM_API. An adapter project can implement this pattern against the real FSM_API API; until that adapter and integration tests exist, this document describes intended composition rather than an implemented scheduler.

## Time, missed ticks, and cross-domain causality

A lower update frequency must not discard elapsed time. Each domain needs an explicit policy for the interval since its last update:

- **Aggregate integration:** integrate the elapsed interval in one call only when the model is valid for that interval and its assumptions remain true.
- **Substeps:** execute bounded smaller steps when numerical stability or event resolution requires them.
- **Event-triggered wake-up:** run sooner when an input changes materially, a threshold is crossed, a fault occurs, or an interactive actor requires fresh detail.
- **Bounded catch-up:** if work falls behind, apply a declared catch-up budget or report a simulation-time slowdown; do not silently skip physical time.

A long interval cannot be treated as one harmless step when events may occur inside it. For example, a load change may trip a circuit breaker before the next scheduled bulk electricity update. The model must preserve that transition and its consequences.

Capabilities exchange explicit quantities and events: for example, electrical power delivered to a heater, heat deposited in a material, temperature-dependent resistance, or a mechanical impact that breaks a conductor. Define ownership and ordering for each coupled quantity so the same transfer is not applied twice and causality does not depend on accidental processing order. Where simultaneous coupling matters, use a staged exchange or an explicitly iterated coupled solve.

## Relevance and detail

A domain can be split into background aggregation and high-detail local work. For electricity, a world-scale network may update aggregate generation and demand on a slower cadence while a nearby panel, fault, or player-operated generator wakes a focused model immediately. This is better than forcing the entire world to run at the local interaction rate—or allowing a player-facing interaction to wait on a coarse background tick.

Moving an entity between detail levels must preserve aggregate state, stored energy, pending events, and logical time. Refinement may increase resolution; it must not reset history.

## Validation before optimization

Every capability scheduler should eventually expose enough data to validate its behavior:

- configured cadence and actual logical interval;
- last and next scheduled simulation time;
- completed updates, catch-up steps, and missed/deferred work;
- events received and emitted;
- cost per update and aggregate cost;
- conservation residuals, numerical error estimates, and model limitations where defined.

Test equivalence against a finer-step reference for supported cases, verify threshold and event behavior at interval boundaries, and measure cost against error. A faster schedule is not correct if it misses a fault, delays an ignition beyond the model's stated tolerance, or loses energy.

## FSM_API precedent

The Workshop's [SimpleDemos](https://github.com/TrentBest/SimpleDemos) demonstrates named processing groups and separates environment work from agent updates in its advanced simulation. FSM_API also documents process rates as counts of processing-group invocations: `-1` updates on every group invocation, `0` requires manual updates, and a positive rate updates every Nth invocation. The physics scheduler should build on these real semantics rather than introduce a second hidden frame loop.

## Resources & Support

[Physics home](../README.md) · [Architecture](ARCHITECTURE.md) · [Roadmap](ROADMAP.md) · [FSM_API](https://github.com/TrentBest/FSM_API) · [SimpleDemos](https://github.com/TrentBest/SimpleDemos) · [Issues](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues)

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="200" />
</p>

<p align="center"><em>Build understanding. Make reality inspectable.</em><br/><strong>The Singularity Workshop</strong></p>
