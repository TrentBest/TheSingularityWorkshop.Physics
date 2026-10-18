using NUnit.Framework;
using TheSingularityWorkshop.Physics.Experiments;

namespace TheSingularityWorkshop.Physics.Tests.Aerodynamics;

[TestFixture]
public sealed class FallingBodyDragExperimentTests
{
    private static FallingBodyDragDefinition CreateDefinition() => new(
        InitialHeight: 100d,
        InitialVelocity: 0d,
        Mass: 1d,
        Gravity: 9.81d,
        AirDensity: 1.225d,
        DragCoefficient: 0.47d,
        ReferenceArea: 0.05d,
        DeltaTime: 0.01d,
        StepCount: 100);

    [Test]
    public void Run_RecordsInitialStateAndEveryStep()
    {
        var result = FallingBodyDragExperiment.Run(CreateDefinition());

        Assert.That(result.Samples, Has.Count.EqualTo(101));
        Assert.That(result.Samples[0].ElapsedTime, Is.EqualTo(0d));
        Assert.That(result.Samples[0].Height, Is.EqualTo(100d));
        Assert.That(result.Samples[0].Velocity, Is.EqualTo(0d));
        Assert.That(result.FinalState.ElapsedTime, Is.EqualTo(1d).Within(1e-12));
    }

    [Test]
    public void Run_DragReducesDownwardSpeedAndDistanceFallen()
    {
        var definition = CreateDefinition();
        var vacuum = FallingBodyDragExperiment.Run(definition, includeDrag: false);
        var air = FallingBodyDragExperiment.Run(definition, includeDrag: true);

        Assert.That(air.FinalState.Velocity, Is.GreaterThan(vacuum.FinalState.Velocity));
        Assert.That(air.FinalState.Height, Is.GreaterThan(vacuum.FinalState.Height));
        Assert.That(air.FinalState.Velocity, Is.LessThan(0d));
    }

    [Test]
    public void Run_ZeroDragAreaMatchesDragDisabledIntegration()
    {
        var definition = CreateDefinition() with { ReferenceArea = 0d };

        var withDragEnabled = FallingBodyDragExperiment.Run(definition, includeDrag: true);
        var withoutDrag = FallingBodyDragExperiment.Run(definition, includeDrag: false);

        Assert.That(withDragEnabled.Samples, Is.EqualTo(withoutDrag.Samples));
    }

    [Test]
    public void Run_RejectsInvalidPhysicalInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FallingBodyDragExperiment.Run(CreateDefinition() with { Mass = 0d }));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FallingBodyDragExperiment.Run(CreateDefinition() with { AirDensity = -1d }));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FallingBodyDragExperiment.Run(CreateDefinition() with { DeltaTime = double.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FallingBodyDragExperiment.Run(CreateDefinition() with { StepCount = 0 }));
    }
}
