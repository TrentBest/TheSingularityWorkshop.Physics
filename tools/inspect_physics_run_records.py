#!/usr/bin/env python3
"""Inspect representative Physics records through one shared envelope while preserving domain payloads."""
from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any

DOMAIN_PAYLOAD_FIELDS = {
    "acoustics.free-field-point-source-observation": ("observation",),
    "acoustics.first-order-room-reflection": ("directObservation", "reflections"),
    "coupling.electrical-to-lumped-thermal": ("electricalObservation", "result"),
    "thermodynamics.lumped-body-conduction": ("result",),
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

    payload_fields = DOMAIN_PAYLOAD_FIELDS[model_id]
    return {
        "envelope": {
            "schemaVersion": record["schemaVersion"],
            "modelId": model_id,
            "modelVersion": record["modelVersion"],
            "status": record["status"],
            **({"units": record["units"]} if "units" in record else {}),
            **({"definition": record["definition"]} if "definition" in record else {}),
            "limitations": record["limitations"],
        },
        "summary": summary,
        "domainPayload": {field: record[field] for field in payload_fields},
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
