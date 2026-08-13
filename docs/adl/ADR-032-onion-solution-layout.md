# ADR-032: Onion solution layout — Core / Infrastructure / App

**Status:** Accepted
**Date:** 2026-08-13
**Supersedes layout in:** ADR-021

## Problem

POC layout is `source/{api,client,shared,tests}`. The shared library holds domain models *and*
is the only shared library, so it attracts anything two projects need — ADR-021 admitted this
and mitigated it with a written rule, i.e. with discipline rather than with structure. All test
levels live in one project. Nothing in the folder names states the dependency rule, so nothing
prevents breaking it.

## Considered

- **Keep `source/shared`** — zero migration cost. The name invites a dumping ground; test
  levels stay mixed in one project; the dependency rule stays a review item forever.
- **Namespace-only layering inside one project** — cheapest. The compiler enforces nothing.
- **One project per ring, grouped into Core / Infrastructure / App folders** — the reference
  graph itself enforces the rule. Costs a restructure and six project files.
- **Add a separate Application ring** (use-cases between Domain and API) — textbook Onion. For
  one read-only endpoint the handlers would be pass-throughs. Rejected as ceremony.

## Decision

**Four rings, six projects, dependency rule enforced by the reference graph.**

| Ring | Project | References |
|---|---|---|
| Domain | `Core/olympiad-quizzer-net.Domain` | nothing |
| Infrastructure | `Infrastructure/olympiad-quizzer-net.SQLite` | Domain |
| Application / API | `App/olympiad-quizzer-net.API` | Domain, Infrastructure |
| Presentation | `App/olympiad-quizzer-net.Client` | Domain |
| Tests L0 | `App/olympiad-quizzer-net.Domain.L0` | Domain |
| Tests L1 | `App/olympiad-quizzer-net.API.L1` | Domain, Infrastructure, API |

Domain has **zero** project references and **zero** package references. A violation is a
compile error, not a review finding. One reflection-based test asserts the referenced-assembly
set as a second line of defence.

The frontend references Domain only. HTTP is the frontend's own infrastructure boundary, so its
implementation of the repository abstraction lives inside the frontend and never sees the
Infrastructure project.

### The Infrastructure project is named for SQLite, not for JSON

SQLite is the decided storage engine (ADR-004). Reading questions from a JSON file is a
temporary stub *inside* that project. Naming it after JSON would force a project rename — plus
Dockerfile, solution file, CI and reference-graph edits — the day SQLite lands. Named for the
destination, so nothing renames.

### Test projects are named by level, not by target

`L0` / `L1`, not per-target test projects. The level *is* the contract
(`docs/architecture-guide.md`): what is real, what is substituted, what it may touch. Naming by
target would let a file-reading test land in the unit-test project. Naming by level makes that
impossible, because the L0 project references Domain only.

### Bounded exception: the JSON serializer is allowed in Domain

Domain contains the serialization contract — the shared serializer configuration and the
converter for the one wire field whose shape varies. The JSON serializer ships in the platform's
shared framework: no package reference, no I/O.

Justification: the JSON document *is* the canonical question format (ADR-011), so the wire
contract is domain knowledge here rather than an infrastructure detail, and keeping one
serializer configuration visible to both ends of the wire is the whole point of ADR-021.

This is the **only** permitted outward dependency in Domain. Adding a second requires an ADR.

### Assembly name and root namespace are always explicit

Project file names carry dashes; namespaces cannot. Every project sets `AssemblyName` and
`RootNamespace` explicitly. Left to the build defaults, the namespace becomes a
dash-to-underscore transliteration of the file name.

**Pros:**
- Dependency rule enforced by the compiler, not by discipline
- Test level is unambiguous and unfakeable
- No project rename when SQLite arrives
- Domain stays framework-free, so unit tests run without a web or WASM toolchain (ADR-021's
  reasoning, kept)

**Cons:**
- Six projects instead of four; longer build graph
- Dashed assembly names mean explicit name properties in every project file and a dashed DLL
  name in the container entry point
- The Infrastructure project currently contains no SQLite, which reads as wrong until you know why
- No architecture-enforcement tooling; the guard is the reference graph plus one weak
  reflection test

## Amendment — 2026-08-13 — test project naming: component prefix added

The layer table and the "named by level, not by target" section above described flat names
(`olympiad-quizzer-net.L0`, `olympiad-quizzer-net.L1`). Changed to component-prefixed names:

| Layer | Project |
|---|---|
| Tests L0 | `App/olympiad-quizzer-net.Domain.L0` |
| Tests L1 | `App/olympiad-quizzer-net.API.L1` |

Rationale: the flat level names made it unclear what each project tests. The compile-enforced
contract (L0 references Domain only; L1 references Domain + Infrastructure + API) is unchanged —
the prefix makes that contract visible in the name rather than requiring a reader to open the csproj.

Assembly names and root namespaces follow the same pattern:
`olympiad-quizzer-net.Domain.L0` / `OlympiadQuizzer.Domain.L0`
`olympiad-quizzer-net.API.L1` / `OlympiadQuizzer.Api.L1`

---

## Remarks / Sources

- ADR-021 (shared class library) — reasoning kept, project superseded. See its amendment.
- ADR-003 (repository seam), ADR-004 (Dapper/SQLite), ADR-033 (language posture),
  ADR-031 (frontend folders inside this layout)
- v1.0 solution design §2 for exact project settings and the reference diagram
- `docs/architecture-guide.md` layer table updated in the same change
