# Electricity: from charge carriers to the Singularity Grid

Electricity must be an evolving physical domain, not a collection of glowing wires and sparks. A switch changes circuit state; that change alters current and power; loads aggregate into a local distribution network; the network changes what the power plant must supply. Every visualization should be an observation of that model.

This document distinguishes the small implemented baseline from the larger program.

## The first executable slice

```csharp
using TheSingularityWorkshop.Physics.Electricity;

var lamp = new ResistiveElectricalLoad("city/block-7/lamp-01", resistanceOhms: 24d);
var heater = new ResistiveElectricalLoad("city/block-7/heater-02", resistanceOhms: 12d);

var grid = ElectricalGrid.Observe(
    busVoltageVolts: 120d,
    generationCapacityWatts: 1_500d,
    loads: [lamp, heater]);

// lamp: 5 A, 600 W; heater: 10 A, 1,200 W.
// Aggregate: 15 A, 1,800 W; declared capacity exceeded by 300 W.
```

This is a deterministic, ideal fixed-voltage DC accounting model. It implements Ohm's law and electrical power for enabled resistive loads, aggregates current and power, and flags demand above declared capacity. Load observations are sorted by stable ID so input enumeration order does not change presentation order.

It is intentionally **not yet** a complete circuit solver. Overload currently reports an energy-supply mismatch; it does not automatically reduce voltage, trip a breaker, shed loads, or simulate generator dynamics. The bus voltage remains the declared nominal voltage even when capacity is exceeded. That limitation is explicit so a UI cannot mistake a capacity warning for a solved brownout.

![Electrical-to-thermal coupling experiment: computed load power and resulting thermal accounting](experiments/electrical-thermal-coupling.svg)

*This image is generated from a bounded run record. The broader Singularity Grid described below is a roadmap, not an implemented city-scale network.*

## The causal chain we want

```text
City object / appliance switched on
  -> load state changes
  -> circuit model calculates voltage, current, and power
  -> feeder / district demand aggregates
  -> grid checks generation, reserve, and network constraints
  -> power plant dispatch responds
  -> generator, turbine, reactor / boiler, coolant, and heat states evolve
  -> observations drive meters, diagrams, sounds, and animations
```

This chain should be composable. A city object owns its own on/off intent and load identity; the grid owns aggregation and distribution accounting; generation assets own their physical conversion models. A user should be able to trace a light turning on all the way to the resulting plant demand without coupling the Physics package to Singularity City or its renderer.

## Electrical model ladder

### 1. Circuit fundamentals

- Explicit SI quantities and sign conventions: volts, amperes, ohms, watts, joules, coulombs, farads, henries, and seconds.
- Ideal resistors and sources first; Kirchhoff current and voltage laws for small DC networks.
- Capacitor and inductor state with timestep/convergence checks before transient circuits are claimed.
- Distinguish conventional current direction from electron drift; charge conservation is not electrons being consumed by a lamp.
- Model energy delivered to loads and losses rather than treating current as a visual particle stream.

### 2. Semiconductors that explain themselves

Transistors deserve their own executable learning experiences. The first version should teach rather than overclaim microscopic precision.

**NPN BJT:** show N-type emitter, thin P-type base, and N-type collector; mark conventional current from collector toward emitter in forward-active operation; show the base-emitter junction forward biased and collector-base junction reverse biased. Explain that a small base current controls a larger collector current within the model's operating region. Animate carrier injection/recombination as a conceptual visualization, not as a literal count of every carrier.

**PNP BJT:** mirror the polarity and doping arrangement; show conventional current entering the emitter and leaving the collector in forward-active operation. Keep current arrows and electron/hole motion separately labelled, because they are not the same direction.

**MOSFET follow-on:** show gate voltage establishing or removing a conductive channel, gate insulation, threshold behavior, and the difference between a conceptual switch and a complete device model.

Every transistor experience should expose operating region and assumptions. A simplified educational model must not be presented as SPICE-grade device simulation. The next numerical step is a bounded, documented piecewise transistor model, with tests for cutoff, active region, saturation, polarity, and limiting behavior before adding circuit coupling.

### 3. Distribution grid

- Give every load and network node a stable identity and a place in the city hierarchy.
- Aggregate demand by building, block, district, and city without losing traceability to individual loads.
- Progress from ideal fixed-voltage accounting to feeder resistance, voltage drop, line losses, transformer limits, and protection behavior.
- Represent generation capacity, reserve margin, demand peaks, and explicit overload / brownout / outage transitions.
- Define load priority and shed/reconnect behavior as explicit policy, not an invisible side effect.
- Preserve electrical state when a location is not visible; observation changes detail, not physical truth.

### 4. Singularity Power Plant

The Power Plant is both a simulated facility and a public learning space. Each technology should be a separate experience built on shared energy, heat, mechanics, and electrical contracts.

- **Grid control room:** live generation, load, reserve, frequency/voltage indicators when those variables are actually modeled, feeder map, and event history.
- **Thermal plant:** fuel energy to heat, working fluid, turbine shaft work, generator electrical output, exhaust/rejected heat, and efficiency accounting.
- **Nuclear fission educational plant:** staged reactor-power control, neutron-population approximation, delayed response, coolant heat removal, steam/turbine conversion, generator output, decay heat after shutdown, alarms, and protective trips. This is an educational simulation, not operating guidance for real reactors.
- **Renewables:** irradiance/wind/water resource input coupled to a declared conversion model and changing availability.
- **Storage:** charge/discharge power limits, stored energy, losses, state of charge, and response to demand peaks.
- **Dispatch experiments:** switch a city load on, trip a generator, change resource availability, and observe the grid's causal response and recovery.

Do not jump straight to a visually convincing reactor. Begin with energy accounting and an explainable thermal-to-shaft-to-electric conversion chain; introduce nuclear kinetics only as a separately validated, bounded educational model. A reactor panel should expose which model is active and what has been simplified.

## Living diagrams: the picture must be the model

The guiding question is: *if a picture is worth a thousand words, how many words is a living picture worth?*

The target is not a static SVG that merely decorates prose. A living diagram should be bound to actual model state and expose:

- a moving conventional-current arrow and a separately labelled electron/hole carrier illustration;
- switch state and a visible cause-and-effect path from switch to load to grid;
- current, voltage, resistance, power, and energy values from the same model result shown in the diagram;
- selectable NPN/PNP polarity, component regions, and operating-point explanations;
- a step/pause/reset control for deterministic experiments, with a recorded time/step;
- model assumptions, units, provenance, and a clear “conceptual visualization” label wherever microscopic motion is schematic.

Animation is presentation, not authority. Physics computes the state; a host such as WebPage/GUI.SVG/Visualize may render it. The core package must not acquire a browser, SVG, Unity, GPU, or UI dependency.

## FSM_API and multirate scheduling

Electrical systems have natural time scales: circuit transients can require small steps; grid accounting and dispatch can run more slowly; thermal plant state evolves on another cadence. These must not be conflated with render frames. Later integration should use explicit logical time and deterministic process groups, with a documented ordering for coupled electrical, thermal, mechanical, and control domains. FSM_API can coordinate lifecycle, switching, protection, and scheduled work; it is not a substitute for solving the numerical equations.

A slower update rate is valid only when its error and coupling consequences are understood. If a plant step changes generator output, the grid must observe a coherent committed state—not a mixture of old and new domain snapshots.

## Validation gates

1. **Implemented now:** fixed-voltage resistive load accounting, switch state, stable load IDs, aggregate demand/current, capacity margin, overload flag, deterministic ordering, invalid-input/duplicate-ID checks.
2. **Next:** living SVG/experiment for Ohm's law and parallel/series DC networks; Kirchhoff-law tests against hand-computable circuits.
3. **Then:** dynamic RC/RL circuits with timestep bounds and energy checks.
4. **Then:** educational NPN/PNP operating-region model, tests, and living carrier/junction diagram.
5. **Then:** grid nodes/feeders with voltage drop and losses, followed by overload protection and load shedding.
6. **Then:** generator and thermal conversion models, with power/energy balance.
7. **Then:** staged power-plant experiences and bounded reactor/renewable/storage models.
8. **Finally:** end-to-end city-to-grid-to-plant scenarios with reproducible runs and causal event history.

A stage is complete only when implementation, tests, documented assumptions, and observable evidence agree. No stage is complete because a diagram looks persuasive.

## Scope and safety

This work is for physical education and fictional-world simulation. Power plant and reactor experiences must be labelled as educational models, include simplified-model disclosures, and avoid implying that a toy simulation is an engineering design, safety case, or procedure for operating real infrastructure.

---

[Physics home](../README.md) · [Roadmap](ROADMAP.md) · [Living theory](LIVING_THEORY.md) · [Process-group scheduling](PROCESS_GROUP_SCHEDULING.md)
