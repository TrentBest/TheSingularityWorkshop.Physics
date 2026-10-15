# Axial Link Failure Threshold Monitor

**Status:** implemented as a bounded threshold-state monitor for the axial spring-damper slice; not a rigid-constraint failure solver.

## Semantics

`AxialLinkFailureMonitor` consumes `AxialSpringDamper.StepResult` observations for one stable link identity. Each observation supplies the link's computed pre-step axial force, logical step index, and end-of-step elapsed time.

The configured threshold is a caller-supplied absolute force in newtons. The monitor records a failure transition only when:

```text
abs(axialForceOnBodyA) > maximumAbsoluteForce
```

A force exactly equal to the threshold does not trigger failure. Both tensile and compressive signs are treated symmetrically because this first monitor accepts one absolute-force limit. The transition preserves link identity, step, elapsed time, measured force, and configured threshold. Only the first transition is returned and retained.

Observations must be for the configured link, with strictly increasing step indices and non-decreasing elapsed time. The monitor does not invent a force sample, calculate material strength, infer fatigue, or mutate bodies or topology.

## Deliberate limitations

This monitor demonstrates explicit threshold semantics and a one-way failure-state transition for the compliant axial link. It is **not** evidence that the articulated-mechanics milestone is complete. It does not consume rigid-constraint reaction loads, torque, impulse, contact state, or a structural material model. It does not detach bodies, change a kinematic tree, or infer gameplay injury.

The next articulated slice still needs a validated rigid constraint scenario that emits achieved body states and typed per-step contact/constraint observations, followed by a failure criterion tied to the declared load semantics.

## Verification

Tests cover below/equal/above-threshold cases, both force signs, first-transition-only behavior, link identity, step/time ordering, and invalid thresholds. CI must pass before this increment is considered validated.

See [Axial Spring-Damper Mechanics Slice](AXIAL_SPRING_DAMPER.md) and the [Articulated Mechanics Contract](ARTICULATED_MECHANICS_CONTRACT.md).
