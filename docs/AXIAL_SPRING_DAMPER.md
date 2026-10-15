# Axial Spring-Damper Mechanics Slice

**Status:** implemented bounded one-dimensional compliant-link model; not an articulated rigid-body solver.

## Scope

`AxialSpringDamper` models two point-like bodies on one coordinate axis connected by one linear spring and one linear damper. It calculates the link force from the pre-step separation and relative velocity, then advances both body snapshots with a fixed timestep using semi-implicit Euler.

For body positions `x_A`, `x_B`, velocities `v_A`, `v_B`, rest length `L_0`, stiffness `k`, and damping coefficient `c`:

$
F_A = k((x_B-x_A)-L_0) + c(v_B-v_A)
$

The equal-and-opposite force on body B is `-F_A`. Acceleration follows `a_A = F_A/m_A` and `a_B = -F_A/m_B`. Velocities are updated before positions. The returned force is in newtons and is a **compliant-link spring/damper force**, not a rigid-joint reaction, impulse, contact load, or failure criterion.

## Output and time

Each result identifies the link, caller-supplied logical step index, end-of-step elapsed time, computed pre-step axial force, and the two achieved body snapshots. The caller owns the step index and elapsed-time convention; this method does not schedule or accumulate simulation time itself.

## Validation and limitations

The implementation validates identities, positive finite masses, finite state, non-negative finite link parameters, and positive finite timestep. It rejects non-finite arithmetic outcomes rather than returning an invalid state. Repeated identical inputs are covered by a deterministic equality test.

This is deliberately a first mechanics increment. It does **not** model 3D rotation or inertia, rigid constraints, multiple links, contact/collision, friction, constraint solving, joint reaction loads, structural failure, topology mutation, or material/injury effects. Semi-implicit Euler has timestep-dependent numerical error; this small slice does not claim a convergence bound or general stability.

See also: [Articulated mechanics contract](ARTICULATED_MECHANICS_CONTRACT.md) · [Roadmap](ROADMAP.md) · [Architecture](ARCHITECTURE.md)
