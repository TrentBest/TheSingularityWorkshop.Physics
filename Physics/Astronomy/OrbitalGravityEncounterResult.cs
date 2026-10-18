namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>A merger event annotated with its elapsed simulation time.</summary>
/// <param name="TimeSeconds">Elapsed seconds from the start of the gravity encounter interval.</param>
/// <param name="MergeEvent">The deterministic pairwise merger result.</param>
public sealed record OrbitalTimedMergeEvent(double TimeSeconds, OrbitalMergeEvent MergeEvent);

/// <summary>The final state and timed merger history of a gravity-integrated encounter interval.</summary>
/// <param name="FinalBodies">Remaining bodies, sorted by ordinal body identity.</param>
/// <param name="MergeEvents">Merger events in chronological, deterministic resolution order.</param>
/// <param name="TimedMergeEvents">The same events annotated with estimated elapsed simulation times.</param>
/// <param name="ElapsedSeconds">The full interval simulated by the coordinator.</param>
public sealed record OrbitalGravityEncounterResult(
    IReadOnlyList<OrbitalBodyState> FinalBodies,
    IReadOnlyList<OrbitalMergeEvent> MergeEvents,
    IReadOnlyList<OrbitalTimedMergeEvent> TimedMergeEvents,
    double ElapsedSeconds);
