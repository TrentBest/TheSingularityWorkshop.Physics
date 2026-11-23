# SURPS Repository Participation Contract

![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

This repository participates in the Singularity Universal Roleplaying System (SURPS) as the authoritative owner of Physics-domain material. This page is the local implementation guide; **the SURPS repository is the single authority for cross-repository identity, participation, rulebook IDs, ownership claims, and the master backlog.**

## Canonical coordination

- [SURPS master rulebook map](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/rulebook-backlog/index.md)
- [Machine-readable master registry](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/.surps/rulebook-backlog.json)
- [SURPS participation and ownership policy](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/docs/books/index.md)
- [Rulebook owner sign-in template](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/.github/ISSUE_TEMPLATE/rulebook-owner-sign-in.yml)
- [Repository participation template](https://github.com/TrentBest/TheSingularityWorkshop.SingularityUniversalRoleplayingSystem/blob/development/.github/ISSUE_TEMPLATE/surps-repository-participation.yml)

Do not create a competing local master list or mint a second ID for a book already registered in SURPS. Update the SURPS registry through a reviewed pull request when ownership, canonical paths, participation, or book status changes.

## Repository-local structure

A participating repository maintains:

1. `.surps/participation.json` — repository identity, domain responsibility, participation status, catalogue path, and the books it owns or contributes to.
2. `docs/books/index.md` — human-readable local catalogue linking to each canonical sourcebook and descriptor.
3. One Markdown sourcebook per distinct book, with a stable globally unique SURPS book ID.
4. One `.book.json` descriptor per book, declaring identity, title, category, LOD, version, status, and canonical source/cover paths.
5. One distinct cover asset per book, with shared SURPS identity/LOD badges kept separate from cover artwork.
6. Evidence links to authoritative domain implementation, validation, worked examples, and explicit limitations.

Use the existing Physics sourcebook catalogue and descriptors as the working examples. Do not create parallel abstractions merely to satisfy a checklist; reuse the repository's existing build, validation, and documentation conventions.

## Ownership boundaries

- **Physics owns:** physical laws, equations, units, materials, numerical methods, models, solvers, simulation, and evidence.
- **SURPS owns:** shared roleplaying ontology, rulebook identity, cross-domain semantics, LOD selection, and campaign/tabletop composition.
- **Experience repositories own:** their narrative, interfaces, objectives, player progression, and experience-specific content.
- **Contributors:** may provide evidence, examples, implementation links, or domain material without becoming the owning repository.

SURPS composition is opt-in. It must not silently become a runtime dependency of Physics. Link to authoritative Physics implementation rather than copying solver code or claiming that a planned capability is implemented.

## Book identity, status, and LOD

- Keep distinct works as distinct book IDs, even when they share a family. A family groups related works; it does not collapse their identities.
- A book has one accountable owning repository. Contributors are recorded separately.
- A proposed title is not a completed sourcebook. Use accurate states such as proposed, draft, and published, and distinguish implemented, partial, experimental, specified, and planned capabilities in the content.
- Assign LOD deliberately and consistently with the SURPS identity standard. Do not infer LOD from repository maturity or use it as a substitute for scope.
- Ownership claims are submitted through the SURPS sign-in process and remain pending until reviewed and accepted. An agent cannot approve its own claim.

## Change workflow

1. Check the SURPS master map and registry before creating a book or assigning ownership.
2. Work in this repository's `development` branch and preserve the two-branch policy (`master` and `development`).
3. Update the local sourcebook, descriptor, cover, catalogue, and participation manifest together when the book's canonical identity or paths change.
4. Submit a reviewed SURPS registry PR for cross-repository changes. Include the stable book ID, owning repository, agent identity, canonical URL, source path, descriptor path, scope, exclusions, and evidence.
5. Validate locally and through CI. Do not claim publication or implementation beyond the evidence.
6. Do not publish packages or release artifacts without explicit release authorization.

## Avoiding reinvention

Before designing a new concept, check the SURPS registry and the owning domain repository. Reuse stable IDs, descriptors, LOD semantics, ownership boundaries, catalogue conventions, and existing validation patterns. Extend the authoritative owner rather than creating a near-duplicate implementation in a consumer repository.

---

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
