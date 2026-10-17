#!/usr/bin/env python3
"""Render a source/listener SVG from a serialized C# FreeFieldAcoustics observation."""
import argparse
import html
import json
import math
from pathlib import Path


def number(value, label):
    if not isinstance(value, (int, float)) or not math.isfinite(value):
        raise ValueError(f"{label} must be a finite number")
    return float(value)


def render(record):
    if record.get("schemaVersion") != 1 or record.get("modelId") != "acoustics.free-field-point-source-observation":
        raise ValueError("unsupported acoustic run record")
    definition = record["definition"]
    observation = record["observation"]
    source = definition["source"]
    listener = definition["listener"]
    sx, sy = (number(source["positionMeters"][axis], f"source {axis}") for axis in ("x", "y"))
    lx, ly = (number(listener["positionMeters"][axis], f"listener {axis}") for axis in ("x", "y"))
    distance = number(observation["distanceMeters"], "distance")
    delay = number(observation["propagationDelaySeconds"], "delay")
    level = number(observation["receivedLevelDbSpl"], "received level")
    speed = number(definition["speedOfSoundMetersPerSecond"], "sound speed")
    reference_level = number(source["referenceLevelDbSpl"], "reference level")
    reference_distance = number(source["referenceDistanceMeters"], "reference distance")
    if distance < 0 or delay < 0 or speed <= 0 or reference_distance <= 0:
        raise ValueError("distance/delay must be non-negative and speed/reference distance positive")

    # Fit the declared XY positions into a stable diagram, preserving their relative geometry.
    points = [(sx, sy), (lx, ly)]
    min_x, max_x = min(p[0] for p in points), max(p[0] for p in points)
    min_y, max_y = min(p[1] for p in points), max(p[1] for p in points)
    span = max(max_x - min_x, max_y - min_y, 1.0)
    scale = 200.0 / span
    project = lambda x, y: (100 + (x - min_x) * scale, 280 - (y - min_y) * scale)
    x1, y1 = project(sx, sy)
    x2, y2 = project(lx, ly)
    esc = lambda value: html.escape(str(value), quote=True)
    return f\'''<svg xmlns="http://www.w3.org/2000/svg" width="720" height="420" viewBox="0 0 720 420" role="img" aria-labelledby="title desc">
  <title id="title">Measured free-field acoustic observation</title>
  <desc id="desc">Source and listener positions and values computed by FreeFieldAcoustics. This is propagation metadata, not generated audio.</desc>
  <rect width="720" height="420" rx="18" fill="#101923"/>
  <text x="28" y="38" fill="#8de8ff" font-family="sans-serif" font-size="19" font-weight="700">FREE-FIELD OBSERVATION · MODEL OUTPUT</text>
  <line x1="48" y1="320" x2="672" y2="320" stroke="#334452" stroke-width="1"/>
  <line x1="48" y1="80" x2="48" y2="320" stroke="#334452" stroke-width="1"/>
  <line x1="{x1:.3f}" y1="{y1:.3f}" x2="{x2:.3f}" y2="{y2:.3f}" stroke="#8de8ff" stroke-width="3" stroke-dasharray="8 6"/>
  <circle cx="{x1:.3f}" cy="{y1:.3f}" r="10" fill="#ffbd69" stroke="#fff0d9" stroke-width="2"/>
  <circle cx="{x2:.3f}" cy="{y2:.3f}" r="10" fill="#8de8ff" stroke="#e3fbff" stroke-width="2"/>
  <text x="{x1:.3f}" y="{y1 - 20:.3f}" fill="#ffcf8d" text-anchor="middle" font-family="sans-serif" font-size="13">{esc(source["id"])}</text>
  <text x="{x2:.3f}" y="{y2 - 20:.3f}" fill="#a6efff" text-anchor="middle" font-family="sans-serif" font-size="13">{esc(listener["id"])}</text>
  <text x="390" y="350" fill="#e4edf4" font-family="sans-serif" font-size="15">Distance: {distance:.6g} m</text>
  <text x="390" y="373" fill="#e4edf4" font-family="sans-serif" font-size="15">Arrival delay: {delay:.8g} s at {speed:g} m/s</text>
  <text x="390" y="396" fill="#e4edf4" font-family="sans-serif" font-size="15">Estimated received level: {level:.6g} dB SPL</text>
  <text x="28" y="396" fill="#94a6b6" font-family="sans-serif" font-size="11">Reference: {reference_level:g} dB SPL at {reference_distance:g} m · 2D projection of declared XYZ positions</text>
</svg>
\'''


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("record", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--check", action="store_true", help="fail if output differs from the rendered record")
    args = parser.parse_args()
    actual = render(json.loads(args.record.read_text(encoding="utf-8")))
    if args.check:
        expected = args.output.read_text(encoding="utf-8")
        if expected != actual:
            raise SystemExit(f"{args.output} differs from the SVG rendered from {args.record}")
        print(f"{args.output} matches the serialized acoustic observation.")
        return
    args.output.write_text(actual, encoding="utf-8")
    print(f"Wrote acoustic observation SVG: {args.output}")


if __name__ == "__main__":
    main()
