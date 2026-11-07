using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TheSingularityWorkshop.Physics.Astronomy;
using TheSingularityWorkshop.Physics.Collision;
using TheSingularityWorkshop.Physics.Numerics;
using TheSingularityWorkshop.Physics.Spatial;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[MemoryDiagnoser]
[BenchmarkCategory("PHY-VECTOR3D-BATCH")]
public class Vector3dBatchBenchmarks
{
    private double[] _leftX = null!, _leftY = null!, _leftZ = null!;
    private double[] _rightX = null!, _rightY = null!, _rightZ = null!;
    private double[] _outputX = null!, _outputY = null!, _outputZ = null!;

    [Params(128, 4_096, 65_536)]
    public int VectorCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _leftX = new double[VectorCount]; _leftY = new double[VectorCount]; _leftZ = new double[VectorCount];
        _rightX = new double[VectorCount]; _rightY = new double[VectorCount]; _rightZ = new double[VectorCount];
        _outputX = new double[VectorCount]; _outputY = new double[VectorCount]; _outputZ = new double[VectorCount];
        for (var i = 0; i < VectorCount; i++)
        {
            _leftX[i] = i * 0.25d; _leftY[i] = i * -0.125d; _leftZ[i] = i * 0.0625d;
            _rightX[i] = i * 0.5d; _rightY[i] = i * 0.375d; _rightZ[i] = i * -0.25d;
        }
    }

    [Benchmark(Baseline = true)]
    public double ScalarSoAAdd()
    {
        for (var i = 0; i < VectorCount; i++)
        {
            _outputX[i] = _leftX[i] + _rightX[i];
            _outputY[i] = _leftY[i] + _rightY[i];
            _outputZ[i] = _leftZ[i] + _rightZ[i];
        }
        return Checksum();
    }

    [Benchmark]
    public double Vector3dBatchAdd()
    {
        Vector3dBatch.Add(_leftX, _leftY, _leftZ, _rightX, _rightY, _rightZ, _outputX, _outputY, _outputZ);
        return Checksum();
    }

    private double Checksum()
    {
        var checksum = 0d;
        for (var i = 0; i < VectorCount; i++) checksum += _outputX[i] + _outputY[i] + _outputZ[i];
        return checksum;
    }
}

[MemoryDiagnoser]
[BenchmarkCategory("PHY-COLLISION-BROADPHASE")]
public class SweepAndPruneBenchmarks
{
    private BroadphaseProxy[] _proxies = null!;

    [Params(100, 1_000, 10_000)]
    public int ProxyCount { get; set; }

    [Params(0.75d, 1.25d)]
    public double GridSpacing { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        const double halfExtent = 0.5d;
        _proxies = new BroadphaseProxy[ProxyCount];
        for (var i = 0; i < ProxyCount; i++)
        {
            var x = (i % 100) * GridSpacing;
            var y = ((i / 100) % 100) * GridSpacing;
            var z = (i / 10_000) * GridSpacing;
            _proxies[i] = new BroadphaseProxy(i, new AxisAlignedBounds(
                x - halfExtent, y - halfExtent, z - halfExtent,
                x + halfExtent, y + halfExtent, z + halfExtent));
        }
    }

    [Benchmark]
    public int FindCandidates() => SweepAndPruneBroadphase.FindCandidates(_proxies).Count;
}

[MemoryDiagnoser]
[BenchmarkCategory("PHY-ASTRONOMY-NBODY")]
public class NewtonianNBodyStepperBenchmarks
{
    private OrbitalBodyState[] _bodies = null!;

    [Params(2, 8, 32, 128)]
    public int BodyCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        const double orbitalRadius = 10_000_000d;
        _bodies = new OrbitalBodyState[BodyCount];
        for (var i = 0; i < BodyCount; i++)
        {
            var angle = (2d * Math.PI * i) / BodyCount;
            var position = new Vector3d(orbitalRadius * Math.Cos(angle), orbitalRadius * Math.Sin(angle), 0d);
            _bodies[i] = new OrbitalBodyState($"body-{i:D4}", 1.0e20d, 1d, position, Vector3d.Zero);
        }
    }

    [Benchmark]
    public double StepOneFixedTick()
    {
        var result = NewtonianNBodyStepper.Step(_bodies, 1d, 6.67430e-11d);
        return result[^1].PositionMeters.X;
    }
}
