# Solution Design — olympiad-quizzer-net (Phase 1 POC)

**Weight class**: S — this doc is ~1 page by design. Sprint-backlog is the implementation plan (no Sprint Planner).
**Date**: 2026-08-08
**Author**: Architect
**Scope**: POC only. Mock content. Goal = prove stack + prove Render.com costs $0.

ADRs are the decision record. This doc wires them together — no ADR content repeated.

---

## 1. Overview

Two deployables, no shared runtime, no DB, no state.

```
Browser
 └─ Blazor WASM  (GitHub Pages, /olympiad-quizzer-net/)     ← ADR-001, ADR-006
      │ HTTPS GET /api/questions   (CORS)
      └─ ASP.NET Core minimal API  (Render.com, Docker)      ← ADR-007, ADR-020
           └─ Data/questions.json  (embedded, 6 mock rows)   ← ADR-011
```

Grading happens **client-side**. API is a dumb JSON server. Answers ship to browser — accepted (ADR-002 rationale: public olympiad material, practice tool).

## 2. Components

| Component | Project | Responsibility |
|---|---|---|
| `OlympiadQuizzer.Shared` | `source/shared/` | `Question`, `ContentBlock`, `QuizFilter`, `QuestionType`, `Grader`, `GradeResult`. No I/O. Referenced by Api + Client + Tests (ADR-021) |
| `OlympiadQuizzer.Api` | `source/api/` | Minimal API. `GET /healthz`, `GET /api/questions`. CORS. Loads `Data/questions.json` once at startup |
| `OlympiadQuizzer.Client` | `source/client/` | Blazor WASM. Pages + per-type question components + terminal CSS |
| `OlympiadQuizzer.Tests` | `source/tests/` | xUnit. L0 grader/model tests + L1 API endpoint tests |

Client depends on API only through `IQuestionRepository` (ADR-003). POC impl = `ApiQuestionRepository`. Phase 2 swap = one DI line.

## 3. Data flow

1. WASM boot → `wwwroot/appsettings.json` → `ApiBaseUrl` (GHA injects `RENDER_API_URL` at publish).
2. User clicks "Rozpocznij quiz" → `Quiz.razor` → `IQuestionRepository.GetAsync(QuizFilter.None)` → `HttpClient` GET `{ApiBaseUrl}/api/questions`.
3. Questions cached in a scoped `QuizSession` service (in-memory, lost on refresh — acceptable for POC).
4. Per question: render → user answers → "Sprawdź" → `Grader.Grade(question, answer)` → verdict shown (`aria-live="polite"`) → "Dalej".
5. After Q6 → `Result.razor` reads `QuizSession` → score + breakdown table.

No timer (POC). No accounts (ADR-015). No `manifest.json` version indirection — ADR-009 applies to Phase 2 static-content delivery; POC serves questions from the API and needs no cache-busting layer.

## 4. Key interfaces

```csharp
public interface IQuestionRepository { Task<List<Question>> GetAsync(QuizFilter filter); }   // ADR-003

public static class Grader                                                                   // ADR-021
{
    public static GradeResult Grade(Question q, AnswerSubmission a);
}

public sealed record GradeResult(bool IsCorrect, double PointsAwarded, double MaxPoints);
```

`AnswerSubmission` is one type carrying the union of shapes (`int[] SelectedIndices`, `string Text`, `bool?[] Booleans`, `int[] Order`, `int[] Matches`). One shape populated per type; grader switches on `Question.Type`. Chosen over per-type answer classes: 6 types, POC, no polymorphic dispatch worth the ceremony.

Full schema: **ADR-011**. POC field bindings for `trueFalse` / `matching`: **ADR-022** (statements and left-column both live in `options`).

## 5. Error handling

| Failure | Behaviour |
|---|---|
| API unreachable / non-2xx / timeout | `Quiz.razor` shows Polish error panel + "Spróbuj ponownie" button. No retry loop, no exponential backoff |
| Render cold start (up to ~60 s, free tier spin-down after 15 min idle) | Loading state text: `Budzenie serwera… może potrwać do minuty.` `HttpClient.Timeout = 90 s` |
| Malformed / missing `questions.json` at API startup | API fails fast at startup — deploy visibly fails rather than serving empty quiz |
| Zero questions returned | Error panel, same as unreachable |
| Unknown `type` value in payload | Deserialize to `QuestionType.Unknown`; `QuestionRenderer` renders a Polish "nieobsługiwany typ pytania" placeholder instead of throwing |

No global exception middleware in API (nothing to swallow). No Polly. No circuit breaker. POC.

## 6. Observability

Weight class S + POC → deliberately thin:

- **API**: default ASP.NET Core console logging (`Information`). Render captures stdout. `GET /healthz` → `{"ok":true}` is the liveness signal, also polled by `deploy-backend.yml` and (optionally) UptimeRobot per ADR-007.
- **Client**: browser console via `ILogger` on fetch failure only.
- **No** App Insights, no OTel, no metrics, no structured logging sinks. Add when real content ships.

## 7. Security

- No auth, no accounts, no secrets in app code (ADR-015).
- CI secrets by name only: `RENDER_API_URL`, `RENDER_DEPLOY_HOOK` (GitHub Actions repo secrets).
- CORS: explicit allow-list — `https://leafsoftwarepoland.github.io` + any `localhost` origin via predicate (ASP.NET Core has no port wildcard; see sprint-backlog T-04).
- API is read-only. No write endpoints, no user input reaches the server.
- HTTPS enforced by both hosts. No `UseHttpsRedirection()` in-container (Render terminates TLS at the edge; redirect would break health checks).

## 8. Non-functional

| Property | Target | Note |
|---|---|---|
| Availability | best-effort | Free tiers. Pages never sleeps; Render sleeps at 15 min idle |
| Cold start | ≤ 60 s backend | Accepted, surfaced in UI copy |
| Latency (warm) | < 300 ms `/api/questions` | 6 rows, in-memory |
| Payload | WASM first load ~3–4 MB Brotli (ADR-001); questions < 10 KB | Cached after first visit |
| Scale | 1 concurrent user | POC. 512 MB / 0.1 CPU Render free instance |
| Responsive | 360 px → desktop, 44×44 px tap targets | ADR-016 |
| Accessibility | WCAG 2.1 AA on quiz components | ADR-017 |
| Language | UI Polish, code English | ADR-019 |

## 9. ADRs this design leans on

ADR-001 (Blazor WASM) · ADR-003 (IQuestionRepository) · ADR-006 (GitHub Pages) · ADR-007 (Render.com) · ADR-011 (schema) · ADR-015 (no accounts) · ADR-016 (responsive) · ADR-017 (ARIA) · ADR-019 (language) · **ADR-020** (POC ships thin API — amends ADR-002 for POC scope) · **ADR-021** (shared class library) · **ADR-022** (POC schema field bindings).

Deferred, not used in POC: ADR-004 (Dapper), ADR-009 (versioned JSON), ADR-010 (images), ADR-012 (explanations), ADR-018 (PWA).

## 10. Sources

- Design spec: `docs/specs/2026-08-08-olympiad-quizzer-poc-design.md`
- Product brief: `.pipeline/0-vision/product-brief.md`
- CSS being ported: `c:\Repositories\py-oij-quizzer\python\static\css\style.css`
- Render free tier / port / deploy hooks: https://render.com/docs/free , https://render.com/docs/environment-variables , https://render.com/docs/deploy-hooks (verified 2026-08-08)
- GitHub Pages + Actions deployment: see `sprint-backlog.md` T-08 sources
