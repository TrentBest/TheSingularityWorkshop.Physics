# Stellar Physics and Planetary Energy: Roadmap

**Status: planned domain; no stellar-structure or stellar-radiation solver is claimed to exist yet.**

## Purpose

Model a star as a physical energy source and make the energy it emits a causal input to planetary systems. We do not need to simulate every nuclear interaction. We need a declared macroscopic model that preserves the important outputs of those interactions—energy generation, luminosity, spectrum, gravity, and their evolution—at a cost appropriate to the question being asked.

The star model and the planet model should remain separate, composable domains. The star publishes observations such as luminosity and spectral output; orbital geometry and radiative transfer determine what reaches each planetary location; planetary material, atmosphere, ocean, and terrain models determine what happens to that energy.

## The hierarchy of models

### Level 1 — Irradiation source

Start with an explicitly configured star whose luminosity and effective spectrum are inputs. For an isotropic point-source approximation, the bolometric irradiance at distance (r) is

`F(r) = L / (4 π r²)`

where luminosity `L` is in watts, distance `r` is in metres, and flux `F` is in watts per square metre.

For a surface element with outward normal `n` and direction-to-star unit vector `s`, direct-beam input scales with `max(0, n · s)`, before atmospheric transmission, shadows, and other effects. A finite stellar disc can later replace the point-source approximation for penumbrae and near-star cases.

This first level can drive a planet's day/night cycle and a simple thermal budget without claiming to simulate the star's interior.

### Level 2 — Planetary energy response

A regional or lumped thermal model can begin with

`C dT/dt = P_absorbed - P_emitted + P_transport + P_internal`

where `C` is the modeled region's heat capacity in joules per kelvin. The terms must be explicitly defined and consistently measured in watts. A simple surface energy budget may include absorbed shortwave radiation, emitted thermal radiation, and separately modeled exchange with air, water, or subsurface material.

Important next ingredients are:
- reflectivity/albedo and wavelength-dependent absorption;
- thermal emission, initially using a declared grey-body approximation;
- rotation, axial tilt, orbit, distance, and surface orientation;
- heat capacity and thermal conductivity of materials;
- atmosphere absorption/scattering and greenhouse behavior, only as their own models;
- ocean and atmosphere transport, rather than an unexplained temperature adjustment.

Energy must be accounted for as stored, transferred, reflected, or emitted. The renderer observes the resulting state; it does not set temperature directly to make the scene look plausible.

### Level 3 — Stellar structure

When the interior matters, use a spherically symmetric stellar-structure model as a first approximation. The core equations include:

- Mass continuity: `dM/dr = 4 π r² ρ`
- Hydrostatic equilibrium: `dP/dr = -G M ρ / r²`
- Energy generation: `dL/dr = 4 π r² ρ ε`
- An energy-transport relation for the temperature gradient, selected for the assumed radiative or convective regime.
- An equation of state relating pressure, density, temperature, and composition.

Here `M(r)` is enclosed mass, `ρ` density, `P` pressure, `L(r)` enclosed luminosity, `ε` energy-generation rate per unit mass, and `G` the gravitational constant. The model also needs boundary conditions, composition, opacity, and a declared treatment of nuclear energy generation. These equations form a coupled boundary-value problem; they are not an instruction to invent arbitrary profiles.

A first implementation should use validated reference cases and compare outputs against trusted stellar models. Time evolution, changing composition, detailed nuclear reaction networks, magnetic activity, flares, winds, and multidimensional magnetohydrodynamics are later levels, not assumptions silently included in a basic star.

## Numerical and scientific guardrails

- **Choose the model by observable.** A fixed-luminosity star can be enough to study planetary heating. It cannot answer questions about stellar aging or changing fusion rates.
- **Keep model and solver distinct.** Stellar equations, material/opacity data, discretization, integration strategy, and validation are separately documented.
- **Declare validity ranges.** A main-sequence star approximation is not automatically valid for a red giant, white dwarf, neutron star, or transient event.
- **Track units and budgets.** Check dimensional consistency and energy accounting. Distinguish luminosity (power) from irradiance (power per area) and received energy (power integrated over time).
- **Make uncertainty visible.** Empirical inputs and reference data carry provenance, supported ranges, and uncertainty where available.
- **No fake coupling.** Planetary temperatures change only through modeled energy flows and internal sources. Stellar variability changes irradiation only when the source model exposes that variability.
- **Avoid premature detail.** Do not resolve individual fusion events for a world-scale simulation when a validated effective energy-generation model answers the required question.

## Validation gates

1. **Geometry:** doubling distance quarters point-source irradiance; rotating a surface normal gives the expected cosine response; the unlit side receives no direct-beam flux in the point-source model.
2. **Energy:** reflected, absorbed, stored, transported, and emitted energy balance within stated numerical tolerances.
3. **Thermal response:** a closed region with no net power does not drift; a region with constant net power changes energy at the expected rate.
4. **Reference star:** chosen outputs such as radius, surface temperature, luminosity, and broad spectral behavior agree with the declared reference model within documented tolerances.
5. **Numerics:** boundary conditions, solver stability, convergence, reproducibility, and failure behavior are tested before a model is called validated.
6. **Coupling:** the same stellar output and planetary state produce the same energy input regardless of renderer or host application.

## Recommended build order

1. Implement and test an explicit stellar-radiation source with luminosity, spectrum metadata, and distance-based irradiance.
2. Connect orbital and surface geometry to incident energy; keep the source and receiver contracts independent.
3. Add a conservative, low-order planetary thermal budget with material heat capacity, albedo, and thermal emission.
4. Add spatial heat transfer and atmosphere/ocean coupling as separate validated capabilities.
5. Build a 1D stellar-structure solver with a documented equation of state, opacity and energy-generation assumptions, boundary conditions, and reference comparisons.
6. Add stellar evolution and variable phenomena only when their governing models and data are ready.

This roadmap is intentionally broader than the first implementation. The earliest useful milestone is not a detailed star interior: it is a star whose measurable radiation causes physically accounted changes on a planet.
