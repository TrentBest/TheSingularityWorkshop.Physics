using NUnit.Framework;
using TheSingularityWorkshop.Physics.Mechanics;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class AxialLinkFailureMonitorTests
{
    private static AxialSpringDamper.StepResult Observation(long step, double time, double force, string linkId = "link-1") =>
        new(linkId, step, time, force,
            new AxialSpringDamper.BodyState("A", 1d, 0d, 0d),
            new AxialSpringDamper.BodyState("B", 1d, 1d, 0d));

    [TestCase(9.99d)]
    [TestCase(10d)]
    public void Observe_BelowOrAtThreshold_DoesNotFail(double force)
    {
        var monitor = new AxialLinkFailureMonitor("link-1", 10d);

        Assert.That(monitor.Observe(Observation(0, 0d, force)), Is.Null);
        Assert.That(monitor.Transition, Is.Null);
    }

    [TestCase(10.01d)]
    [TestCase(-10.01d)]
    public void Observe_AboveAbsoluteThreshold_RecordsFirstTransition(double force)
    {
        var monitor = new AxialLinkFailureMonitor("link-1", 10d);

        var transition = monitor.Observe(Observation(4, 0.04d, force));

        Assert.That(transition, Is.Not.Null);
        Assert.That(transition!.LinkId, Is.EqualTo("link-1"));
        Assert.That(transition.StepIndex, Is.EqualTo(4));
        Assert.That(transition.ElapsedTime, Is.EqualTo(0.04d));
        Assert.That(transition.AxialForceOnBodyA, Is.EqualTo(force));
        Assert.That(transition.MaximumAbsoluteForce, Is.EqualTo(10d));
        Assert.That(monitor.Transition, Is.EqualTo(transition));
    }

    [Test]
    public void Observe_AfterFailure_ReturnsNoSecondTransition()
    {
        var monitor = new AxialLinkFailureMonitor("link-1", 10d);
        var first = monitor.Observe(Observation(1, 0.01d, 11d));

        var later = monitor.Observe(Observation(2, 0.02d, 100d));

        Assert.That(first, Is.Not.Null);
        Assert.That(later, Is.Null);
        Assert.That(monitor.Transition, Is.EqualTo(first));
    }

    [Test]
    public void Observe_RequiresMatchingLinkAndIncreasingSteps()
    {
        var monitor = new AxialLinkFailureMonitor("link-1", 10d);

        Assert.Throws<ArgumentException>(() => monitor.Observe(Observation(0, 0d, 0d, "other")));
        Assert.That(monitor.Observe(Observation(1, 0.01d, 0d)), Is.Null);
        Assert.Throws<ArgumentException>(() => monitor.Observe(Observation(1, 0.02d, 0d)));
        Assert.Throws<ArgumentException>(() => monitor.Observe(Observation(2, 0.005d, 0d)));
    }

    [Test]
    public void Constructor_RejectsInvalidThreshold()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AxialLinkFailureMonitor("link-1", -1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AxialLinkFailureMonitor("link-1", double.NaN));
        Assert.Throws<ArgumentException>(() => new AxialLinkFailureMonitor(" ", 1d));
    }
}
