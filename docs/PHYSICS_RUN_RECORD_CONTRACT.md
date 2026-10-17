# Physics run-record contract

**Status: proposed interoperability contract; not yet a universal runtime type.** Existing experiment records remain authoritative for their domain-specific payloads. This document defines the smallest common envelope that tools, renderers, and other Workshop repositories can rely on without flattening every physical model into the same state shape.

## Why an envelope, not a universal observation

A projectile trajectory, a room-reflection path, an electrical bus, and a thermal body do not share the same result schema. They do share a need to answer:

1. Which model produced this record, and which version?
2. Which inputs and conventions were declared?
3. What units and logical time apply?
4. Is this computed output, supplied reference data, an estimate, or an illustrative fixture?
5. What assumptions and validation evidence limit the conclusions?

The envelope identifies the run. The payload remains domain-owned.

## Required envelope

| Field | Type | Meaning |
|---|---|---|
| `schemaVersion` | positive integer | Version of the record envelope, independent of the model version |
| `modelId` | non-empty string | Stable, namespaced model identity |
| `modelVersion` | positive integer or version string | Version of the model definition used |
| `status` | controlled string | Epistemic/production status of the result |
| `definition` | object for normalized records; optional for legacy records | Declared inputs and initial conditions |
| `result` or domain payload | object | Domain-specific observation(s), never guessed by the envelope |
| `limitations` | array of strings | Known exclusions and conditions on interpretation |

Existing records predate this contract and may use domain-specific names such as `electricalObservation`, `samples`, `directObservation`, or `reflections` instead of a single `result` object. Legacy records may also keep inputs such as `room`, `source`, and `listener` at the top level rather than grouping them under `definition`. They remain valid legacy shapes until deliberately migrated; consumers must preserve those fields and must not rename payloads merely to satisfy superficial uniformity.

## Recommended metadata

- **`units`**: explicit SI symbols or clearly defined unit strings for quantities not self-describing from field names.
- **`logicalTime`**: model-local time basis, start, step size, step count, and/or observation time when applicable. Static observations may legitimately have no time series.
- **`provenance`**: source identifiers, versions, retrieval/measurement date where relevant, and whether values are measured, sourced, estimated, or illustrative. Never invent a source citation.
- **`validation`**: named checks, tolerances, residuals, reference data, or test identifiers, distinguishing checks actually run from planned checks.
- **`runId`**: stable identity for a particular execution when runs need to be compared or referenced. Deterministic fixtures may choose a fixed ID; live runs should not pretend to be the same execution.
- **`coordinateConvention`**: axis orientation, handedness, origin, and frame identity whenever spatial values appear.
- **`assumptions`**: machine-readable assumptions when consumers need to branch on them; human-readable explanation should remain available.

These are conditional requirements: include metadata when it affects interpretation, reproducibility, comparison, or safe composition. Do not add empty placeholders to every record.

## Status vocabulary

Use one of these meanings (the exact serialized spelling can be adapted by an existing schema):

- **computed** — produced by the identified executable model for the recorded inputs.
- **measured** — captured from a measurement instrument or empirical observation; include measurement provenance.
- **sourced** — copied from an identified external source; retain citation and source version/date where available.
- **estimated** — inferred or approximated rather than directly measured or calculated by the stated model; state method and uncertainty when known.
- **illustrative** — chosen to explain a concept, not asserted as an empirical property.
- **invalid** — input or execution failed validation; include diagnostics and do not present partial output as a successful result.

A computed record can still use illustrative input values. Status must not imply that the model has been experimentally validated.

## Validation and determinism rules

1. **Units and frames are part of meaning.** A number without a known unit or spatial convention is not safely composable.
2. **Separate declared inputs from computed outputs.** A user-declared absorption fraction is not a predicted material efficiency; a chosen reflection coefficient is not a measured wall property.
3. **Make logical time explicit for time-dependent runs.** Record the time step and sample semantics, including whether the initial state counts as a sample.
4. **Keep validation evidence honest.** An algebraic accounting identity, a unit test, a numerical reference comparison, and real-world validation are different evidence classes.
5. **Preserve domain detail.** Do not force every observation into position/velocity, and do not force static fields into time series.
6. **Reproducibility is executable.** Where a deterministic fixture is checked in, CI should regenerate it from the actual model and compare outputs. A checked-in JSON file by itself is not proof of model execution.
7. **Unknown future fields should be ignorable.** Consumers should reject unsupported required schema versions, not fail because a producer adds optional metadata.

## Current evidence and next migration

The electrical-to-lumped-thermal experiment demonstrates a computed, deterministic coupling record with explicit units, declared definition, computed electrical observation, result, and limitations. Its 50% energy-transfer fraction is an input, not an empirical material parameter. The C# generator calls the model; CI verifies the record and SVG against fresh output.

The projectile, free-fall, acoustic, contact-impulse, and room-reflection experiments retain their domain-owned record structures. The electrical-to-thermal fixture now carries a provenance-bearing, explicitly illustrative specific-heat property: value and property unit, evidence status, source/fixture version, validity-condition quantity and unit, valid range, and conditions. The [material-property definition](../Physics/Materials/MaterialPropertyDefinition.cs) validates this metadata but does not implement temperature-dependent material behavior or make the fixture value empirical.

The run-record catalog and validator now demonstrate cross-domain metadata discovery, and the catalog invokes the shared validator before indexing each record. Regression tests confirm malformed status and empty limitation entries are rejected rather than silently indexed. The payload-aware inspector described below has already crossed the initial acceptance gate for four domains while preserving each domain's original payload shape. The remaining interoperability gate is external consumption or a second independent implementation, not another in-repository proof of concept.

## Acceptance evidence and remaining gate

The first cross-domain acceptance gate now has a concrete demonstrator: [inspect_physics_run_records.py](../tools/inspect_physics_run_records.py) reads acoustic observations, room-reflection paths, electrical-to-thermal coupling, and two-body thermal conduction through their shared envelope. It extracts domain-specific summary values while preserving every non-envelope field—including legacy top-level inputs—and passing through recognized optional envelope metadata such as units, logical time, provenance, validation evidence, run identity, coordinate convention, and assumptions. CI executes the consumer and tests that the payloads and metadata are preserved.

The version-1 validator and regression tests reject missing model identity, unsupported schema versions, unknown status, malformed units/provenance/validation metadata, boolean-as-integer versions, and malformed limitations. The [run-record catalog](experiments/run-record-catalog.md) remains a discovery index; the inspector is the payload-aware example.

**This does not promote the proposal to a runtime API.** The next gate is a real consumer outside this repository (or a second independent implementation) and review of schema evolution, provenance, and validation semantics. The contract remains guidance until that interoperability is demonstrated; no universal physics result schema is claimed.
