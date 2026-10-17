using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;

namespace TheSingularityWorkshop.Physics.Tests.Materials;

[TestFixture]
public sealed class MaterialPropertyRangeEvaluatorTests
{
    [TestCase(250d)]
    [TestCase(400d)]
    [TestCase(325d)]
    public void Assess_ReportsWithinDeclaredRangeIncludingEndpoints(double temperature)
    {
        var result = MaterialPropertyRangeEvaluator.Assess(
            Property(), "temperature", "K", temperature);

        Assert.That(result.Status, Is.EqualTo(MaterialPropertyRangeStatus.WithinDeclaredRange));
        Assert.That(result.PropertyId, Is.EqualTo("specific-heat-capacity"));
        Assert.That(result.MaterialId, Is.EqualTo("material:steel"));
        Assert.That(result.ObservedValue, Is.EqualTo(temperature));
        Assert.That(result.EvidenceStatus, Is.EqualTo(PropertyEvidenceStatus.Illustrative));
        Assert.That(result.SourceId, Is.EqualTo("steel-fixture"));
        Assert.That(result.SourceVersion, Is.EqualTo("1"));
        Assert.That(result.ValidMinimum, Is.EqualTo(250d));
        Assert.That(result.ValidMaximum, Is.EqualTo(400d));
        Assert.That(result.Explanation, Does.Contain("Other validity conditions"));
    }

    [TestCase(249.99d)]
    [TestCase(400.01d)]
    public void Assess_ReportsOutsideDeclaredRange(double temperature)
    {
        var result = MaterialPropertyRangeEvaluator.Assess(
            Property(), "temperature", "K", temperature);

        Assert.That(result.Status, Is.EqualTo(MaterialPropertyRangeStatus.OutsideDeclaredRange));
    }

    [TestCase("pressure", "K")]
    [TestCase("temperature", "C")]
    public void Assess_ReportsIncomparableWhenQuantityOrUnitDoesNotMatch(
        string quantity,
        string unit)
    {
        var result = MaterialPropertyRangeEvaluator.Assess(Property(), quantity, unit, 300d);

        Assert.That(result.Status, Is.EqualTo(MaterialPropertyRangeStatus.Incomparable));
        Assert.That(result.Explanation, Does.Contain("no unit conversion"));
    }

    [Test]
    public void Assess_RejectsNonFiniteObservedValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MaterialPropertyRangeEvaluator.Assess(Property(), "temperature", "K", double.NaN));
    }

    [Test]
    public void Assess_RejectsInvalidPropertyDefinition()
    {
        var invalid = Property() with { ValidMinimum = 500d, ValidMaximum = 400d };

        Assert.Throws<ArgumentException>(() =>
            MaterialPropertyRangeEvaluator.Assess(invalid, "temperature", "K", 300d));
    }

    private static MaterialPropertyDefinition Property() =>
        new("material:steel", "specific-heat-capacity", 490d, "J/(kg*K)",
            PropertyEvidenceStatus.Illustrative, "steel-fixture", "1",
            "temperature", "K", 250d, 400d,
            "Illustrative fixture only; not an empirical material claim.");
}
