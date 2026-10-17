# Apple free fall: an unintentional lesson in calculus

An apple falls from rest. We watch it move, then quietly notice that the motion contains two different stories:

- **Height changes with time.**
- **Velocity changes with time.**

The scene and both plots below are built from the same eleven state samples produced by the C# experiment. The SVG does not implement a second physics solver. Its dots are the recorded states; connecting lines are visual guides.

![Apple free-fall scene and height/velocity plots derived from the recorded experiment](apple-free-fall.svg)

## The experiment

| Input or outcome | Value |
|---|---:|
| Model | `kinematics.constant-acceleration`, version 1 |
| Initial height above ground | 4.905 m |
| Initial vertical velocity | 0 m/s |
| Constant vertical acceleration | −9.81 m/s² |
| Fixed logical timestep | 0.1 s |
| Completed steps / recorded states | 10 / 11 |
| Recorded duration | 1.0 s |
| Final height | 0 m, within floating-point tolerance |
| Final vertical velocity | −9.81 m/s |

**Convention:** upward is positive, so gravity is negative. The idealized model has constant gravity and no air resistance. The tree is illustration, not collision geometry; ground contact is not a collision solver. The initial height is chosen so that the constant-acceleration equation predicts ground level at one second.

## The rules behind the picture

For initial height \(h_0\), initial vertical velocity \(v_0\), constant acceleration \(a\), and elapsed time \(t\):

\[
h(t)=h_0+v_0t+\frac{1}{2}at^2
\]

\[
v(t)=v_0+at
\]

Here \(h_0=4.905\text{ m}\), \(v_0=0\), and \(a=-9.81\text{ m/s}^2\). At \(t=1\text{ s}\), the predicted height is zero and the predicted velocity is \(-9.81\text{ m/s}\).

The experiment uses the repository's existing [ConstantAccelerationExperiment](../../Physics/Experiments/ConstantAccelerationExperiment.cs) and [ConstantAccelerationSolver](../../Physics/Kinematics/ConstantAccelerationSolver.cs). The [run generator](../../tools/FreeFallRunRecordGenerator/Program.cs) writes the actual samples as a versioned JSON run record; the [SVG renderer](../../tools/render_free_fall.py) reads those samples and displays them. The [lesson manifest](apple-free-fall.lesson.json) gives an experience host the question, model identity, default inputs, learning objectives, assumptions, limitations, and visualization references using the [shared lesson-manifest schema](lesson-manifest.schema.json). This is instructional metadata for consumption by a host, not a claim that it is already a MicroBundleDomain runtime manifest.

## Where calculus sneaks in

The picture makes a calculus idea intuitive before asking the learner to memorize notation:

- The **slope of the height-versus-time graph** is velocity.
- The **slope of the velocity-versus-time graph** is acceleration.
- In calculus language, \(v(t)=\frac{dh}{dt}\) and \(a(t)=\frac{dv}{dt}\) for this smooth model.
- The area under a velocity-versus-time graph gives the change in height, with the sign determined by the coordinate convention.

Newton's work on motion and the development of calculus are historically connected, but an apple anecdote is not proof of the equations. This is a controlled modern teaching experiment: the assumptions are declared, the model is executable, and its predictions can be checked against the recorded states.

## Try it again

From the repository root:

```bash
dotnet run --project tools/FreeFallRunRecordGenerator/FreeFallRunRecordGenerator.csproj --configuration Release -- /tmp/apple-free-fall.json
python3 tools/render_free_fall.py /tmp/apple-free-fall.json /tmp/apple-free-fall.svg
```

The generator checks the expected duration, sample count, ground-level final position, and final velocity. CI verifies that the committed SVG contains the model identity, equations, and every recorded state value. The SVG's tree and visual styling are illustrative; the numbers and plotted points are tied to the run record.

## What this lesson does not claim

This is not a model of a real apple's drag, rotation, branch separation, deformation, or impact with the ground. It does not infer that every object falls at exactly this rate in every environment. The gravitational acceleration is an explicit teaching input, and the result is valid under the model's stated assumptions.

The next teaching slices should reuse this pattern: a short question, one bounded model, declared inputs, visible measurements, an executable validation, and an illustration that can be traced back to the run.
