namespace TheSingularityWorkshop.Physics.Mechanics;

/// <summary>
/// Detects the first threshold crossing in the absolute axial spring-damper force
/// reported for one link. This is a load-threshold monitor for a compliant link,
/// not a rigid-constraint solver or a material damage model.
/// Force is measured in newtons. Failure occurs only when the absolute force is
/// strictly greater than the configured threshold.
/// </summary>
public sealed class AxialLinkFailureMonitor
{
    private readonly object _gate = new();
    private readonly string _linkId;
    private readonly double _maximumAbsoluteForce;
    private long _lastStepIndex = -1;
    private double _lastElapsedTime = -1d;
    private FailureTransition? _transition;

    /// <summary>Immutable evidence for the first threshold-crossing step.</summary>
    public sealed record FailureTransition(
        string LinkId,
        long StepIndex,
        double ElapsedTime,
        double AxialForceOnBodyA,
        double MaximumAbsoluteForce);

    /// <summary>
    /// Creates a monitor for one link. The threshold is a caller-supplied
    /// engineering criterion; this class does not infer material strength.
    /// </summary>
    public AxialLinkFailureMonitor(string linkId, double maximumAbsoluteForce)
    {
        if (string.IsNullOrWhiteSpace(linkId))
            throw new ArgumentException("Link identity is required.", nameof(linkId));
        if (!double.IsFinite(maximumAbsoluteForce) || maximumAbsoluteForce < 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumAbsoluteForce),
                "Force threshold must be finite and non-negative.");

        _linkId = linkId;
        _maximumAbsoluteForce = maximumAbsoluteForce;
    }

    /// <summary>The first recorded failure transition, or null before failure.</summary>
    public FailureTransition? Transition
    {
        get { lock (_gate) return _transition; }
    }

    /// <summary>
    /// Consumes one step result for the monitored link. Observations must arrive
    /// in strictly increasing step order and non-decreasing elapsed time.
    /// Returns a transition only on the first sample whose absolute axial force
    /// exceeds the threshold; equal-to-threshold is not a failure.
    /// </summary>
    public FailureTransition? Observe(AxialSpringDamper.StepResult observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        if (!string.Equals(observation.LinkId, _linkId, StringComparison.Ordinal))
            throw new ArgumentException("Observation belongs to a different link.", nameof(observation));
        if (observation.StepIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(observation), "Step index must be non-negative.");
        if (!double.IsFinite(observation.ElapsedTime) || observation.ElapsedTime < 0d)
            throw new ArgumentOutOfRangeException(nameof(observation), "Elapsed time must be finite and non-negative.");
        if (!double.IsFinite(observation.AxialForceOnBodyA))
            throw new ArgumentOutOfRangeException(nameof(observation), "Axial force must be finite.");

        lock (_gate)
        {
            if (_transition is not null)
                return null;

            if (observation.StepIndex <= _lastStepIndex)
                throw new ArgumentException("Observation step indices must be strictly increasing.", nameof(observation));
            if (observation.ElapsedTime < _lastElapsedTime)
                throw new ArgumentException("Observation elapsed time must be non-decreasing.", nameof(observation));

            _lastStepIndex = observation.StepIndex;
            _lastElapsedTime = observation.ElapsedTime;

            if (Math.Abs(observation.AxialForceOnBodyA) <= _maximumAbsoluteForce)
                return null;

            _transition = new FailureTransition(
                _linkId,
                observation.StepIndex,
                observation.ElapsedTime,
                observation.AxialForceOnBodyA,
                _maximumAbsoluteForce);
            return _transition;
        }
    }
}
