#!/usr/bin/env python3
"""Render a quadrature run record as a deterministic area-accumulation SVG."""
import argparse
import json
import math
from pathlib import Path
from xml.sax.saxutils import escape


def render(record):
    samples = record["samples"]
    if record["schemaVersion"] != 1 or record["modelId"] != "mathematics.composite-simpson-quadrature":
        raise ValueError("Unsupported quadrature run-record contract.")
    if len(samples) != 17:
        raise ValueError("The area visualization requires seventeen cumulative samples.")
    x = [float(sample["coordinate"]) for sample in samples]
    y = [float(sample["functionValue"]) for sample in samples]
    cumulative = [float(sample["cumulativeIntegral"]) for sample in samples]
    lower, upper = float(record["lowerBound"]), float(record["upperBound"])
    values = x + y + cumulative + [
        float(record["integralEstimate"]),
        float(record["estimatedAbsoluteErrorIndicator"]),
    ]
    if not all(math.isfinite(value) for value in values) or not lower < upper:
        raise ValueError("Bounds, samples, and integral values must be finite and ordered.")
    if any(x[index] >= x[index + 1] for index in range(len(x) - 1)):
        raise ValueError("Sample coordinates must be strictly increasing.")
    if any(cumulative[index] > cumulative[index + 1] for index in range(len(cumulative) - 1)):
        raise ValueError("Cumulative area samples must be non-decreasing for this positive integrand.")

    left, right = 100.0, 1060.0
    top, bottom = 100.0, 440.0
    y_min = min(0.0, min(y))
    y_max = max(y) * 1.08
    if y_max <= y_min:
        raise ValueError("The function plot has no finite vertical span.")

    def sx(value):
        return left + (value - lower) / (upper - lower) * (right - left)

    def sy(value):
        return bottom - (value - y_min) / (y_max - y_min) * (bottom - top)

    curve = " ".join(
        ("M" if index == 0 else "L") + f" {sx(px):.2f} {sy(py):.2f}"
        for index, (px, py) in enumerate(zip(x, y))
    )
    area = (
        f"M {sx(x[0]):.2f} {sy(0):.2f} "
        + " ".join(f"L {sx(px):.2f} {sy(py):.2f}" for px, py in zip(x, y))
        + f" L {sx(x[-1]):.2f} {sy(0):.2f} Z"
    )
    circles = "\n".join(
        f'<circle cx="{sx(px):.2f}" cy="{sy(py):.2f}" r="4" fill="#1d4ed8"/>'
        for px, py in zip(x, y)
    )

    cumulative_top, cumulative_bottom = 535.0, 680.0
    cumulative_max = max(cumulative) * 1.08

    def cy(value):
        return cumulative_bottom - value / cumulative_max * (cumulative_bottom - cumulative_top)

    accumulated = " ".join(
        ("M" if index == 0 else "L") + f" {sx(px):.2f} {cy(value):.2f}"
        for index, (px, value) in enumerate(zip(x, cumulative))
    )
    expression = escape(record["functionExpression"])
    integral_unit = escape(record["integralUnit"])
    estimate = float(record["integralEstimate"])
    indicator = float(record["estimatedAbsoluteErrorIndicator"])
    return f'''<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="1160" height="760" viewBox="0 0 1160 760" role="img" aria-labelledby="title desc">
<title id="title">Numerical quadrature: accumulated area under the curve</title>
<desc id="desc">Model-backed area visualization from seventeen cumulative Simpson samples. The upper plot shows the exponential decay function and shaded area. The lower plot shows accumulated integral values. The refined integral is compared with an analytical reference.</desc>
<rect width="1160" height="760" fill="#ffffff"/>
<text x="100" y="42" font-family="Arial,sans-serif" font-size="25" font-weight="700" fill="#0f172a">Accumulation by numerical quadrature</text>
<text x="100" y="72" font-family="Arial,sans-serif" font-size="17" fill="#475569">{expression} · t in {escape(record["inputUnit"])}</text>
<path d="{area}" fill="#bfdbfe" fill-opacity="0.72" stroke="none"/>
<path d="M {left} {bottom} H {right}" stroke="#334155" stroke-width="1.5"/>
<path d="M {left} {top} V {bottom}" stroke="#334155" stroke-width="1.5"/>
<path d="{curve}" fill="none" stroke="#1d4ed8" stroke-width="3.5" stroke-linejoin="round"/>
{circles}
<text x="100" y="475" font-family="Arial,sans-serif" font-size="14" fill="#475569">Input coordinate (s)</text>
<text x="580" y="510" text-anchor="middle" font-family="Arial,sans-serif" font-size="18" font-weight="700" fill="#0f172a">Accumulated integral</text>
<path d="M {left} {cumulative_bottom} H {right}" stroke="#334155" stroke-width="1.5"/>
<path d="{accumulated}" fill="none" stroke="#0f766e" stroke-width="3.5" stroke-linejoin="round"/>
<text x="100" y="720" font-family="Arial,sans-serif" font-size="15" fill="#0f766e">Final refined integral: {estimate:.12g} {integral_unit}</text>
<text x="610" y="720" font-family="Arial,sans-serif" font-size="15" fill="#475569">Richardson correction indicator: {indicator:.4g} (not a bound)</text>
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
        print(f"Verified {args.output} against actual quadrature samples.")
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(expected, encoding="utf-8")
        print(f"Rendered {args.output} from {args.record}.")


if __name__ == "__main__":
    main()
