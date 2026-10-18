#!/usr/bin/env python3
"""Render the falling-body drag run record; this script does not solve the physics."""
import argparse
import html
import json
import math
from pathlib import Path


def render(record):
    if record.get("schemaVersion") != 1 or record.get("modelId") != "aerodynamics.falling-body-quadratic-drag":
        raise ValueError("unsupported falling-body drag run record")
    vacuum = record.get("vacuumSamples")
    drag = record.get("dragSamples")
    if not isinstance(vacuum, list) or not isinstance(drag, list) or len(vacuum) != len(drag) or len(drag) < 2:
        raise ValueError("expected equally sized vacuum and drag sample series")
    for label, samples in (("vacuum", vacuum), ("drag", drag)):
        for index, sample in enumerate(samples):
            if not all(isinstance(sample.get(key), (int, float)) and math.isfinite(sample[key])
                       for key in ("elapsedTime", "height", "velocity")):
                raise ValueError(f"invalid {label} state at sample {index}")
    if not record["validation"]["dragReducesDownwardSpeed"] or not record["validation"]["dragReducesDistanceFallen"]:
        raise ValueError("record does not satisfy the declared drag comparison")
    duration = drag[-1]["elapsedTime"] - drag[0]["elapsedTime"]
    if duration <= 0:
        raise ValueError("duration must be positive")
    all_heights = [s["height"] for s in vacuum + drag]
    low, high = min(all_heights), max(all_heights)
    if math.isclose(low, high):
        raise ValueError("height range is degenerate")
    def esc(value):
        return html.escape(str(value), quote=True)
    x0, x1 = 100, 900
    top, bottom = 130, 390
    vtop, vbottom = 465, 565
    def x(t):
        return x0 + (t - drag[0]["elapsedTime"]) / duration * (x1 - x0)
    def yh(value):
        return bottom - (value - low) / (high - low) * (bottom - top)
    vmax = max(abs(s["velocity"]) for s in vacuum + drag) or 1
    def yv(value):
        return (vtop + vbottom) / 2 - value / vmax * ((vbottom - vtop) / 2)
    def points(samples, y):
        return " ".join(f"{x(s['elapsedTime']):.2f},{y(s['height'] if y is yh else s['velocity']):.2f}" for s in samples)
    p = [
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 650" role="img" aria-labelledby="title desc">',
        '<title id="title">Falling body: drag changes the motion</title>',
        f'<desc id="desc">Two trajectories from {len(drag)} recorded C# samples each. The no-drag case falls farther and reaches a greater downward speed than the same body with quadratic drag. The SVG renders recorded values and does not solve the physics.</desc>',
        '<rect width="1000" height="650" rx="18" fill="#f7fafc"/>',
        '<text x="36" y="43" font-family="Segoe UI,Arial,sans-serif" font-size="26" font-weight="700" fill="#183047">Same body. Same gravity. One difference: air resistance.</text>',
        f'<text x="36" y="72" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#526578">Actual recorded C# states · {len(drag)} samples per case · {duration:.2f} s · semi-implicit Euler</text>',
        '<rect x="36" y="100" width="928" height="310" rx="14" fill="#fff" stroke="#d9e2ea"/>',
        '<text x="60" y="126" font-family="Segoe UI,Arial,sans-serif" font-size="16" font-weight="700" fill="#183047">Height over time</text>',
        f'<polyline points="{points(vacuum, yh)}" fill="none" stroke="#b35434" stroke-width="3" stroke-linejoin="round"/>',
        f'<polyline points="{points(drag, yh)}" fill="none" stroke="#226b9a" stroke-width="3" stroke-linejoin="round"/>',
        '<text x="60" y="435" font-family="Segoe UI,Arial,sans-serif" font-size="16" font-weight="700" fill="#183047">Vertical velocity over time</text>',
        '<rect x="36" y="445" width="928" height="145" rx="14" fill="#fff" stroke="#d9e2ea"/>',
        f'<line x1="{x0}" y1="{yv(0):.2f}" x2="{x1}" y2="{yv(0):.2f}" stroke="#c6b8ae" stroke-dasharray="4 4"/>',
        f'<polyline points="{points(vacuum, yv)}" fill="none" stroke="#b35434" stroke-width="3" stroke-linejoin="round"/>',
        f'<polyline points="{points(drag, yv)}" fill="none" stroke="#226b9a" stroke-width="3" stroke-linejoin="round"/>',
        '<line x1="60" y1="615" x2="92" y2="615" stroke="#b35434" stroke-width="3"/>',
        '<text x="102" y="620" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#526578">No drag</text>',
        '<line x1="220" y1="615" x2="252" y2="615" stroke="#226b9a" stroke-width="3"/>',
        '<text x="262" y="620" font-family="Segoe UI,Arial,sans-serif" font-size="13" fill="#526578">Quadratic drag</text>',
        f'<text x="500" y="620" font-family="Segoe UI,Arial,sans-serif" font-size="12" fill="#526578">Final height: {drag[-1]["height"]:.2f} m with drag vs {vacuum[-1]["height"]:.2f} m without</text>',
        '<text x="500" y="640" font-family="Segoe UI,Arial,sans-serif" font-size="11" fill="#526578">Dots/lines use run-record samples; interpolation is a visual guide.</text>',
        '</svg>'
    ]
    return "\n".join(p) + "\n"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("record", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    record = json.loads(args.record.read_text(encoding="utf-8"))
    svg = render(record)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(svg, encoding="utf-8")
    print(f"Rendered recorded falling-body states to {args.output}")


if __name__ == "__main__":
    main()
