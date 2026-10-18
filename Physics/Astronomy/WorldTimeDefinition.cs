namespace TheSingularityWorkshop.Physics.Astronomy;

/// <summary>
/// Declares the rotation clock for a world without assuming Earth days or years.
/// </summary>
/// <param name="WorldId">Stable identity of the world or simulated body.</param>
/// <param name="EpochId">Stable identity of the epoch at which elapsed time is zero.</param>
/// <param name="RotationPeriodSeconds">Positive sidereal rotation period in SI seconds.</param>
public sealed record WorldTimeDefinition(
    string WorldId,
    string EpochId,
    double RotationPeriodSeconds)
{
    /// <summary>Earth's conventional 24-hour day expressed in SI seconds, for comparison only.</summary>
    public const double EarthReferenceDaySeconds = 86400d;

    /// <summary>Whether the definition contains stable identities and a finite positive rotation period.</summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(WorldId) &&
        !string.IsNullOrWhiteSpace(EpochId) &&
        double.IsFinite(RotationPeriodSeconds) &&
        RotationPeriodSeconds > 0d;

    /// <summary>Validates the definition and returns this instance.</summary>
    public WorldTimeDefinition EnsureValid()
    {
        if (!IsValid)
        {
            throw new ArgumentException("World and epoch IDs are required and the rotation period must be positive and finite.");
        }

        return this;
    }

    /// <summary>Returns the number of local rotation cycles elapsed since the declared epoch.</summary>
    public double GetLocalRotationCycles(double elapsedSeconds)
    {
        ValidateElapsedSeconds(elapsedSeconds);
        EnsureValid();
        return elapsedSeconds / RotationPeriodSeconds;
    }

    /// <summary>Returns the fractional local rotation phase in [0, 1), measured from the epoch.</summary>
    public double GetLocalRotationPhase(double elapsedSeconds)
    {
        var cycles = GetLocalRotationCycles(elapsedSeconds);
        return cycles - Math.Floor(cycles);
    }

    /// <summary>Returns elapsed time in conventional Earth-reference days, not local calendar days.</summary>
    public double GetEarthReferenceDays(double elapsedSeconds)
    {
        ValidateElapsedSeconds(elapsedSeconds);
        return elapsedSeconds / EarthReferenceDaySeconds;
    }

    /// <summary>Returns how many Earth-reference days fit in one local rotation period.</summary>
    public double LocalRotationPeriodInEarthDays()
    {
        EnsureValid();
        return RotationPeriodSeconds / EarthReferenceDaySeconds;
    }

    private static void ValidateElapsedSeconds(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Elapsed seconds must be finite and non-negative.");
        }
    }
}
