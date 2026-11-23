namespace TheSingularityWorkshop.Physics.Geology;

/// <summary>
/// A geological interval with explicit age-before-present and simulation-clock conventions.
/// </summary>
/// <remarks>
/// Geological age decreases as the simulation advances toward the present. Simulation elapsed
/// seconds increase. These clocks are intentionally separate: a large geological interval may
/// summarize many internal solver steps, and an event may occupy an instantaneous geological age.
/// </remarks>
public readonly record struct GeologicalTimeInterval
{
    public double StartAgeYearsBeforePresent { get; }
    public double EndAgeYearsBeforePresent { get; }
    public double SimulationStartSeconds { get; }
    public double SimulationEndSeconds { get; }
    public long StepIndex { get; }

    /// <summary>Gets the geological duration represented by this interval in years.</summary>
    public double GeologicalDurationYears => StartAgeYearsBeforePresent - EndAgeYearsBeforePresent;

    /// <summary>Gets the elapsed execution duration represented by this interval in seconds.</summary>
    public double SimulationDurationSeconds => SimulationEndSeconds - SimulationStartSeconds;

    public GeologicalTimeInterval(
        double startAgeYearsBeforePresent,
        double endAgeYearsBeforePresent,
        double simulationStartSeconds,
        double simulationEndSeconds,
        long stepIndex)
    {
        RequireFiniteNonNegative(startAgeYearsBeforePresent, nameof(startAgeYearsBeforePresent));
        RequireFiniteNonNegative(endAgeYearsBeforePresent, nameof(endAgeYearsBeforePresent));
        RequireFiniteNonNegative(simulationStartSeconds, nameof(simulationStartSeconds));
        RequireFiniteNonNegative(simulationEndSeconds, nameof(simulationEndSeconds));
        if (startAgeYearsBeforePresent < endAgeYearsBeforePresent)
            throw new ArgumentException("Geological age must not increase as simulation time advances.");
        if (simulationEndSeconds < simulationStartSeconds)
            throw new ArgumentException("Simulation elapsed time must not decrease.");
        if (stepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(stepIndex));

        StartAgeYearsBeforePresent = startAgeYearsBeforePresent;
        EndAgeYearsBeforePresent = endAgeYearsBeforePresent;
        SimulationStartSeconds = simulationStartSeconds;
        SimulationEndSeconds = simulationEndSeconds;
        StepIndex = stepIndex;
    }

    private static void RequireFiniteNonNegative(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0d)
            throw new ArgumentOutOfRangeException(parameterName, "Time values must be finite and non-negative.");
    }
}

/// <summary>
/// Immutable provenance record for a geological event or process result over a declared time interval.
/// </summary>
/// <remarks>
/// This record describes a validated event/result boundary; it does not execute a geological
/// process, mutate terrain, or guarantee that the referenced revisions exist. A coordinator must
/// validate referenced state revisions and commit candidate state atomically.
/// </remarks>
public sealed record GeologicalEventRecord
{
    public string EventId { get; }
    public string EventTypeId { get; }
    public GeologicalTimeInterval Time { get; }
    public string SourceRevisionId { get; }
    public string ResultRevisionId { get; }
    public string ModelId { get; }
    public string ModelVersion { get; }
    public string AlgorithmId { get; }
    public string AlgorithmVersion { get; }
    public string ParametersJson { get; }
    public string Summary { get; }

    public GeologicalEventRecord(
        string eventId,
        string eventTypeId,
        GeologicalTimeInterval time,
        string sourceRevisionId,
        string resultRevisionId,
        string modelId,
        string modelVersion,
        string algorithmId,
        string algorithmVersion,
        string parametersJson,
        string summary)
    {
        EventId = RequireText(eventId, nameof(eventId));
        EventTypeId = RequireText(eventTypeId, nameof(eventTypeId));
        Time = time;
        SourceRevisionId = RequireText(sourceRevisionId, nameof(sourceRevisionId));
        ResultRevisionId = RequireText(resultRevisionId, nameof(resultRevisionId));
        ModelId = RequireText(modelId, nameof(modelId));
        ModelVersion = RequireText(modelVersion, nameof(modelVersion));
        AlgorithmId = RequireText(algorithmId, nameof(algorithmId));
        AlgorithmVersion = RequireText(algorithmVersion, nameof(algorithmVersion));
        ParametersJson = RequireJsonObject(parametersJson);
        Summary = RequireText(summary, nameof(summary));
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A non-empty value is required.", parameterName);
        return value;
    }

    private static string RequireJsonObject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A JSON object is required.", nameof(value));
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
                throw new ArgumentException("Event parameters must be a JSON object.", nameof(value));
            return value;
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ArgumentException("Event parameters must be valid JSON.", nameof(value), exception);
        }
    }
}
