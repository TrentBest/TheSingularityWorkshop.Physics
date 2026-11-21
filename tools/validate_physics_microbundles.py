#!/usr/bin/env python3
"""Fail-closed build-time validation for Physics MicroBundle manifests."""
from __future__ import annotations

import json
import sys
from pathlib import Path

from jsonschema import Draft202012Validator

ROOT = Path(__file__).resolve().parents[1]
SCHEMA_PATH = ROOT / "schemas" / "physics-microbundle.schema.json"
MANIFESTS = sorted((ROOT / "microbundles").glob("*/microbundle.json"))
REFERENCE_FIELDS = (
    ("evidence", "sourceFiles"),
    ("evidence", "testFiles"),
    ("education", "theoryFiles"),
    ("education", "experimentFiles"),
)


def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> None:
    if not SCHEMA_PATH.is_file():
        fail(f"Missing schema: {SCHEMA_PATH.relative_to(ROOT)}")
    schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
    Draft202012Validator.check_schema(schema)
    validator = Draft202012Validator(schema)
    if not MANIFESTS:
        fail("No microbundles/*/microbundle.json manifests found")

    bundle_ids: dict[int, Path] = {}
    provider_ids: dict[str, Path] = {}
    checked_references = 0

    for manifest_path in MANIFESTS:
        relative_manifest = manifest_path.relative_to(ROOT)
        try:
            manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            fail(f"{relative_manifest}: cannot parse JSON ({exc})")

        errors = sorted(validator.iter_errors(manifest), key=lambda e: list(e.absolute_path))
        if errors:
            for error in errors:
                location = ".".join(str(part) for part in error.absolute_path) or "<root>"
                print(f"ERROR: {relative_manifest}:{location}: {error.message}", file=sys.stderr)
            raise SystemExit(1)

        bundle_id = manifest["bundle"]["id"]
        if bundle_id in bundle_ids:
            fail(f"Duplicate bundle ID {bundle_id}: {bundle_ids[bundle_id]} and {relative_manifest}")
        bundle_ids[bundle_id] = relative_manifest

        declared_providers = manifest["bundle"]["providerIds"]
        capability_id = manifest["capability"]["id"]
        if capability_id not in declared_providers:
            fail(f"{relative_manifest}: capability.id is not declared in bundle.providerIds")
        for provider_id in declared_providers:
            if provider_id in provider_ids:
                fail(f"Duplicate provider ID {provider_id}: {provider_ids[provider_id]} and {relative_manifest}")
            provider_ids[provider_id] = relative_manifest

        for section, field in REFERENCE_FIELDS:
            for reference in manifest[section][field]:
                candidate = ROOT / reference
                if not candidate.is_file():
                    fail(f"{relative_manifest}: {section}.{field} references missing file {reference}")
                checked_references += 1

    print(
        f"Validated {len(MANIFESTS)} Physics MicroBundle manifests against "
        f"JSON Schema; {len(bundle_ids)} unique bundle IDs, "
        f"{len(provider_ids)} unique provider IDs, and "
        f"{checked_references} existing file references."
    )


if __name__ == "__main__":
    main()
