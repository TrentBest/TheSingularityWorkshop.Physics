![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Physics MicroBundles

This project is a separate adapter assembly. The Physics core stays independent of MicroBundleDomain and host composition.

**Manifest contract:** [versioned JSON Schema](schemas/physics-microbundle.schema.json) · [contract, runtime mapping, scientific declaration rules, and textbook/laboratory traceability](docs/PHYSICS_MICROBUNDLE_MANIFEST_CONTRACT.md). Manifests are descriptive metadata; they do not load providers or prove scientific validity.

## Implemented adapters

| Bundle ID | Provider ID | Actual behavior exposed | Manifest |
|---:|---|---|---|
| 5201 | physics.contact.sphere-sphere.v1 | Calls the tested sphere-sphere narrowphase detector and returns contact evidence. | [SphereSphereContact](microbundles/SphereSphereContact/microbundle.json) |
| 5202 | physics.contact.normal-impulse.v1 | Calls the tested frictionless translational normal-impulse resolver. | [NormalImpulseResponse](microbundles/NormalImpulseResponse/microbundle.json) |
| 5203 | physics.kinematics.prescribed-motion.v1 | Exposes constant-velocity and quintic smooth-step prescribed motion, analytical speed/acceleration ceiling validation, fixed-step/absolute-time evaluation, and achieved-state commit validation through a typed provider contract. | [PrescribedKinematicMotion](microbundles/PrescribedKinematicMotion/microbundle.json) |

These adapters expose typed interfaces so a composition host can resolve a loaded bundle instance to a contract. Provider IDs in MicroBundleDescriptor are declarations, not a universal registry. The prescribed-motion adapter implements IMicroBundle and IPrescribedKinematicMotionProvider; the host must explicitly discover and cast the loaded bundle instance. MicroBundleDomain does not perform typed provider resolution.

## Explicit limits

These adapters do not implement a simulation loop, gravity integration, box/domino collision, friction, angular impulse, persistent resting contacts, or a high-count marble solver. The detector reports geometric evidence only; the normal impulse response operates on a supplied active contact and body snapshots. The prescribed-motion provider evaluates either constant-velocity motion (with endpoint velocity discontinuities) or quintic smooth-step motion (with zero endpoint velocity/acceleration and validated analytical peak speed/acceleration ceilings), then validates a separately supplied achieved snapshot. It is not a force-driven solver, collision solver, or host world-step coordinator. None of these adapters implements a general simulation loop.

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
