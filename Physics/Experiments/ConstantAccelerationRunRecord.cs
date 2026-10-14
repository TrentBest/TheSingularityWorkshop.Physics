using System.Text.Json;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// A durable, deterministic representation of one executed constant-acceleration
/// experiment. This is an initial schema, not yet a general physics provenance format.
/// </summary>
public sealed record ConstantAccelerationRunRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Stable identifier for the analytical model.</summary>
    public string ModelId { get; init; } = "kinematics.constant-acceleration";

    /// <summary>Version of the model contract, independent of package version.</summary>
    public string ModelVersion { get; init; } = "1";

    /// <summary>Version of this run-record schema.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Units used by this model.</summary>
    public IReadOnlyDictionary<string, string> Units { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["acceleration"] = "m/s^2",
            ["position"] = "m",
            ["time"] = "s",
            ["velocity"] = "m/s"
        };

    /// <summary>The declared inputs that produced the run.</summary>
    public required ConstantAccelerationExperimentDefinition Definition { get; init; }

    /// <summary>Recorded state samples, including the initial state.</summary>
    public required IReadOnlyList<KinematicState> Samples { get; init; }

    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final observed state from the recorded samples.</summary>
    public KinematicState FinalState => Samples[^1];

    /// <summary>Creates a record from an executed experiment's actual samples.</summary>
    public static ConstantAccelerationRunRecord FromResult(
        ConstantAccelerationExperimentResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ConstantAccelerationRunRecord
        {
            Definition = result.Definition,
            Samples = Array.AsReadOnly(result.Samples.ToArray())
        };
    }

    /// <summary>Serializes this record as stable, human-readable JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}
