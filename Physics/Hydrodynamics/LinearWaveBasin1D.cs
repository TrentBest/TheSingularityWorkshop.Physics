namespace TheSingularityWorkshop.Physics.Hydrodynamics;

/// <summary>
/// A one-dimensional linear wave-equation reference model on a closed basin.
/// Surface elevation is measured in metres; distance in metres; time in seconds.
/// This is a verification reference, not a nonlinear shallow-water solver.
/// </summary>
public sealed class LinearWaveBasin1D
{
    private double[] _elevation;
    private double[] _previousElevation;
    private double[] _nextElevation;

    /// <summary>Creates a closed one-dimensional basin initialized from a surface-elevation profile at rest.</summary>
    /// <param name="cellCount">Number of uniformly spaced cells; must be at least three.</param>
    /// <param name="cellWidth">Cell width in metres.</param>
    /// <param name="waveSpeed">Linear wave speed in metres per second.</param>
    /// <param name="timeStep">Fixed simulation time step in seconds.</param>
    /// <param name="initialElevation">Initial free-surface elevation in metres, one value per cell.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension or physical parameter is invalid, or the Courant number exceeds one.</exception>
    /// <exception cref="ArgumentException">The initial profile has the wrong length or contains non-finite values.</exception>
    public LinearWaveBasin1D(
        int cellCount,
        double cellWidth,
        double waveSpeed,
        double timeStep,
        IReadOnlyList<double> initialElevation)
    {
        if (cellCount < 3)
            throw new ArgumentOutOfRangeException(nameof(cellCount), "At least three cells are required.");
        if (!double.IsFinite(cellWidth) || cellWidth <= 0d)
            throw new ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be finite and positive.");
        if (!double.IsFinite(waveSpeed) || waveSpeed <= 0d)
            throw new ArgumentOutOfRangeException(nameof(waveSpeed), "Wave speed must be finite and positive.");
        if (!double.IsFinite(timeStep) || timeStep <= 0d)
            throw new ArgumentOutOfRangeException(nameof(timeStep), "Time step must be finite and positive.");
        ArgumentNullException.ThrowIfNull(initialElevation);
        if (initialElevation.Count != cellCount)
            throw new ArgumentException("Initial elevation count must equal cellCount.", nameof(initialElevation));

        var courantNumber = waveSpeed * timeStep / cellWidth;
        if (!double.IsFinite(courantNumber) || courantNumber > 1d)
            throw new ArgumentOutOfRangeException(nameof(timeStep),
                "The explicit wave update requires waveSpeed * timeStep / cellWidth <= 1.");

        CellWidth = cellWidth;
        WaveSpeed = waveSpeed;
        TimeStep = timeStep;
        CourantNumber = courantNumber;
        _elevation = new double[cellCount];
        _previousElevation = new double[cellCount];
        _nextElevation = new double[cellCount];

        for (var i = 0; i < cellCount; i++)
        {
            var value = initialElevation[i];
            if (!double.IsFinite(value))
                throw new ArgumentException("Initial elevations must be finite.", nameof(initialElevation));
            _elevation[i] = value;
        }

        // Start from rest: the Taylor expansion supplies the first previous-time level.
        var ratioSquared = courantNumber * courantNumber;
        for (var i = 0; i < cellCount; i++)
            _previousElevation[i] = _elevation[i] + 0.5d * ratioSquared * Laplacian(_elevation, i);
    }

    /// <summary>Gets the number of cells in the basin.</summary>
    public int CellCount => _elevation.Length;
    /// <summary>Gets the cell width in metres.</summary>
    public double CellWidth { get; }
    /// <summary>Gets the modeled linear wave speed in metres per second.</summary>
    public double WaveSpeed { get; }
    /// <summary>Gets the fixed time step in seconds.</summary>
    public double TimeStep { get; }
    /// <summary>Gets the Courant number, wave speed multiplied by time step and divided by cell width.</summary>
    public double CourantNumber { get; }
    /// <summary>Gets the number of successfully completed simulation steps.</summary>
    public long StepCount { get; private set; }
    /// <summary>Gets elapsed simulation time in seconds.</summary>
    public double SimulationTime => StepCount * TimeStep;

    /// <summary>Returns a defensive copy of the current free-surface elevation profile.</summary>
    public double[] SnapshotElevation() => (double[])_elevation.Clone();

    /// <summary>Returns the current free-surface elevation at a cell, in metres.</summary>
    /// <param name="cellIndex">Zero-based cell index.</param>
    /// <returns>Current elevation in metres.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the basin.</exception>
    public double SampleElevation(int cellIndex)
    {
        if ((uint)cellIndex >= (uint)_elevation.Length)
            throw new ArgumentOutOfRangeException(nameof(cellIndex));
        return _elevation[cellIndex];
    }

    /// <summary>Advance one fixed step using a centered explicit wave-equation update.</summary>
    public void Advance()
    {
        var ratioSquared = CourantNumber * CourantNumber;
        for (var i = 0; i < _elevation.Length; i++)
        {
            _nextElevation[i] = 2d * _elevation[i] - _previousElevation[i]
                + ratioSquared * Laplacian(_elevation, i);
            if (!double.IsFinite(_nextElevation[i]))
                throw new OverflowException("Wave update produced a non-finite elevation.");
        }

        var reusable = _previousElevation;
        _previousElevation = _elevation;
        _elevation = _nextElevation;
        _nextElevation = reusable;
        StepCount = checked(StepCount + 1);
    }

    private double Laplacian(double[] values, int index)
    {
        // Reflecting wall: mirror ghost cells impose zero normal derivative.
        var left = index == 0 ? values[1] : values[index - 1];
        var right = index == values.Length - 1 ? values[^2] : values[index + 1];
        return left - 2d * values[index] + right;
    }
}
