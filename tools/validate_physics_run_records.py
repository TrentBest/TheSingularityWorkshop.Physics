#!/usr/bin/env python3
"""Validate the minimal interoperable envelope on representative Physics run records.

This intentionally does not normalize or interpret domain-specific payloads.
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

REQUIRED = ("schemaVersion", "modelId", "modelVersion", "status", "limitations")
ACCEPTED_STATUSES = {
    "computed",
    "computed-from-physics-model",  # existing serialized spelling
    "measured",
    "sourced",
    "estimated",
    "illustrative",
    "invalid",
}
DOMAIN_PAYLOADS = (
    "result",
    "samples",
    "electricalObservation",
    "directObservation",
    "reflections",
    "observation",
    "impacts",
    "contacts",
)


def validate(path: Path) -> list[str]:
    errors: list[str] = []
    try:
        record = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        return [f"{path}: cannot read JSON: {exc}"]

    if not isinstance(record, dict):
        return [f"{path}: record root must be an object"]

    for key in REQUIRED:
        if key not in record:
            errors.append(f"{path}: missing required envelope field '{key}'")

    schema_version = record.get("schemaVersion")
    if (
        not isinstance(schema_version, int)
        or isinstance(schema_version, bool)
        or schema_version < 1
    ):
        errors.append(f"{path}: schemaVersion must be a positive integer")

    if not isinstance(record.get("modelId"), str) or not record.get("modelId", "").strip():
        errors.append(f"{path}: modelId must be a non-empty string")

    model_version = record.get("modelVersion")
    if not (
        (isinstance(model_version, int) and not isinstance(model_version, bool) and model_version > 0)
        or (isinstance(model_version, str) and model_version.strip())
    ):
        errors.append(f"{path}: modelVersion must be a positive integer or non-empty string")

    if record.get("status") not in ACCEPTED_STATUSES:
        errors.append(f"{path}: status must use a recognized epistemic status")

    limitations = record.get("limitations")
    if not isinstance(limitations, list) or not all(
        isinstance(item, str) and item.strip() for item in limitations
    ):
        errors.append(f"{path}: limitations must be an array of non-empty strings")

    if not any(key in record for key in DOMAIN_PAYLOADS):
        errors.append(f"{path}: no recognized domain payload field found")

    if "definition" in record and not isinstance(record["definition"], dict):
        errors.append(f"{path}: definition, when present, must be an object")
    if "units" in record and not isinstance(record["units"], dict):
        errors.append(f"{path}: units, when present, must be an object")
    if "provenance" in record and not isinstance(record["provenance"], (dict, list)):
        errors.append(f"{path}: provenance, when present, must be an object or array")
    if "validation" in record and not isinstance(record["validation"], (dict, list)):
        errors.append(f"{path}: validation, when present, must be an object or array")

    return errors


def main(args: list[str]) -> int:
    if not args:
        print("Usage: validate_physics_run_records.py <record.json> [...]", file=sys.stderr)
        return 2

    errors = [error for arg in args for error in validate(Path(arg))]
    if errors:
        print("\\n".join(errors), file=sys.stderr)
        return 1

    print(f"Validated minimal run-record envelope for {len(args)} record(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
