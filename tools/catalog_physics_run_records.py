#!/usr/bin/env python3
"""Build a small cross-domain index using only the shared run-record envelope.

Domain payloads are named and linked, never normalized or rewritten.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

PAYLOAD_FIELDS = (
    "result", "samples", "electricalObservation", "directObservation",
    "reflections", "observation", "impacts", "contacts",
)


def load_record(path: Path) -> dict:
    record = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(record, dict):
        raise ValueError(f"{path}: record root must be an object")
    for key in ("schemaVersion", "modelId", "modelVersion", "status", "limitations"):
        if key not in record:
            raise ValueError(f"{path}: missing envelope field {key!r}")
    payloads = [key for key in PAYLOAD_FIELDS if key in record]
    if not payloads:
        raise ValueError(f"{path}: no recognized domain payload")
    if not isinstance(record["limitations"], list):
        raise ValueError(f"{path}: limitations must be an array")
    return {"path": path, "record": record, "payloads": payloads}


def render(entries: list[dict]) -> str:
    lines = [
        "# Physics run-record catalog",
        "",
        "Generated from checked-in run records using the shared envelope only. "
        "Domain payloads remain owned by their models.",
        "",
        "| Model | Version | Status | Domain payload fields | Unit keys | Limitations |",
        "|---|---:|---|---|---|---:|",
    ]
    for entry in entries:
        record = entry["record"]
        relpath = entry["path"].as_posix()
        model_id = record["modelId"]
        version = record["modelVersion"]
        status = record["status"]
        payloads = ", ".join(f"`{name}`" for name in entry["payloads"])
        units = record.get("units", {})
        unit_keys = ", ".join(f"`{key}`" for key in units) if isinstance(units, dict) else "—"
        lines.append(
            f"| [{model_id}](./{relpath}) | {version} | `{status}` | "
            f"{payloads} | {unit_keys or '—'} | {len(record['limitations'])} |"
        )
    lines.extend([
        "",
        "This index does not claim that records share a physical result schema. "
        "It proves only that a consumer can discover model identity, status, "
        "payload boundaries, units when supplied, and limitations through the "
        "common envelope.",
        "",
    ])
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("records", nargs="+", type=Path)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--check", action="store_true",
                        help="fail if output differs from the committed file")
    args = parser.parse_args()
    try:
        entries = [load_record(path) for path in args.records]
        generated = render(entries)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        parser.error(str(exc))
    if args.check:
        try:
            existing = args.output.read_text(encoding="utf-8")
        except OSError as exc:
            parser.error(str(exc))
        if existing != generated:
            print(f"{args.output}: differs from generated run-record catalog")
            return 1
        print(f"Run-record catalog is reproducible across {len(entries)} domains.")
        return 0
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(generated, encoding="utf-8")
    print(f"Wrote {args.output} from {len(entries)} run records.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
