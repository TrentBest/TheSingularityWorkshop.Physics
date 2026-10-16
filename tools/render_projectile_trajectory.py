#!/usr/bin/env python3
"""Render an X/Z SVG projection from a serialized ProjectileTrajectoryRunRecord.

This tool is deliberately outside the Physics assembly: it reads recorded samples
and never re-solves or alters the physical model.
"""
import argparse
import html
import json
import math
from pathlib import Path


def render(record):
    samples = record.get("samples")
    definition = record.get("definition", {})
    if not isinstance(samples, list) or len(samples) < 2:
        raise ValueError("run record must contain at least two recorded samples")
    if record.get("schemaVersion") != 1:
        raise ValueError("unsupported or missing run-record schemaVersion")
    for index, sample in enumerate(samples):
        for field in ("elapsedTime",):
            if field not in sample or not math.isfinite(sample[field]):
                raise ValueError(f"sample {index} has invalid {field}")
        position = sample.get("position", {})
        for axis in ("x", "y", "z"):
            if axis not in position or not math.isfinite(position[axis]):
                raise ValueError(f"sample {index} has invalid position.{axis}")
    xs = [s["position"]["x"] for s in samples]
    zs = [s["position"]["z"] for s in samples]
    xmin, xmax = min(xs), max(xs)
    if xmin == xmax:
        xmax = xmin + 1.0
    zmin, zmax = min(0.0, min(zs)), max(1.0, max(zs))
    zspan = zmax - zmin
    zmin -= zspan * 0.08
    zmax += zspan * 0.08
    W, H = 960, 560
    left, right, top, bottom = 110.0, 875.0, 105.0, 425.0
    sx = lambda x: left + (x - xmin) / (xmax - xmin) * (right - left)
    sz = lambda z: bottom - (z - zmin) / (zmax - zmin) * (bottom - top)
    points = [(sx(x), sz(z)) for x, z in zip(xs, zs)]
    esc = lambda value: html.escape(str(value), quote=True)
    model = esc(record.get("modelId", "unknown-model"))
    version = esc(record.get("modelVersion", "unknown"))
    frame = esc(definition.get("coordinateFrame", "unspecified frame"))
    dt = definition.get("deltaTime")
    duration = samples[-1]["elapsedTime"] - samples[0]["elapsedTime"]
    lines = [
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" role="img" aria-labelledby="title desc">',
        '<title id="title">Recorded projectile trajectory — X/Z projection</title>',
        f'<desc id="desc">{len(samples)} recorded states from {model}, coordinate frame {frame}. Plot positions directly from the run record; no physics is recomputed by this renderer.</desc>',
        '<rect width="960" height="560" rx="18" fill="#f7f9fc"/>',
        '<text x="48" y="44" font-family="Segoe UI, Arial, sans-serif" font-size="23" font-weight="700" fill="#14253d">Projectile trajectory · recorded solver states</text>',
        f'<text x="48" y="72" font-family="Segoe UI, Arial, sans-serif" font-size="14" fill="#475569">Model: {model} · model version {version} · {len(samples)} recorded states</text>',
        f'<rect x="{left:g}" y="{top:g}" width="{right-left:g}" height="{bottom-top:g}" fill="#ffffff" stroke="#9aa8b8" stroke-width="1.5"/>',
    ]
    xstep = (xmax - xmin) / 8
    for i in range(9):
        value = xmin + xstep * i
        x = sx(value)
        lines.append(f'<line x1="{x:.3f}" y1="{top:g}" x2="{x:.3f}" y2="{bottom:g}" stroke="#d5dce5"/>')
        lines.append(f'<text x="{x:.3f}" y="450" text-anchor="middle" font-size="13" fill="#364152">{value:.3g}</text>')
    zstep = (zmax - zmin) / 4
    for i in range(5):
        value = zmin + zstep * i
        y = sz(value)
        lines.append(f'<line x1="{left:g}" y1="{y:.3f}" x2="{right:g}" y2="{y:.3f}" stroke="#d5dce5"/>')
        lines.append(f'<text x="92" y="{y+5:.3f}" text-anchor="end" font-size="13" fill="#364152">{value:.3g}</text>')
    polyline = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
    lines.append(f'<polyline points="{polyline}" fill="none" stroke="#1267a5" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round"/>')
    for index, (sample, (x, y)) in enumerate(zip(samples, points)):
        p = sample["position"]
        radius = 5 if index in (0, len(samples)-1) else 4
        lines.append(
            f'<circle cx="{x:.2f}" cy="{y:.2f}" r="{radius}" fill="#1267a5" stroke="#ffffff" stroke-width="1.5">'
            f'<title>t = {sample["elapsedTime"]:.6g} s; x = {p["x"]:.6g} m; z = {p["z"]:.6g} m</title></circle>'
        )
    lines.extend([
        f'<text x="{(left+right)/2:g}" y="492" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="16" fill="#14253d">X position ({esc(record.get("units", {}).get("position", "unit unspecified"))})</text>',
        f'<text x="34" y="{(top+bottom)/2:g}" transform="rotate(-90 34 {(top+bottom)/2:g})" text-anchor="middle" font-family="Segoe UI, Arial, sans-serif" font-size="16" fill="#14253d">Z position ({esc(record.get("units", {}).get("position", "unit unspecified"))})</text>',
        f'<text x="110" y="530" font-family="Segoe UI, Arial, sans-serif" font-size="12" fill="#475569">{frame} · timestep {dt} s · {len(samples)-1} steps · duration {duration:.6g} s · plotted from stored samples</text>',
        '</svg>',
        ''
    ])
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path, help="JSON from ProjectileTrajectoryRunRecord.ToJson()")
    parser.add_argument("output_svg", type=Path, help="destination SVG path")
    parser.add_argument("--check", action="store_true", help="fail if output differs from generated SVG")
    args = parser.parse_args()
    record = json.loads(args.run_record.read_text(encoding="utf-8"))
    generated = render(record)
    if args.check:
        existing = args.output_svg.read_text(encoding="utf-8")
        if existing != generated:
            raise SystemExit("SVG is stale: regenerate it from the supplied run record")
        print("SVG matches the supplied run record.")
        return
    args.output_svg.parent.mkdir(parents=True, exist_ok=True)
    args.output_svg.write_text(generated, encoding="utf-8", newline="\n")
    print(f"Rendered {len(record['samples'])} recorded samples to {args.output_svg}")


if __name__ == "__main__":
    main()
