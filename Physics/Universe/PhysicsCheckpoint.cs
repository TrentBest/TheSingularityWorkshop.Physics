using System.Security.Cryptography;

namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Immutable envelope for a serialized authoritative world state at one logical tick.
/// The payload format belongs to the model/persistence adapter; this type does not serialize physics objects.
/// </summary>
public sealed class PhysicsCheckpoint
{
    private readonly byte[] _statePayload;

    /// <summary>Creates a checkpoint envelope and copies the supplied state payload.</summary>
    /// <param name="id">Stable checkpoint identifier.</param>
    /// <param name="realityId">Reality containing the state.</param>
    /// <param name="timelineId">Timeline containing the state.</param>
    /// <param name="tick">Logical tick represented by the state.</param>
    /// <param name="schemaVersion">Version of the serialized state schema.</param>
    /// <param name="modelVersion">Version of the physical model/configuration.</param>
    /// <param name="statePayload">Non-empty opaque serialized state bytes.</param>
    public PhysicsCheckpoint(
        string id,
        string realityId,
        string timelineId,
        long tick,
        string schemaVersion,
        string modelVersion,
        ReadOnlySpan<byte> statePayload)
    {
        Id = RequireIdentifier(id, nameof(id));
        RealityId = RequireIdentifier(realityId, nameof(realityId));
        TimelineId = RequireIdentifier(timelineId, nameof(timelineId));
        SchemaVersion = RequireIdentifier(schemaVersion, nameof(schemaVersion));
        ModelVersion = RequireIdentifier(modelVersion, nameof(modelVersion));

        if (tick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tick), "Logical ticks cannot be negative.");
        }

        if (statePayload.IsEmpty)
        {
            throw new ArgumentException("A checkpoint must contain a non-empty state payload.", nameof(statePayload));
        }

        Tick = tick;
        _statePayload = statePayload.ToArray();
        StateSha256 = Convert.ToHexString(SHA256.HashData(_statePayload));
    }

    /// <summary>Stable checkpoint identifier.</summary>
    public string Id { get; }
    /// <summary>Reality identifier.</summary>
    public string RealityId { get; }
    /// <summary>Timeline identifier.</summary>
    public string TimelineId { get; }
    /// <summary>Logical tick represented by the checkpoint.</summary>
    public long Tick { get; }
    /// <summary>Serialized state schema version.</summary>
    public string SchemaVersion { get; }
    /// <summary>Physical model/configuration version.</summary>
    public string ModelVersion { get; }

    /// <summary>SHA-256 digest of the opaque serialized payload, useful for integrity checks.</summary>
    public string StateSha256 { get; }

    /// <summary>Length of the serialized state payload in bytes.</summary>
    public int PayloadLength => _statePayload.Length;

    /// <summary>Returns an isolated copy so callers cannot mutate the checkpoint's stored bytes.</summary>
    public byte[] CopyStatePayload() => (byte[])_statePayload.Clone();

    private static string RequireIdentifier(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Identifiers and version labels must be non-empty.", parameterName);
        }

        return value;
    }
}
