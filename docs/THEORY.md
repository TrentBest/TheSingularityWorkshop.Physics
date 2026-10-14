# Theory: Explain What the Model Can Prove

The Workshop's physics documentation should help readers move between a physical question, its mathematical model, an experiment, measured results, and the limits of the conclusion. This is an evidence contract, not a claim that theory generation is already implemented.

## A theory record

Every model-backed explanation should identify:
- **Question:** what is being investigated?
- **Model:** which governing relationship or algorithm is used?
- **Assumptions:** what is held constant, idealized, omitted, or approximated?
- **Domain:** supported units, ranges, geometry, phases, and boundary conditions.
- **Inputs:** initial state and external conditions.
- **Prediction:** expected outcome and derivation.
- **Observation:** actual output of the recorded run.
- **Residual:** difference between prediction, reference, and computed result.
- **Evidence:** source, edition/version, section/page or dataset identifier, license, and retrieval details.
- **Limitations:** what the result does not establish.

## Types of statement

Keep these distinct:
1. **Definition** — a convention chosen by the system.
2. **Established relationship** — a recognized law/model, with conditions and provenance.
3. **Measured datum** — a reported measurement with source and uncertainty where available.
4. **Derived result** — computed from declared inputs and equations.
5. **Simulation output** — produced by a particular numerical method/configuration.
6. **Assumption** — a simplification introduced by the experiment.
7. **Hypothesis** — a claim the experiment is designed to test.
8. **Interpretation** — an explanation beyond direct measurement, labeled accordingly.

A visually persuasive result is not, by itself, a validated physical result.

## Generated explanations

Living theory should be generated from versioned model metadata and actual experiment records, not from visual appearance alone. An explanation can show equations and symbol definitions, labeled diagrams with units, interactive parameter changes, expected/observed curves, conservation checks, numerical residuals, references, licenses, and explicit “not modeled” disclosures.

When an experiment changes model version, make the change visible and retain the lineage of prior runs.

## Reference discipline

Record enough provenance to identify the source used. Do not copy restricted textbook prose, diagrams, or datasets without permission. Summaries, independently authored diagrams, and properly licensed/open data should preserve attribution and terms.

## The living document loop

**Model → Run → Measure → Compare → Explain → Review → Version**

Generated documentation can make evidence inspectable; it cannot convert weak assumptions into strong science.
