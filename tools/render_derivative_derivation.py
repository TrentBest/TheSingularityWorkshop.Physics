#!/usr/bin/env python3
"""Render a derivative derivation record as a deterministic secant-to-tangent SVG."""
import argparse
import json
import math
from pathlib import Path
from xml.sax.saxutils import escape


def render(record):
    samples = record["samples"]
    if len(samples) != 5:
        raise ValueError("A derivative visualization requires four secant samples and one center anchor.")
    x = [float(sample["coordinate"]) for sample in samples]
    y = [float(sample["value"]) for sample in samples]
    if not all(math.isfinite(value) for value in x + y):
        raise ValueError("All recorded samples must be finite.")
    center_x = float(record["evaluationCoordinate"])
    center_y = y[2]
    slope = float(record["derivativeEstimate"])
    coarse = float(record["coarseSecantSlope"])
    fine = float(record["fineSecantSlope"])
    if not all(math.isfinite(value) for value in (center_x, center_y, slope, coarse, fine)):
        raise ValueError("All derivative values must be finite.")

    left, right = min(x), max(x)
    span = right - left
    y_values = y + [center_y + slope * (left - center_x), center_y + slope * (right - center_x)]
    y_min, y_max = min(y_values), max(y_values)
    y_pad = max((y_max - y_min) * 0.12, 1e-9)
    y_min -= y_pad
    y_max += y_pad
    plot_left, plot_right = 105.0, 865.0
    plot_top, plot_bottom = 90.0, 395.0

    def sx(value):
        return plot_left + (value - left) / span * (plot_right - plot_left)

    def sy(value):
        return plot_bottom - (value - y_min) / (y_max - y_min) * (plot_bottom - plot_top)

    curve = " ".join(("M" if i == 0 else "L") + f" {sx(px):.3f} {sy(py):.3f}" for i, (px, py) in enumerate(zip(x, y)))
    tangent = f"M {sx(left):.3f} {sy(center_y + slope * (left-center_x)):.3f} L {sx(right):.3f} {sy(center_y + slope * (right-center_x)):.3f}"
    coarse_line = f"M {sx(x[0]):.3f} {sy(y[0]):.3f} L {sx(x[4]):.3f} {sy(y[4]):.3f}"
    fine_line = f"M {sx(x[1]):.3f} {sy(y[1]):.3f} L {sx(x[3]):.3f} {sy(y[3]):.3f}"
    circles = "\n".join(
        f'<circle cx="{sx(px):.3f}" cy="{sy(py):.3f}" r="6" fill="#172554" stroke="#ffffff" stroke-width="2"><title>f({px:.6g}) = {py:.6g}</title></circle>'
        for px, py in zip(x, y)
    )
    unit = escape(record["derivativeUnit"])
    expression = escape(record["functionExpression"])
    return f'''<svg xmlns="http://www.w3.org/2000/svg" width="960" height="540" viewBox="0 0 960 540" role="img" aria-labelledby="title desc">
<title id="title">Central secants approach the derivative tangent</title>
<desc id="desc">A model-backed derivative visualization generated from five actual function samples. It compares coarse and fine central secants with the Richardson tangent estimate.</desc>
<rect width="960" height="540" fill="#ffffff"/>
<text x="48" y="42" font-family="Arial, sans-serif" font-size="24" font-weight="700" fill="#172554">From secant slopes to a tangent estimate</text>
<text x="48" y="70" font-family="Arial, sans-serif" font-size="15" fill="#334155">{expression}</text>
<path d="M {plot_left} {plot_bottom} H {plot_right}" stroke="#64748b" stroke-width="1.5"/>
<path d="M {plot_left} {plot_top} V {plot_bottom}" stroke="#64748b" stroke-width="1.5"/>
<path d="{coarse_line}" fill="none" stroke="#94a3b8" stroke-width="3" stroke-dasharray="8 6"/>
<path d="{fine_line}" fill="none" stroke="#0f766e" stroke-width="3" stroke-dasharray="5 4"/>
<path d="{curve}" fill="none" stroke="#1d4ed8" stroke-width="4" stroke-linejoin="round" stroke-linecap="round"/>
<path d="{tangent}" fill="none" stroke="#c2410c" stroke-width="3"/>
{circles}
<circle cx="{sx(center_x):.3f}" cy="{sy(center_y):.3f}" r="9" fill="#ffffff" stroke="#c2410c" stroke-width="3"/>
<text x="105" y="435" font-family="Arial, sans-serif" font-size="14" fill="#334155">Input coordinate ({escape(record["inputUnit"])})</text>
<text x="105" y="470" font-family="Arial, sans-serif" font-size="16" fill="#64748b">Coarse secant: {coarse:.9g} {unit}</text>
<text x="390" y="470" font-family="Arial, sans-serif" font-size="16" fill="#0f766e">Fine secant: {fine:.9g} {unit}</text>
<text x="650" y="470" font-family="Arial, sans-serif" font-size="16" fill="#c2410c">Richardson: {slope:.9g} {unit}</text>
<text x="48" y="510" font-family="Arial, sans-serif" font-size="13" fill="#475569">Model-backed visualization · five recorded function evaluations · correction is an asymptotic indicator, not a bound</text>
</svg>
'''


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("record", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    expected = render(json.loads(args.record.read_text(encoding="utf-8")))
    if args.check:
        actual = args.output.read_text(encoding="utf-8")
        if actual != expected:
            raise SystemExit(f"{args.output} is stale; regenerate it from {args.record}.")
        print(f"Verified {args.output} against actual derivative samples.")
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(expected, encoding="utf-8")
        print(f"Rendered {args.output} from {args.record}.")


if __name__ == "__main__":
    main()
