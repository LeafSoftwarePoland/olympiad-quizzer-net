# ADR-020: POC ships a thin API instead of static JSON

**Status:** Accepted
**Date:** 2026-08-08
**Amends:** ADR-002 (for POC scope only)

## Problem

ADR-002 decided: no backend in Phase 1, questions served as static JSON from `wwwroot/data/`. ADR-007 decided Render.com as the *Phase 2* API host, marked **"Accepted (test pending) — must test before relying on it"**.

Conflict: if Phase 1 ships no backend, the Render.com bet stays untested until Phase 2, when the whole content pipeline already depends on it. ADR-007's own escape hatch (ADR-008 Oracle Cloud) becomes expensive to take at that point.

## Considered

- **Follow ADR-002 literally** — questions.json in `wwwroot/`, no API. Simplest POC, fewest moving parts. Leaves ADR-007 unproven and defers the CORS/deploy-hook/cold-start unknowns to the phase that can least afford them. Also leaves `IQuestionRepository` (ADR-003) with only one implementation, so the seam is never exercised.
- **Ship a thin read-only API in the POC** — `GET /healthz` + `GET /api/questions`, questions still a JSON file but read server-side. Costs one extra project, one Dockerfile, one GHA workflow. Proves Render deploy, Docker build, CORS, cold-start UX, and the repository seam in one shot.
- **Ship both** (static JSON fallback + API) — belt and braces. Two code paths to keep in sync for a throwaway POC. Rejected as overengineering.

## Decision

**POC ships the thin API.** ADR-002 remains the standing decision for *content delivery strategy* — questions are still a flat JSON file, not a database, and the API holds no state. What changes is only *who serves the file*.

ADR-002 is **not** superseded: if the POC proves Render unusable (see risks), Phase 1 falls back to ADR-002's static-JSON delivery unchanged, and only `ApiQuestionRepository` → `JsonQuestionRepository` swaps in DI (ADR-003 exists precisely for this).

Scope guard — the POC API does **not** get: database, auth, write endpoints, admin surface, caching layer, rate limiting.

## What this buys

Falsifies or confirms, before real content work starts:

- Render.com free tier accepts a .NET Docker image and stays $0
- Deploy hook + `workflow_dispatch` deploy path works from a self-hosted runner
- CORS GitHub Pages → Render works from a real browser
- Cold-start latency is tolerable to a student, or is not
- `IQuestionRepository` seam actually absorbs a backend swap

**Pros:**
- The riskiest ADR (007) gets tested when it is cheap to be wrong
- Repository abstraction validated with two real implementations, not one
- Deploy plumbing built once, reused in Phase 2

**Cons:**
- POC has two deployables instead of one — more surface to get working before anything is visible
- Frontend now has a hard runtime dependency on a sleeping free-tier service; POC UX must handle cold start explicitly
- Answers still travel to the browser (grading is client-side), so the API buys no answer secrecy — that was never its purpose here

## Remarks / Sources

- ADR-002 (static JSON), ADR-003 (repository seam), ADR-007 (Render, test pending), ADR-008 (Oracle Cloud fallback)
- POC plan and success checklist: `docs/pocs/2026-08-08-olympiad-quizzer-poc-design.md`
- Render free tier limits (512 MB, 0.1 CPU, 750 instance-hours/mo, 15-min idle spin-down, ~1 min spin-up): https://render.com/docs/free (verified 2026-08-08)
- Open risk carried into `assumptions.md`: Render sign-up may require a card-verification hold, which would contradict the "no card" premise of ADR-007. The POC is the test.

## Amendment — 2026-08-12 — grading is client-side; static-JSON fallback dropped; server-side filtering

**Overrides:** "Answers still travel to the browser (grading is client-side)" — this was already the intent, but clarified explicitly. Also overrides the static-JSON fallback premise.

- Grading is UI-side (client) only. Answers are delivered alongside questions in the API response. The API does not grade.
- The static-JSON fallback (`JsonQuestionRepository` swapped in via ADR-003) is dropped. Server down = graceful error page. No fallback to static data.
- Server-side filtering: the API filters questions by tags/categories and returns a random subset. The client does NOT download the full bank. This is a v1.0 decision — not the POC scope.
- ADR-002's static-JSON delivery approach is superseded for question delivery (questions are no longer a static file served to WASM). ADR-002's status changes to Superseded for question delivery; the seam (ADR-003) remains valid.
