using System.Buffers.Binary;
using System.Globalization;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.Universe;
using NUnit.Framework;

namespace TheSingularityWorkshop.Physics.Tests;

/// <summary>
/// Exercises the generic replay seam against the repository's real constant-acceleration solver.
/// The event adapter here is a small example contract, not a universal Physics event schema.
/// </summary>
[TestFixture]
public sealed class ConstantAccelerationTimelineReplayTests
{
    private const double DeltaTimePerTick = 1d;
    private const double Acceleration = 2d;

    [Test]
    public void ReconstructRunsActualSolverAndAppliesRecordedVelocityChangeAtItsTick()
    {
        var root = TimelineBranch.CreateRoot("kinematics-root", "kinematics-reality").WithHeadTick(4);
        var lineage = new TimelineLineage([root]);
        var initial = new KinematicState(Position: 0d, Velocity: 1d, ElapsedTime: 0d);
        var checkpoint = new PhysicsCheckpoint(
            "kinematics-checkpoint-0",
            "kinematics-reality",
            "kinematics-root",
            0,
            "kinematics-state-v1",
            "kinematics.constant-acceleration-v1",
            Encode(initial));
        var plan = new TimelineReplayPlan(lineage, checkpoint, 4, "kinematics-state-v1", "kinematics.constant-acceleration-v1");
        var history = new TimelineHistorySegment(
            checkpoint,
            [new TimelineEvent(0, 2, "velocity-delta", "3")],
            4);
        var segment = new TimelineReplaySegment(plan, plan.CopyIntervals()[0], history);

        KinematicState reconstructed = TimelineReplayExecutor.Reconstruct(
            segment,
            targetTick: 4,
            Restore,
            ApplyEvent,
            Advance);

        // Tick 2 event adds 3 m/s after the first two solver steps.
        Assert.That(reconstructed.Position, Is.EqualTo(29d));
        Assert.That(reconstructed.Velocity, Is.EqualTo(12d));
        Assert.That(reconstructed.ElapsedTime, Is.EqualTo(4d));
    }

    private static KinematicState Restore(PhysicsCheckpoint checkpoint)
    {
        byte[] payload = checkpoint.CopyStatePayload();
        if (payload.Length != sizeof(double) * 3)
        {
            throw new InvalidOperationException("The kinematics-state-v1 checkpoint must contain exactly three doubles.");
        }

        return new KinematicState(
            ReadDouble(payload.AsSpan(0, sizeof(double))),
            ReadDouble(payload.AsSpan(sizeof(double), sizeof(double))),
            ReadDouble(payload.AsSpan(sizeof(double) * 2, sizeof(double))));
    }

    private static KinematicState ApplyEvent(KinematicState state, TimelineEvent item)
    {
        if (!string.Equals(item.Kind, "velocity-delta", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unsupported kinematics event kind '{item.Kind}'.");
        }

        double deltaVelocity = double.Parse(item.Payload, NumberStyles.Float, CultureInfo.InvariantCulture);
        return state with { Velocity = state.Velocity + deltaVelocity };
    }

    private static KinematicState Advance(KinematicState state, long fromTick, long toTick)
    {
        if (toTick < fromTick)
        {
            throw new ArgumentOutOfRangeException(nameof(toTick), "This example adapter only advances forward.");
        }

        for (long tick = fromTick; tick < toTick; tick++)
        {
            state = ConstantAccelerationSolver.Advance(state, Acceleration, DeltaTimePerTick);
        }

        return state;
    }

    private static byte[] Encode(KinematicState state)
    {
        byte[] payload = new byte[sizeof(double) * 3];
        WriteDouble(payload.AsSpan(0, sizeof(double)), state.Position);
        WriteDouble(payload.AsSpan(sizeof(double), sizeof(double)), state.Velocity);
        WriteDouble(payload.AsSpan(sizeof(double) * 2, sizeof(double)), state.ElapsedTime);
        return payload;
    }

    private static double ReadDouble(ReadOnlySpan<byte> bytes) =>
        BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(bytes));

    private static void WriteDouble(Span<byte> bytes, double value) =>
        BinaryPrimitives.WriteInt64LittleEndian(bytes, BitConverter.DoubleToInt64Bits(value));
}
