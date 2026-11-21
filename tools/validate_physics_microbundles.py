#!/usr/bin/env python3
"""Fail-closed build-time validation for Physics MicroBundle manifests."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

from jsonschema import Draft202012Validator

ROOT = Path(__file__).resolve().parents[1]
SCHEMA_PATH = ROOT / "schemas" / "physics-microbundle.schema.json"
MANIFESTS = sorted((ROOT / "microbundles").glob("*/microbundle.json"))
ADAPTER_SOURCES = sorted((ROOT / "Physics" / "MicroBundles").rglob("*.cs"))
ADAPTER_CLASS_PATTERN = re.compile(
    r"public\\s+sealed\\s+class\\s+(\\w+)([^{}]*)\\{([\\s\\S]*?)(?=\\npublic\\s+sealed\\s+class|\\Z)"
)
BUNDLE_ID_PATTERN = re.compile(r"public\\s+const\\s+ulong\\s+BundleId\\s*=\\s*(\\d+)\\s*;")
PROVIDER_ID_PATTERN = re.compile(r'public\\s+const\\s+string\\s+ProviderId\\s*=\\s*"([^"]+)"\\s*;')
DESCRIPTOR_VERSION_PATTERN = re.compile(r'new\\s*\\(\\s*BundleId\\s*,\\s*"([^"]+)"')
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

    manifest_bundle_ids: dict[int, Path] = {}
    manifest_provider_ids: dict[str, Path] = {}
    checked_references = 0
    runtime_adapters: dict[int, tuple[str, set[str], str]] = {}

    for source_path in ADAPTER_SOURCES:
        source = source_path.read_text(encoding="utf-8")
        for match in ADAPTER_CLASS_PATTERN.finditer(source):
            class_name, declaration, body = match.groups()
            if "IMicroBundle" not in declaration:
                continue
            bundle_values = BUNDLE_ID_PATTERN.findall(body)
            provider_values = PROVIDER_ID_PATTERN.findall(body)
            versions = DESCRIPTOR_VERSION_PATTERN.findall(body)
            if len(bundle_values) != 1 or not provider_values or len(versions) != 1:
                fail(
                    f"{source_path.relative_to(ROOT)}:{class_name}: "
                    "IMicroBundle adapter must declare exactly one BundleId, "
                    "at least one ProviderId, and one descriptor version"
                )
            bundle_id = int(bundle_values[0])
            if bundle_id in runtime_adapters:
                fail(
                    f"Duplicate runtime bundle ID {bundle_id}: "
                    f"{runtime_adapters[bundle_id][2]} and {source_path.relative_to(ROOT)}"
                )
            runtime_adapters[bundle_id] = (
                versions[0],
                set(provider_values),
                str(source_path.relative_to(ROOT)),
            )

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
        if bundle_id in manifest_bundle_ids:
            fail(
                f"Duplicate bundle ID {bundle_id}: "
                f"{manifest_bundle_ids[bundle_id]} and {relative_manifest}"
            )
        manifest_bundle_ids[bundle_id] = relative_manifest

        runtime_identity = runtime_adapters.get(bundle_id)
        if runtime_identity is None:
            fail(
                f"{relative_manifest}: bundle ID {bundle_id} has no matching "
                "IMicroBundle adapter under Physics/MicroBundles"
            )
        runtime_version, runtime_providers, runtime_source = runtime_identity
        if manifest["bundle"]["version"] != runtime_version:
            fail(
                f"{relative_manifest}: bundle version {manifest['bundle']['version']} "
                f"does not match {runtime_source} descriptor version {runtime_version}"
            )

        declared_providers = manifest["bundle"]["providerIds"]
        if set(declared_providers) != runtime_providers:
            fail(
                f"{relative_manifest}: provider IDs {sorted(declared_providers)} "
                f"do not match {runtime_source} provider constants {sorted(runtime_providers)}"
            )

        capability_id = manifest["capability"]["id"]
        if capability_id not in declared_providers:
            fail(f"{relative_manifest}: capability.id is not declared in bundle.providerIds")
        for provider_id in declared_providers:
            if provider_id in manifest_provider_ids:
                fail(
                    f"Duplicate provider ID {provider_id}: "
                    f"{manifest_provider_ids[provider_id]} and {relative_manifest}"
                )
            manifest_provider_ids[provider_id] = relative_manifest

        for section, field in REFERENCE_FIELDS:
            for reference in manifest[section][field]:
                candidate = ROOT / reference
                if not candidate.is_file():
                    fail(f"{relative_manifest}: {section}.{field} references missing file {reference}")
                checked_references += 1

    print(
        f"Validated {len(MANIFESTS)} Physics MicroBundle manifests against "
        f"JSON Schema; {len(manifest_bundle_ids)} unique bundle IDs, "
        f"{len(manifest_provider_ids)} unique provider IDs matched to their runtime adapters and versions, and "
        f"{checked_references} existing file references."
    )


if __name__ == "__main__":
    main()
