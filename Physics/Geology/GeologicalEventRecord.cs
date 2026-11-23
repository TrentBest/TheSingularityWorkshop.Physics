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
    /// <summary>Geological age at the beginning of the interval, measured in years before present.</summary>
    public double StartAgeYearsBeforePresent { get; }
    /// <summary>Geological age at the end of the interval, measured in years before present.</summary>
    public double EndAgeYearsBeforePresent { get; }
    /// <summary>Cumulative simulation elapsed time at the beginning of the interval, in seconds.</summary>
    public double SimulationStartSeconds { get; }
    /// <summary>Cumulative simulation elapsed time at the end of the interval, in seconds.</summary>
    public double SimulationEndSeconds { get; }
    /// <summary>Zero-based or caller-defined non-negative geological step identity.</summary>
    public long StepIndex { get; }

    /// <summary>Gets the geological duration represented by this interval in years.</summary>
    public double GeologicalDurationYears => StartAgeYearsBeforePresent - EndAgeYearsBeforePresent;

    /// <summary>Gets the elapsed execution duration represented by this interval in seconds.</summary>
    public double SimulationDurationSeconds => SimulationEndSeconds - SimulationStartSeconds;

    /// <summary>Creates a validated geological interval with separate geological and simulation clocks.</summary>
    /// <param name="startAgeYearsBeforePresent">Geological age at interval start.</param>
    /// <param name="endAgeYearsBeforePresent">Geological age at interval end.</param>
    /// <param name="simulationStartSeconds">Cumulative simulation seconds at interval start.</param>
    /// <param name="simulationEndSeconds">Cumulative simulation seconds at interval end.</param>
    /// <param name="stepIndex">Non-negative step identity.</param>
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
    /// <summary>Gets the stable identity of this event.</summary>
    public string EventId { get; }
    /// <summary>Gets the stable event taxonomy identity.</summary>
    public string EventTypeId { get; }
    /// <summary>Gets the geological and simulation interval occupied by this event.</summary>
    public GeologicalTimeInterval Time { get; }
    /// <summary>Gets the immutable input world-state revision identity.</summary>
    public string SourceRevisionId { get; }
    /// <summary>Gets the candidate/result world-state revision identity.</summary>
    public string ResultRevisionId { get; }
    /// <summary>Gets the stable physical model identity.</summary>
    public string ModelId { get; }
    /// <summary>Gets the physical model version.</summary>
    public string ModelVersion { get; }
    /// <summary>Gets the stable algorithm identity.</summary>
    public string AlgorithmId { get; }
    /// <summary>Gets the algorithm version.</summary>
    public string AlgorithmVersion { get; }
    /// <summary>Gets the validated JSON-object parameters supplied to the event model.</summary>
    public string ParametersJson { get; }
    /// <summary>Gets the concise human-readable event summary.</summary>
    public string Summary { get; }

    /// <summary>Creates an immutable, validated geological event provenance record.</summary>
    /// <param name="eventId">Stable event identity.</param>
    /// <param name="eventTypeId">Stable event taxonomy identity.</param>
    /// <param name="time">Geological and simulation interval for the event.</param>
    /// <param name="sourceRevisionId">Input world-state revision identity.</param>
    /// <param name="resultRevisionId">Candidate/result world-state revision identity.</param>
    /// <param name="modelId">Physical model identity.</param>
    /// <param name="modelVersion">Physical model version.</param>
    /// <param name="algorithmId">Algorithm identity.</param>
    /// <param name="algorithmVersion">Algorithm version.</param>
    /// <param name="parametersJson">Validated JSON-object model parameters.</param>
    /// <param name="summary">Human-readable event summary.</param>
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
