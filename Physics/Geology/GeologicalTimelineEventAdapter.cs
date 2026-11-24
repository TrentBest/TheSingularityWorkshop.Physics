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

    /// <summary>Encodes a validated geological event as a timeline event at its geological step index.</summary>
    /// <param name="sequence">Strictly increasing sequence position within the containing history segment.</param>
    /// <param name="geologicalEvent">Immutable geological event provenance to encode.</param>
    /// <returns>A timeline event whose tick is the event's geological step index.</returns>
    public static TimelineEvent Encode(long sequence, GeologicalEventRecord geologicalEvent)
    {
        ArgumentNullException.ThrowIfNull(geologicalEvent);

        var eventData = new GeologicalEventData(
            geologicalEvent.EventId,
            geologicalEvent.EventTypeId,
            geologicalEvent.Time.StartAgeYearsBeforePresent,
            geologicalEvent.Time.EndAgeYearsBeforePresent,
            geologicalEvent.Time.SimulationStartSeconds,
            geologicalEvent.Time.SimulationEndSeconds,
            geologicalEvent.Time.StepIndex,
            geologicalEvent.SourceRevisionId,
            geologicalEvent.ResultRevisionId,
            geologicalEvent.ModelId,
            geologicalEvent.ModelVersion,
            geologicalEvent.AlgorithmId,
            geologicalEvent.AlgorithmVersion,
            geologicalEvent.ParametersJson,
            geologicalEvent.Summary);

        string payload = JsonSerializer.Serialize(
            new GeologicalEventPayload(PayloadSchemaVersion, eventData),
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

        GeologicalEventData data = payload.Event;
        if (data.StepIndex != timelineEvent.Tick)
            throw new ArgumentException("Timeline tick must equal the geological event step index.", nameof(timelineEvent));

        try
        {
            return new GeologicalEventRecord(
                data.EventId,
                data.EventTypeId,
                new GeologicalTimeInterval(
                    data.StartAgeYearsBeforePresent,
                    data.EndAgeYearsBeforePresent,
                    data.SimulationStartSeconds,
                    data.SimulationEndSeconds,
                    data.StepIndex),
                data.SourceRevisionId,
                data.ResultRevisionId,
                data.ModelId,
                data.ModelVersion,
                data.AlgorithmId,
                data.AlgorithmVersion,
                data.ParametersJson,
                data.Summary);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException("Geological timeline event fields do not satisfy the event contract.", nameof(timelineEvent), exception);
        }
    }

    private sealed record GeologicalEventPayload(int SchemaVersion, GeologicalEventData Event);

    private sealed record GeologicalEventData(
        string EventId,
        string EventTypeId,
        double StartAgeYearsBeforePresent,
        double EndAgeYearsBeforePresent,
        double SimulationStartSeconds,
        double SimulationEndSeconds,
        long StepIndex,
        string SourceRevisionId,
        string ResultRevisionId,
        string ModelId,
        string ModelVersion,
        string AlgorithmId,
        string AlgorithmVersion,
        string ParametersJson,
        string Summary);
}
