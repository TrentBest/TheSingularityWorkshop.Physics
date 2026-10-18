![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Physics MicroBundles

This project is a separate adapter assembly. The Physics core stays independent of MicroBundleDomain and host composition.

## Implemented adapters

| Bundle ID | Provider ID | Actual behavior exposed |
|---:|---|---|
| 5201 | physics.contact.sphere-sphere.v1 | Calls the tested sphere-sphere narrowphase detector and returns contact evidence. |
| 5202 | physics.contact.normal-impulse.v1 | Calls the tested frictionless translational normal-impulse resolver. |

The matching manifest files are under microbundles/. These adapters expose typed interfaces so a composition host can resolve a loaded bundle instance to a contract. Provider IDs in MicroBundleDescriptor are declarations, not a universal registry.

## Explicit limits

These adapters do not implement a simulation loop, gravity integration, box/domino collision, friction, angular impulse, persistent resting contacts, or a high-count marble solver. The detector reports geometric evidence only; the normal impulse response operates on a supplied active contact and body snapshots. Neither one advances time.

The experience projects and the host must not treat these adapters as proof that a domino chain or marble run is fully simulated. See the [capability catalog](docs/PHYSICS_CAPABILITY_CATALOG.md) and the [Experiences ownership contract](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/blob/development/docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md).

No NuGet publication or release is authorized by this package's existence.

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
