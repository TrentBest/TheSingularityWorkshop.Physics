using System.Text.Json;
using TheSingularityWorkshop.Physics.Experiments;
using TheSingularityWorkshop.Physics.Materials;
using TheSingularityWorkshop.Physics.Thermodynamics;

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--check"))
{
    Console.Error.WriteLine("Usage: ElectricalThermalCouplingRunRecordGenerator <output.json> [--check]");
    return 2;
}

var specificHeatCapacity = new MaterialPropertyDefinition(
    MaterialId: "reference-thermal-body",
    PropertyId: "specific-heat-capacity",
    Value: 500d,
    Unit: "J/(kg*K)",
    EvidenceStatus: PropertyEvidenceStatus.Illustrative,
    SourceId: "workshop.electrical-thermal-fixture",
    SourceVersion: "1",
    ValidityQuantity: "temperature",
    ValidityUnit: "K",
    ValidMinimum: 273.15d,
    ValidMaximum: 373.15d,
    ValidConditions: "Illustrative constant value over the example temperature interval.",
    StandardUncertainty: null);
specificHeatCapacity.EnsureValid();

var initial = new LumpedThermalState(
    MassKilograms: 2d,
    TemperatureKelvin: 300d,
    SpecificHeatCapacityJoulesPerKilogramKelvin: specificHeatCapacity.Value,
    ReferenceTemperatureKelvin: 273.15d);
const double voltage = 12d;
const double resistance = 6d;
const double capacity = 100d;
const double duration = 10d;
const double absorbedFraction = 0.5d;
var result = ElectricalThermalCouplingExperiment.Run(
    initial, voltage, resistance, capacity, duration, absorbedFraction);

var record = new
{
    schemaVersion = 1,
    modelId = "coupling.electrical-to-lumped-thermal",
    modelVersion = 1,
    status = "computed-from-physics-model",
    units = new
    {
        voltage = "V",
        resistance = "ohm",
        current = "A",
        power = "W",
        duration = "s",
        energy = "J",
        mass = "kg",
        specificHeatCapacity = "J/(kg*K)",
        temperature = "K"
    },
    definition = new
    {
        busVoltageVolts = voltage,
        loadResistanceOhms = resistance,
        generationCapacityWatts = capacity,
        durationSeconds = duration,
        absorbedPowerFraction = absorbedFraction,
        initialThermalState = new
        {
            massKilograms = initial.MassKilograms,
            temperatureKelvin = initial.TemperatureKelvin,
            specificHeatCapacityJoulesPerKilogramKelvin = initial.SpecificHeatCapacityJoulesPerKilogramKelvin,
            specificHeatCapacityProperty = new
            {
                materialId = specificHeatCapacity.MaterialId,
                propertyId = specificHeatCapacity.PropertyId,
                value = specificHeatCapacity.Value,
                unit = specificHeatCapacity.Unit,
                evidenceStatus = specificHeatCapacity.EvidenceStatus.ToString().ToLowerInvariant(),
                sourceId = specificHeatCapacity.SourceId,
                sourceVersion = specificHeatCapacity.SourceVersion,
                validityQuantity = specificHeatCapacity.ValidityQuantity,
                validityUnit = specificHeatCapacity.ValidityUnit,
                validMinimum = specificHeatCapacity.ValidMinimum,
                validMaximum = specificHeatCapacity.ValidMaximum,
                validConditions = specificHeatCapacity.ValidConditions,
                standardUncertainty = specificHeatCapacity.StandardUncertainty
            },
            referenceTemperatureKelvin = initial.ReferenceTemperatureKelvin
        }
    },
    electricalObservation = new
    {
        busVoltageVolts = result.ElectricalObservation.BusVoltageVolts,
        generationCapacityWatts = result.ElectricalObservation.GenerationCapacityWatts,
        demandWatts = result.ElectricalObservation.DemandWatts,
        totalCurrentAmperes = result.ElectricalObservation.TotalCurrentAmperes,
        remainingCapacityWatts = result.ElectricalObservation.RemainingCapacityWatts,
        isOverloaded = result.ElectricalObservation.IsOverloaded,
        loads = result.ElectricalObservation.Loads.Select(load => new
        {
            loadId = load.LoadId,
            isEnabled = load.IsEnabled,
            voltageVolts = load.VoltageVolts,
            resistanceOhms = load.ResistanceOhms,
            currentAmperes = load.CurrentAmperes,
            powerWatts = load.PowerWatts
        }).ToArray()
    },
    result = new
    {
        electricalEnergyJoules = result.ElectricalEnergyJoules,
        thermalEnergyTransferJoules = result.ThermalEnergyTransferJoules,
        energyNotTransferredToThermalBodyJoules = result.EnergyNotTransferredToThermalBodyJoules,
        finalThermalState = new
        {
            massKilograms = result.FinalThermalState.MassKilograms,
            temperatureKelvin = result.FinalThermalState.TemperatureKelvin,
            specificHeatCapacityJoulesPerKilogramKelvin = result.FinalThermalState.SpecificHeatCapacityJoulesPerKilogramKelvin,
            referenceTemperatureKelvin = result.FinalThermalState.ReferenceTemperatureKelvin
        }
    },
    limitations = new[]
    {
        "Ideal fixed-voltage DC bus and constant resistive load.",
        "Absorbed fraction is a declared input, not a predicted efficiency.",
        "Energy not transferred to the thermal body is not assigned to a loss mechanism or destination.",
        "No temperature-dependent resistance, external heat exchange, phase change, or spatial temperature gradient."
    }
};
var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
var path = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(path)!);
if (args.Length == 2 && args[1] == "--check")
{
    if (!File.Exists(path) || File.ReadAllText(path) != json)
    {
        Console.Error.WriteLine("Electrical-to-thermal run record differs from current Physics model output.");
        Console.Error.WriteLine(json);
        return 1;
    }
    Console.WriteLine("Electrical-to-thermal run record matches current Physics model output.");
    return 0;
}
File.WriteAllText(path, json);
Console.WriteLine($"Wrote electrical-to-thermal run record: {path}");
return 0;
