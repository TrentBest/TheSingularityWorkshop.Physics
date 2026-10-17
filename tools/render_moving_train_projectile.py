#!/usr/bin/env python3
"""Render a paired train/ground-frame SVG from a serialized C# run record."""
import argparse
import html
import json
import math
from pathlib import Path


def render(record):
    if record.get("schemaVersion") != 1 or record.get("modelId") != "kinematics.moving-train-projectile":
        raise ValueError("unsupported paired moving-train run record")
    samples = record.get("samples")
    if not isinstance(samples, list) or len(samples) < 2:
        raise ValueError("at least two paired samples are required")
    times, train_x, ground_x, train_z, ground_z = [], [], [], [], []
    for i, pair in enumerate(samples):
        train = pair.get("trainFrameState", {})
        ground = pair.get("groundFrameState", {})
        t = train.get("elapsedTime")
        gt = ground.get("elapsedTime")
        if not isinstance(t, (int, float)) or not math.isfinite(t) or abs(t - gt) > 1e-12:
            raise ValueError(f"sample {i} has invalid or mismatched frame time")
        for state_name, state in (("train", train), ("ground", ground)):
            for key in ("x", "y", "z"):
                value = state.get("position", {}).get(key)
                if not isinstance(value, (int, float)) or not math.isfinite(value):
                    raise ValueError(f"sample {i} has invalid {state_name} position.{key}")
        times.append(float(t))
        train_x.append(float(train["position"]["x"]))
        ground_x.append(float(ground["position"]["x"]))
        train_z.append(float(train["position"]["z"]))
        ground_z.append(float(ground["position"]["z"]))
    if any(times[i] <= times[i-1] for i in range(1, len(times))):
        raise ValueError("sample times must increase strictly")
    duration = times[-1] - times[0]
    if duration <= 0:
        raise ValueError("run duration must be positive")
    tmin, tmax = times[0], times[-1]
    x_min = min(train_x + ground_x)
    x_max = max(train_x + ground_x)
    if x_min == x_max:
        x_max = x_min + 1.0
    z_min = min(0.0, min(train_z + ground_z))
    z_max = max(1.0, max(train_z + ground_z))
    zpad = (z_max - z_min) * 0.08
    z_min -= zpad
    z_max += zpad

    W, H = 1100, 620
    plot_top, plot_bottom = 170.0, 470.0
    x_plot = (85.0, 510.0)
    z_plot = (590.0, 1015.0)
    sx = lambda t, box: box[0] + (t - tmin) / duration * (box[1] - box[0])
    sy = lambda v, lo, hi: plot_bottom - (v - lo) / (hi - lo) * (plot_bottom - plot_top)
    xys = lambda values, box, lo, hi: " ".join(f"{sx(t, box):.2f},{sy(v, lo, hi):.2f}" for t, v in zip(times, values))
    path = lambda values, box, lo, hi: "M " + " L ".join(f"{sx(t, box):.2f} {sy(v, lo, hi):.2f}" for t, v in zip(times, values))
    esc = lambda value: html.escape(str(value), quote=True)
    lines = [
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" role="img" aria-labelledby="title desc">',
        '<title id="title">Moving-train projectile — two observers, one trajectory</title>',
        f'<desc id="desc">Animated comparison generated from {len(samples)} paired states in run record {esc(record.get("modelId"))}. The train-frame horizontal position remains fixed while the ground-frame position advances. Both observers report the same vertical trajectory at each shared logical time.</desc>',
        '<rect width="1100" height="620" rx="18" fill="#f7f9fc"/>',
        '<text x="42" y="45" fill="#14253d" font-family="Segoe UI, Arial, sans-serif" font-size="25" font-weight="700">ONE PROJECTILE · TWO REFERENCE FRAMES</text>',
        f'<text x="42" y="76" fill="#475569" font-family="Segoe UI, Arial, sans-serif" font-size="14">{len(samples)} paired states · {duration:.2f} s · model {esc(record.get("modelVersion", "unknown"))} · animated markers follow recorded samples</text>',
        '<rect x="60" y="115" width="480" height="405" rx="12" fill="#ffffff" stroke="#cbd5e1"/>',
        '<rect x="560" y="115" width="480" height="405" rx="12" fill="#ffffff" stroke="#cbd5e1"/>',
        '<text x="85" y="145" fill="#14253d" font-size="18" font-weight="700">Horizontal position vs time</text>',
        '<text x="585" y="145" fill="#14253d" font-size="18" font-weight="700">Vertical position vs time</text>',
    ]
    for box, lo, hi, title in ((x_plot, x_min, x_max, "x (m)"), (z_plot, z_min, z_max, "z (m)")):
        for i in range(5):
            val = lo + (hi - lo) * i / 4
            y = sy(val, lo, hi)
            lines.append(f'<line x1="{box[0]:.2f}" y1="{y:.2f}" x2="{box[1]:.2f}" y2="{y:.2f}" stroke="#e2e8f0"/>')
            lines.append(f'<text x="{box[0]-12:.2f}" y="{y+4:.2f}" text-anchor="end" fill="#64748b" font-size="12">{val:.2f}</text>')
        for i in range(5):
            t = tmin + duration * i / 4
            x = sx(t, box)
            lines.append(f'<line x1="{x:.2f}" y1="{plot_top:.2f}" x2="{x:.2f}" y2="{plot_bottom:.2f}" stroke="#e2e8f0"/>')
            lines.append(f'<text x="{x:.2f}" y="492" text-anchor="middle" fill="#64748b" font-size="12">{t:.2f}</text>')
        lines.append(f'<text x="{(box[0]+box[1])/2:.2f}" y="512" text-anchor="middle" fill="#475569" font-size="13">Elapsed time (s) · {title}</text>')
    train_x_path = path(train_x, x_plot, x_min, x_max)
    ground_x_path = path(ground_x, x_plot, x_min, x_max)
    train_z_path = path(train_z, z_plot, z_min, z_max)
    ground_z_path = path(ground_z, z_plot, z_min, z_max)
    lines.extend([
        f'<path d="{train_x_path}" fill="none" stroke="#1267a5" stroke-width="4" stroke-linejoin="round"/>',
        f'<path d="{ground_x_path}" fill="none" stroke="#c2410c" stroke-width="4" stroke-dasharray="9 6" stroke-linejoin="round"/>',
        f'<path d="{train_z_path}" fill="none" stroke="#1267a5" stroke-width="5" stroke-linejoin="round"/>',
        f'<path d="{ground_z_path}" fill="none" stroke="#c2410c" stroke-width="2.5" stroke-dasharray="8 6" stroke-linejoin="round"/>',
        f'<circle r="7" fill="#1267a5" stroke="#ffffff" stroke-width="2"><animateMotion dur="4s" repeatCount="indefinite" path="{train_x_path}"/></circle>',
        f'<circle r="7" fill="#c2410c" stroke="#ffffff" stroke-width="2"><animateMotion dur="4s" repeatCount="indefinite" path="{ground_x_path}"/></circle>',
        f'<circle r="8" fill="#ffffff" stroke="#1267a5" stroke-width="4"><animateMotion dur="4s" repeatCount="indefinite" path="{train_z_path}"/></circle>',
        '<text x="85" y="555" fill="#1267a5" font-size="14" font-weight="700">● Train frame: horizontal position stays at 0 m</text>',
        '<text x="85" y="580" fill="#c2410c" font-size="14" font-weight="700">● Ground frame: horizontal position advances with the train</text>',
        '<text x="590" y="555" fill="#14253d" font-size="14" font-weight="700">The vertical curves coincide: both observers agree.</text>',
        '<text x="590" y="580" fill="#64748b" font-size="12">Dashed orange curve overlays the blue curve to show equality.</text>',
        '<text x="42" y="607" fill="#64748b" font-size="11">Classical model · parallel axes · constant train velocity · plotted paths from recorded C# states</text>',
        '</svg>',
        ''
    ])
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path)
    parser.add_argument("output_svg", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    record = json.loads(args.run_record.read_text(encoding="utf-8"))
    generated = render(record)
    if args.check:
        if not args.output_svg.is_file() or args.output_svg.read_text(encoding="utf-8") != generated:
            raise SystemExit("SVG is stale: regenerate it from the supplied run record")
        print("Moving-train SVG matches the supplied run record.")
    else:
        args.output_svg.parent.mkdir(parents=True, exist_ok=True)
        args.output_svg.write_text(generated, encoding="utf-8", newline="\n")
        print(f"Rendered paired states to {args.output_svg}")


if __name__ == "__main__":
    main()
