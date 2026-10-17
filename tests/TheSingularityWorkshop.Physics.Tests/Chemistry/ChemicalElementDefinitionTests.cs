using NUnit.Framework;
using TheSingularityWorkshop.Physics.Chemistry;

namespace TheSingularityWorkshop.Physics.Tests.Chemistry;

[TestFixture]
public sealed class ChemicalElementDefinitionTests
{
    [Test]
    public void ValidElementIdentityRequiresProvenance()
    {
        var element = new ChemicalElementDefinition(
            6, "C", "Carbon", "test-periodic-table", "1",
            "https://example.invalid/element/6", "illustrative-test-fixture");

        Assert.That(element.IsValid, Is.True);
        Assert.DoesNotThrow(element.EnsureValid);
    }

    [TestCase(0)]
    [TestCase(119)]
    public void AtomicNumberOutsideRecognizedRangeIsInvalid(int atomicNumber)
    {
        var element = new ChemicalElementDefinition(
            atomicNumber, "C", "Carbon", "source", "1", "https://example.invalid", "fixture");

        Assert.That(element.IsValid, Is.False);
        Assert.Throws<ArgumentException>(element.EnsureValid);
    }

    [TestCase("c")]
    [TestCase("CAR")]
    [TestCase("C1")]
    [TestCase("")]
    public void InvalidSymbolIsRejected(string symbol)
    {
        var element = new ChemicalElementDefinition(
            6, symbol, "Carbon", "source", "1", "https://example.invalid", "fixture");

        Assert.That(element.IsValid, Is.False);
    }

    [Test]
    public void MissingSourceAttributionIsInvalid()
    {
        var element = new ChemicalElementDefinition(
            6, "C", "Carbon", "", "1", "https://example.invalid", "fixture");

        Assert.That(element.IsValid, Is.False);
    }
}
