using NUnit.Framework;
using TheSingularityWorkshop.Physics.Numerics;

namespace TheSingularityWorkshop.Physics.Tests.Mathematics;

[TestFixture]
public sealed class DerivativeDerivationRecordTests
{
    [Test]
    public void Create_PreservesActualSamplesRulesAssumptionsAndUnits()
    {
        var calls = new List<double>();
        var record = DerivativeDerivationRecord.Create(
            "calculus.quadratic.derivative-at-four",
            "f(x) = 3x^2 + 2x - 7",
            "m",
            "m^2",
            4d,
            0.1d,
            new[] { "f is smooth in a neighborhood of x = 4.", "The function is deterministic and side-effect free." },
            x => { calls.Add(x); return (3d * x * x) + (2d * x) - 7d; });

        Assert.That(record.DerivativeUnit, Is.EqualTo("m^2/m"));
        Assert.That(record.Samples, Has.Count.EqualTo(5));
        Assert.That(record.Samples.Select(sample => sample.Coordinate),
            Is.EqualTo(new[] { 3.9d, 3.95d, 4d, 4.05d, 4.1d }));
        Assert.That(record.Samples.Select(sample => sample.Value),
            Is.EqualTo(calls.Select(x => (3d * x * x) + (2d * x) - 7d)));
        Assert.That(record.DerivativeEstimate, Is.EqualTo(26d).Within(1e-12));
        Assert.That(record.Steps, Has.Count.EqualTo(3));
        Assert.That(record.Steps[2].Rule, Does.Contain("Richardson"));
        Assert.That(record.Assumptions, Has.Count.EqualTo(2));
    }

    [Test]
    public void Create_RejectsMissingAssumptionsAndInvalidSamples()
    {
        Assert.Throws<ArgumentException>(() => DerivativeDerivationRecord.Create(
            "id", "f(x)=x", "s", "m", 0d, 0.1d, Array.Empty<string>(), x => x));

        Assert.Throws<ArithmeticException>(() => DerivativeDerivationRecord.Create(
            "id", "f(x)=x", "s", "m", 0d, 0.1d, new[] { "smooth" }, _ => double.NaN));
    }

    [Test]
    public void ToJson_EmitsVersionedRendererIndependentContract()
    {
        var record = DerivativeDerivationRecord.Create(
            "calculus.linear", "f(x) = 2x", "s", "m", 1d, 0.1d,
            new[] { "f is smooth." }, x => 2d * x);

        var json = record.ToJson();

        Assert.That(json, Does.Contain("\"schemaVersion\": 1"));
        Assert.That(json, Does.Contain("\"samples\""));
        Assert.That(json, Does.Contain("\"steps\""));
        Assert.That(json, Does.Contain("\"derivativeUnit\": \"m/s\""));
    }
}
