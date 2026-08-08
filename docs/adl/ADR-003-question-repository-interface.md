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
