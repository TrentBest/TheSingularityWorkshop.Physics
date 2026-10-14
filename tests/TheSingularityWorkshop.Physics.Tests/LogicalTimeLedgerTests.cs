using NUnit.Framework;
using TheSingularityWorkshop.Physics.Universe;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class LogicalTimeLedgerTests
{
    [Test]
    public void RequestAndBoundedBatches_PreserveCatchUpDebt()
    {
        var ledger = new LogicalTimeLedger();
        ledger.RequestThrough(100);

        var first = ledger.PlanNextBatch(30);
        Assert.That(first, Is.EqualTo(new LogicalTickBatch(0, 30)));
        Assert.That(ledger.CurrentTick, Is.Zero, "Planning must not claim work has completed.");

        ledger.Commit(first);
        Assert.That(ledger.CurrentTick, Is.EqualTo(30));
        Assert.That(ledger.PendingTicks, Is.EqualTo(70));

        var second = ledger.PlanNextBatch(50);
        ledger.Commit(second);
        Assert.That(ledger.CurrentTick, Is.EqualTo(80));
        Assert.That(ledger.PendingTicks, Is.EqualTo(20));
        Assert.That(ledger.IsCaughtUp, Is.False);
    }

    [Test]
    public void BatchCannotSkipTicksOrCommitPastRequestedTarget()
    {
        var ledger = new LogicalTimeLedger(10);
        ledger.RequestThrough(20);

        Assert.Throws<InvalidOperationException>(
            () => ledger.Commit(new LogicalTickBatch(11, 15)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ledger.Commit(new LogicalTickBatch(10, 21)));

        Assert.That(ledger.CurrentTick, Is.EqualTo(10));
    }

    [Test]
    public void StaleBatchCannotOverwriteNewProgress()
    {
        var ledger = new LogicalTimeLedger();
        ledger.RequestThrough(10);
        var planned = ledger.PlanNextBatch(5);

        ledger.Commit(planned);
        Assert.Throws<InvalidOperationException>(() => ledger.Commit(planned));
        Assert.That(ledger.CurrentTick, Is.EqualTo(5));
    }

    [Test]
    public void NewRequestCannotSilentlyDiscardCatchUpDebt()
    {
        var ledger = new LogicalTimeLedger();
        ledger.RequestThrough(100);

        Assert.Throws<ArgumentOutOfRangeException>(() => ledger.RequestThrough(50));
        Assert.That(ledger.TargetTick, Is.EqualTo(100));
        Assert.That(ledger.PendingTicks, Is.EqualTo(100));
    }

    [Test]
    public void RequestCanExtendHorizonWhileCatchUpIsPending()
    {
        var ledger = new LogicalTimeLedger(5);
        ledger.RequestThrough(10);
        ledger.Commit(ledger.PlanNextBatch(2));
        ledger.RequestThrough(20);

        Assert.That(ledger.CurrentTick, Is.EqualTo(7));
        Assert.That(ledger.TargetTick, Is.EqualTo(20));
        Assert.That(ledger.PendingTicks, Is.EqualTo(13));
    }

    [Test]
    public void BatchSizeMustBePositiveAndInitialTickNonNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogicalTimeLedger(-1));

        var ledger = new LogicalTimeLedger();
        Assert.Throws<ArgumentOutOfRangeException>(() => ledger.PlanNextBatch(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ledger.PlanNextBatch(-1));
    }

    [Test]
    public void CatchUpPlansOnlyRemainingTicks()
    {
        var ledger = new LogicalTimeLedger(40);
        ledger.RequestThrough(45);

        var batch = ledger.PlanNextBatch(100);
        Assert.That(batch, Is.EqualTo(new LogicalTickBatch(40, 45)));
        Assert.That(batch.TickCount, Is.EqualTo(5));

        ledger.Commit(batch);
        Assert.That(ledger.IsCaughtUp, Is.True);
        Assert.That(ledger.PendingTicks, Is.Zero);
        Assert.That(ledger.PlanNextBatch(10).TickCount, Is.Zero);
    }
}
