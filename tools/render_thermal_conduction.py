#!/usr/bin/env python3
"""Render an explanatory SVG from a thermal-conduction C# run record."""
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
    if record.get("schemaVersion") != 1 or record.get("modelId") != "thermodynamics.lumped-body-conduction":
        raise ValueError("unsupported thermal-conduction run record")
    definition = record["definition"]
    result = record["result"]
    first_initial = number(definition["firstInitialState"]["temperatureKelvin"], "first initial temperature")
    second_initial = number(definition["secondInitialState"]["temperatureKelvin"], "second initial temperature")
    first_final = number(result["finalFirst"]["temperatureKelvin"], "first final temperature")
    second_final = number(result["finalSecond"]["temperatureKelvin"], "second final temperature")
    energy = number(result["energyTransferredFromFirstToSecondJoules"], "transferred energy")
    conductance = number(result["thermalConductanceWattsPerKelvin"], "thermal conductance")
    energy_before = number(result["totalSensibleEnergyBeforeJoules"], "initial total energy")
    energy_after = number(result["totalSensibleEnergyAfterJoules"], "final total energy")
    residual = number(result["energyAccountingResidualJoules"], "energy residual")
    property_record = definition["thermalConductivity"]
    conductivity = number(property_record["value"], "thermal conductivity")
    if abs(residual) > 1e-8 * max(1.0, abs(energy_before)) or abs((energy_after - energy_before) - residual) > 1e-8:
        raise ValueError("run record does not close the sensible-energy accounting")
    esc = lambda value: html.escape(str(value), quote=True)
    validity = (
        f'{property_record["validMinimum"]:g}–{property_record["validMaximum"]:g} '
        f'{property_record["validityUnit"]}'
    )
    return "\n".join([
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 900 560" role="img" aria-labelledby="title desc">',
        '<title id="title">Heat conduction between two lumped thermal bodies</title>',
        f'<desc id="desc">The Physics model transfers {energy:g} joules from a body at {first_initial:g} kelvin to a body at {second_initial:g} kelvin. Final temperatures are {first_final:g} and {second_final:g} kelvin. Total sensible-energy residual is {residual:g} joules. Conductivity is {property_record["evidenceStatus"]}.</desc>',
        '<rect width="900" height="560" rx="24" fill="#f7f9fc"/>',
        '<text x="40" y="46" font-family="Segoe UI, Arial, sans-serif" font-size="26" font-weight="700" fill="#14253d">Thermal conduction: heat moves down the temperature gradient</text>',
        f'<text x="40" y="75" font-size="14" fill="#475569">Model {esc(record["modelId"])} · v{record["modelVersion"]} · deterministic C# output</text>',
        '<rect x="40" y="105" width="270" height="150" rx="16" fill="#fff7ed" stroke="#fb923c" stroke-width="2"/>',
        '<text x="60" y="137" font-size="19" font-weight="700" fill="#9a3412">Body A · initially hotter</text>',
        f'<text x="60" y="170" font-size="17" fill="#14253d">{first_initial:g} K → {first_final:g} K</text>',
        f'<text x="60" y="200" font-size="14" fill="#475569">Mass {definition["firstInitialState"]["massKilograms"]:g} kg · cₚ {definition["firstInitialState"]["specificHeatCapacityJoulesPerKilogramKelvin"]:g} J/(kg·K)</text>',
        '<path d="M 315 180 H 360" stroke="#c2410c" stroke-width="5" stroke-linecap="round"/><path d="M 360 180 l -12 -8 v 16 z" fill="#c2410c"/>',
        '<rect x="375" y="120" width="150" height="120" rx="16" fill="#ffffff" stroke="#cbd5e1" stroke-width="2"/>',
        '<text x="397" y="151" font-size="17" font-weight="700" fill="#14253d">Conduction</text>',
        f'<text x="397" y="181" font-size="17" fill="#14253d">{energy:g} J →</text>',
        f'<text x="397" y="208" font-size="14" fill="#475569">G = {conductance:g} W/K</text>',
        '<path d="M 540 180 H 585" stroke="#2563a6" stroke-width="5" stroke-linecap="round"/><path d="M 585 180 l -12 -8 v 16 z" fill="#2563a6"/>',
        '<rect x="600" y="105" width="260" height="150" rx="16" fill="#eff6ff" stroke="#60a5fa" stroke-width="2"/>',
        '<text x="620" y="137" font-size="19" font-weight="700" fill="#1d4ed8">Body B · initially cooler</text>',
        f'<text x="620" y="170" font-size="17" fill="#14253d">{second_initial:g} K → {second_final:g} K</text>',
        f'<text x="620" y="200" font-size="14" fill="#475569">Mass {definition["secondInitialState"]["massKilograms"]:g} kg · cₚ {definition["secondInitialState"]["specificHeatCapacityJoulesPerKilogramKelvin"]:g} J/(kg·K)</text>',
        '<rect x="40" y="280" width="820" height="95" rx="16" fill="#ffffff" stroke="#cbd5e1"/>',
        '<text x="60" y="310" font-size="18" font-weight="700" fill="#14253d">Energy accounting</text>',
        f'<text x="60" y="339" font-size="16" fill="#14253d">Before {energy_before:g} J · after {energy_after:g} J · residual {residual:g} J</text>',
        '<text x="60" y="360" font-size="13" fill="#475569">Positive transfer means A → B; the model rejects a step that would overshoot equilibrium.</text>',
        '<rect x="40" y="395" width="820" height="120" rx="16" fill="#f0fdf4" stroke="#86efac"/>',
        '<text x="60" y="425" font-size="18" font-weight="700" fill="#166534">Property provenance and limits</text>',
        f'<text x="60" y="450" font-size="14" fill="#14532d">k = {conductivity:g} {esc(property_record["unit"])} · status: {esc(property_record["evidenceStatus"])}</text>',
        f'<text x="60" y="473" font-size="13" fill="#14532d">Validity: {esc(property_record["validityQuantity"])} {esc(validity)} · source/fixture {esc(property_record["sourceId"])} v{esc(property_record["sourceVersion"])}</text>',
        '<text x="60" y="496" font-size="13" fill="#14532d">Illustrative constant properties; no internal gradients, contact resistance, phase change, convection, or radiation.</text>',
        '<text x="40" y="542" font-size="12" fill="#475569">A bounded two-body conduction primitive · not a validated real-material prediction</text>',
        '</svg>',
        ''
    ])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("run_record", type=Path)
    parser.add_argument("output_svg", type=Path)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    generated = render(json.loads(args.run_record.read_text(encoding="utf-8")))
    if args.check:
        if not args.output_svg.is_file() or args.output_svg.read_text(encoding="utf-8") != generated:
            raise SystemExit("SVG is stale: regenerate it from the thermal-conduction run record")
        print("Thermal-conduction SVG matches the run record.")
        return
    args.output_svg.parent.mkdir(parents=True, exist_ok=True)
    args.output_svg.write_text(generated, encoding="utf-8")
    print(f"Wrote {args.output_svg}")


if __name__ == "__main__":
    main()
