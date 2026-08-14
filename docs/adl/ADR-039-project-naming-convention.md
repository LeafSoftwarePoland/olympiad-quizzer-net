# ADR-039: Project and solution naming convention

**Status:** Accepted
**Date:** 2026-08-14
**Clarifies:** ADR-032

## Problem

Project file names encode the component but not the ring that owns it. `olympiad-quizzer-net.Domain`
sits in `Core/`, `olympiad-quizzer-net.SQLite` sits in `Infrastructure/`, and nothing in either name
says so. Two test projects sit in `App/` while one of them tests `Core/`. A reader holding an assembly
name, a build log line or a stack frame cannot derive the folder it came from.

## Considered

**Naming rule**

- **Keep component-only names** — zero churn. Ring stays invisible; `Infrastructure.SQLite` root
  namespace already contradicts it.
- **`{SolutionName}.{FolderName}[.{SubName}]`** — assembly name maps 1:1 to a folder path. Costs a
  one-shot rename of every project, plus solution, Dockerfile, workflow and doc references.
- **Ring as a folder only, flat names** — status quo by another word. Same defect.

**Root namespace**

- **Namespaces stay `OlympiadQuizzer.<Part>`** — no `.cs` churn. Leaves a permanent gap: the assembly
  `olympiad-quizzer-net.App.API` holds `OlympiadQuizzer.Api.*`. Worse, `Infrastructure.SQLite`
  *already* carries its folder segment, so the exception would apply to four of six projects and not
  to the fifth. A documented inconsistency, forever.
- **Namespaces follow the same rule** — mechanical rewrite of every `namespace` and `using`
  directive. Compiler catches every miss; `TreatWarningsAsErrors` plus a full-solution CI build makes
  a partial rename impossible to merge.

**Test project location**

- **All test projects in `App/`** — status quo. `Domain.L0` would be named `App.Domain.L0` under the
  rule, which is a lie: it tests `Core/` and references `Core/` only.
- **Test project lives in the folder of the ring it exercises** — L0 moves to `Core/`, L1 stays in
  `App/`. Name and reference graph agree.
- **Dedicated top-level `Tests/` folder** — clean separation of shipping from non-shipping, but
  discards the ring information the rule exists to surface, and would need a second rule for the
  `{FolderName}` token.

## Decision

**Rule: `{SolutionName}.{FolderName}[.{SubName}]` for project file, folder and assembly name.
Root namespace follows the same shape. Test projects live in the folder of the ring they exercise.**

### Token definitions

- `{SolutionName}` is the product slug `olympiad-quizzer-net`. In a namespace it transliterates to
  `OlympiadQuizzer` — dashes are illegal in namespaces and the `-net` suffix is a repository-name
  artefact, not part of the product. This transliteration is not mechanical; it is fixed here.
- `{FolderName}` is the immediate ring folder under `source/` — `Core`, `Infrastructure`, `App`.
- `{SubName}` is the component and, for test projects, the level suffix.
- Acronyms keep the ADR-032 split: dashed names use the folder casing (`API`), namespaces use the
  Framework Design Guidelines casing (`Api`). Unchanged.

### Rename table (definitive)

| # | Old project path | New project path |
|---|---|---|
| 1 | `source/Core/olympiad-quizzer-net.Domain/olympiad-quizzer-net.Domain.csproj` | `source/Core/olympiad-quizzer-net.Core.Domain/olympiad-quizzer-net.Core.Domain.csproj` |
| 2 | `source/Infrastructure/olympiad-quizzer-net.SQLite/olympiad-quizzer-net.SQLite.csproj` | `source/Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite/olympiad-quizzer-net.Infrastructure.SQLite.csproj` |
| 3 | `source/App/olympiad-quizzer-net.API/olympiad-quizzer-net.API.csproj` | `source/App/olympiad-quizzer-net.App.API/olympiad-quizzer-net.App.API.csproj` |
| 4 | `source/App/olympiad-quizzer-net.Client/olympiad-quizzer-net.Client.csproj` | `source/App/olympiad-quizzer-net.App.Client/olympiad-quizzer-net.App.Client.csproj` |
| 5 | `source/App/olympiad-quizzer-net.Domain.L0/olympiad-quizzer-net.Domain.L0.csproj` | `source/Core/olympiad-quizzer-net.Core.Domain.L0/olympiad-quizzer-net.Core.Domain.L0.csproj` |
| 6 | `source/App/olympiad-quizzer-net.API.L1/olympiad-quizzer-net.API.L1.csproj` | `source/App/olympiad-quizzer-net.App.API.L1/olympiad-quizzer-net.App.API.L1.csproj` |
| 7 | *(new project)* | `source/Core/olympiad-quizzer-net.Core.Tests.Common/olympiad-quizzer-net.Core.Tests.Common.csproj` |

| # | Old `AssemblyName` | New `AssemblyName` | Old `RootNamespace` | New `RootNamespace` |
|---|---|---|---|---|
| 1 | `olympiad-quizzer-net.Domain` | `olympiad-quizzer-net.Core.Domain` | `OlympiadQuizzer.Domain` | `OlympiadQuizzer.Core.Domain` |
| 2 | `olympiad-quizzer-net.SQLite` | `olympiad-quizzer-net.Infrastructure.SQLite` | `OlympiadQuizzer.Infrastructure.SQLite` | *(unchanged)* |
| 3 | `olympiad-quizzer-net.API` | `olympiad-quizzer-net.App.API` | `OlympiadQuizzer.Api` | `OlympiadQuizzer.App.Api` |
| 4 | `olympiad-quizzer-net.Client` | `olympiad-quizzer-net.App.Client` | `OlympiadQuizzer.Client` | `OlympiadQuizzer.App.Client` |
| 5 | `olympiad-quizzer-net.Domain.L0` | `olympiad-quizzer-net.Core.Domain.L0` | `OlympiadQuizzer.Domain.L0` | `OlympiadQuizzer.Core.Domain.L0` |
| 6 | `olympiad-quizzer-net.API.L1` | `olympiad-quizzer-net.App.API.L1` | `OlympiadQuizzer.Api.L1` | `OlympiadQuizzer.App.Api.L1` |
| 7 | — | `olympiad-quizzer-net.Core.Tests.Common` | — | `OlympiadQuizzer.Core.Tests.Common` |

`AssemblyName` always equals the project file stem. Both properties stay explicit in every project
file (ADR-032) — the rule is now derivable, but MSBuild's default still transliterates dashes to
underscores.

### New project — `Core/olympiad-quizzer-net.Core.Tests.Common`

Holds test constants and shared test data only. References Domain. Referenced by both test projects,
by nothing that ships. Exists so a tag vocabulary value or a fixture identifier is written once
rather than once per test project.

### Test projects sit with the ring they exercise

L0 exercises Domain, references Domain, moves to `Core/`. L1 exercises the API, references
Domain + Infrastructure + API, stays in `App/`. The name now agrees with the reference graph rather
than merely coexisting with it.

Consequence: `Core/` and `App/` each contain shipping and non-shipping projects. Non-shipping is
marked by the `.L0` / `.L1` / `.Tests.` segment in the name and by the packability flag, not by
folder. Container and publish exclusion must key on the name segment, not on the folder — see
Consequences.

### Solution file name is out of scope

The solution file stays `OlympiadQuizzer.slnx`. It is the `{SolutionName}` token's *product* form,
not its slug form; it is referenced by `.github/workflows/ci.yml`, and the CI job name is the
required check on protected `main` (ADR-037), so renaming it is a branch-protection change for
cosmetics. Recorded as a known, accepted asymmetry.

**Pros:**
- Assembly name → folder path is a total function; no lookup table needed to read a build log.
- Namespace, assembly name and folder all agree; no documented exception to remember.
- The `Infrastructure.SQLite` namespace stops being the odd one out — it becomes the pattern.
- Test project name states which ring it may reference; a misplaced test project is visible in the name.

**Cons:**
- Every `namespace` and `using` directive in the repo changes. Large, low-risk, compiler-verified diff.
- Names get longer: `olympiad-quizzer-net.Infrastructure.SQLite` is 42 characters, and the folder
  repeats its own name (`Core/olympiad-quizzer-net.Core.Domain`).
- One-shot collateral edits across solution file, container build, workflows, docs and README.
- `Core/` now holds non-shipping projects; container exclusion rules must be name-based.

## Consequences — collateral edits (all one-shot, all in the rename change)

| Artefact | What changes |
|---|---|
| `OlympiadQuizzer.slnx` | Six project paths; L0 entry moves from the `/App/` solution folder to `/Core/`; new entry for `Core.Tests.Common`. |
| `source/App/olympiad-quizzer-net.App.API/Dockerfile` | `COPY` source paths, the publish project path, and the entry-point assembly file name. |
| `.dockerignore` | Existing test-project entries are already stale — they omit the `source/` prefix and match nothing. Replace with name-segment patterns that survive the move of L0 into `Core/`. See ADR-040 for the separate `source/` exclusion defect. |
| `.github/workflows/deploy-frontend.yml` | Client publish project path. |
| `.github/workflows/ci.yml` | No change — builds the solution file, which is not renamed. |
| `appsettings.Production.json` | No change — the log-level filter keys on the `OlympiadQuizzer` prefix, which every new namespace still starts with. |
| `docs/architecture-guide.md`, `docs/coding-standards.md`, `README.md` | Project tables and run commands. |
| Every `.cs` file | `namespace` and `using` directives. |

Rename order matters: rename on disk, fix project references, fix the solution file, then rewrite
namespaces. Doing namespaces first leaves the solution unbuildable for the whole change.

## Remarks / Sources

- ADR-032 (Onion layout) — ring folders and the explicit-name rule this ADR extends. Its layer table
  now shows pre-rename names; superseded by the table above, ADR-032 body not retro-edited.
- ADR-037 (protected `main`) — the reason the solution file is not renamed.
- ADR-040 — data folder move, which lands in the same structural change and shares the Dockerfile edit.
- ADR-041 — API internal file layout, which lands in the same structural change.
- Acronym casing: [Framework Design Guidelines — capitalization conventions](https://learn.microsoft.com/dotnet/standard/design-guidelines/capitalization-conventions).
- Risk: the `Infrastructure.SQLite` project still contains no SQLite (ADR-032). This rename does not
  change that and must not be read as a signal that it did.
