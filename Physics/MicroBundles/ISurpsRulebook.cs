using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.SingularityUniversalRolePlayingSystem;

/// <summary>
/// Marks a MicroBundle that presents domain knowledge through the SURPS rulebook layer.
/// </summary>
/// <remarks>
/// This is the minimal SURPS-owned marker contract copied into the Physics adapter assembly
/// so Physics can integrate without taking a package dependency on a sibling rulebook.
/// Keep this shape aligned with the canonical SURPS contract as it evolves.
/// </remarks>
public interface ISurpsRulebook : IMicroBundle
{
}
