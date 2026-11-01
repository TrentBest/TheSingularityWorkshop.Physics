using System.Text.Json;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>Versioned evidence record for one analytical frictionless inclined-plane run.</summary>
public sealed record InclinedPlaneRunRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Stable identifier for the physical model.</summary>
    public string ModelId { get; init; } = "mechanics.frictionless-inclined-plane";

    /// <summary>Version of the model contract.</summary>
    public string ModelVersion { get; init; } = "1";

    /// <summary>Version of this serialized record schema.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Units used by the model.</summary>
    public IReadOnlyDictionary<string, string> Units { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["acceleration"] = "m/s^2",
            ["angle"] = "rad",
            ["force"] = "N",
            ["length"] = "m",
            ["mass"] = "kg",
            ["time"] = "s",
            ["velocity"] = "m/s"
        };

    /// <summary>Exact inputs supplied to the experiment.</summary>
    public required InclinedPlaneExperimentDefinition Definition { get; init; }

    /// <summary>Computed physical outputs from the Physics model.</summary>
    public required InclinedPlaneResult Result { get; init; }

    /// <summary>Creates a record from the actual experiment result.</summary>
    public static InclinedPlaneRunRecord FromResult(InclinedPlaneExperimentResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new InclinedPlaneRunRecord
        {
            Definition = result.Definition,
            Result = result.Outcome
        };
    }

    /// <summary>Serializes the record as human-readable JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}
