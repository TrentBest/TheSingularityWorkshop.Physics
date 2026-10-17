#!/usr/bin/env python3
"""Inspect representative Physics records through one shared envelope while preserving domain payloads."""
from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any

ENVELOPE_FIELDS = {
    "schemaVersion", "modelId", "modelVersion", "status", "units", "definition",
    "logicalTime", "provenance", "validation", "runId", "coordinateConvention",
    "assumptions", "limitations",
}


def summarize(record: dict[str, Any]) -> dict[str, Any]:
    model_id = record["modelId"]
    if model_id == "acoustics.free-field-point-source-observation":
        observation = record["observation"]
        summary = {
            "distanceMeters": observation["distanceMeters"],
            "propagationDelaySeconds": observation["propagationDelaySeconds"],
            "receivedLevelDbSpl": observation["receivedLevelDbSpl"],
        }
    elif model_id == "acoustics.first-order-room-reflection":
        direct = record["directObservation"]
        reflections = record["reflections"]
        summary = {
            "directPathDistanceMeters": direct["distanceMeters"],
            "reflectionCount": len(reflections),
            "reflectionPathLengthsMeters": [item["pathLengthMeters"] for item in reflections],
        }
    elif model_id == "coupling.electrical-to-lumped-thermal":
        result = record["result"]
        summary = {
            "electricalEnergyJoules": result["electricalEnergyJoules"],
            "thermalEnergyTransferJoules": result["thermalEnergyTransferJoules"],
            "energyNotTransferredToThermalBodyJoules": result["energyNotTransferredToThermalBodyJoules"],
            "finalTemperatureKelvin": result["finalThermalState"]["temperatureKelvin"],
        }
    elif model_id == "thermodynamics.lumped-body-conduction":
        result = record["result"]
        summary = {
            "energyTransferredFromFirstToSecondJoules": result["energyTransferredFromFirstToSecondJoules"],
            "totalSensibleEnergyBeforeJoules": result["totalSensibleEnergyBeforeJoules"],
            "totalSensibleEnergyAfterJoules": result["totalSensibleEnergyAfterJoules"],
            "energyAccountingResidualJoules": result["energyAccountingResidualJoules"],
            "firstFinalTemperatureKelvin": result["finalFirst"]["temperatureKelvin"],
            "secondFinalTemperatureKelvin": result["finalSecond"]["temperatureKelvin"],
        }
    else:
        raise ValueError(f"unsupported modelId: {model_id}")

    envelope = {field: record[field] for field in ENVELOPE_FIELDS if field in record}
    # Preserve every non-envelope field, including legacy top-level inputs such as
    # room/source/listener. Do not silently discard model-owned data just because
    # an older record has not grouped it under `definition` yet.
    domain_payload = {field: value for field, value in record.items() if field not in ENVELOPE_FIELDS}
    return {
        "envelope": envelope,
        "summary": summary,
        "domainPayload": domain_payload,
    }


def inspect_path(path: Path) -> dict[str, Any]:
    record = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(record, dict):
        raise ValueError(f"{path}: record root must be an object")
    for field in ("schemaVersion", "modelId", "modelVersion", "status", "limitations"):
        if field not in record:
            raise ValueError(f"{path}: missing envelope field {field!r}")
    return {"sourcePath": path.as_posix(), **summarize(record)}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("records", nargs="+", type=Path)
    args = parser.parse_args()
    try:
        inspected = [inspect_path(path) for path in args.records]
    except (OSError, json.JSONDecodeError, KeyError, ValueError) as exc:
        parser.error(str(exc))
    print(json.dumps(inspected, indent=2, ensure_ascii=False, allow_nan=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
