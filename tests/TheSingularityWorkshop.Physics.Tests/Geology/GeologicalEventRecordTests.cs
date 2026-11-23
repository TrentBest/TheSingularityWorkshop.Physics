using NUnit.Framework;
using TheSingularityWorkshop.Physics.Geology;

namespace TheSingularityWorkshop.Physics.Tests.Geology;

[TestFixture]
public sealed class GeologicalEventRecordTests
{
    [Test]
    public void TimeInterval_SeparatesGeologicalAgeFromIncreasingSimulationClock()
    {
        var interval = new GeologicalTimeInterval(
            startAgeYearsBeforePresent: 10_000_000d,
            endAgeYearsBeforePresent: 9_000_000d,
            simulationStartSeconds: 12d,
            simulationEndSeconds: 13.5d,
            stepIndex: 4);

        Assert.That(interval.GeologicalDurationYears, Is.EqualTo(1_000_000d));
        Assert.That(interval.SimulationDurationSeconds, Is.EqualTo(1.5d));
        Assert.That(interval.StepIndex, Is.EqualTo(4));
    }

    [Test]
    public void TimeInterval_AllowsInstantaneousEventAtOneGeologicalAge()
    {
        var interval = new GeologicalTimeInterval(66_000_000d, 66_000_000d, 5d, 5.01d, 8);

        Assert.That(interval.GeologicalDurationYears, Is.Zero);
        Assert.That(interval.SimulationDurationSeconds, Is.EqualTo(0.01d).Within(1e-12));
    }

    [Test]
    public void TimeInterval_RejectsReversedClocksAndInvalidValues()
    {
        Assert.Throws<ArgumentException>(() => new GeologicalTimeInterval(1d, 2d, 0d, 1d, 0));
        Assert.Throws<ArgumentException>(() => new GeologicalTimeInterval(2d, 1d, 2d, 1d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeologicalTimeInterval(double.NaN, 0d, 0d, 1d, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeologicalTimeInterval(1d, 0d, 0d, 1d, -1));
    }

    [Test]
    public void EventRecord_PreservesTypedTimeAndProvenance()
    {
        var time = new GeologicalTimeInterval(66_000_000d, 66_000_000d, 4d, 4.1d, 12);
        var record = new GeologicalEventRecord(
            "event/impact-001",
            "impact.crater-formation",
            time,
            "world/revision-11",
            "world/revision-12",
            "physics.geology.impact",
            "0.1.0",
            "impact.crater-emplacement",
            "1.0.0",
            """{"projectileDiameterKm":10,"targetMaterial":"silicate"}""",
            "Impact forms a crater and ejecta blanket.");

        Assert.That(record.EventId, Is.EqualTo("event/impact-001"));
        Assert.That(record.EventTypeId, Is.EqualTo("impact.crater-formation"));
        Assert.That(record.Time, Is.EqualTo(time));
        Assert.That(record.SourceRevisionId, Is.EqualTo("world/revision-11"));
        Assert.That(record.ResultRevisionId, Is.EqualTo("world/revision-12"));
        Assert.That(record.ParametersJson, Does.Contain("projectileDiameterKm"));
    }

    [Test]
    public void EventRecord_RejectsMissingIdentityAndNonObjectParameters()
    {
        var time = new GeologicalTimeInterval(1d, 0d, 0d, 1d, 0);
        Assert.Throws<ArgumentException>(() => new GeologicalEventRecord(
            " ", "impact", time, "a", "b", "model", "1", "algorithm", "1", "{}", "summary"));
        Assert.Throws<ArgumentException>(() => new GeologicalEventRecord(
            "event", "impact", time, "a", "b", "model", "1", "algorithm", "1", "[]", "summary"));
        Assert.Throws<ArgumentException>(() => new GeologicalEventRecord(
            "event", "impact", time, "a", "b", "model", "1", "algorithm", "1", "{bad", "summary"));
    }
}
