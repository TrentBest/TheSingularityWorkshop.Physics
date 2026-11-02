<!-- The Singularity Workshop moniker -->
<p align="center"><a href="https://github.com/TrentBest"><img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop animated moniker" width="720" /></a></p>

# Calculus I — Estimating a Derivative from Samples

**Status:** Implemented bounded numerical primitive; source-checked against the development branch. The central-difference routine is tested in CI; it does not yet produce an error estimate or a living plot.

## 01 — What: a derivative is a local rate of change

Imagine measuring the position of a moving object at nearby times. Average velocity over an interval is displacement divided by elapsed time. Instantaneous velocity asks a sharper question: what rate of change applies at one instant?

For a scalar function (f(x)), the derivative at (x) is the limit of the secant slope as the two sample locations approach one another:

\[
f'(x)=\lim_{h\to 0}\frac{f(x+h)-f(x)}{h}.
\]

The derivative has units of *output units per input unit*. If (f) is position in metres and (x) is time in seconds, (f'(x)) is velocity in metres per second.

## 02 — Why: measurements and models are often sampled

A closed-form derivative is not always available. A model may be a numerical procedure, a measured data stream, or a function whose formula is supplied by another component. Finite differences estimate local rates from function evaluations.

The central difference samples symmetrically on both sides of the target:

\[
D_h f(x)=\frac{f(x+h)-f(x-h)}{2h}.
\]

Symmetry cancels the leading first-order bias found in a one-sided difference. It is a useful first derivative estimator when the function can be evaluated on both sides of the requested point.

## 03 — How: assumptions and numerical behavior

Assume (f) is sufficiently smooth near (x), and choose a positive step (h). Taylor expansion gives:

\[
f(x+h)=f(x)+hf'(x)+\frac{h^2}{2}f''(x)+\frac{h^3}{6}f'''(x)+O(h^4)
\]

\[
f(x-h)=f(x)-hf'(x)+\frac{h^2}{2}f''(x)-\frac{h^3}{6}f'''(x)+O(h^4).
\]

Subtracting the second expansion from the first and dividing by (2h) yields:

\[
D_h f(x)=f'(x)+\frac{h^2}{6}f'''(x)+O(h^4).
\]

For a sufficiently smooth function, the leading truncation error is therefore proportional to (h^2). This does **not** mean that making (h) arbitrarily small always improves the answer. Floating-point evaluations have finite precision; when (f(x+h)) and (f(x-h)) are nearly equal, their subtraction can lose significant digits. A practical step balances truncation error against roundoff and the scale of the function.

The current implementation deliberately requires the caller to choose (h). It validates that:
- (x) and (h) are finite, and (h) is positive;
- (x-h) and (x+h) are distinct, finite sample coordinates;
- both function samples, the numerator, denominator, and final estimate are finite.

It throws rather than silently returning a non-finite result. It does not infer units, choose an optimal step, estimate uncertainty, handle discontinuities, or differentiate vector-valued functions.

## 04 — See it in a minute: verify a known derivative

For (f(x)=3x^2+2x-7), the analytical derivative is (f'(x)=6x+2). At (x=4), the expected result is (26).

```csharp
using TheSingularityWorkshop.Physics.Numerics;

double estimate = FiniteDifferences.CentralDifference(
    x => (3d * x * x) + (2d * x) - 7d,
    x: 4d,
    step: 1e-3);

// estimate is approximately 26
```

The focused tests also compare the numerical estimate for sine against its analytical cosine derivative, verify a constant function, and exercise invalid coordinates, invalid steps, non-finite samples, and overflow.

**Evidence classification:** source-backed implementation with focused automated tests. No measured physical data or independent reference dataset is involved. A living secant-to-tangent picture and an explicit step-size/error sweep remain future work.

## 05 — Choose your next step

- Implementation: [FiniteDifferences.cs](../Physics/Math/FiniteDifferences.cs)
- Tests: [FiniteDifferencesTests.cs](../tests/TheSingularityWorkshop.Physics.Tests/Math/FiniteDifferencesTests.cs)
- Domain inventory: [Mathematics Domain Map](MATHEMATICS_DOMAIN_MAP.md)
- Numerical evidence rules: [Numerical Validation](NUMERICAL_VALIDATION.md)
- Learning route: [Physics Learning Path](LEARNING_PATH.md)

A natural extension is to compare estimates at (h) and (h/2), report an error estimate with clearly stated smoothness assumptions, and generate a living plot from those actual computed samples.

---

<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="320" /></p>
