using System.Text.Json;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// Encodes geological event provenance into the existing ordered timeline event stream.
/// </summary>
/// <remarks>
/// This adapter deliberately reuses <see cref="TimelineEvent"/> and
/// <see cref="TimelineReplayExecutor"/> rather than introducing a second replay system.
/// It transports event metadata only; a geological model-specific replay callback remains
/// responsible for interpreting the event and deterministically reconstructing physical state.
/// </remarks>
public static class GeologicalTimelineEventAdapter
{
    /// <summary>Stable timeline event kind used for geological event records.</summary>
    public const string EventKind = "geology.event-record";

    /// <summary>Current version of the JSON payload envelope.</summary>
    public const int PayloadSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false
    };

    /// <summary>
    /// Encodes a validated geological event as an ordered timeline event at its geological step index.
    /// </summary>
    /// <param name="sequence">Strictly increasing sequence position within the containing history segment.</param>
    /// <param name="geologicalEvent">Immutable geological event provenance to encode.</param>
    /// <returns>A timeline event whose tick is the event's geological step index.</returns>
    public static TimelineEvent Encode(long sequence, GeologicalEventRecord geologicalEvent)
    {
        ArgumentNullException.ThrowIfNull(geologicalEvent);

        string payload = JsonSerializer.Serialize(
            new GeologicalEventPayload(PayloadSchemaVersion, geologicalEvent),
            SerializerOptions);

        return new TimelineEvent(sequence, geologicalEvent.Time.StepIndex, EventKind, payload);
    }

    /// <summary>Decodes and validates a geological event from a timeline event.</summary>
    /// <param name="timelineEvent">Timeline event to decode.</param>
    /// <returns>The immutable geological event record carried by the timeline event.</returns>
    /// <exception cref="ArgumentException">The event kind, payload, schema, or tick is invalid.</exception>
    public static GeologicalEventRecord Decode(TimelineEvent timelineEvent)
    {
        ArgumentNullException.ThrowIfNull(timelineEvent);

        if (!string.Equals(timelineEvent.Kind, EventKind, StringComparison.Ordinal))
            throw new ArgumentException("Timeline event is not a geological event record.", nameof(timelineEvent));

        GeologicalEventPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<GeologicalEventPayload>(timelineEvent.Payload, SerializerOptions);
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Geological timeline payload is not valid JSON for this schema.", nameof(timelineEvent), exception);
        }

        if (payload is null || payload.SchemaVersion != PayloadSchemaVersion || payload.Event is null)
            throw new ArgumentException("Geological timeline payload has an unsupported or incomplete schema.", nameof(timelineEvent));

        if (payload.Event.Time.StepIndex != timelineEvent.Tick)
            throw new ArgumentException("Timeline tick must equal the geological event step index.", nameof(timelineEvent));

        return payload.Event;
    }

    private sealed record GeologicalEventPayload(int SchemaVersion, GeologicalEventRecord Event);
}
