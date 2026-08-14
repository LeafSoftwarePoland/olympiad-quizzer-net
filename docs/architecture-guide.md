# Architecture Guide

General architecture rules for this repo. Supplements the ADL — does not replace it.

## Architecture style

**Clean Architecture / Onion Architecture.**

Dependency direction: inner layers know nothing about outer layers.

| Layer | Project | Contents | Rules |
|---|---|---|---|
| Domain | `Core/olympiad-quizzer-net.Core.Domain` | `Question`, `ContentBlock`, `QuestionType`, `Grader`, `SubmittedAnswer`, `GradeResult`, `QuestionQuery`, `FilterOptions`, `IQuestionRepository`, `QuizSessionState` + session logic, `JsonOptions` | References **nothing**. No I/O, no HTTP, no DI, no logging. One bounded exception: `System.Text.Json` in `Domain/Serialization` (ADR-032). |
| Infrastructure | `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite` | `JsonQuestionRepository`, `QuestionBankLoader`, `IShuffler`, DI extension | References Domain only. Named for its destination (SQLite, ADR-004); JSON reading is the current stub. |
| Application / API | `App/olympiad-quizzer-net.App.API` | `Program` (class-based), endpoints under `Endpoints/`, startup extensions under `Extensions/`, Dockerfile | References Domain + Infrastructure. Stateless, read-only. |
| Presentation / Client | `App/olympiad-quizzer-net.App.Client` | Blazor WASM, feature folders, `ApiQuestionRepository`, localStorage services | References Domain only. HTTP is the Client's own infrastructure boundary. |
| Tests — L0 | `Core/olympiad-quizzer-net.Core.Domain.L0` | Domain unit tests | References Domain only. Cannot touch the filesystem — enforced by the project graph. |
| Tests — L1 | `App/olympiad-quizzer-net.App.API.L1` | Repository against real JSON; API via `WebApplicationFactory`; real-bank integrity suite | References Domain + Infrastructure + API. |

Full structure, reference graph and exact `.csproj` settings: see the v1.0 solution design.
Code conventions: `docs/coding-standards.md`.

ADRs are the authoritative record of architectural decisions. When code and an ADR conflict, the ADR is wrong — fix the ADR (amendment), then decide whether to fix the code.

## Test levels

Illustrative definitions for this project. Tooling may change — the intent does not.

| Level | Scope | Project | Tooling |
|---|---|---|---|
| L0 — Unit | Single class or method. Hand-authored objects. No I/O, no HTTP, no DI. | `Core/olympiad-quizzer-net.Core.Domain.L0` | xUnit 2.9 + `Microsoft.NET.Test.Sdk` |
| L1 — In-app integration | Multiple real classes in one process: repository + real JSON file, API + real DI via in-memory test host. Only the shuffler (seeded) and loggers are substituted. | `App/olympiad-quizzer-net.App.API.L1` | xUnit + `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) |
| L2 — Out-app integration | Real process, real external dependencies, spun up and torn down on demand (Docker). | *not created* | undecided — Docker + xUnit is the intent |
| L3 — E2E / UI | Browser-driven. Real frontend against a real or stubbed backend. | *not created* | undecided — Playwright is the intent |

Both test projects are in `OlympiadQuizzer.slnx`, so `dotnet test OlympiadQuizzer.slnx` runs
L0 and L1 in one invocation and CI needs no per-project step.

Current state (v1.0): L0 and L1 implemented. L2 and L3 deliberately not created — see the
v1.0 test strategy for what that leaves uncovered and what would trigger adding them.

**Real-data validation.** The real `questions.json` is an L1 fixture, not just production
data: an integrity suite asserts the schema invariants the type system cannot express
(mandatory `category[]`, answers existing among `options`, mandatory image `alt`, tag values
drawn from `docs/tags.md`). It re-runs on every bank refresh, followed by a manual
spot-check — an invariant cannot catch an answer that is simply mis-transcribed.

## Document types in this repo

| Type | Location | Schema ref |
|---|---|---|
| ADR | `docs/adl/` | `docs/adl/ADR-SCHEMA.md` |
| Integration doc | `docs/integrations/` | `docs/integrations/INDEX.md` |
| POC doc | `docs/pocs/` | Standalone — no fixed schema |
| Functionality registry | `docs/functionalities.md` | See file header |
| Glossary | `docs/Glossary.md` | — |
| Competition rules | `docs/rules/` | `docs/rules/README.md` |
| Architecture guide | `docs/architecture-guide.md` | This file |
| Coding standards | `docs/coding-standards.md` | See file header |

**Hierarchy (no circular references):**
- POC docs are standalone.
- ADRs can reference POC docs.
- Everything else (architecture guide, functionalities, rules, integrations) can reference ADRs.
- Nothing references up to functionalities or rules (no circular deps).

## ADR amendment rules

See `docs/adl/INDEX.md` header and `docs/adl/ADR-SCHEMA.md`.

Short form: append `## Amendment — YYYY-MM-DD — reason` at the end of the file. Never edit the original body.

## Standing hygiene rule

Whenever an ADR is added:
- Track it in git in the same commit.
- Add it to `docs/adl/INDEX.md` in the same commit.

Same applies to integration docs (`docs/integrations/INDEX.md`) and new competition rules files (`docs/rules/README.md` or a pointer).

No orphaned docs.
