![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# SURPS — Physics Domain — LOD 1 — Weather and Climate

![SURPS Rulebook LOD 1](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/development/docs/assets/rulebook-lod-1.svg)

![Weather and Climate cover](covers/weather-and-climate.svg)

**Descriptor:** [weather-and-climate.book.json](weather-and-climate.book.json)  
**Status:** Foundational sourcebook. This is a teaching and simulation contract, not a claim of an operational forecasting system.

## 1. The atmosphere is a changing physical system

Weather is the evolving state of the atmosphere. Climate describes the statistical behavior and distribution of weather over longer periods. They are related, but they are not interchangeable. A storm is an event; a climate description concerns the frequency, range, seasonality, and persistence of many events.

The atmosphere interacts with terrain, oceans, lakes, ice, vegetation, and built environments. A mountain changes airflow and precipitation patterns; a warm ocean can supply moisture and energy; snow cover changes surface energy exchange. These interactions make weather a major driver of how terrain changes.

## 2. Atmospheric state

A model or observation should distinguish:
- pressure;
- temperature;
- density;
- water-vapor content and humidity measures;
- wind velocity and direction;
- cloud and hydrometeor state;
- precipitation type and rate;
- visibility and relevant aerosol content;
- radiative and surface energy fluxes.

Every quantity needs units, a reference frame, location, altitude or vertical coordinate, and time. Wind direction conventions must state whether direction indicates where wind comes from or travels toward. Observations, forecasts, reanalysis, and synthetic scenario inputs must be labeled distinctly.

## 3. Pressure, temperature, and circulation

Pressure gradients drive acceleration; heating and cooling create density differences and influence buoyancy and circulation. Rotation, friction, terrain, moisture, and boundary-layer exchange shape atmospheric motion.

Teach the system from local pressure and temperature contrasts through convection, fronts, large-scale circulation, jet streams, and regional wind systems. A simplified model must state which forces and scales it omits. A static wind vector painted across a map is an environmental input, not a circulation simulation.

## 4. Moisture, clouds, and precipitation

Water changes phase and transports latent heat. Evaporation, condensation, freezing, melting, deposition, and precipitation connect the atmosphere to the surface water cycle.

Cloud formation depends on moisture, cooling, condensation nuclei, vertical motion, and atmospheric stability. Precipitation type and intensity depend on the vertical temperature/moisture profile and cloud processes. A teaching model may simplify microphysics, but it must disclose the approximation and avoid implying that a cloud icon is a cloud solver.

## 5. Weather systems

A sourcebook should explain and distinguish:
- local convection, showers, and thunderstorms;
- fronts and mid-latitude cyclones;
- tropical cyclones and their environment;
- high- and low-pressure systems;
- fog, frost, freezing rain, hail, snow, and other visibility or icing hazards;
- downslope winds, gap winds, sea/land breezes, and terrain-driven effects.

A scenario may prescribe a storm track or weather sequence. A predictive model must instead state its governing equations, initial and boundary data, resolution, parameterizations, uncertainty, and validation domain.

## 6. Climate, seasons, and variability

Climate is not a single average temperature. Describe distributions, extremes, variability, persistence, seasonality, and spatial differences. Relevant controls include latitude, elevation, distance from oceans, circulation, ocean currents, topography, albedo, land cover, and greenhouse forcing.

Use climate normals and longer records with their averaging periods and provenance. Averages can conceal dangerous extremes; preserve the distribution and relevant tail behavior. Distinguish natural variability, external forcing, observed trends, scenario assumptions, and attribution claims.

## 7. Terrain and weather interaction

Terrain affects weather through elevation, slope, aspect, roughness, barriers, valleys, and surface properties. Weather affects terrain through:
- rainfall and runoff;
- snow accumulation and melt;
- freeze-thaw cycling;
- wind loading and aeolian transport;
- heat, moisture, and chemical weathering;
- storm waves and coastal flooding;
- drought, vegetation stress, and fire-weather conditions.

The atmosphere supplies forcing. Hydrology, snow/ice, weathering, erosion, and coastal processes determine the resulting material and terrain changes. Keep these models separate and connect them through explicit variables and time intervals.

## 8. Weather hazards and consequences

Hazards are conditional physical events, not generic random penalties. Model or describe the relevant variables and exposure:
- wind speed, gusts, duration, direction, and structural exposure;
- rainfall intensity, duration, antecedent wetness, and drainage capacity;
- snow load, icing, visibility, and temperature;
- heat, cold, humidity, and exposure duration;
- lightning, hail, and convective severity;
- storm surge and wave exposure at coasts.

A hazard rule should state thresholds, uncertainty, spatial extent, duration, and the evidence behind them. Game-facing abstractions may simplify consequences, but the abstraction should remain traceable to the physical concept it represents.

## 9. Time, data, and simulation fidelity

Weather evolves on seconds-to-days scales; climate analysis spans years to geological eras. Do not run every process on one universal clock. Define forcing intervals, interpolation, aggregation, and coupling rules. Record source, version, units, spatial/temporal resolution, and missing-data treatment for every external dataset.

Separate four modes:
1. **Observed:** measurements with provenance and quality information.
2. **Forecast:** a model product with initialization, lead time, resolution, and uncertainty.
3. **Reanalysis or historical reconstruction:** a best-effort synthesis with stated methodology.
4. **Synthetic scenario:** authored inputs for education or play, explicitly not a real observation or forecast.

## 10. Validation and limitations

Compare models with appropriate observations and benchmark cases. Evaluate bias, error distributions, timing, spatial structure, extremes, and uncertainty—not only visual similarity. Resolution and parameterization limits must be stated.

Do not present a toy model as a forecast, a climate scenario as a prediction, or a fictional storm as a historical event. Operational weather warnings and safety decisions must rely on authoritative services and qualified professionals.

## 11. Connected books and implementation

- [Geology and Planetary Evolution](geology-and-planetary-evolution.md) covers planetary history and crustal structure.
- [Geomorphology and Surface Processes](geomorphology-and-surface-processes.md) covers weathering, erosion, transport, and deposition.
- [Ocean, Atmosphere, and Maritime Phenomena](ocean-atmosphere-maritime-phenomena.md) covers coupled ocean/atmosphere and maritime applications.
- [Physics capability catalogue](../PHYSICS_CAPABILITY_CATALOG.md) records implemented models and their limits.

The book establishes the vocabulary and evidence contract. It does not claim a global atmospheric circulation model, operational weather forecast, or validated climate projection engine.
