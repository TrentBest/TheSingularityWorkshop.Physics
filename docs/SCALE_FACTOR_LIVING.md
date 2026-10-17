# Living scale factor: make the property visible

![Living scale factor: the solid object smoothly changes between one-half and twice the reference size, while the dashed reference outline stays fixed.](images/scale-factor-living.svg)

**Change the scale, see the consequence.** The solid object cycles smoothly from a linear scale factor of 0.5 to 2.0. The dashed square is the fixed reference at scale 1.0. The number changes with the animation endpoints so the reader can connect the property to the visible object.

This is a conceptual geometry demonstration, not a physics simulation. The SVG contains the animation and accessible description; no JavaScript, network access, or solver is needed.

## What the number means

For a uniform linear scale factor (s), a length (L) becomes

\[
L' = sL
\]

For a square or cube, area and volume do **not** change linearly with the same factor:

\[
A' = s^2 A, \qquad V' = s^3 V
\]

| Linear scale (s) | Length | Area | Volume |
|---:|---:|---:|---:|
| 0.5 | half | one quarter | one eighth |
| 1.0 | unchanged | unchanged | unchanged |
| 2.0 | double | four times | eight times |

That distinction matters in physics: scaling a shape is not automatically a valid way to scale its mass, inertia, strength, surface area, or response. Those depend on geometry, material, and the model's assumptions.

## What a living image must do

- Expose the property that changes and its value.
- Make the consequence visible in the same frame.
- Keep a fixed reference when comparison helps.
- Label conceptual illustrations as conceptual, and generate model-specific plots from actual run records.
- Respect reduced-motion preferences and include accessible title/description text.

The same rule applies beyond scale: show a force changing its vector, a spring changing extension, heat moving between bodies, a projectile changing position over time, and observer resolution changing with declared inputs. If an image would make an explanation clearer, put it beside the explanation instead of leaving it in a future-work list.

Related: [Living Theory](LIVING_THEORY.md) · [Lookup Tables and Visual Resolution](PHYSICS_MATH_LUTS.md) · [Observer-Driven Resolution](UNIVERSE_AND_OBSERVER_RESOLUTION.md).
