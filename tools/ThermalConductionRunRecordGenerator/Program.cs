using System.Text.Json;
using TheSingularityWorkshop.Physics.Materials;
using TheSingularityWorkshop.Physics.Thermodynamics;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: ThermalConductionRunRecordGenerator <output.json> [--check]");
    return 2;
}

var conductivity = new MaterialPropertyDefinition(
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
conductivity.EnsureValid();

var first = new LumpedThermalState(2d, 350d, 500d, 273.15d);
var second = new LumpedThermalState(2d, 300d, 500d, 273.15d);
const double area = 1d;
const double separation = 1d;
const double duration = 1d;
var result = ThermalConduction.Transfer(first, second, conductivity, area, separation, duration);
var energyBefore = first.SensibleEnergyRelativeToReferenceJoules
    + second.SensibleEnergyRelativeToReferenceJoules;
var energyAfter = result.FinalFirst.SensibleEnergyRelativeToReferenceJoules
    + result.FinalSecond.SensibleEnergyRelativeToReferenceJoules;

var record = new
{
    schemaVersion = 1,
    modelId = "thermodynamics.lumped-body-conduction",
    modelVersion = 1,
    status = "computed-from-physics-model",
    units = new
    {
        temperature = "K",
        mass = "kg",
        specificHeatCapacity = "J/(kg*K)",
        thermalConductivity = "W/(m*K)",
        area = "m^2",
        separation = "m",
        duration = "s",
        thermalConductance = "W/K",
        energy = "J"
    },
    definition = new
    {
        firstInitialState = new
        {
            massKilograms = first.MassKilograms,
            temperatureKelvin = first.TemperatureKelvin,
            specificHeatCapacityJoulesPerKilogramKelvin = first.SpecificHeatCapacityJoulesPerKilogramKelvin,
            referenceTemperatureKelvin = first.ReferenceTemperatureKelvin
        },
        secondInitialState = new
        {
            massKilograms = second.MassKilograms,
            temperatureKelvin = second.TemperatureKelvin,
            specificHeatCapacityJoulesPerKilogramKelvin = second.SpecificHeatCapacityJoulesPerKilogramKelvin,
            referenceTemperatureKelvin = second.ReferenceTemperatureKelvin
        },
        thermalConductivity = new
        {
            materialId = conductivity.MaterialId,
            propertyId = conductivity.PropertyId,
            value = conductivity.Value,
            unit = conductivity.Unit,
            evidenceStatus = conductivity.EvidenceStatus.ToString().ToLowerInvariant(),
            sourceId = conductivity.SourceId,
            sourceVersion = conductivity.SourceVersion,
            validityQuantity = conductivity.ValidityQuantity,
            validityUnit = conductivity.ValidityUnit,
            validMinimum = conductivity.ValidMinimum,
            validMaximum = conductivity.ValidMaximum,
            validConditions = conductivity.ValidConditions,
            standardUncertainty = conductivity.StandardUncertainty
        },
        contactAreaSquareMeters = area,
        separationMeters = separation,
        durationSeconds = duration
    },
    result = new
    {
        thermalConductanceWattsPerKelvin = result.ThermalConductanceWattsPerKelvin,
        energyTransferredFromFirstToSecondJoules = result.EnergyTransferredFromFirstToSecondJoules,
        totalSensibleEnergyBeforeJoules = energyBefore,
        totalSensibleEnergyAfterJoules = energyAfter,
        energyAccountingResidualJoules = energyAfter - energyBefore,
        finalFirst = new
        {
            massKilograms = result.FinalFirst.MassKilograms,
            temperatureKelvin = result.FinalFirst.TemperatureKelvin,
            specificHeatCapacityJoulesPerKilogramKelvin = result.FinalFirst.SpecificHeatCapacityJoulesPerKilogramKelvin,
            referenceTemperatureKelvin = result.FinalFirst.ReferenceTemperatureKelvin
        },
        finalSecond = new
        {
            massKilograms = result.FinalSecond.MassKilograms,
            temperatureKelvin = result.FinalSecond.TemperatureKelvin,
            specificHeatCapacityJoulesPerKilogramKelvin = result.FinalSecond.SpecificHeatCapacityJoulesPerKilogramKelvin,
            referenceTemperatureKelvin = result.FinalSecond.ReferenceTemperatureKelvin
        }
    },
    limitations = new[]
    {
        "Lumped bodies with constant specific heat capacity and one constant conductivity value.",
        "Conducting area and path length are declared inputs; contact resistance and changing geometry are not modeled.",
        "Conductivity is illustrative, not a sourced or measured value for a named real material.",
        "Explicit interval is rejected if it would reverse the bodies' temperature ordering.",
        "No spatial temperature gradients within either body, phase change, convection, or radiation."
    }
};

var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
var path = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
if (args.Length == 2 && args[1] == "--check")
{
    if (!File.Exists(path) || File.ReadAllText(path) != json)
    {
        Console.Error.WriteLine("Thermal conduction run record differs from current Physics model output.");
        Console.Error.WriteLine(json);
        return 1;
    }
    Console.WriteLine("Thermal conduction run record matches current Physics model output.");
    return 0;
}
File.WriteAllText(path, json);
Console.WriteLine($"Wrote thermal conduction run record: {path}");
return 0;
