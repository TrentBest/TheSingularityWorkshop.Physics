using NUnit.Framework;
using TheSingularityWorkshop.Physics.Electricity;

namespace TheSingularityWorkshop.Physics.Tests;

[TestFixture]
public sealed class ElectricalGridTests
{
    [Test]
    public void Observe_ComputesOhmsLawAndPowerForEnabledResistiveLoad()
    {
        var grid = ElectricalGrid.Observe(12d, 100d, [new ResistiveElectricalLoad("lamp", 6d)]);

        Assert.That(grid.Loads, Has.Count.EqualTo(1));
        Assert.That(grid.Loads[0].CurrentAmperes, Is.EqualTo(2d).Within(1e-12));
        Assert.That(grid.Loads[0].PowerWatts, Is.EqualTo(24d).Within(1e-12));
        Assert.That(grid.DemandWatts, Is.EqualTo(24d).Within(1e-12));
        Assert.That(grid.TotalCurrentAmperes, Is.EqualTo(2d).Within(1e-12));
        Assert.That(grid.RemainingCapacityWatts, Is.EqualTo(76d).Within(1e-12));
        Assert.That(grid.IsOverloaded, Is.False);
    }

    [Test]
    public void Observe_DisabledLoadDrawsNoCurrentOrPower()
    {
        var grid = ElectricalGrid.Observe(12d, 10d, [new ResistiveElectricalLoad("lamp", 6d, isEnabled: false)]);

        Assert.That(grid.DemandWatts, Is.Zero);
        Assert.That(grid.TotalCurrentAmperes, Is.Zero);
        Assert.That(grid.Loads[0].CurrentAmperes, Is.Zero);
        Assert.That(grid.Loads[0].PowerWatts, Is.Zero);
    }

    [Test]
    public void Observe_CombinesLoadsAndFlagsCapacityOverload()
    {
        var grid = ElectricalGrid.Observe(120d, 100d,
        [
            new ResistiveElectricalLoad("heater", 120d),
            new ResistiveElectricalLoad("lamp", 240d)
        ]);

        Assert.That(grid.Loads.Select(load => load.LoadId), Is.EqualTo(new[] { "heater", "lamp" }));
        Assert.That(grid.Loads[0].PowerWatts, Is.EqualTo(120d).Within(1e-12));
        Assert.That(grid.Loads[1].PowerWatts, Is.EqualTo(60d).Within(1e-12));
        Assert.That(grid.DemandWatts, Is.EqualTo(180d).Within(1e-12));
        Assert.That(grid.TotalCurrentAmperes, Is.EqualTo(1.5d).Within(1e-12));
        Assert.That(grid.RemainingCapacityWatts, Is.EqualTo(-80d).Within(1e-12));
        Assert.That(grid.IsOverloaded, Is.True);
    }

    [Test]
    public void Observe_IsIndependentOfInputOrder()
    {
        var first = ElectricalGrid.Observe(24d, 1000d,
            [new ResistiveElectricalLoad("z", 12d), new ResistiveElectricalLoad("a", 24d)]);
        var second = ElectricalGrid.Observe(24d, 1000d,
            [new ResistiveElectricalLoad("a", 24d), new ResistiveElectricalLoad("z", 12d)]);

        Assert.That(second.Loads.Select(load => load.LoadId), Is.EqualTo(first.Loads.Select(load => load.LoadId)));
        Assert.That(second.DemandWatts, Is.EqualTo(first.DemandWatts));
        Assert.That(second.TotalCurrentAmperes, Is.EqualTo(first.TotalCurrentAmperes));
    }

    [Test]
    public void Observe_RejectsDuplicateLoadIdentities()
    {
        Assert.Throws<ArgumentException>(() => ElectricalGrid.Observe(12d, 100d,
            [new ResistiveElectricalLoad("lamp", 6d), new ResistiveElectricalLoad("lamp", 12d)]));
    }

    [Test]
    public void ResistiveLoad_WithEnabledReturnsNewSwitchState()
    {
        var original = new ResistiveElectricalLoad("lamp", 6d, isEnabled: false);
        var switchedOn = original.WithEnabled(true);

        Assert.That(original.IsEnabled, Is.False);
        Assert.That(switchedOn.IsEnabled, Is.True);
        Assert.That(switchedOn.Id, Is.EqualTo(original.Id));
        Assert.That(switchedOn.ResistanceOhms, Is.EqualTo(original.ResistanceOhms));
    }

    [TestCase(0d)]
    [TestCase(-1d)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidResistance(double resistance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResistiveElectricalLoad("load", resistance));
    }

    [Test]
    public void Observe_RejectsInvalidGridInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ElectricalGrid.Observe(0d, 100d, []));
        Assert.Throws<ArgumentOutOfRangeException>(() => ElectricalGrid.Observe(12d, -1d, []));
        Assert.Throws<ArgumentNullException>(() => ElectricalGrid.Observe(12d, 100d, null!));
    }
}
