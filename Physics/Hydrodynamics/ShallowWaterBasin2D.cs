namespace TheSingularityWorkshop.Physics.Hydrodynamics;

/// <summary>
/// A deterministic, depth-averaged two-dimensional shallow-water model on a flat,
/// closed rectangular basin. Depth and depth-integrated momentum are cell-centered.
/// </summary>
/// <remarks>
/// Uses conservative Rusanov interface fluxes and impermeable walls. This first
/// version assumes positive water depth in every cell and a flat bed; terrain,
/// wet/dry shorelines, friction, Coriolis forces, and moving bodies are not modeled.
/// All lengths are metres, time is seconds, depth is metres, and velocity is metres per second.
/// </remarks>
public sealed class ShallowWaterBasin2D
{
    private readonly double _gravity;
    private readonly double _cellWidth;
    private readonly double _cellHeight;
    private readonly double _timeStep;
    private double[] _depth;
    private double[] _momentumX;
    private double[] _momentumY;

    /// <summary>Creates a flat-bed basin at rest with the supplied positive depth field.</summary>
    /// <param name="width">Number of cells along the X axis.</param>
    /// <param name="height">Number of cells along the Y axis.</param>
    /// <param name="cellWidth">Cell width in metres.</param>
    /// <param name="cellHeight">Cell height in metres.</param>
    /// <param name="timeStep">Fixed time step in seconds.</param>
    /// <param name="initialDepth">Positive water depth in metres, row-major, with width × height entries.</param>
    /// <param name="gravity">Gravitational acceleration in metres per second squared.</param>
    public ShallowWaterBasin2D(
        int width,
        int height,
        double cellWidth,
        double cellHeight,
        double timeStep,
        IReadOnlyList<double> initialDepth,
        double gravity = 9.80665d)
    {
        if (width < 2)
            throw new ArgumentOutOfRangeException(nameof(width), "At least two cells are required along X.");
        if (height < 2)
            throw new ArgumentOutOfRangeException(nameof(height), "At least two cells are required along Y.");
        if (!double.IsFinite(cellWidth) || cellWidth <= 0d)
            throw new ArgumentOutOfRangeException(nameof(cellWidth), "Cell width must be finite and positive.");
        if (!double.IsFinite(cellHeight) || cellHeight <= 0d)
            throw new ArgumentOutOfRangeException(nameof(cellHeight), "Cell height must be finite and positive.");
        if (!double.IsFinite(timeStep) || timeStep <= 0d)
            throw new ArgumentOutOfRangeException(nameof(timeStep), "Time step must be finite and positive.");
        if (!double.IsFinite(gravity) || gravity <= 0d)
            throw new ArgumentOutOfRangeException(nameof(gravity), "Gravity must be finite and positive.");
        ArgumentNullException.ThrowIfNull(initialDepth);
        if (initialDepth.Count != checked(width * height))
            throw new ArgumentException("Depth count must equal width × height.", nameof(initialDepth));

        Width = width;
        Height = height;
        _cellWidth = cellWidth;
        _cellHeight = cellHeight;
        _timeStep = timeStep;
        _gravity = gravity;
        _depth = new double[initialDepth.Count];
        _momentumX = new double[initialDepth.Count];
        _momentumY = new double[initialDepth.Count];

        for (var i = 0; i < initialDepth.Count; i++)
        {
            if (!double.IsFinite(initialDepth[i]) || initialDepth[i] <= 0d)
                throw new ArgumentException("Every initial depth must be finite and positive.", nameof(initialDepth));
            _depth[i] = initialDepth[i];
        }

        EnsureStableTimeStep(_depth, _momentumX, _momentumY);
    }

    /// <summary>Gets the number of cells along X.</summary>
    public int Width { get; }
    /// <summary>Gets the number of cells along Y.</summary>
    public int Height { get; }
    /// <summary>Gets the fixed simulation time step in seconds.</summary>
    public double TimeStep => _timeStep;
    /// <summary>Gets the number of successfully completed steps.</summary>
    public long StepCount { get; private set; }
    /// <summary>Gets elapsed simulation time in seconds.</summary>
    public double SimulationTime => StepCount * _timeStep;
    /// <summary>Gets the sum of depth × cell area, in cubic metres, for this flat-bed model.</summary>
    public double WaterVolume => _depth.Sum(value => value) * _cellWidth * _cellHeight;

    /// <summary>Returns a defensive copy of the cell depth field in row-major order.</summary>
    public double[] SnapshotDepth() => (double[])_depth.Clone();

    /// <summary>Returns the depth at the specified cell in metres.</summary>
    public double SampleDepth(int x, int y) => _depth[GetIndex(x, y)];

    /// <summary>Returns the depth-averaged horizontal velocity at the specified cell in metres per second.</summary>
    public double SampleVelocityX(int x, int y)
    {
        var i = GetIndex(x, y);
        return _momentumX[i] / _depth[i];
    }

    /// <summary>Returns the depth-averaged vertical velocity at the specified cell in metres per second.</summary>
    public double SampleVelocityY(int x, int y)
    {
        var i = GetIndex(x, y);
        return _momentumY[i] / _depth[i];
    }

    /// <summary>Advance one conservative finite-volume step with closed-wall boundaries.</summary>
    /// <exception cref="InvalidOperationException">The configured time step violates the current CFL bound.</exception>
    /// <exception cref="ArithmeticException">The update would produce non-finite state or non-positive depth.</exception>
    public void Advance()
    {
        EnsureStableTimeStep(_depth, _momentumX, _momentumY);
        var nextStepCount = checked(StepCount + 1);
        var nextDepth = new double[_depth.Length];
        var nextMomentumX = new double[_depth.Length];
        var nextMomentumY = new double[_depth.Length];

        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var i = y * Width + x;
            var left = FluxX(x, y);
            var right = FluxX(x + 1, y);
            var bottom = FluxY(y, x);
            var top = FluxY(y + 1, x);

            nextDepth[i] = _depth[i]
                - _timeStep / _cellWidth * (right.Depth - left.Depth)
                - _timeStep / _cellHeight * (top.Depth - bottom.Depth);
            nextMomentumX[i] = _momentumX[i]
                - _timeStep / _cellWidth * (right.MomentumX - left.MomentumX)
                - _timeStep / _cellHeight * (top.MomentumX - bottom.MomentumX);
            nextMomentumY[i] = _momentumY[i]
                - _timeStep / _cellWidth * (right.MomentumY - left.MomentumY)
                - _timeStep / _cellHeight * (top.MomentumY - bottom.MomentumY);

            if (!double.IsFinite(nextDepth[i]) || nextDepth[i] <= 0d
                || !double.IsFinite(nextMomentumX[i]) || !double.IsFinite(nextMomentumY[i]))
                throw new ArithmeticException("Shallow-water update produced invalid state; no state was committed.");
        }

        _depth = nextDepth;
        _momentumX = nextMomentumX;
        _momentumY = nextMomentumY;
        StepCount = nextStepCount;
    }

    private int GetIndex(int x, int y)
    {
        if ((uint)x >= (uint)Width)
            throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(y));
        return y * Width + x;
    }

    private void EnsureStableTimeStep(double[] depth, double[] momentumX, double[] momentumY)
    {
        var maximumRate = 0d;
        for (var i = 0; i < depth.Length; i++)
        {
            var h = depth[i];
            var u = momentumX[i] / h;
            var v = momentumY[i] / h;
            var waveSpeed = Math.Sqrt(_gravity * h);
            maximumRate = Math.Max(maximumRate,
                (Math.Abs(u) + waveSpeed) / _cellWidth + (Math.Abs(v) + waveSpeed) / _cellHeight);
        }

        if (!double.IsFinite(maximumRate) || _timeStep * maximumRate > 0.45d)
            throw new InvalidOperationException("Time step violates the 2D shallow-water CFL bound (dt × max rate must be ≤ 0.45).");
    }

    private Flux FluxX(int faceX, int y)
    {
        if (faceX == 0)
        {
            var h = _depth[y * Width];
            return new Flux(0d, 0.5d * _gravity * h * h, 0d);
        }
        if (faceX == Width)
        {
            var h = _depth[y * Width + Width - 1];
            return new Flux(0d, 0.5d * _gravity * h * h, 0d);
        }

        var left = y * Width + faceX - 1;
        var right = left + 1;
        var hL = _depth[left];
        var hR = _depth[right];
        var uL = _momentumX[left] / hL;
        var uR = _momentumX[right] / hR;
        var vL = _momentumY[left] / hL;
        var vR = _momentumY[right] / hR;
        var speed = Math.Max(Math.Abs(uL) + Math.Sqrt(_gravity * hL),
                             Math.Abs(uR) + Math.Sqrt(_gravity * hR));
        return new Flux(
            0.5d * (_momentumX[left] + _momentumX[right]) - 0.5d * speed * (hR - hL),
            0.5d * (_momentumX[left] * uL + 0.5d * _gravity * hL * hL
                  + _momentumX[right] * uR + 0.5d * _gravity * hR * hR)
                - 0.5d * speed * (_momentumX[right] - _momentumX[left]),
            0.5d * (_momentumX[left] * vL + _momentumX[right] * vR)
                - 0.5d * speed * (_momentumY[right] - _momentumY[left]));
    }

    private Flux FluxY(int faceY, int x)
    {
        if (faceY == 0)
        {
            var h = _depth[x];
            return new Flux(0d, 0d, 0.5d * _gravity * h * h);
        }
        if (faceY == Height)
        {
            var h = _depth[(Height - 1) * Width + x];
            return new Flux(0d, 0d, 0.5d * _gravity * h * h);
        }

        var lower = (faceY - 1) * Width + x;
        var upper = lower + Width;
        var hL = _depth[lower];
        var hR = _depth[upper];
        var uL = _momentumX[lower] / hL;
        var uR = _momentumX[upper] / hR;
        var vL = _momentumY[lower] / hL;
        var vR = _momentumY[upper] / hR;
        var speed = Math.Max(Math.Abs(vL) + Math.Sqrt(_gravity * hL),
                             Math.Abs(vR) + Math.Sqrt(_gravity * hR));
        return new Flux(
            0.5d * (_momentumY[lower] + _momentumY[upper]) - 0.5d * speed * (hR - hL),
            0.5d * (_momentumY[lower] * uL + _momentumY[upper] * uR)
                - 0.5d * speed * (_momentumX[upper] - _momentumX[lower]),
            0.5d * (_momentumY[lower] * vL + 0.5d * _gravity * hL * hL
                  + _momentumY[upper] * vR + 0.5d * _gravity * hR * hR)
                - 0.5d * speed * (_momentumY[upper] - _momentumY[lower]));
    }

    private readonly record struct Flux(double Depth, double MomentumX, double MomentumY);
}
