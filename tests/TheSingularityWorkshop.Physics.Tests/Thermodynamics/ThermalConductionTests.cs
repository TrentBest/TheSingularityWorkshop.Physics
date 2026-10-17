using NUnit.Framework;
using TheSingularityWorkshop.Physics.Materials;
using TheSingularityWorkshop.Physics.Thermodynamics;

namespace TheSingularityWorkshop.Physics.Tests.Thermodynamics;

[TestFixture]
public sealed class ThermalConductionTests
{
    [Test]
    public void Transfer_MovesHeatDownGradientAndConservesSensibleEnergy()
    {
        var first = CreateBody(350d);
        var second = CreateBody(300d);

        var result = ThermalConduction.Transfer(
            first, second, CreateConductivity(),
            contactAreaSquareMeters: 1d,
            separationMeters: 1d,
            durationSeconds: 1d);

        Assert.That(result.ThermalConductanceWattsPerKelvin, Is.EqualTo(10d).Within(1e-12));
        Assert.That(result.EnergyTransferredFromFirstToSecondJoules, Is.EqualTo(500d).Within(1e-12));
        Assert.That(result.FinalFirst.TemperatureKelvin, Is.EqualTo(349.5d).Within(1e-12));
        Assert.That(result.FinalSecond.TemperatureKelvin, Is.EqualTo(300.5d).Within(1e-12));
        Assert.That(
            result.FinalFirst.SensibleEnergyRelativeToReferenceJoules
            + result.FinalSecond.SensibleEnergyRelativeToReferenceJoules,
            Is.EqualTo(first.SensibleEnergyRelativeToReferenceJoules
                + second.SensibleEnergyRelativeToReferenceJoules).Within(1e-9));
    }

    [Test]
    public void Transfer_UsesSignedEnergyWhenSecondBodyIsHotter()
    {
        var result = ThermalConduction.Transfer(
            CreateBody(300d), CreateBody(350d), CreateConductivity(),
            contactAreaSquareMeters: 1d, separationMeters: 1d, durationSeconds: 1d);

        Assert.That(result.EnergyTransferredFromFirstToSecondJoules, Is.EqualTo(-500d).Within(1e-12));
        Assert.That(result.FinalFirst.TemperatureKelvin, Is.EqualTo(300.5d).Within(1e-12));
        Assert.That(result.FinalSecond.TemperatureKelvin, Is.EqualTo(349.5d).Within(1e-12));
    }

    [Test]
    public void Transfer_RejectsIntervalThatWouldOvershootThermalEquilibrium()
    {
        Assert.Throws<InvalidOperationException>(() => ThermalConduction.Transfer(
            CreateBody(350d), CreateBody(300d), CreateConductivity(),
            contactAreaSquareMeters: 1d, separationMeters: 1d, durationSeconds: 100d));
    }

    [Test]
    public void Transfer_RejectsBodyOutsidePropertyValidityRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ThermalConduction.Transfer(
            CreateBody(400d), CreateBody(300d), CreateConductivity(),
            contactAreaSquareMeters: 1d, separationMeters: 1d, durationSeconds: 1d));
    }

    [Test]
    public void Transfer_RejectsInvalidGeometryAndWrongProperty()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ThermalConduction.Transfer(
            CreateBody(350d), CreateBody(300d), CreateConductivity(),
            contactAreaSquareMeters: 0d, separationMeters: 1d, durationSeconds: 1d));

        Assert.Throws<ArgumentException>(() => ThermalConduction.Transfer(
            CreateBody(350d), CreateBody(300d),
            CreateConductivity() with { PropertyId = "specific-heat-capacity" },
            contactAreaSquareMeters: 1d, separationMeters: 1d, durationSeconds: 1d));
    }

    private static LumpedThermalState CreateBody(double temperatureKelvin) =>
        new(2d, temperatureKelvin, 500d, 273.15d);

    private static MaterialPropertyDefinition CreateConductivity() => new(
        MaterialId: "illustrative-solid",
        PropertyId: "thermal-conductivity",
        Value: 10d,
        Unit: "W/(m*K)",
        EvidenceStatus: PropertyEvidenceStatus.Illustrative,
        SourceId: "workshop.thermal-conduction-fixture",
        SourceVersion: "1",
        ValidityQuantity: "temperature",
        ValidityUnit: "K",
        ValidMinimum: 273.15d,
        ValidMaximum: 373.15d,
        ValidConditions: "Illustrative constant conductivity over the example temperature interval.",
        StandardUncertainty: null);
}
