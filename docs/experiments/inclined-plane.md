![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Inclined plane: why the longer path can take longer

A shallow ramp feels like an easier route, but it is also a longer route. For an ideal frictionless slider released from rest through the same vertical drop, every ramp angle gives the same final speed. The shallow ramp has less acceleration and more distance to cover, so it takes longer.

This lesson connects the free-body diagram, Newton's second law, constant-acceleration kinematics, and conservation of mechanical energy. The model is executable and tested, and CI generates a versioned run record from its actual output. The interactive GUI laboratory is the next presentation slice.

## Model boundary

The current Physics implementation is an **analytical frictionless sliding model**:

- a point mass starts from rest;
- it slides down a straight, rigid incline;
- the incline angle is measured above the horizontal;
- the vertical drop and gravitational acceleration are fixed inputs;
- gravity is uniform and constant;
- there is no friction, rolling, air resistance, deformation, or contact solver.

The solver reports the force components, normal force, path length, acceleration, travel time, and final speed. It does not simulate a body shape or resolve contacts.

## Geometry and forces

Let:

- \(m\) be the body's mass in kilograms;
- \(g\) be gravitational acceleration in metres per second squared;
- \(\theta\) be the incline angle above horizontal;
- \(h\) be the vertical drop in metres;
- \(L\) be the distance travelled along the incline.

The ramp is a right triangle. Its vertical side is \(h\), its hypotenuse is \(L\), and the angle at the base is \(\theta\). Therefore:

\[
\sin\theta=\frac{h}{L}
\qquad\Longrightarrow\qquad
L=\frac{h}{\sin\theta}
\]

The body's weight points vertically downward and has magnitude \(mg\). Resolve it into axes parallel and perpendicular to the ramp:

\[
F_{g,\parallel}=mg\sin\theta
\qquad
F_{g,\perp}=mg\cos\theta
\]

With no acceleration through the ramp surface, the normal force balances the perpendicular component:

\[
N=mg\cos\theta
\]

Along the ramp, the frictionless net force is the parallel component of gravity. Newton's second law gives:

\[
ma=mg\sin\theta
\qquad\Longrightarrow\qquad
a=g\sin\theta
\]

Mass cancels. In this ideal model, changing the body's mass changes the forces proportionally but does not change its acceleration.

## Travel time and final speed

The body starts from rest and accelerates constantly over path length \(L\):

\[
L=\frac{1}{2}at^2
\qquad\Longrightarrow\qquad
t=\sqrt{\frac{2L}{a}}
\]

Substitute \(L=h/\sin\theta\) and \(a=g\sin\theta\):

\[
t=\frac{\sqrt{2h/g}}{\sin\theta}
\]

As the angle becomes shallower, \(\sin\theta\) becomes smaller, so the travel time increases.

The final speed follows from conservation of mechanical energy. Starting from rest, the loss of gravitational potential energy becomes kinetic energy:

\[
mgh=\frac{1}{2}mv_f^2
\qquad\Longrightarrow\qquad
v_f=\sqrt{2gh}
\]

Notice what is absent from the final expression: the ramp angle and the mass. For a fixed vertical drop, the ideal final speed is the same at every incline angle. The path and travel time differ, but the ideal final kinetic energy does not.

## What changes when mass or angle changes?

**Change mass while holding angle and vertical drop fixed.** The down-slope gravity component and normal force scale linearly with mass. Acceleration, travel time, and final speed remain unchanged in this ideal model.

**Change angle while holding mass and vertical drop fixed.** A steeper ramp has a larger down-slope gravity component and acceleration, but a shorter path. The final speed remains the same; the travel time becomes shorter.

**Compare the same drop.** Do not accidentally compare ramps with the same length when the intended experiment is the same vertical energy change. A same-drop comparison requires different path lengths at different angles.

## Implementation and evidence

The executable model is [InclinedPlaneSolver](../../Physics/Kinematics/InclinedPlaneSolver.cs), wrapped by [InclinedPlaneExperiment](../../Physics/Experiments/InclinedPlaneExperiment.cs). Its focused tests are in [InclinedPlaneSolverTests](../../tests/TheSingularityWorkshop.Physics.Tests/InclinedPlaneSolverTests.cs) and [InclinedPlaneExperimentTests](../../tests/TheSingularityWorkshop.Physics.Tests/InclinedPlaneExperimentTests.cs). The [versioned run record](../../Physics/Experiments/InclinedPlaneRunRecord.cs) preserves declared inputs, SI units, model identity, and actual model output. CI regenerates and checks the default record against the analytical identities.

The current implementation is not a rolling model. A rolling body requires rotational inertia and a no-slip contact constraint; friction then supplies a torque even when there is no energy-dissipating slip. Sliding friction, static friction, rolling resistance, and air resistance are different model terms and must not be silently combined.

## Visualization and laboratory status

The Physics core remains renderer-independent. The Workshop GUI package is the intended reusable presentation and interaction layer; this repository does not introduce a competing UI or renderer. The interactive Pages controls, synchronized plots/animation, and a GUI-backed living visual remain pending GUI integration. The Physics-side run-record evidence pipeline is implemented and checked in CI.

When that laboratory is connected, it should expose only supported controls, display SI units, distinguish input from computed output, and let learners compare mass and angle changes independently. Its displayed values must come from the Physics model's reproducible run record, not from a second browser-side physics implementation.

## Source and next steps

- [Physics learning path](../LEARNING_PATH.md)
- [Mathematics domain map](../MATHEMATICS_DOMAIN_MAP.md)
- [Inclined-plane experiment generator](../../tools/InclinedPlaneRunRecordGenerator/Program.cs)
- [Constant-acceleration solver](../../Physics/Kinematics/ConstantAccelerationSolver.cs)
- [Numerical validation playbook](../NUMERICAL_VALIDATION.md)
- [Issue #39: inclined-plane teaching lab](https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/39)

---

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
