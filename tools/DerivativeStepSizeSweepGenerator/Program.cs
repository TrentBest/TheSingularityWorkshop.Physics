using System.Text.Json;
using System.Text.Json.Serialization;
using TheSingularityWorkshop.Physics.Numerics;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: DerivativeStepSizeSweepGenerator <output.json>");
    return 2;
}

const double coordinate = 0.7d;
var samples = new List<SweepSample>(12);
for (var exponent = 1; exponent <= 12; exponent++)
{
    var step = Math.Pow(10d, -exponent);
    var estimate = FiniteDifferences.CentralDifferenceWithErrorEstimate(Math.Sin, coordinate, step);
    var reference = Math.Cos(coordinate);
    samples.Add(new SweepSample(
        step,
        estimate.CoarseStep,
        estimate.FineStep,
        estimate.Value,
        estimate.EstimatedAbsoluteError,
        reference,
        Math.Abs(estimate.Value - reference)));
}

var record = new SweepRecord(
    1,
    "calculus.richardson.step-size-sweep.sin-at-0.7",
    "f(x) = sin(x)",
    coordinate,
    "rad",
    "dimensionless",
    "dimensionless/rad",
    "Math.Sin / Math.Cos analytical identity",
    "model-backed numerical experiment compared with an analytical reference",
    "Richardson correction magnitude is an asymptotic indicator, not a rigorous error bound.",
    samples);

var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(outputPath, JsonSerializer.Serialize(record, options) + Environment.NewLine);

var minimum = samples.MinBy(sample => sample.ActualAbsoluteError)!;
if (minimum.Step >= samples[0].step || minimum.Step <= samples[^1].step)
{
    Console.Error.WriteLine("The sweep did not expose an interior minimum in actual error.");
    return 1;
}

Console.WriteLine($"Wrote {samples.Count} Richardson estimates to {outputPath}; minimum actual error {minimum.actualAbsoluteError:R} at h={minimum.Step:R}.");
return 0;

internal sealed record SweepRecord(
    int SchemaVersion,
    string ExperimentId,
    string FunctionExpression,
    double Coordinate,
    string InputUnit,
    string OutputUnit,
    string DerivativeUnit,
    string Reference,
    string EvidenceClassification,
    string ErrorIndicatorMeaning,
    IReadOnlyList<SweepSample> Samples);

internal sealed record SweepSample(
    double Step,
    double CoarseStep,
    double FineStep,
    double DerivativeEstimate,
    double RichardsonCorrectionIndicator,
    double AnalyticalDerivative,
    double ActualAbsoluteError);
