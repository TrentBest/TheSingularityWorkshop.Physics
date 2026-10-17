# Physics run-record catalog

Generated from checked-in run records using the shared envelope only. Domain payloads remain owned by their models, including legacy top-level inputs.

| Model | Version | Status | Domain payload fields | Unit keys | Limitations |
|---|---:|---|---|---|---:|
| [acoustics.free-field-point-source-observation](./acoustic-observation.run.json) | 1 | `computed-from-physics-model` | `observation` | `position`, `propagationDelay`, `receivedLevel`, `direction` | 2 |
| [acoustics.first-order-room-reflection](./first-order-room-reflection.run.json) | 1 | `computed-from-physics-model` | `room`, `source`, `listener`, `speedOfSoundMetersPerSecond`, `directObservation`, `reflections` | — | 4 |
| [coupling.electrical-to-lumped-thermal](./electrical-thermal-coupling.run.json) | 1 | `computed-from-physics-model` | `electricalObservation`, `result` | `voltage`, `resistance`, `current`, `power`, `duration`, `energy`, `mass`, `specificHeatCapacity`, `temperature` | 4 |
| [thermodynamics.lumped-body-conduction](./thermal-conduction.run.json) | 1 | `computed-from-physics-model` | `result` | `temperature`, `mass`, `specificHeatCapacity`, `thermalConductivity`, `area`, `separation`, `duration`, `thermalConductance`, `energy` | 5 |

This index does not claim that records share a physical result schema. It proves only that a consumer can discover model identity, status, all model-owned payload fields, units when supplied, and limitations through the common envelope boundary.
