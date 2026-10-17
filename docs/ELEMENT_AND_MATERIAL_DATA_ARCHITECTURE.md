# Element, Material, and Property Data Architecture

**Status:** data architecture contract and implementation plan. This document does not claim that a complete 118-element/property warehouse is already populated.

**Owner:** Physics owns the physical meaning, typed contracts, applicability rules, and model evaluation. Data bundles may provide sourced records. MicroBundle composition discovers and arbitrates bundles; it does not belong inside a Physics data class. Presentation clients consume records and observations rather than keeping a competing periodic table.

Related: [Physics domain map](PHYSICS_DOMAIN_MAP.md) · [Physics + Math: chemistry and combustion](PHYSICS_MATH_CHEMISTRY_COMBUSTION.md) · [Surface combustion model](SURFACE_COMBUSTION_MODEL.md) · [Capability catalog](PHYSICS_CAPABILITY_CATALOG.md).

## 1. The goal is breadth, not a tiny periodic-table card

The data model must be capable of representing the broad body of established chemical and physical knowledge found in a serious chemistry reference: atomic identity, electron structure, phase behavior, thermodynamics, transport, electrical and magnetic behavior, mechanical response, chemical compatibility, and nuclear behavior. A small identity record is only the join key—not the intended limit of the system.

The fact that a property exists in science does not mean it is a single context-free number. Values may depend on temperature, pressure, phase, crystal structure, purity, composition, orientation, field strength, wavelength, sample history, and measurement method. A reference book may summarize one convention or set of conditions; our model must preserve those conditions rather than flatten them away.

**A missing value is not zero.** Each property must distinguish at least:
- known value or range;
- unknown / not yet sourced;
- not applicable under the declared subject/model;
- not measured or not reported by this source;
- intentionally omitted from a selected data profile;
- out of the model's validity range.

Never infer “does not participate in radioactivity” merely because no half-life is present. Stable nuclides still have nuclear identity and evidence; radioactivity and decay behavior belong to nuclide-level records and applicable nuclear models.

## 2. Separate identity, facts, and behavior

Keep these as related, versioned records with stable identifiers:

1. **Element identity** — atomic number, canonical symbol, name/aliases, identity source and revision.
2. **Nuclide/isotope** — proton and neutron counts, nuclide identity, mass data, abundance where defined, decay modes/half-life and uncertainties when supported. A nuclide is not interchangeable with the average atomic weight shown for an element.
3. **Property fact** — which subject and property it describes, value/range, unit, conditions, phase/composition, method, uncertainty, evidence class, provenance, and applicability.
4. **Material/mixture definition** — a specific substance, alloy, compound, mixture, grade, or sample definition with composition and state. Bulk material properties do not automatically follow from an element record.
5. **Reaction/interaction profile** — participating species, balanced stoichiometry or declared interaction law, operating conditions, products, energy exchange, rate/model identity, evidence and limits.
6. **Constitutive/physical model** — the rule that uses facts to calculate a response, including its equations, valid range, assumptions, numerical method, and validation tests.
7. **Observation/result** — a calculation or measurement with input identities, model/data versions, units, timestamp or logical step, and relevant residuals/uncertainty.

This avoids a giant “Element” object that conflates a chemical identity, an isotope, a chunk of copper, and a solver. The relationships remain traversable so an educator or simulation can inspect how a result was obtained.

## 3. Property families

The schema should support typed property families without requiring every subject to fill every field:

| Family | Examples |
|---|---|
| Identity and atomic structure | atomic number, symbol, electron configuration, oxidation states, valence conventions |
| Mass and composition | nuclide masses, isotopic abundance, molar mass, elemental/chemical composition |
| Phase and thermodynamics | phase at stated conditions, density, heat capacity, enthalpy, entropy, melting/boiling/sublimation points, latent heat, equations of state |
| Transport | thermal conductivity, diffusivity, viscosity, species diffusivity, electrical conductivity |
| Electrical and magnetic | resistivity/conductivity, permittivity, permeability, work function, magnetic response, breakdown behavior where applicable |
| Optical and radiative | refractive index, absorption/emission spectra, emissivity, reflectivity, wavelength/temperature dependencies |
| Mechanical and structural | elastic constants, strength, hardness, ductility, toughness, creep, fatigue, anisotropy, fracture data |
| Chemical | reactivity, compatible/incompatible species, equilibrium data, reaction rates, corrosion, oxidation, flammability and ignition evidence |
| Nuclear | nuclide mass/energy, decay modes, half-life, radiation emissions, branching ratios, daughter products, cross-sections and dose/interaction models where sourced |
| Environmental and sample context | pressure, temperature, humidity/moisture, purity, grain structure, treatment, orientation, measurement conditions |

This table defines representational breadth, not a claim that every listed family is populated or solved today. Some properties are measured data; others are calculated from explicit equations; others require a model-specific table or specialist MicroBundle.

## 4. Core profile plus optional extended profiles

A consumer should be able to request a useful profile without loading every specialist dataset.

- **Core identity profile:** canonical identities and enough source/version metadata to join related records.
- **General physical/chemical profile:** common thermodynamic, phase, transport, electrical, mechanical and chemical facts where defensibly sourced.
- **Specialist profiles:** optical/spectral, advanced mechanical, corrosion, combustion, electrochemistry, semiconductor behavior, nuclear decay/radiation, and other domain-specific data.
- **Material profiles:** real alloys, grades, compounds, mixtures, fuels, wick/wax/paper/wood examples, and user-authored material definitions with explicit composition and evidence.
- **Derived profile:** reproducibly calculated quantities whose derivation, source inputs, equation/model version, and uncertainty propagation are recorded.

Profiles can be composed, extended, or narrowed by a host. A missing specialist profile must not invalidate the core identity. Conversely, loading the core profile must not imply that a specialized calculation is supported.

Nuclear data is an example of a specialized capability: ordinary material/thermal/combustion use should not need to load every decay-chain and radiation-interaction table. When a scenario needs those behaviors, a nuclear profile supplies the applicable nuclide data and model providers. This is a data-loading and capability boundary, not a claim that radiation can always be ignored physically.

## 5. Provenance and scientific integrity

Every externally sourced property value needs provenance at the property-record level—not merely a citation on the element as a whole. Preserve:
- source/dataset identifier and revision;
- source record or stable URL/citation;
- license and redistribution terms;
- value, unit, significant figures and/or uncertainty as supplied;
- temperature, pressure, phase, composition, method, and other reported conditions;
- whether the value is measured, critically evaluated, calculated, estimated, inferred, or illustrative;
- any transformation, unit conversion, normalization, or derivation applied by our importer.

A dataset's existence is not permission to redistribute it. Before committing bulk data, review the source's actual license/terms, attribution requirements, revision policy, and whether derived/redistributed data is permitted. Prefer authoritative, machine-readable sources with clear reuse terms. Keep import scripts and source manifests reproducible. Do not hand-transcribe a full periodic table from a copyrighted book or silently blend values from incompatible sources.

**Do not let data collection block the schema.** We can implement contracts, validation, import adapters, and tests before choosing or loading the first full dataset. Until an admissible source is selected, do not claim the complete element catalog exists.

## 6. Physics evaluation consumes evidence; it does not invent it

A model declares which properties it requires and the conditions under which they are valid. Resolution returns explicit outcomes such as supported, missing evidence, incompatible conditions, out of range, or conflicting evidence. Conflicting providers must be resolved by declared arbitration policy or exposed to the caller; never silently choose whichever number was loaded last.

Prefer equations when the model and inputs support them; use sourced tables/LUTs when justified. Derived facts should retain links to their inputs and method. Never store a derived value as if it were an independent measurement.

For combustion, the relevant path is material composition and sourced properties → applicable reaction profile → ignition/transport/energy model → observed heat and products → presentation particles. A lighter contributes a flame-source condition; it does not issue a command that a candle must light. If fuel chemistry, oxygen, energy transfer, or model coverage is insufficient, report that limitation rather than fabricate a result.

For an electrical lesson, the same approach can connect source voltage, winding/phase relationships, relay coil behavior, pressure switch, limit switch, and valve state through explicit circuit, electromagnetic, mechanical, and control-state models. The educational experience should expose the causal chain and its assumptions, not merely animate a relay because a script says so.

## 7. Acceptance criteria

- Element identity remains separate from nuclides, property facts, materials, reactions, and models.
- All values have explicit units and condition/applicability metadata appropriate to their property family.
- Unknown, not applicable, omitted, and out-of-range are distinguishable.
- Source/license/version and evidence class survive import and serialization.
- Optional specialist profiles can be loaded independently without duplicating canonical identity.
- Conflicts and missing required facts produce explicit results.
- Derived values are reproducible from versioned inputs and named equations/models.
- Tests cover unit conversion, validity ranges, condition matching, provenance preservation, missing data, conflicting evidence, and deterministic derivation.
- The capability catalog claims only source-backed, tested behavior.

## 8. Implementation order

1. Extend the current element identity seed with shared provenance/property-value primitives without turning identity into a god-object.
2. Define typed records for nuclide identity and property applicability/status.
3. Define material composition and condition-qualified property records.
4. Define import manifests/adapters and license review before bulk population.
5. Populate a small, cited vertical slice spanning different families (for example, copper and iron plus a simple compound), retaining source and conditions.
6. Implement lookup and conflict/missing-evidence outcomes with tests.
7. Add one specialized provider/profile at a time, beginning with the domain experiment that can validate it.
8. Keep nuclear decay/radiation as a specialized, composable profile with explicit models and nuclide data.
9. Connect the data to combustion and educational experiences only as the corresponding calculations become executable and validated.

The goal is not to pretend every possible measurement is already known to the runtime. The goal is a schema and composable data pipeline broad enough to carry the known science, make its evidence inspectable, and say honestly what is known, conditional, unsupported, or not yet loaded.
