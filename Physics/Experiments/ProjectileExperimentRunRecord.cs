using System.Text.Json;
using TheSingularityWorkshop.Physics.Kinematics;

namespace TheSingularityWorkshop.Physics.Experiments;

/// <summary>
/// Durable record of one executed ideal 3D projectile experiment.
/// This is a bounded model-specific schema, not a universal provenance format.
/// </summary>
public sealed record ProjectileExperimentRunRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Stable identifier for the model contract.</summary>
    public string ModelId { get; init; } = "kinematics.constant-acceleration-projectile";

    /// <summary>Version of the model contract, independent of package version.</summary>
    public string ModelVersion { get; init; } = "1";

    /// <summary>Version of this run-record schema.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Coordinate convention used by the model.</summary>
    public string CoordinateConvention { get; init; } =
        "Right-handed, z-up; azimuth in the x-y plane from +X toward +Y; elevation from the x-y plane.";

    /// <summary>Units used by this model.</summary>
    public IReadOnlyDictionary<string, string> Units { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["acceleration"] = "m/s^2",
            ["angle"] = "degree",
            ["position"] = "m",
            ["speed"] = "m/s",
            ["time"] = "s",
            ["velocity"] = "m/s"
        };

    /// <summary>Exact inputs that produced the recorded run.</summary>
    public required ProjectileExperimentDefinition Definition { get; init; }

    /// <summary>Recorded states, including the initial state.</summary>
    public required IReadOnlyList<ProjectileState> Samples { get; init; }

    /// <summary>Number of completed logical steps.</summary>
    public int CompletedSteps => Samples.Count - 1;

    /// <summary>Final observed state from the recorded samples.</summary>
    public ProjectileState FinalState => Samples[^1];

    /// <summary>Creates a record from the actual samples of an executed experiment.</summary>
    public static ProjectileExperimentRunRecord FromResult(ProjectileExperimentResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ProjectileExperimentRunRecord
        {
            Definition = result.Definition,
            Samples = Array.AsReadOnly(result.Samples.ToArray())
        };
    }

    /// <summary>Serializes this record as human-readable JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, SerializerOptions);
}
