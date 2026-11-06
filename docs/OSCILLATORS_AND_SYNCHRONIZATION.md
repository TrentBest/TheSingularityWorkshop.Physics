# Oscillators, Timing Signals, and Synchronization

**Status:** research and implementation contract. The ideal metronome and analytical two-oscillator phase comparison are implemented; the physical pendulum, phase coupling, and entrainment models remain research targets.

## 1. Why oscillators belong in Physics

Many systems repeat in time: pendulums swing, metronomes mark beats, rotating mechanisms cycle, and coupled systems exchange energy. Their common analytical language includes phase, period, frequency, phase difference, drift, and synchronization.

That shared language must not erase the difference between a prescribed timing signal and a physical system. A metronome can define event times without having mass or momentum. A pendulum has physical state and obeys a mechanical equation; its phase is an analysis derived from that state, not a replacement for it.

## 2. Core quantities and conventions

Use SI units unless a model explicitly declares another unit system.

| Quantity | Symbol | Meaning | Unit |
|---|---|---|---|
| Time | `t` | Elapsed logical simulation time | s |
| Phase | `φ` | Position in a cycle, represented unwrapped internally | rad |
| Angular frequency | `ω` | Rate of phase change | rad/s |
| Frequency | `f` | Cycles per second | Hz |
| Period | `T` | Time per cycle | s |
| Phase difference | `Δφ` | Signed relative phase | rad |

For a uniform timing oscillator:

\[
φ(t)=φ_0+ωt, \qquad ω=2πf, \qquad T=1/f.
\]

The unwrapped phase is the authoritative value for event counting. A wrapped phase in [0, 2π) is a presentation/measurement view. Define a single documented signed-wrap convention for phase differences (for example, (-π, π]); tests must cover the branch cut. Do not repeatedly add a rounded frame delta to an event timer.

For BPM, one beat per minute corresponds to frequency BPM/60 Hz. A subdivision factor N creates N equal phase intervals per beat. A tempo map or changing tempo is a separate model contract: it must define whether phase remains continuous when the rate changes.

## 3. Timing oscillator: metronome reference

The ideal metronome is a non-mechanical phase source. Its configuration contains a stable participant ID, tempo, initial phase, subdivision count, logical start time, and a deterministic event-index convention.

Beat and subdivision events are derived from phase crossings. For constant tempo, event k occurs at the analytically defined phase threshold, rather than by repeatedly scheduling the next event using a rounded interval. At exact boundaries, event inclusion must be consistent (for example, a half-open interval [t0, t1) for incremental queries, with a separate initial-event rule).

A query over a time interval should calculate event indices from phase and return events in deterministic order. The implementation must define overflow behavior for very long runs and reject non-finite or non-positive tempo, invalid subdivisions, and invalid time intervals. A timing oscillator emits timestamps/indices; it does not create a rigid body or apply forces.

## 4. Physical oscillator: pendulum reference

For a point-mass pendulum of length L in a uniform gravitational field g, with angle θ measured from the downward vertical:

\[
\ddot{θ}+\frac{g}{L}\sin θ=0.
\]

For small angular displacement, sin θ ≈ θ and the linearized equation has natural angular frequency:

\[
ω_0=\sqrt{g/L}, \qquad T_0=2π\sqrt{L/g}.
\]

This small-angle result is an analytical reference, not an exact period for arbitrary amplitude. At larger amplitudes, the nonlinear equation predicts an amplitude-dependent period. Damping, external drive, and mechanical coupling each add declared terms and parameters; they must not be silently folded into the ideal reference.

A numerical pendulum model must declare its integration method and timestep. Validate the small-angle limit against the analytical solution, test convergence as the timestep is reduced, and monitor energy for the undamped/unforced case. For damped or driven cases, account for dissipated and supplied energy. Do not label a numerical trace physically validated merely because it looks sinusoidal.

## 5. Synchronization and relationships

For two ideal oscillators, `OscillatorPhaseComparison.Compare` reports unwrapped phase at both observation endpoints, signed wrapped phase difference in [-π, π), frequency difference, and measured phase-drift rate. Positive relative phase means the second oscillator is ahead of the first. The calculation is analytic and does not apply coupling or alter either oscillator. Always retain the unwrapped difference when measuring accumulated drift; wrapped endpoints can coincide even after one or more complete relative cycles. The observation interval must be finite, positive, and begin no earlier than either oscillator's logical start.\n\nFor general physical or driven oscillators, analyze the unwrapped phase difference and a documented wrapped representation.

- **In-phase:** wrapped phase difference is near zero within a declared tolerance.
- **Anti-phase:** wrapped phase difference is near π (with the chosen wrap convention).
- **Drift:** phase difference changes systematically because rates differ.
- **Beat:** a relative phase repeatedly sweeps through alignment; for two nearby pure frequencies, the beat rate is |f1 - f2|.
- **Rational frequency ratio:** frequencies have a ratio p:q, producing a repeating relative pattern when the ratio is exact. This describes rhythm/pattern, not necessarily synchronization.
- **Phase/frequency locking:** relative phase remains bounded and the long-term average frequencies agree under a declared criterion and observation window.
- **Entrainment:** a driven oscillator locks to an external periodic driver over a parameter range. The range must be measured, not assumed.

For a population of phase oscillators, the Kuramoto order parameter may summarize phase concentration:

\[
re^{iψ}=\frac{1}{N}\sum_{j=1}^{N}e^{iφ_j}.
\]

Here r measures phase concentration and ψ is the mean phase. This is an analysis of phase states, not a mechanical coupling law. A Kuramoto-style phase model and a mechanically coupled pendulum system are distinct experiments.

## 6. Rhythm is not acoustic harmony

A metronome or phase network can establish beat alignment, polyrhythms, phase drift, and entrainment. It does not by itself establish musical consonance. Acoustic harmony depends on generated sound spectra, frequency content, timbre, and psychoacoustic perception. A later acoustics/audio capability may consume oscillator outputs, but rational frequency ratios alone must not be reported as proof of perceived consonance.

## 7. Configurable experiment contract

A multi-participant experiment should declare:

- schema/model version and stable participant IDs;
- model kind (ideal timing, phase-only, or physical oscillator);
- initial state/phase and units;
- natural frequency or physical parameters;
- damping, drive, and coupling only where supported by that model;
- explicit coupling topology and strength for coupled models;
- logical start, duration, timestep/integration method where applicable;
- event and sampling boundary conventions;
- deterministic participant ordering and replay inputs;
- measurements, tolerances, and stopping/budget limits.

A configuration must not attach a mechanical coupling parameter to an ideal timing signal unless a separate, explicitly defined phase-coupling model owns that parameter. Missing physical constants are invalid or unresolved inputs, not invitations to invent defaults.

## 8. Validation sequence

1. Validate phase/frequency/period dimensional consistency and wrap semantics.
2. Test an ideal metronome at multiple tempos, offsets, and subdivisions; include exact-boundary and long-duration event-count tests.
3. Compare two ideal oscillators in-phase, anti-phase, offset, drifting, and rational-ratio cases against analytical expectations; distinguish wrapped coincidence from accumulated unwrapped drift.
4. Validate the small-angle pendulum against its analytical solution and period.
5. Test large-angle period dependence and timestep convergence.
6. Check energy behavior for undamped, damped, and driven pendula.
7. Test entrainment boundaries separately from passive phase alignment.
8. Add mechanical coupling only with explicit equations, deterministic ordering, and energy-transfer accounting.
9. Verify deterministic replay, invalid/non-finite input handling, overflow limits, and bounded evaluation.

Every reported synchronization result must state the model, observation interval, sampling policy, tolerance, and criterion. A short visual coincidence is not sufficient evidence of locking.

## 10. Implemented first slice: ideal metronome

The first timing-source implementation is available in `Physics/Timing/IdealMetronome.cs`:

- `IdealMetronomeDefinition` validates a stable participant ID, positive finite BPM, initial phase in [0, 2π), positive subdivisions per beat, and non-negative finite logical start time.
- `IdealMetronome.GetEvents` derives subdivision crossings analytically from the declared phase and angular frequency. It does not accumulate rounded tick intervals.
- Queries use half-open intervals [start, end): an event exactly at the query start is included, and an event exactly at the query end is excluded. Adjacent queries therefore assign each boundary event to exactly one interval. The oscillator's phase origin is represented by its definition; there is no implicit event emitted at construction.
- Each event records its integer phase-grid index, zero-based beat index, subdivision index, exact model timestamp, and whether it is a beat boundary.
- Queries require an explicit positive event budget (default 100,000) and fail when the requested interval exceeds it. Invalid inputs and index/time overflow fail explicitly.

Focused tests cover tempo, subdivisions, initial phase/start time, adjacent interval boundaries, repeatability, invalid inputs, and event-budget enforcement in `IdealMetronomeTests.cs`.

This is a deterministic ideal timing source only. It is not a physical oscillator, does not implement tempo changes or phase coupling, and does not establish acoustic harmony.\n\n## 11. Implemented second slice: analytical phase comparison\n\n`Physics/Timing/OscillatorPhaseComparison.cs` compares two ideal metronome definitions over a finite observation interval. It reports both unwrapped phases at the interval endpoints, unwrapped and wrapped relative phase, frequency difference in Hz, and phase-drift rate in rad/s. The signed wrap convention is [-π, π), so the anti-phase boundary is represented as -π. The comparison is a measurement only: it does not couple, synchronize, entrain, or modify either oscillator.\n\nFocused tests cover in-phase and anti-phase cases, unequal-frequency drift, a rational 3:2 frequency ratio whose relative phase repeats without remaining fixed, distinct logical start times, wrap boundaries, and invalid intervals. The rational-ratio test deliberately checks accumulated unwrapped phase: a wrapped endpoint returning to zero is not evidence of phase locking.\n\nThe nonlinear pendulum reference, population order parameter, phase-coupled models, and entrainment measurements remain subsequent work.

## 12. Ownership and non-goals

Physics owns physical oscillator state, numerical integration, mechanical coupling, phase/frequency measurements, and authoritative observations. An ideal timing source may be modeled here as a clearly non-mechanical time-domain model. Gestures may later describe rhythmic intent or keyframe timing, but Physics must not depend on Gestures. Host adapters bridge domains. Visualize and Renderer may display phase plots and traces but do not decide physical outcomes.

The first implementation excludes audio playback, audio-device dependencies, and claims of acoustic harmony. It also excludes a universal oscillator abstraction before the ideal timing and physical reference models are independently validated.
