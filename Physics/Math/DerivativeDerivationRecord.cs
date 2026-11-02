using System.Text.Json;

namespace TheSingularityWorkshop.Physics.Numerics;

/// <summary>
/// A renderer-independent, reproducible record of a sampled scalar derivative derivation.
/// </summary>
/// <remarks>
/// The record preserves the actual function samples used for the coarse and fine central
/// secants, the Richardson correction, the declared mathematical rule, assumptions, and
/// caller-supplied unit labels. It is evidence for a numerical demonstration, not a symbolic
/// proof or a rigorous error bound.
/// </remarks>
public sealed record DerivativeDerivationRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Version of this derivation-record schema.</summary>
    public int SchemaVersion { get; init; } = 1;
    /// <summary>Stable identifier for the mathematical demonstration.</summary>
    public required string DerivationId { get; init; }
    /// <summary>Human-readable expression for the sampled function.</summary>
    public required string FunctionExpression { get; init; }
    /// <summary>Caller-declared unit label for the independent variable.</summary>
    public required string InputUnit { get; init; }
    /// <summary>Caller-declared unit label for the function output.</summary>
    public required string OutputUnit { get; init; }
    /// <summary>Output-unit per input-unit label; no dimensional analysis is performed.</summary>
    public required string DerivativeUnit { get; init; }
    /// <summary>Invariant-culture representation of the evaluation coordinate.</summary>
    public required string EvaluationCoordinate { get; init; }
    /// <summary>Declared smoothness, determinism, and model assumptions.</summary>
    public required IReadOnlyList<string> Assumptions { get; init; }
    /// <summary>Actual function evaluations ordered from outer lower to outer upper coordinate.</summary>
    public required IReadOnlyList<DerivativeSample> Samples { get; init; }
    /// <summary>Ordered mathematical operations used to obtain the estimate.</summary>
    public required IReadOnlyList<DerivativeRuleStep> Steps { get; init; }
    /// <summary>Outer symmetric sample offset.</summary>
    public double CoarseStep { get; init; }
    /// <summary>Inner symmetric sample offset, equal to half the coarse step.</summary>
    public double FineStep { get; init; }
    /// <summary>Central-difference slope computed from the outer sample pair.</summary>
    public double CoarseSecantSlope { get; init; }
    /// <summary>Central-difference slope computed from the inner sample pair.</summary>
    public double FineSecantSlope { get; init; }
    /// <summary>Richardson extrapolation correction added to the fine slope.</summary>
    public double RichardsonCorrection { get; init; }
    /// <summary>Richardson-extrapolated derivative estimate.</summary>
    public double DerivativeEstimate { get; init; }
    /// <summary>Correction magnitude; an asymptotic indicator, not a rigorous error bound.</summary>
    public double EstimatedAbsoluteErrorIndicator { get; init; }

    /// <summary>
    /// Evaluates a deterministic scalar function at the four points used by central
    /// differences with steps h and h/2, then records the derivation and its actual samples.
    /// </summary>
    public static DerivativeDerivationRecord Create(
        string derivationId,
        string functionExpression,
        string inputUnit,
        string outputUnit,
        double coordinate,
        double coarseStep,
        IReadOnlyList<string> assumptions,
        Func<double, double> function)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(derivationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(functionExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputUnit);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputUnit);
        ArgumentNullException.ThrowIfNull(assumptions);
        ArgumentNullException.ThrowIfNull(function);

        if (!double.IsFinite(coordinate))
            throw new ArgumentOutOfRangeException(nameof(coordinate));
        if (!double.IsFinite(coarseStep) || coarseStep <= 0d)
            throw new ArgumentOutOfRangeException(nameof(coarseStep));
        if (assumptions.Count == 0 || assumptions.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("At least one non-empty assumption is required.", nameof(assumptions));

        var fineStep = coarseStep / 2d;
        var coordinates = new[]
        {
            coordinate - coarseStep,
            coordinate - fineStep,
            coordinate + fineStep,
            coordinate + coarseStep
        };
        if (coordinates.Any(value => !double.IsFinite(value) || value == coordinate) ||
            coordinates.Distinct().Count() != coordinates.Length)
            throw new ArgumentException("The steps must produce four distinct finite sample coordinates.", nameof(coarseStep));

        var samples = coordinates
            .Select(sampleX => new DerivativeSample(sampleX, function(sampleX)))
            .ToArray();
        if (samples.Any(sample => !double.IsFinite(sample.Value)))
            throw new ArithmeticException("The function returned a non-finite sample.");

        var coarse = (samples[3].Value - samples[0].Value) / (2d * coarseStep);
        var fine = (samples[2].Value - samples[1].Value) / (2d * fineStep);
        var correction = (fine - coarse) / 3d;
        var estimate = fine + correction;
        if (!double.IsFinite(coarse) || !double.IsFinite(fine) ||
            !double.IsFinite(correction) || !double.IsFinite(estimate))
            throw new ArithmeticException("The derivative derivation produced a non-finite intermediate.");

        return new DerivativeDerivationRecord
        {
            DerivationId = derivationId,
            FunctionExpression = functionExpression,
            InputUnit = inputUnit,
            OutputUnit = outputUnit,
            DerivativeUnit = $"{outputUnit}/{inputUnit}",
            EvaluationCoordinate = coordinate.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            Assumptions = Array.AsReadOnly(assumptions.ToArray()),
            Samples = Array.AsReadOnly(samples),
            CoarseStep = coarseStep,
            FineStep = fineStep,
            CoarseSecantSlope = coarse,
            FineSecantSlope = fine,
            RichardsonCorrection = correction,
            DerivativeEstimate = estimate,
            EstimatedAbsoluteErrorIndicator = Math.Abs(correction),
            Steps = Array.AsReadOnly(new[]
            {
                new DerivativeRuleStep(1, "Central difference", "D(h) = [f(x+h) - f(x-h)] / (2h)", "Compute the coarse secant slope from the two outer samples."),
                new DerivativeRuleStep(2, "Central difference at half-step", "D(h/2) = [f(x+h/2) - f(x-h/2)] / h", "Compute the fine secant slope from the two inner samples."),
                new DerivativeRuleStep(3, "Second-order Richardson extrapolation", "D_R = D(h/2) + [D(h/2) - D(h)] / 3", "Cancel the leading O(h^2) central-difference truncation term when the smooth-function asymptotic assumptions hold.")
            })
        };
    }

    /// <summary>Serializes the derivation as stable, human-readable JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}

/// <summary>A function evaluation preserved as part of a derivative derivation.</summary>
public sealed record DerivativeSample(double Coordinate, double Value);

/// <summary>A named mathematical operation in a structured derivation.</summary>
public sealed record DerivativeRuleStep(int Sequence, string Rule, string Expression, string Explanation);
