namespace TheSingularityWorkshop.Physics.Hydrodynamics;

/// <summary>
/// A deterministic, depth-averaged shallow-water model over fixed bed elevation.
/// Uses hydrostatic reconstruction so a lake at rest remains at rest over uneven terrain,
/// including dry cells above the still-water free surface.
/// </summary>
/// <remarks>
/// Depth may be zero to represent a dry cell. Momentum in a dry cell is zero;
/// wet/dry positivity and robust hydrostatic reconstruction are handled conservatively.
/// Moving terrain, friction, and sediment exchange are not modeled. Arrays are row-major.
/// </remarks>
public sealed class BathymetricShallowWaterBasin2D
{
    private readonly double _gravity;
    private readonly double _dx;
    private readonly double _dy;
    private readonly double _dt;
    private readonly double[] _bed;
    private double[] _depth;
    private double[] _qx;
    private double[] _qy;

    /// <summary>Creates a basin from cell depths (non-negative) and fixed cell-centered bed elevations.</summary>
    public BathymetricShallowWaterBasin2D(
        int width, int height, double cellWidth, double cellHeight, double timeStep,
        IReadOnlyList<double> initialDepth, IReadOnlyList<double> bedElevation,
        double gravity = 9.80665d)
    {
        if (width < 2) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 2) throw new ArgumentOutOfRangeException(nameof(height));
        if (!double.IsFinite(cellWidth) || cellWidth <= 0d) throw new ArgumentOutOfRangeException(nameof(cellWidth));
        if (!double.IsFinite(cellHeight) || cellHeight <= 0d) throw new ArgumentOutOfRangeException(nameof(cellHeight));
        if (!double.IsFinite(timeStep) || timeStep <= 0d) throw new ArgumentOutOfRangeException(nameof(timeStep));
        if (!double.IsFinite(gravity) || gravity <= 0d) throw new ArgumentOutOfRangeException(nameof(gravity));
        ArgumentNullException.ThrowIfNull(initialDepth);
        ArgumentNullException.ThrowIfNull(bedElevation);
        var count = checked(width * height);
        if (initialDepth.Count != count) throw new ArgumentException("Depth count must equal width × height.", nameof(initialDepth));
        if (bedElevation.Count != count) throw new ArgumentException("Bed elevation count must equal width × height.", nameof(bedElevation));

        Width = width;
        Height = height;
        _dx = cellWidth;
        _dy = cellHeight;
        _dt = timeStep;
        _gravity = gravity;
        _depth = new double[count];
        _qx = new double[count];
        _qy = new double[count];
        _bed = new double[count];
        for (var i = 0; i < count; i++)
        {
            if (!double.IsFinite(initialDepth[i]) || initialDepth[i] < 0d)
                throw new ArgumentException("Every initial depth must be finite and non-negative.", nameof(initialDepth));
            if (!double.IsFinite(bedElevation[i]))
                throw new ArgumentException("Every bed elevation must be finite.", nameof(bedElevation));
            _depth[i] = initialDepth[i];
            _bed[i] = bedElevation[i];
        }
        EnsureStableTimeStep();
    }

    /// <summary>Number of cells along X.</summary>
    public int Width { get; }
    /// <summary>Number of cells along Y.</summary>
    public int Height { get; }
    /// <summary>Fixed time step in seconds.</summary>
    public double TimeStep => _dt;
    /// <summary>Number of successfully committed steps.</summary>
    public long StepCount { get; private set; }
    /// <summary>Elapsed simulation time in seconds.</summary>
    public double SimulationTime => StepCount * _dt;
    /// <summary>Water volume in cubic metres.</summary>
    public double WaterVolume => _depth.Sum(h => h) * _dx * _dy;
    /// <summary>Returns a defensive copy of bed elevation in metres.</summary>
    public double[] SnapshotBedElevation() => (double[])_bed.Clone();
    /// <summary>Returns a defensive copy of water depth in metres.</summary>
    public double[] SnapshotDepth() => (double[])_depth.Clone();
    /// <summary>Samples water depth in metres; zero indicates a dry cell.</summary>
    public double SampleDepth(int x, int y) => _depth[Index(x, y)];
    /// <summary>Samples fixed bed elevation in metres.</summary>
    public double SampleBedElevation(int x, int y) => _bed[Index(x, y)];
    /// <summary>Samples free-surface elevation (bed plus depth), in metres.</summary>
    public double SampleFreeSurface(int x, int y)
    {
        var i = Index(x, y);
        return _bed[i] + _depth[i];
    }
    /// <summary>Samples depth-averaged X velocity in metres per second; returns zero in a dry cell.</summary>
    public double SampleVelocityX(int x, int y) { var i = Index(x, y); return Velocity(_qx[i], _depth[i]); }
    /// <summary>Samples depth-averaged Y velocity in metres per second; returns zero in a dry cell.</summary>
    public double SampleVelocityY(int x, int y) { var i = Index(x, y); return Velocity(_qy[i], _depth[i]); }

    /// <summary>Advances one conservative finite-volume step with impermeable walls.</summary>
    public void Advance()
    {
        EnsureStableTimeStep();
        var nextStep = checked(StepCount + 1);
        var nh = new double[_depth.Length];
        var nx = new double[_depth.Length];
        var ny = new double[_depth.Length];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var i = y * Width + x;
            var left = FluxX(x, y).Right;
            var right = FluxX(x + 1, y).Left;
            var bottom = FluxY(y, x).Right;
            var top = FluxY(y + 1, x).Left;
            nh[i] = _depth[i] - _dt / _dx * (right.H - left.H) - _dt / _dy * (top.H - bottom.H);
            nx[i] = _qx[i] - _dt / _dx * (right.Qx - left.Qx) - _dt / _dy * (top.Qx - bottom.Qx);
            ny[i] = _qy[i] - _dt / _dx * (right.Qy - left.Qy) - _dt / _dy * (top.Qy - bottom.Qy);

            if (!double.IsFinite(nh[i]) || nh[i] < 0d || !double.IsFinite(nx[i]) || !double.IsFinite(ny[i]))
                throw new ArithmeticException("Bathymetric shallow-water update produced invalid state; no state was committed.");
            if (nh[i] == 0d)
            {
                // A dry cell has no meaningful depth-averaged velocity or stored momentum.
                nx[i] = 0d;
                ny[i] = 0d;
            }
        }
        _depth = nh;
        _qx = nx;
        _qy = ny;
        StepCount = nextStep;
    }

    private int Index(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        return y * Width + x;
    }

    private static double Velocity(double momentum, double depth) => depth > 0d ? momentum / depth : 0d;

    private void EnsureStableTimeStep()
    {
        var maxRate = 0d;
        for (var i = 0; i < _depth.Length; i++)
        {
            var h = _depth[i];
            var u = Velocity(_qx[i], h);
            var v = Velocity(_qy[i], h);
            var c = Math.Sqrt(_gravity * h);
            maxRate = Math.Max(maxRate, (Math.Abs(u) + c) / _dx + (Math.Abs(v) + c) / _dy);
        }
        if (!double.IsFinite(maxRate) || _dt * maxRate > 0.45d)
            throw new InvalidOperationException("Time step violates the 2D shallow-water CFL bound (dt × max rate must be ≤ 0.45).");
    }

    private FacePair FluxX(int faceX, int y)
    {
        if (faceX == 0)
        {
            var i = y * Width;
            var f = new Flux(0d, 0.5d * _gravity * _depth[i] * _depth[i], 0d);
            return new FacePair(f, f);
        }
        if (faceX == Width)
        {
            var i = y * Width + Width - 1;
            var f = new Flux(0d, 0.5d * _gravity * _depth[i] * _depth[i], 0d);
            return new FacePair(f, f);
        }
        var l = y * Width + faceX - 1;
        return HydrostaticFace(l, l + 1, true);
    }

    private FacePair FluxY(int faceY, int x)
    {
        if (faceY == 0)
        {
            var i = x;
            var f = new Flux(0d, 0d, 0.5d * _gravity * _depth[i] * _depth[i]);
            return new FacePair(f, f);
        }
        if (faceY == Height)
        {
            var i = (Height - 1) * Width + x;
            var f = new Flux(0d, 0d, 0.5d * _gravity * _depth[i] * _depth[i]);
            return new FacePair(f, f);
        }
        var l = (faceY - 1) * Width + x;
        return HydrostaticFace(l, l + Width, false);
    }

    private FacePair HydrostaticFace(int left, int right, bool xNormal)
    {
        var hL = _depth[left];
        var hR = _depth[right];
        var bStar = Math.Max(_bed[left], _bed[right]);
        var hsL = Math.Max(0d, hL + _bed[left] - bStar);
        var hsR = Math.Max(0d, hR + _bed[right] - bStar);
        var uL = Velocity(_qx[left], hL);
        var uR = Velocity(_qx[right], hR);
        var vL = Velocity(_qy[left], hL);
        var vR = Velocity(_qy[right], hR);
        var qxL = hsL * uL;
        var qxR = hsR * uR;
        var qyL = hsL * vL;
        var qyR = hsR * vR;
        var normalL = xNormal ? uL : vL;
        var normalR = xNormal ? uR : vR;
        var speed = Math.Max(Math.Abs(normalL) + Math.Sqrt(_gravity * hsL),
                             Math.Abs(normalR) + Math.Sqrt(_gravity * hsR));
        var fH = 0.5d * ((xNormal ? qxL : qyL) + (xNormal ? qxR : qyR)) - 0.5d * speed * (hsR - hsL);
        var fQx = xNormal
            ? 0.5d * (qxL * uL + 0.5d * _gravity * hsL * hsL + qxR * uR + 0.5d * _gravity * hsR * hsR) - 0.5d * speed * (qxR - qxL)
            : 0.5d * (qyL * uL + qyR * uR) - 0.5d * speed * (qxR - qxL);
        var fQy = xNormal
            ? 0.5d * (qxL * vL + qxR * vR) - 0.5d * speed * (qyR - qyL)
            : 0.5d * (qyL * vL + 0.5d * _gravity * hsL * hsL + qyR * vR + 0.5d * _gravity * hsR * hsR) - 0.5d * speed * (qyR - qyL);
        var leftFlux = xNormal
            ? new Flux(fH, fQx + 0.5d * _gravity * (hL * hL - hsL * hsL), fQy)
            : new Flux(fH, fQx, fQy + 0.5d * _gravity * (hL * hL - hsL * hsL));
        var rightFlux = xNormal
            ? new Flux(fH, fQx + 0.5d * _gravity * (hR * hR - hsR * hsR), fQy)
            : new Flux(fH, fQx, fQy + 0.5d * _gravity * (hR * hR - hsR * hsR));
        return new FacePair(leftFlux, rightFlux);
    }

    private readonly record struct Flux(double H, double Qx, double Qy);
    private readonly record struct FacePair(Flux Left, Flux Right);
}
