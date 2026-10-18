namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>Direction of change in the simplified local orbital tidal-potential signal.</summary>
public enum TidalForcingState
{
    /// <summary>The sampled potential is increasing.</summary>
    Rising,
    /// <summary>The sampled potential is decreasing.</summary>
    Falling,
    /// <summary>The sampled derivative is within the configured turning-point tolerance.</summary>
    TurningPoint
}

/// <summary>Inputs for a circular-orbit, equatorial, single-perturber tidal-potential reference model.</summary>
/// <remarks>
/// This model returns differential tidal potential, not ocean height, shoreline position, or a king-tide
/// forecast. It assumes a circular orbit and an equatorial observer; eccentricity, inclination, multiple
/// perturbers, planetary deformation, ocean response, basin geometry, and weather are outside this model.
/// The gravitational constant is supplied explicitly so its value and provenance remain a caller decision.
/// </remarks>
public sealed record OrbitalTideForcingInput(
    string WorldId,
    string EpochId,
    double PerturberMassKg,
    double OrbitalDistanceMeters,
    double WorldRadiusMeters,
    double GravitationalConstantM3PerKgSecondSquared,
    double OrbitalPeriodSeconds,
    double WorldRotationPeriodSeconds,
    double InitialSubBodyLongitudeRadians,
    double ObserverLongitudeRadians)
{
    /// <summary>Checks identities and physical input domains.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(WorldId) &&
        !string.IsNullOrWhiteSpace(EpochId) &&
        IsPositiveFinite(PerturberMassKg) &&
        IsPositiveFinite(OrbitalDistanceMeters) &&
        IsPositiveFinite(WorldRadiusMeters) &&
        IsPositiveFinite(GravitationalConstantM3PerKgSecondSquared) &&
        IsPositiveFinite(OrbitalPeriodSeconds) &&
        IsPositiveFinite(WorldRotationPeriodSeconds) &&
        double.IsFinite(InitialSubBodyLongitudeRadians) &&
        double.IsFinite(ObserverLongitudeRadians);

    /// <summary>Validates this input and returns it.</summary>
    public OrbitalTideForcingInput EnsureValid()
    {
        if (!IsValid)
        {
            throw new ArgumentException("Orbital forcing requires IDs, positive finite physical magnitudes, and finite longitudes.");
        }

        if (OrbitalDistanceMeters <= WorldRadiusMeters)
        {
            throw new ArgumentException("The perturbing body's orbital distance must exceed the world's radius.");
        }

        return this;
    }

    private static bool IsPositiveFinite(double value) => double.IsFinite(value) && value > 0d;
}

/// <summary>Controls the canonical CPU sampling resolution and turning-point classification.</summary>
public sealed record TidalTableBakeOptions(
    double SampleIntervalSeconds,
    int SampleCount,
    double TurningPointDerivativeTolerance = 1e-12)
{
    /// <summary>Checks that the sample grid and classification tolerance are valid.</summary>
    public bool IsValid =>
        double.IsFinite(SampleIntervalSeconds) &&
        SampleIntervalSeconds > 0d &&
        SampleCount >= 2 &&
        double.IsFinite(TurningPointDerivativeTolerance) &&
        TurningPointDerivativeTolerance >= 0d;

    /// <summary>Validates this options record and returns it.</summary>
    public TidalTableBakeOptions EnsureValid()
    {
        if (!IsValid)
        {
            throw new ArgumentException("Sample interval must be positive and finite, count must be at least two, and tolerance must be finite and non-negative.");
        }

        if (!double.IsFinite(SampleIntervalSeconds * (SampleCount - 1d)))
        {
            throw new ArgumentException("The requested table horizon exceeds the finite double range.");
        }

        return this;
    }
}

/// <summary>A single canonical sample of the simplified tidal-potential field.</summary>
/// <param name="ElapsedSeconds">Seconds since the declared epoch.</param>
/// <param name="PotentialM2PerS2">Differential tidal potential in square metres per square second.</param>
/// <param name="RateOfChangeM2PerS3">Time derivative of differential tidal potential.</param>
/// <param name="State">Increasing, decreasing, or near a turning point.</param>
public readonly record struct TidalTableSample(
    double ElapsedSeconds,
    double PotentialM2PerS2,
    double RateOfChangeM2PerS3,
    TidalForcingState State);

/// <summary>An immutable uniformly sampled tidal-potential table baked from explicit orbital inputs.</summary>
public sealed class TidalPotentialTable
{
    private readonly TidalTableSample[] _samples;

    internal TidalPotentialTable(
        OrbitalTideForcingInput input,
        TidalTableBakeOptions options,
        TidalTableSample[] samples)
    {
        Input = input;
        Options = options;
        _samples = (TidalTableSample[])samples.Clone();
    }

    /// <summary>Gets the inputs that define this table's model and reference frame.</summary>
    public OrbitalTideForcingInput Input { get; }

    /// <summary>Gets the sampling and classification settings.</summary>
    public TidalTableBakeOptions Options { get; }

    /// <summary>Gets the number of canonical samples.</summary>
    public int Count => _samples.Length;

    /// <summary>Gets the inclusive end of the sampled horizon in seconds after the epoch.</summary>
    public double EndElapsedSeconds => _samples[^1].ElapsedSeconds;

    /// <summary>Gets a copy of the canonical samples, preventing callers from mutating table storage.</summary>
    public TidalTableSample[] CopySamples() => (TidalTableSample[])_samples.Clone();

    /// <summary>Returns a sample by index.</summary>
    public TidalTableSample GetSample(int index) => _samples[index];

    /// <summary>
    /// Samples the table with linear interpolation. Times outside the baked closed horizon are rejected;
    /// no extrapolation or hidden wraparound is performed.
    /// </summary>
    public TidalTableSample SampleAt(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0d || elapsedSeconds > EndElapsedSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Sample time must lie inside the table's closed horizon.");
        }

        var exactIndex = elapsedSeconds / Options.SampleIntervalSeconds;
        var lowerIndex = (int)Math.Floor(exactIndex);
        if (lowerIndex >= _samples.Length - 1 || exactIndex == lowerIndex)
        {
            return _samples[lowerIndex];
        }

        var upperIndex = lowerIndex + 1;
        var lower = _samples[lowerIndex];
        var upper = _samples[upperIndex];
        var fraction = exactIndex - lowerIndex;
        var potential = Lerp(lower.PotentialM2PerS2, upper.PotentialM2PerS2, fraction);
        var rate = Lerp(lower.RateOfChangeM2PerS3, upper.RateOfChangeM2PerS3, fraction);
        return new TidalTableSample(
            elapsedSeconds,
            potential,
            rate,
            OrbitalTidePotentialBaker.Classify(rate, Options.TurningPointDerivativeTolerance));
    }

    private static double Lerp(double a, double b, double fraction) => a + (b - a) * fraction;
}

/// <summary>Bakes a deterministic CPU reference table for a simplified orbital tidal-potential signal.</summary>
public static class OrbitalTidePotentialBaker
{
    /// <summary>
    /// Builds a uniformly sampled table. The analytic quadrupole potential is
    /// U = G M R² / r³ × (3 cos²(ψ) − 1) / 2, where ψ is the angular separation
    /// between the equatorial observer and the sub-body point.
    /// </summary>
    public static TidalPotentialTable Bake(OrbitalTideForcingInput input, TidalTableBakeOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(options);
        input.EnsureValid();
        options.EnsureValid();

        var samples = new TidalTableSample[options.SampleCount];
        var amplitude = input.GravitationalConstantM3PerKgSecondSquared *
                        input.PerturberMassKg *
                        input.WorldRadiusMeters *
                        input.WorldRadiusMeters /
                        (input.OrbitalDistanceMeters * input.OrbitalDistanceMeters * input.OrbitalDistanceMeters);
        if (!double.IsFinite(amplitude))
        {
            throw new ArgumentException("The derived potential amplitude is outside the finite double range.", nameof(input));
        }

        var relativeAngularRate = (2d * Math.PI / input.OrbitalPeriodSeconds) -
                                  (2d * Math.PI / input.WorldRotationPeriodSeconds);
        for (var i = 0; i < samples.Length; i++)
        {
            var elapsed = i * options.SampleIntervalSeconds;
            var separation = input.InitialSubBodyLongitudeRadians +
                             relativeAngularRate * elapsed -
                             input.ObserverLongitudeRadians;
            var cosine = Math.Cos(separation);
            var sine = Math.Sin(separation);
            var potential = amplitude * (1.5d * cosine * cosine - 0.5d);
            var derivative = amplitude * (-3d * cosine * sine * relativeAngularRate);

            if (!double.IsFinite(elapsed) || !double.IsFinite(potential) || !double.IsFinite(derivative))
            {
                throw new ArgumentException($"Non-finite tidal sample at index {i}; reduce the horizon or revise the inputs.", nameof(options));
            }

            samples[i] = new TidalTableSample(
                elapsed,
                potential,
                derivative,
                Classify(derivative, options.TurningPointDerivativeTolerance));
        }

        return new TidalPotentialTable(input, options, samples);
    }

    internal static TidalForcingState Classify(double derivative, double tolerance)
    {
        if (Math.Abs(derivative) <= tolerance)
        {
            return TidalForcingState.TurningPoint;
        }

        return derivative > 0d ? TidalForcingState.Rising : TidalForcingState.Falling;
    }
}
