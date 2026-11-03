using System.Text.Json;
using TheSingularityWorkshop.Physics.Numerics;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: QuadratureRunRecordGenerator <output.json>");
    return 2;
}

const double lowerBound = 0d;
const double upperBound = 1d;
const int coarseIntervalCount = 16;
Func<double, double> function = x => Math.Exp(-x);

var estimate = Quadrature.CompositeSimpsonWithErrorEstimate(
    function, lowerBound, upperBound, coarseIntervalCount);
var analyticalIntegral = 1d - Math.Exp(-1d);
if (Math.Abs(estimate.Value - analyticalIntegral) > 1e-8)
{
    Console.Error.WriteLine("The refined quadrature estimate diverged from the analytical integral.");
    return 1;
}

var samples = Enumerable.Range(0, 17).Select(index =>
{
    var coordinate = lowerBound + ((upperBound - lowerBound) * index / 16d);
    var cumulativeIntegral = index == 0
        ? 0d
        : Quadrature.CompositeSimpson(function, lowerBound, coordinate, index * 2);
    return new
    {
        index,
        coordinate,
        functionValue = function(coordinate),
        cumulativeIntegral
    };
}).ToArray();

var record = new
{
    schemaVersion = 1,
    modelId = "mathematics.composite-simpson-quadrature",
    modelVersion = "1",
    functionExpression = "f(t) = exp(-t)",
    lowerBound,
    upperBound,
    inputUnit = "s",
    outputUnit = "1/s",
    integralUnit = "1",
    coarseIntervalCount = estimate.CoarseIntervalCount,
    fineIntervalCount = estimate.FineIntervalCount,
    integralEstimate = estimate.Value,
    analyticalReference = analyticalIntegral,
    actualAbsoluteError = Math.Abs(estimate.Value - analyticalIntegral),
    estimatedAbsoluteErrorIndicator = estimate.EstimatedAbsoluteError,
    evidenceClassification = "model-backed numerical experiment with analytical reference",
    errorIndicatorMeaning = "Richardson correction magnitude for fourth-order Simpson refinement; not a guaranteed bound",
    samples
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, JsonSerializer.Serialize(record, options) + Environment.NewLine);

Console.WriteLine($"Wrote {samples.Length} cumulative integration samples to {outputPath}.");
Console.WriteLine($"Richardson integral: {estimate.Value:R}; analytical reference: {analyticalIntegral:R}; correction indicator: {estimate.EstimatedAbsoluteError:R}.");
return 0;
