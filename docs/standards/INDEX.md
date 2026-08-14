---
project: olympiad-quizzer-net
language: C#
framework: .NET 10 / Blazor WebAssembly / ASP.NET Core
test_framework: xUnit 2.9
---

# Coding Standards — index

**Agent instruction — read this before you read anything else.**

**This index is a map, not a substitute.** Read every file listed below, in full, before writing or reviewing a single line of code. Not the headings. Not this page. Not the files you guess are relevant. All of them. An agent that reads this index and starts writing code has not read the standards.

Every rule in these files is enforced at every pull request. A violation is a blocking finding, not a suggestion — **including the rules whose value is not obvious on first reading**. Several exist because a specific failure already happened in this repository, and the reasoning is stated inline where it does. Read the reasoning before deciding a rule is arbitrary.

Where these files and an ADR disagree, **these files win**. ADRs record what was decided and why; this is how it is enforced.

## Files

| File | Read it for |
|---|---|
| [projects-and-solution.md](projects-and-solution.md) | `.csproj` settings, the project naming rule, program entry points, what is and is not committed |
| [api.md](api.md) | Controllers, route shape and versioning, startup composition |
| [testing-tiers.md](testing-tiers.md) | L0–L3 definitions, which project a test belongs in, the mirror rule, what may be mocked |
| [testing-conventions.md](testing-conventions.md) | Test naming pattern, AAA structure, tier traits |
| [csharp.md](csharp.md) | Null safety, `var`, initializers, error handling, logging, method length |
| [naming-and-comments.md](naming-and-comments.md) | Identifier naming and language, comment policy, JSON rules |
| [blazor.md](blazor.md) | Component and frontend rules |
| [security.md](security.md) | Secrets, browser storage, import surface, XSS. **Non-negotiable.** |
| [process.md](process.md) | PR and commit format, ADR content rules |

## Scope

These files cover conventions specific to this repository. Where they are silent, follow the
[.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
and the [Framework Design Guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/).

**Test levels are defined in [testing-tiers.md](testing-tiers.md)** and nowhere else. `docs/architecture-guide.md` restates them for orientation; where the two disagree, this directory wins.
