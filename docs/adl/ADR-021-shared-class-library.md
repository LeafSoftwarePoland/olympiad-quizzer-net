# ADR-021: Shared class library for models and grader

**Status:** Accepted
**Date:** 2026-08-08

## Problem

`Question`, `ContentBlock`, `QuizFilter` are needed by both the API (serialize) and the WASM client (deserialize). The grader is needed by the client (runtime) and by xUnit (tests). Where does this code live?

## Considered

- **Duplicate the models in each project** — zero project references. Two definitions of one wire format drift silently; a renamed field breaks at runtime in the browser, not at compile time. The whole point of choosing C# end-to-end (ADR-001) was compile-time safety.
- **Models in the API, client references the API project** — client drags in ASP.NET Core references it cannot use under WASM. Backwards dependency direction.
- **Grader in the Client project, tests reference Client** — works (xUnit can reference a Blazor WASM project), but pulls the WASM SDK and browser-targeted assets into the test run for no benefit, and slows every test cycle.
- **`OlympiadQuizzer.Shared` class library** (`net10.0`, no framework reference) referenced by Api, Client, Tests.

## Decision

**`source/shared/OlympiadQuizzer.Shared.csproj`** — plain `net10.0` class library, no `Microsoft.NET.Sdk.Web`, no WASM SDK, no third-party packages.

Contents: `Question`, `ContentBlock`, `QuestionType`, `QuizFilter`, `AnswerSubmission`, `GradeResult`, `Grader`, `JsonOptions` (the single shared `JsonSerializerOptions` with `JsonNamingPolicy.CamelCase` per ADR-011).

Dependency direction: `Api → Shared`, `Client → Shared`, `Tests → Shared + Api`. Shared references nothing.

**Pros:**
- One definition of the wire format — schema drift becomes a compile error
- Grader tests run against a plain library: fast, no WASM/browser toolchain
- One `JsonSerializerOptions` instance shared by both ends, so camelCase can't be configured on one side only
- Phase 2 server-side grading (if ever wanted) reuses the same code unchanged

**Cons:**
- One more project in the solution than the POC design spec's repo layout showed (`source/api/` + `source/client/`)
- Slight temptation to make Shared a dumping ground — mitigated by the rule: **Shared holds no I/O, no HTTP, no DI, no UI**

## Remarks / Sources

- Deviates from the repo layout drawn in `docs/specs/2026-08-08-olympiad-quizzer-poc-design.md` §"Repo layout" — that layout predates the decision to put the grader under unit test (test-strategy L0).
- ADR-001 (C# end-to-end for compile-time safety), ADR-003 (repository seam lives in Client, not Shared — it is I/O), ADR-011 (camelCase serialization contract)
