namespace TheSingularityWorkshop.Physics.Universe;

/// <summary>
/// Tracks requested logical time separately from time whose work has been committed.
/// This ledger does not execute a physical model; callers commit only after processing a batch.
/// </summary>
public sealed class LogicalTimeLedger
{
    public LogicalTimeLedger(long initialTick = 0)
    {
        if (initialTick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialTick), "Logical ticks cannot be negative.");
        }

        CurrentTick = initialTick;
        TargetTick = initialTick;
    }

    /// <summary>The latest tick for which the caller has committed completed work.</summary>
    public long CurrentTick { get; private set; }

    /// <summary>The furthest requested tick that must be reached before the ledger is caught up.</summary>
    public long TargetTick { get; private set; }

    /// <summary>Number of requested ticks still awaiting processing.</summary>
    public long PendingTicks => TargetTick - CurrentTick;

    public bool IsCaughtUp => CurrentTick == TargetTick;

    /// <summary>
    /// Extends the requested horizon. A request cannot silently discard existing catch-up debt.
    /// </summary>
    public void RequestThrough(long targetTick)
    {
        if (targetTick < TargetTick)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetTick),
                "A new request cannot discard previously requested logical time. Use a timeline fork for alternate history.");
        }

        TargetTick = targetTick;
    }

    /// <summary>
    /// Plans a bounded, contiguous batch. Planning does not advance the ledger.
    /// </summary>
    public LogicalTickBatch PlanNextBatch(long maximumTicks)
    {
        if (maximumTicks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumTicks), "Batch size must be positive.");
        }

        var count = Math.Min(PendingTicks, maximumTicks);
        return new LogicalTickBatch(CurrentTick, checked(CurrentTick + count));
    }

    /// <summary>
    /// Commits a completed planned batch only if it starts at the current tick and stays within the target.
    /// The caller is responsible for ensuring every tick in the range was successfully processed.
    /// </summary>
    public void Commit(LogicalTickBatch batch)
    {
        if (batch.StartTick != CurrentTick)
        {
            throw new InvalidOperationException("The batch is stale or skips uncommitted logical time.");
        }

        if (batch.EndTick < batch.StartTick || batch.EndTick > TargetTick)
        {
            throw new ArgumentOutOfRangeException(nameof(batch), "The batch must be ordered and cannot exceed the requested target.");
        }

        if (batch.EndTick == batch.StartTick && PendingTicks > 0)
        {
            throw new InvalidOperationException("A pending ledger cannot commit an empty batch.");
        }

        CurrentTick = batch.EndTick;
    }
}

/// <summary>An inclusive-start, exclusive-end range of logical ticks to process.</summary>
public readonly record struct LogicalTickBatch(long StartTick, long EndTick)
{
    public long TickCount => EndTick - StartTick;
}
