# ADR-025: Quiz session persisted in localStorage

**Status:** Accepted
**Date:** 2026-08-12

## Problem

Quiz state is lost on browser refresh. For exam-simulation mode this is punishing — a timer question answered correctly is lost, and the timer resets to full.

## Considered

- **In-memory only (current POC)** — simplest. Refresh = restart. Unacceptable in timed mode.
- **sessionStorage** — survives refresh, lost on tab close. Insufficient for timer continuity across OS-level tab restore.
- **localStorage** — survives refresh AND tab close AND browser restart. Sufficient for exam simulation.
- **Server-side session** — requires auth or session ID. Rejected (ADR-015, no accounts).

## Decision

**localStorage.** Quiz session state for the active session only.

What is cached:
- Full quiz payload: up to 30 questions, their answers, explanations, and images
- Timer start timestamp (wall-clock strict — remaining = limit − (now − start))
- Current question index + all given answers so far
- Questions are shuffled once by the API; client caches in that order, iterates sequentially

Behaviour:
- Navigate away mid-quiz → return → resume from cached question. Not treated as session end.
- Session ends when: time expires OR user explicitly cancels → summary screen auto-shown → on leaving summary, cache cleared.
- Applies only to exam simulation mode. Free-learning modes are not timed by default.
- Timer is non-pausable. Refreshing the page does not buy extra time.

Security posture: user CAN edit localStorage. Acceptable — worst case is cheating on a personal practice run, which harms nobody. Hard requirement: everything read from localStorage must be sanitized and validated before use. No edited value may put the app into a broken or exploitable state.

No export/import of settings or state. Deliberate — importing user-provided JSON is an attack surface with no meaningful benefit.

**Pros:**
- Exam timer survives refresh — core exam simulation requirement
- No server round-trip for state
- Additive to existing in-memory model

**Cons:**
- User can inspect and edit state (acceptable — self-practice tool, no stakes)
- localStorage quota (~5 MB) limits image payload — images are small enough in practice

## Remarks / Sources

- ADR-015 (no accounts) — server-side session not viable
- Session cache: questions + images + answers + explanations. Images from API response.
- State invalidation: cache cleared on leaving the summary page, or on starting a new quiz.
- Resolves assumption A-11 from `.pipeline/1-architecture/assumptions.md`.
