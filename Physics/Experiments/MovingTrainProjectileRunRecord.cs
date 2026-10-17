using System.Text.Json;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>Versioned, reproducible record of a paired moving-train projectile run.</summary>
public sealed record MovingTrainProjectileRunRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static MovingTrainProjectileRunRecord()
    {
        SerializerOptions.Converters.Add(new Vector3dJsonConverter());
    }

    /// <summary>Stable identity of the paired experiment.</summary>
    public string ModelId { get; init; } = "kinematics.moving-train-projectile";

    /// <summary>Version of the model contract.</summary>
    public string ModelVersion { get; init; } = "1";

    /// <summary>Version of this JSON schema.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Explicit units for the physical quantities in this record.</summary>
    public IReadOnlyDictionary<string, string> Units { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["acceleration"] = "m/s^2",
            ["position"] = "m",
            ["time"] = "s",
            ["velocity"] = "m/s"
        };

    /// <summary>Inputs that produced the paired samples.</summary>
    public required MovingTrainProjectileExperimentDefinition Definition { get; init; }

    /// <summary>Train and ground states paired at each identical logical timestamp.</summary>
    public required IReadOnlyList<MovingTrainProjectileSample> Samples { get; init; }

    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Creates a durable record from the actual paired experiment output.</summary>
    public static MovingTrainProjectileRunRecord FromResult(MovingTrainProjectileExperimentResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new MovingTrainProjectileRunRecord
        {
            Definition = result.Definition,
            Samples = Array.AsReadOnly(result.Samples.ToArray())
        };
    }

    /// <summary>Serializes this run record as human-readable JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}
