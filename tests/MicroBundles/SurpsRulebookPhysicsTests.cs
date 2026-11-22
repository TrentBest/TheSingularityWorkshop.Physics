using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.SingularityUniversalRolePlayingSystem;
using TheSingularityWorkshop.SingularityUniversalRolePlayingSystem.Rulebooks;
using Xunit;

namespace TheSingularityWorkshop.Physics.MicroBundles.Tests;

public sealed class SurpsRulebookPhysicsTests
{
    [Fact]
    public void PhysicsRulebookDeclaresTheThreeImplementedPhysicsCapabilities()
    {
        var rulebook = new SurpsRulebookPhysics();

        Assert.IsAssignableFrom<ISurpsRulebook>(rulebook);
        Assert.Equal(
            "TheSingularityWorkshop.SingularityUniversalRolePlayingSystem.SurpsRulebookPhysics",
            SurpsRulebookPhysicsIdentity.CanonicalId);
        Assert.Equal(0x0B5175163AE21E02UL, rulebook.Descriptor.Id);
        Assert.Equal("0.1.0-alpha.1", rulebook.Descriptor.Version);
        Assert.Equal("surps.rulebook.physics", rulebook.Descriptor.Providers.Single().Id);

        Assert.Equal(
            new ulong[] { 5201, 5202, 5203 },
            rulebook.Descriptor.Dependencies.Select(dependency => dependency.BundleId).ToArray());
        Assert.Equal(
            new ulong[] { 5201, 5202, 5203 },
            rulebook.Dependencies.Select(dependency => dependency.BundleId).ToArray());
    }

    [Fact]
    public void PhysicsRulebookRequestsItsDomainDependenciesWithoutConfiguration()
    {
        var rulebook = new SurpsRulebookPhysics();

        Assert.Equal(3, rulebook.Dependencies.Count);
        Assert.All(rulebook.Dependencies, dependency => Assert.True(dependency.Configuration.IsEmpty));
    }
}
