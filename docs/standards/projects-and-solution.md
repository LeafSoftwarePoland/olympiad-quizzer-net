# Projects and solution

## Project settings

Every `.csproj` in this repo sets all five properties:

```xml
<AssemblyName>olympiad-quizzer-net.<FolderName>.<SubName></AssemblyName>
<RootNamespace>OlympiadQuizzer.<FolderName>.<SubName></RootNamespace>
<Nullable>disable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

`TreatWarningsAsErrors` is on. To suppress a specific diagnostic, suppress that ID with a
one-line comment stating why. **Never turn the property off.** It is usable precisely because
nullable annotations are off (see [csharp.md](csharp.md) § Null safety) — with that noise gone,
a warning means something, so it can be fatal.

### Domain's zero-dependency rule is enforced by the build, not by a test

Domain has zero project references and zero package references. That is enforced in
`olympiad-quizzer-net.Core.Domain.csproj`:

```xml
<Target Name="AssertDomainHasNoDependencies" BeforeTargets="Build">
  <Error Condition="'@(ProjectReference)' != ''"
         Text="Domain must have zero project references. Found: @(ProjectReference)" />
  <Error Condition="'@(PackageReference)' != ''"
         Text="Domain must have zero package references. Found: @(PackageReference)" />
</Target>
```

**Use the platform's own mechanisms rather than hand-rolled equivalents.** A reflection test
asserting the referenced-assembly set was the previous approach; it failed late (at test time,
after a green build), could be skipped by a test filter, and amounted to asserting that the project
file contains what the project file contains. The MSBuild target fails at **build**, cannot be
filtered out, and names the offender. The general rule: if the build system, the SDK or the
analyser platform can enforce a constraint, enforce it there — do not write a test to police your
own configuration.

## Naming rule — `{SolutionName}.{FolderName}[.{SubName}]`

Applies to the project folder, the `.csproj` file name, the `AssemblyName` **and** the
`RootNamespace`. All four agree.

| Token | In a file/assembly name | In a namespace |
|---|---|---|
| `{SolutionName}` | `olympiad-quizzer-net` | `OlympiadQuizzer` — dashes are illegal, `-net` is a repo-name artefact |
| `{FolderName}` | the ring folder under `source/` | same |
| `{SubName}` | component, plus the tier suffix for test projects | same, acronym casing per [naming-and-comments.md](naming-and-comments.md) |

| Project folder | AssemblyName | RootNamespace |
|---|---|---|
| `source/Core/olympiad-quizzer-net.Core.Domain` | `olympiad-quizzer-net.Core.Domain` | `OlympiadQuizzer.Core.Domain` |
| `source/Core/olympiad-quizzer-net.Core.Domain.L0` | `olympiad-quizzer-net.Core.Domain.L0` | `OlympiadQuizzer.Core.Domain.L0` |
| `source/Core/olympiad-quizzer-net.Core.Tests.Common` | `olympiad-quizzer-net.Core.Tests.Common` | `OlympiadQuizzer.Core.Tests.Common` |
| `source/Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite` | `olympiad-quizzer-net.Infrastructure.SQLite` | `OlympiadQuizzer.Infrastructure.SQLite` |
| `source/Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L0` | `olympiad-quizzer-net.Infrastructure.SQLite.L0` | `OlympiadQuizzer.Infrastructure.SQLite.L0` |
| `source/Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L1` | `olympiad-quizzer-net.Infrastructure.SQLite.L1` | `OlympiadQuizzer.Infrastructure.SQLite.L1` |
| `source/App/olympiad-quizzer-net.App.API` | `olympiad-quizzer-net.App.API` | `OlympiadQuizzer.App.Api` |
| `source/App/olympiad-quizzer-net.App.API.L0` | `olympiad-quizzer-net.App.API.L0` | `OlympiadQuizzer.App.Api.L0` |
| `source/App/olympiad-quizzer-net.App.API.L1` | `olympiad-quizzer-net.App.API.L1` | `OlympiadQuizzer.App.Api.L1` |
| `source/App/olympiad-quizzer-net.App.API.L2` | `olympiad-quizzer-net.App.API.L2` | `OlympiadQuizzer.App.Api.L2` |
| `source/App/olympiad-quizzer-net.App.Client` | `olympiad-quizzer-net.App.Client` | `OlympiadQuizzer.App.Client` |
| `source/Solution/olympiad-quizzer-net.Solution.DataIntegrityTests` | `olympiad-quizzer-net.Solution.DataIntegrityTests` | `OlympiadQuizzer.Solution.DataIntegrityTests` |
| `source/Solution/olympiad-quizzer-net.Solution.BankSync` | `olympiad-quizzer-net.Solution.BankSync` | `OlympiadQuizzer.Solution.BankSync` |

`API` in the dashed name, `Api` in the namespace — the dashed name mirrors the folder, the
namespace follows the acronym rule.

A test project is named after the **one** production project it exercises, with the tier as the
last segment, and lives in that project's ring folder: Domain L0 tests in `Core/`, Infrastructure
tests in `Infrastructure/`, API tests in `App/`. A test project with two production subjects is a
filing defect — see [testing-tiers.md](testing-tiers.md) § Test projects.

### `source/Solution/` — the one recorded exception

`Solution` is **not a ring.** It holds projects whose subject is the repository's own output — the
committed question bank, the generated database, the machine-readable rule blocks — rather than any
production project's code. That covers both the suites that validate those artefacts and the tooling
that produces them; the justification below never depended on them being tests.

The deviation is deliberate and confined to **two axes**: a folder that is not a ring, and a tier
outside L0–L3. Everything else complies — the naming form, the explicit assembly and namespace
properties, the tier trait, the solution listing. Both the folder name and the project name
announce the deviation on sight, which is the point of choosing them.

It qualifies as an exception under § Breaking a rule with sense in [INDEX.md](INDEX.md), including
the sharp test: moving these suites out of the tiered projects leaves each of those with a single
clean subject. It improves compliance elsewhere rather than merely relieving pressure here.

**This is the only such exception.** Another one requires the same six-point justification, decided
before the work and recorded — not discovered afterwards.

The solution file stays `OlympiadQuizzer.slnx` — deliberate exception: the solution names the
product, not a component, so the `{SolutionName}` token rule does not apply. It is also the
required-status-check target on the protected branch, so renaming it is a branch-protection
change, not a cosmetic one.

`AssemblyName` and `RootNamespace` are **always explicit** even though the rule makes them
derivable. Project file names contain dashes; namespaces cannot. Left to MSBuild's default, the
root namespace becomes `olympiad_quizzer_net_Core_Domain`.

## Program entry points

**No top-level statements.** Explicit class, explicit `Main`, declared `public` so the logger
factory can reach the type as a category without ceremony. **Not `partial`** — routes and startup
configuration live in their own units, so there is nothing to split. No `Program.*.cs` file may
exist.

## What is committed

`.pipeline/` is **entirely gitignored**. This is a deliberate project constraint, not a temporary
scaffold. **Do not modify the `.pipeline/` entry in `.gitignore` during any task, for any reason.**

Committed:

- `docs/` — ADRs, standards, architecture guide, integrations, domain rules, tag vocabulary
- `.github/` — workflows, PR template, issue templates
- `data/` — the question bank and its images

Not committed:

- Every `.pipeline/` artifact without exception — state, journal, design, plans, feedback, critiques.

Pipeline artifacts are local scratch. They are **not** version history: the commit messages, the
ADRs and `docs/` are the durable record. Nothing committed may reference a `.pipeline/` path —
that folder is deletable by design, so any such link is a dangling reference waiting to happen.
Copy the substance in instead of linking to it.
