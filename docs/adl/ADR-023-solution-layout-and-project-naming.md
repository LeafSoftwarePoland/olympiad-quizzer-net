# ADR-023: Solution layout and project naming

**Status:** Accepted
**Date:** 2026-08-13

## Problem

Two problems, one structure.

Domain models and the grader are needed by the API, by the frontend and by tests. A single "shared" library attracts anything two projects need, so the dependency rule stays a review item enforced by discipline rather than by structure, and all test levels land in one project.

Separately, a project name that encodes only the component tells a reader nothing about which ring owns it. Holding an assembly name, a build-log line or a stack frame, a reader could not derive the folder it came from.

## Considered

**Layout**

- **One "shared" library plus api/client/tests** — zero migration. The name invites a dumping ground; test levels stay mixed; the dependency rule is never enforced.
- **Namespace-only layering inside one project** — cheapest. The compiler enforces nothing.
- **One project per ring, grouped into ring folders** — the reference graph enforces the rule. Costs a restructure.
- **A separate Application ring of use-case handlers between Domain and API** — textbook Onion. For read-only endpoints the handlers are pass-throughs. Rejected as ceremony.

**Naming**

- **Component-only names** — zero churn. Ring stays invisible, and one project's namespace already contradicted the scheme.
- **`{SolutionName}.{FolderName}[.{SubName}]`, namespaces following the same shape** — assembly name maps to a folder path as a total function. Costs a one-shot rename of every project, namespace and using directive; the compiler catches every miss.

**Test project placement**

- **All test projects in one folder** — status quo. A project named for one ring while testing another is a lie.
- **Test project lives in the folder of the ring it exercises** — name and reference graph agree.
- **A dedicated top-level tests folder** — separates shipping from non-shipping, but discards the ring information the naming rule exists to surface.

## Decision

**Rings as folders, one project per ring, dependency rule enforced by the reference graph. Names take the form `{SolutionName}.{FolderName}[.{SubName}]` for folder, project file, assembly and root namespace alike. A test project lives in the folder of the ring it exercises.**

| Ring | Project | References |
|---|---|---|
| Domain | `Core/olympiad-quizzer-net.Core.Domain` | **nothing** |
| Test support | `Core/olympiad-quizzer-net.Core.Tests.Common` | Domain |
| Domain tests | `Core/olympiad-quizzer-net.Core.Domain.L0` | Domain, Tests.Common |
| Infrastructure | `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite` | Domain |
| Infrastructure tests | `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L1` | Domain, Infrastructure, Tests.Common |
| API | `App/olympiad-quizzer-net.App.API` | Domain, Infrastructure |
| API tests | `App/olympiad-quizzer-net.App.API.L1` | Domain, Infrastructure, API, Tests.Common |
| Presentation | `App/olympiad-quizzer-net.App.Client` | Domain |

- **Domain has zero project references and zero package references.** A violation is a compile error, not a review finding. One reflection-based test asserts the referenced-assembly set as a second line of defence.
- The frontend references Domain only. HTTP is the frontend's own infrastructure boundary, so its implementation of the repository abstraction lives inside the frontend and never sees the Infrastructure project.
- **One test project per production project**, named after it with the tier as the last segment. A test whose subject lives in another production project belongs in that project's test project.
- Test support code shared by two or more test projects lives in the shared test project. It carries no tier and ships nothing.
- The Infrastructure project is named for the destination storage engine (ADR-029), not for whatever it currently reads, so it does not rename when storage lands.
- **Bounded exception — the JSON serializer is allowed in Domain.** The serialization contract, its options and the converter for the one wire field whose shape varies live there. The serializer ships in the platform's shared framework, so this costs no package reference and does no I/O. Justified because JSON is the wire format on both sides of the HTTP call and the shape of the authored bank (ADR-007), making the serialization contract domain knowledge here rather than an infrastructure detail, and because one serializer configuration visible to both ends of the wire is the whole point of this layout. **This is the only permitted outward dependency in Domain.** A second one requires an amendment.
- Assembly name and root namespace stay **explicit** in every project file. Project file names carry dashes; namespaces cannot. Left to the build defaults the namespace becomes a dash-to-underscore transliteration.
- **The solution file name is out of scope** and stays as it is. It names the product, not a component; it is referenced by the CI workflow, whose job name is the required check on the protected branch (ADR-027), so renaming it is a branch-protection change for cosmetics. Recorded as an accepted asymmetry.

Accepted cons:

- Eight projects instead of four; longer build graph.
- Names are long, and a ring folder repeats its own name in the project folder beneath it.
- Ring folders hold both shipping and non-shipping projects. Container and publish exclusion must key on the name segment, not on the folder.
- Dashed assembly names mean a dashed entry-point file name in the container.
- No architecture-enforcement tooling. The guard is the reference graph plus one reflection test.

## Remarks / Sources

- Naming form details, acronym casing and the per-project settings list are enforced as coding standards, not restated here.
- ADR-002 (the abstraction Domain owns), ADR-007 (why the serializer exception exists), ADR-029 (storage), ADR-022 (frontend folders sit inside the Presentation ring), ADR-027 (why the solution file is not renamed)
- Acronym casing: [Framework Design Guidelines — capitalization conventions](https://learn.microsoft.com/dotnet/standard/design-guidelines/capitalization-conventions)

## Amendment — 2026-08-15 — build-enforced Domain isolation, a non-ring folder, and Infrastructure's responsibility boundary

**Overrides:** the Decision bullet stating that "one reflection-based test asserts the
referenced-assembly set as a second line of defence".
**Adds:** `source/Solution/`; the Infrastructure responsibility rule.

- Domain's zero-dependency rule is enforced by an **MSBuild target in its own project file**, which
  fails the build when a project or package reference is present. The reflection test is deleted.
  Reason: it failed late (after a green build), could be removed by a test filter, had no
  production counterpart, and amounted to asserting that the project file contains what the project
  file contains. Platform mechanisms enforce build constraints; tests do not police configuration.
- **New non-ring folder `source/Solution/`.** Holds test projects whose subject is the repository's
  committed output — the question bank, the generated database, the machine-readable rule blocks —
  rather than any production project's code. Such suites have no production counterpart, so the
  one-counterpart rule excludes them from every tiered project.
  Deviation is confined to two axes: a folder that is not a ring, and a tier outside L0–L3.
  Everything else complies. Recorded as the single standing exception; another requires the same
  justification, decided beforehand.
- **Infrastructure does I/O and nothing else.** A persistence class reads and writes. It does not
  validate and does not convert, because neither is its responsibility — it receives what is
  already prepared. Connection-level faults it may handle; **data-level faults bubble**, because
  interpreting them is a decision for a ring that knows what the data means.
- **Two seams, nested — and they are not the same seam.** Conflating them is the reading this
  bullet exists to prevent.

  | | Declared in | Implemented in | Speaks | Domain sees it |
  |---|---|---|---|---|
  | **Outer** — the repository abstraction (ADR-002) | **Domain** | Infrastructure, and separately in the frontend | domain types only | yes — Domain owns it |
  | **Inner** — the I/O seam | Infrastructure | Infrastructure | row types | **never** |

  The outer seam is what ADR-002 mandates and what the Decision above means by Domain owning the
  abstraction. Domain states *"this is where questions come from"* and knows nothing of how they
  are delivered.

  The inner seam is Infrastructure's own business. Its row types are shaped by the storage engine,
  so they must not cross into Domain; the mapping from row to domain object happens in the
  repository implementation, below the outer seam.

  **The L0-testability of Infrastructure is bought by the inner seam, not the outer one.** Mocking
  it puts the repository's real logic — filtering, clamping, shuffle-then-cap — under test with no
  database present, which was impossible while I/O and logic were welded together in one class. An
  earlier wording of this bullet described a single seam and credited it with both roles; read
  literally, that demanded the row-level abstraction live in Domain, which would drag storage-shaped
  types into the one ring that must not have them.
