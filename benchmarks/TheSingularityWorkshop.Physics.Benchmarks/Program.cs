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


[MemoryDiagnoser]
[BenchmarkCategory("PHY-CPU-WORLD-STEPPER")]
public class DeterministicCpuWorldStepperBenchmarks
{
    private TheSingularityWorkshop.Physics.Dynamics.RigidBodyState[] _states = null!;
    private TheSingularityWorkshop.Physics.World.CpuWorldBody[] _worldBodies = null!;
    private BroadphaseProxy[] _proxies = null!;
    private TheSingularityWorkshop.Physics.Collision.ContactObservation _contact;
    private TheSingularityWorkshop.Physics.Dynamics.RigidBodyState _contactA, _contactB;
    private TheSingularityWorkshop.Physics.World.CpuWorldConfiguration _configuration = null!;

    [Params(2, 8, 32, 128)]
    public int BodyCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _configuration = new TheSingularityWorkshop.Physics.World.CpuWorldConfiguration(
            "benchmark-cpu-world-v1", 1d / 120d);
        _states = new TheSingularityWorkshop.Physics.Dynamics.RigidBodyState[BodyCount];
        _worldBodies = new TheSingularityWorkshop.Physics.World.CpuWorldBody[BodyCount];
        _proxies = new BroadphaseProxy[BodyCount];

        for (var i = 0; i < BodyCount; i++)
        {
            var position = new Vector3d(i * 4d, 0d, 0d);
            var state = new TheSingularityWorkshop.Physics.Dynamics.RigidBodyState(
                $"body-{i:D4}", new Pose3d(position, Quaterniond.Identity),
                Vector3d.Zero, Vector3d.Zero, 0, 0d);
            var definition = new TheSingularityWorkshop.Physics.World.CpuWorldBodyDefinition(
                state.BodyId, 1d, 0.5d, new Vector3d(0d, -9.81d, 0d));
            _states[i] = state;
            _worldBodies[i] = new TheSingularityWorkshop.Physics.World.CpuWorldBody(definition, state);
            _proxies[i] = new BroadphaseProxy(i + 1L, new AxisAlignedBounds(
                position.X - 0.5d, -0.5d, -0.5d,
                position.X + 0.5d, 0.5d, 0.5d));
        }

        _contactA = new TheSingularityWorkshop.Physics.Dynamics.RigidBodyState(
            "contact-a", new Pose3d(new Vector3d(-0.45d, 0d, 0d), Quaterniond.Identity),
            new Vector3d(1d, 0d, 0d), Vector3d.Zero, 1, 1d / 120d);
        _contactB = new TheSingularityWorkshop.Physics.Dynamics.RigidBodyState(
            "contact-b", new Pose3d(new Vector3d(0.45d, 0d, 0d), Quaterniond.Identity),
            new Vector3d(-1d, 0d, 0d), Vector3d.Zero, 1, 1d / 120d);
        _contact = TheSingularityWorkshop.Physics.Collision.SphereSphereContactDetector.Detect(
            "contact-a-b", _contactA, 0.5d, _contactB, 0.5d, 1, 1d / 120d)!.Value;
    }

    [Benchmark]
    public double IntegrationStage()
    {
        var checksum = 0d;
        foreach (var state in _states)
        {
            var advanced = TheSingularityWorkshop.Physics.Dynamics.RigidBodyIntegrator.Advance(
                state, new Vector3d(0d, -9.81d, 0d), 1d / 120d);
            checksum += advanced.Pose.Position.Y + advanced.LinearVelocity.Y;
        }
        return checksum;
    }

    [Benchmark]
    public int BroadphaseStage() =>
        SweepAndPruneBroadphase.FindCandidates(_proxies).Count;

    [Benchmark]
    public double NarrowphaseStage()
    {
        var contact = TheSingularityWorkshop.Physics.Collision.SphereSphereContactDetector.Detect(
            "contact-a-b", _contactA, 0.5d, _contactB, 0.5d, 1, 1d / 120d);
        return contact?.SignedSeparation ?? double.NaN;
    }

    [Benchmark]
    public double SingleContactResponseStage()
    {
        var result = TheSingularityWorkshop.Physics.Collision.ContactImpulseResolver.Resolve(
            _contact, _contactA, 1d, _contactB, 1d);
        return result.BodyA.LinearVelocity.X + result.BodyB.LinearVelocity.X;
    }

    [Benchmark]
    public double FullWorldStep()
    {
        var result = TheSingularityWorkshop.Physics.World.DeterministicCpuWorldStepper.Step(
            _configuration, _worldBodies);
        return result.Bodies[^1].Pose.Position.Y;
    }
}


/// <summary>
/// Measures complete world steps across increasing counts of independent contact pairs.
/// Each pair is isolated from every other pair so the benchmark stays inside the
/// stepper's explicitly supported single-contact-per-body response model.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("PHY-CPU-WORLD-CONTACT-SCALING")]
public class DeterministicCpuWorldContactScalingBenchmarks
{
    private TheSingularityWorkshop.Physics.World.CpuWorldConfiguration _configuration = null!;
    private TheSingularityWorkshop.Physics.World.CpuWorldBody[] _bodies = null!;
    private int _expectedContactCount;

    [Params(1, 4, 16, 64)]
    public int ContactPairCount { get; set; }

    public int BodyCount => ContactPairCount * 2;

    [GlobalSetup]
    public void Setup()
    {
        const double deltaTime = 1d / 120d;
        _configuration = new TheSingularityWorkshop.Physics.World.CpuWorldConfiguration(
            "benchmark-independent-contact-pairs-v1", deltaTime);
        _expectedContactCount = ContactPairCount;
        _bodies = new TheSingularityWorkshop.Physics.World.CpuWorldBody[BodyCount];

        for (var pair = 0; pair < ContactPairCount; pair++)
        {
            var centre = pair * 4d;
            var leftId = $"body-{pair:D3}-a";
            var rightId = $"body-{pair:D3}-b";
            var leftState = new TheSingularityWorkshop.Physics.Dynamics.RigidBodyState(
                leftId, new Pose3d(new Vector3d(centre - 0.45d, 0d, 0d), Quaterniond.Identity),
                new Vector3d(1d, 0d, 0d), Vector3d.Zero, 0, 0d);
            var rightState = new TheSingularityWorkshop.Physics.Dynamics.RigidBodyState(
                rightId, new Pose3d(new Vector3d(centre + 0.45d, 0d, 0d), Quaterniond.Identity),
                new Vector3d(-1d, 0d, 0d), Vector3d.Zero, 0, 0d);
            var leftDefinition = new TheSingularityWorkshop.Physics.World.CpuWorldBodyDefinition(
                leftId, 1d, 0.5d, Vector3d.Zero);
            var rightDefinition = new TheSingularityWorkshop.Physics.World.CpuWorldBodyDefinition(
                rightId, 1d, 0.5d, Vector3d.Zero);
            _bodies[pair * 2] = new TheSingularityWorkshop.Physics.World.CpuWorldBody(leftDefinition, leftState);
            _bodies[pair * 2 + 1] = new TheSingularityWorkshop.Physics.World.CpuWorldBody(rightDefinition, rightState);
        }

        // Correctness gate for the measured fixture: the benchmark must exercise
        // exactly the declared number of isolated contacts and conserve momentum.
        var reference = TheSingularityWorkshop.Physics.World.DeterministicCpuWorldStepper.Step(
            _configuration, _bodies);
        if (reference.Diagnostics.EndpointContactCount != _expectedContactCount ||
            reference.Diagnostics.ResolvedContactCount != _expectedContactCount ||
            reference.Contacts.Count != _expectedContactCount ||
            reference.Diagnostics.MomentumResidualKilogramMetresPerSecond.Magnitude > 1e-10d)
        {
            throw new InvalidOperationException(
                $"Invalid contact benchmark fixture: expected {_expectedContactCount} resolved contacts; " +
                $"observed {reference.Diagnostics.EndpointContactCount} endpoint and " +
                $"{reference.Diagnostics.ResolvedContactCount} resolved contacts; " +
                $"momentum residual {reference.Diagnostics.MomentumResidualKilogramMetresPerSecond.Magnitude:R} kg m/s.");
        }
    }

    [Benchmark]
    public double FullWorldStepWithIndependentContacts()
    {
        var result = TheSingularityWorkshop.Physics.World.DeterministicCpuWorldStepper.Step(
            _configuration, _bodies);
        return result.Diagnostics.KineticEnergyAfterJoules +
            result.Diagnostics.TotalPositionCorrectionMetres +
            result.Diagnostics.MomentumResidualKilogramMetresPerSecond.Magnitude;
    }
}


/// <summary>
/// Measures the discrete volumetric terrain surface extraction paths. The fixture
/// contains many allocated chunks so localized edits can demonstrate chunk reuse.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("PHY-VOLUMETRIC-TERRAIN-SURFACE")]
public class VolumetricTerrainSurfaceBenchmarks
{
    private SparseMaterialVoxelVolume3d _volume = null!;
    private bool _alternateMaterial;

    [Params(8, 16)]
    public int ChunkCountPerAxis { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        const int chunkSize = 8;
        _volume = new SparseMaterialVoxelVolume3d(Vector3d.Zero, 1d, chunkSize);
        var extent = ChunkCountPerAxis * chunkSize;

        for (var x = 0; x < extent; x++)
        for (var y = 0; y < extent; y++)
        for (var z = 0; z < extent; z++)
        {
            _volume.SetMaterial(new GridCoordinate3i(x, y, z), 1);
        }

        // Prime both caches outside the measured operation.
        _volume.ExtractBoundarySurfaceCached();
        _volume.ExtractBoundarySurfaceIncremental();
    }

    [Benchmark(Baseline = true)]
    public int FullReferenceExtraction() =>
        _volume.ExtractBoundarySurface().TriangleCount;

    [Benchmark]
    public int WholeMeshCachedExtraction() =>
        _volume.ExtractBoundarySurfaceCached().TriangleCount;

    [Benchmark]
    public int IncrementalCachedExtraction() =>
        _volume.ExtractBoundarySurfaceIncremental().TriangleCount;

    [Benchmark]
    public int IncrementalExtractionAfterLocalizedEdit()
    {
        _alternateMaterial = !_alternateMaterial;
        _volume.SetMaterial(
            new GridCoordinate3i(ChunkCountPerAxis * 4, ChunkCountPerAxis * 4, ChunkCountPerAxis * 4),
            _alternateMaterial ? 2 : 1);
        return _volume.ExtractBoundarySurfaceIncremental().TriangleCount;
    }
}
