# ADR-003: IQuestionRepository abstraction

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Phase 1 uses static JSON. Phase 2 adds API + DB. Without abstraction: WASM code must change when backend added. Risk of widespread refactor.

## Considered

- **Direct HttpClient calls** throughout — no indirection. Simple. Coupling to JSON URL baked everywhere.
- **IQuestionRepository interface** — one seam. Swap implementation via DI config. WASM component code unchanged.

## Decision

**IQuestionRepository interface from day one.**

```csharp
public interface IQuestionRepository
{
    Task<List<Question>> GetAsync(QuizFilter filter);
}

// Phase 1: fetches questions.json from CDN
public class JsonQuestionRepository : IQuestionRepository { ... }

// Phase 2: calls /api/quiz endpoint
public class ApiQuestionRepository : IQuestionRepository { ... }

// Server-side (Phase 2):
public class SqliteQuestionRepository : IQuestionRepository { ... }
```

**Pros:**
- One DI config change to swap backends
- Components stay unchanged across phases
- Testable in isolation

**Cons:**
- One extra interface + class per backend (minimal overhead)

## Remarks / Sources

- Related: ADR-002 (Phase 1 JSON), ADR-004 (Dapper), ADR-015 (accounts)

## Amendment — 2026-08-13 — abstraction shape changes for v1.0; seam itself unchanged

**Overrides:** the abstraction's shape as sketched in the Decision section.

- The single query operation now takes a **structured query object** (categories, algorithms,
  years, stages, count) instead of the POC filter type, and accepts a cancellation token.
  Reason: server-side filtering (ADR-035) needs multi-value predicates per tag type; the POC
  filter type carried single scalars and no tag axes.
- A **second operation** is added, returning the filter values actually present in the bank with
  a count each. Reason: the UI must not offer a filter for data that does not exist (F-04), and
  it cannot know what exists without asking.
- The POC filter type is deleted. The query object lives in the Domain project alongside the
  abstraction.

**Adds:** the seam is now implemented on **both** sides.

- Server side: the JSON-file implementation in the Infrastructure project (SQLite later, ADR-004).
- Client side: an HTTP implementation inside the frontend project.
- Both implement the **same** Domain abstraction. This is the payoff this ADR predicted — one
  definition, two implementations, and a schema disagreement across the wire becomes a compile
  error rather than a runtime surprise in the browser.
- The static-JSON fallback implementation named in the original Decision is **dropped**
  (ADR-020 amendment): server down means a graceful error page, not a fallback data source.
- Structural note: the abstraction moves from the POC shared library to the Domain project
  (ADR-032). It does **not** move into a frontend feature folder (ADR-031) — one abstraction,
  one home.
