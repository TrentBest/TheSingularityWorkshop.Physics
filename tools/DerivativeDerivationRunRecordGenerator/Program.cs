using TheSingularityWorkshop.Physics.Numerics;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: DerivativeDerivationRunRecordGenerator <output.json>");
    return 2;
}

var record = DerivativeDerivationRecord.Create(
    derivationId: "calculus.central-difference.quadratic-at-four",
    functionExpression: "f(x) = 3x^2 + 2x - 7",
    inputUnit: "s",
    outputUnit: "m",
    coordinate: 4d,
    coarseStep: 0.2d,
    assumptions:
    [
        "The scalar function is smooth over the sampled interval.",
        "The function is deterministic and side-effect free.",
        "The displayed units are caller-declared labels; the numerical routine does not perform dimensional analysis."
    ],
    function: x => (3d * x * x) + (2d * x) - 7d);

var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, record.ToJson() + Environment.NewLine);

var analyticalDerivative = 6d * 4d + 2d;
if (Math.Abs(record.DerivativeEstimate - analyticalDerivative) > 1e-10)
{
    Console.Error.WriteLine("The recorded derivative estimate diverged from the analytical derivative.");
    return 1;
}

Console.WriteLine($"Wrote {record.Samples.Count} actual function samples and {record.Steps.Count} derivation steps to {outputPath}");
Console.WriteLine($"Richardson estimate: {record.DerivativeEstimate:R} {record.DerivativeUnit}; analytical reference: {analyticalDerivative:R} {record.DerivativeUnit}.");
return 0;
