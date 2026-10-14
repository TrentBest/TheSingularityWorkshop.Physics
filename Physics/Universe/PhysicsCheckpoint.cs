using System.Security.Cryptography;

namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Immutable envelope for a serialized authoritative world state at one logical tick.
/// The payload format belongs to the model/persistence adapter; this type does not serialize physics objects.
/// </summary>
public sealed class PhysicsCheckpoint
{
    private readonly byte[] _statePayload;

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

    public string Id { get; }
    public string RealityId { get; }
    public string TimelineId { get; }
    public long Tick { get; }
    public string SchemaVersion { get; }
    public string ModelVersion { get; }

    /// <summary>SHA-256 digest of the opaque serialized payload, useful for integrity checks.</summary>
    public string StateSha256 { get; }

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
