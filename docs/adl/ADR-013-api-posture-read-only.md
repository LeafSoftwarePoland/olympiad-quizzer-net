# ADR-013: API posture — read-only, stateless, grading in the browser

**Status:** Accepted
**Date:** 2026-08-12

## Problem

The app ships a backend. What is that backend allowed to be? Left open, it accrues auth, an admin surface, write endpoints and a session store, none of which any requirement asks for.

## Considered

- **Serve the whole bank, filter in the browser** — no endpoint logic. The client downloads every question to show 30, and answers for the whole bank leave the server on first load. Rejected.
- **Filter server-side, grade server-side** — answers never reach the browser. Requires a session, so it requires identity (ADR-009 forbids it), and grading is pure logic that runs fine in WASM. Rejected.
- **Filter server-side, grade in the browser** — answers travel with the questions; the server holds nothing between requests.

## Decision

**Read-only, stateless API. Server filters; the browser grades.**

- Server-side filtering returns a random subset matching the caller's filters (ADR-025). The client never downloads the full bank.
- The response carries the **full** payload including correct answers and explanations, because grading is client-side.
- The API holds no session, no per-user state, and has **no write endpoints**.
- Scope guard, explicit: no auth, no admin surface, no caching layer, no rate limiting. Adding any of them is an amendment, not a PR.
- Server unreachable means a graceful error screen. No fallback data source (ADR-002).

Accepted cons:

- Answers are visible to anyone who opens the network tab. The API buys no answer secrecy and was never meant to — the content is public olympiad material, and the worst outcome is a student cheating themselves.
- The frontend has a hard runtime dependency on a service that sleeps (ADR-005), so cold start must be handled in the UI (ADR-019).
- Two deployables instead of one.

## Remarks / Sources

- ADR-025 (filter contract), ADR-016 (the browser caches the returned order for the active session), ADR-020 (one concurrent user — the reason no caching layer or rate limiting is needed)
