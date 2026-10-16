#!/usr/bin/env python3
"""Update or verify the generated run-summary section in the trajectory Markdown page.

The summary is a presentation of the serialized experiment record. It never
recomputes the trajectory or edits prose outside the marked section.
"""
import argparse
import json
import math
from pathlib import Path

BEGIN = "<!-- BEGIN GENERATED RUN SUMMARY -->"
END = "<!-- END GENERATED RUN SUMMARY -->"


def number(value):
    if not isinstance(value, (int, float)) or not math.isfinite(value):
        raise ValueError(f"expected a finite number, got {value!r}")
    return format(value, ".6g")


def vector(value, unit):
    if not isinstance(value, dict) or any(axis not in value for axis in ("x", "y", "z")):
        raise ValueError("expected a vector with x, y, and z components")
    return f"({number(value['x'])}, {number(value['y'])}, {number(value['z'])}) {unit}"


def render(record):
    definition = record.get("definition")
    samples = record.get("samples")
    units = record.get("units")
    if record.get("schemaVersion") != 1:
        raise ValueError("unsupported or missing run-record schemaVersion")
    if not isinstance(definition, dict) or not isinstance(samples, list) or not samples:
        raise ValueError("run record requires a definition and at least one sample")
    if not isinstance(units, dict):
        raise ValueError("run record requires unit metadata")
    initial = definition.get("initialState", {})
    final = samples[-1]
    acceleration_unit = units.get("acceleration", "unit unspecified")
    position_unit = units.get("position", "unit unspecified")
    velocity_unit = units.get("velocity", "unit unspecified")
    time_unit = units.get("elapsedTime", "unit unspecified")
    duration = final["elapsedTime"] - samples[0]["elapsedTime"]
    lines = [
        "## Declared experiment",
        "",
        "| Input / outcome | Recorded value |",
        "|---|---:|",
        f"| Model identity | `{record.get('modelId', 'unknown-model')}` (model version {record.get('modelVersion', 'unknown')}; schema {record['schemaVersion']}) |",
        f"| Initial position | {vector(initial.get('position'), position_unit)} |",
        f"| Initial velocity | {vector(initial.get('velocity'), velocity_unit)} |",
        f"| Constant acceleration | {vector(definition.get('acceleration'), acceleration_unit)} |",
        f"| Coordinate frame | {definition.get('coordinateFrame', 'unspecified')} |",
        f"| Fixed timestep | {number(definition.get('deltaTime'))} {time_unit} |",
        f"| Completed steps | {len(samples) - 1} |",
        f"| Recorded samples | {len(samples)}, including the initial state |",
        f"| Simulated duration | {number(duration)} {time_unit} |",
        f"| Final position | {vector(final.get('position'), position_unit)} |",
        f"| Final velocity | {vector(final.get('velocity'), velocity_unit)} |",
        "",
        f"<!-- Record-derived: model={record.get('modelId', 'unknown-model')}; samples={len(samples)}; duration={number(duration)} {time_unit}. -->",
    ]
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path, help="serialized C# experiment run record")
    parser.add_argument("markdown_page", type=Path, help="Markdown page containing generated-section markers")
    parser.add_argument("--check", action="store_true", help="fail if the marked section is stale")
    args = parser.parse_args()

    record = json.loads(args.run_record.read_text(encoding="utf-8"))
    generated = render(record)
    page = args.markdown_page.read_text(encoding="utf-8")
    if page.count(BEGIN) != 1 or page.count(END) != 1 or page.index(BEGIN) >= page.index(END):
        raise SystemExit("Markdown page must contain exactly one ordered pair of generated-summary markers")
    start = page.index(BEGIN) + len(BEGIN)
    finish = page.index(END)
    current = page[start:finish].strip("\n")
    if args.check:
        if current != generated:
            raise SystemExit("Markdown run summary is stale: regenerate it from the run record")
        print("Markdown run summary matches the supplied run record.")
        return
    updated = page[:start] + "\n" + generated + "\n" + page[finish:]
    args.markdown_page.write_text(updated, encoding="utf-8", newline="\n")
    print(f"Updated generated run summary in {args.markdown_page}")


if __name__ == "__main__":
    main()
