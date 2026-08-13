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

- Deviates from the repo layout drawn in `docs/pocs/2026-08-08-olympiad-quizzer-poc-design.md` §"Repo layout" — that layout predates the decision to put the grader under unit test (test-strategy L0).
- ADR-001 (C# end-to-end for compile-time safety), ADR-003 (repository seam lives in Client, not Shared — it is I/O), ADR-011 (camelCase serialization contract)

## Amendment — 2026-08-13 — project superseded by the Domain project; reasoning kept

**Overrides:** the project name, its path, and its contents list.

- `source/shared/OlympiadQuizzer.Shared` is superseded by `Core/olympiad-quizzer-net.Domain`
  (ADR-032). Same idea, better name, and the name no longer invites a dumping ground — which
  this ADR's own Cons section flagged as the main risk and mitigated only with a written rule.
- Contents change with the v1.0 schema: the POC filter type is deleted, a structured query
  object and a filter-values result type are added (ADR-003 amendment), the answer-submission
  shape collapses to one collection (ADR-034), and quiz-session state plus the two pure session
  functions (remaining-time calculation, snapshot validation) move in from the frontend so they
  can be unit tested at all.
- The repository abstraction now lives here too, not in the frontend. This ADR's original
  parenthetical — "the repository seam lives in Client, not Shared — it is I/O" — was about the
  *implementation*, which is still in the frontend. The abstraction is not I/O and belongs
  inward.

**Adds:** the "Shared holds no I/O, no HTTP, no DI, no UI" rule is now structurally enforced.

- The Domain project has zero project references and zero package references, so the rule is a
  compile error rather than discipline (ADR-032).
- One bounded exception, explicit and the only one: the JSON serializer configuration and the
  converter for the one wire field whose shape varies. Rationale in ADR-032 — the JSON document
  *is* the canonical question format, and one shared serializer configuration is precisely what
  this ADR's Pros section asked for.
- Dependency direction is unchanged in spirit and widened in practice:
  API → Domain + Infrastructure, Client → Domain, L0 → Domain, L1 → Domain + Infrastructure + API,
  Domain → nothing.
- The single test project is split by test level (ADR-032), so the grader is still tested against
  a framework-free library with no web or WASM toolchain — the benefit this ADR was written for.
