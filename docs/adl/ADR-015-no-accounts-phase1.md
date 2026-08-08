# ADR-015: No user accounts in Phase 1

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Should app track individual students, scores, progress from day one?

## Considered

- **Full auth + accounts** — ASP.NET Identity, login, progress tracking, per-student analytics, teacher dashboard. Requires backend, DB, GDPR handling, email verification, session management.
- **No accounts** — stateless tool. Quiz runs, results shown in-session, nothing persisted. No backend needed (ADR-002).

## Decision

**No accounts in Phase 1.**

**Pros:**
- No backend, no ops (enables Phase 1 static-only architecture)
- No GDPR obligations (no personal data stored)
- Faster to build and ship
- Students can use without registration friction

**Cons:**
- No progress tracking between sessions
- No per-student analytics
- No teacher dashboard

## Remarks / Sources

- ASP.NET Identity seam preserved: adding it in Phase 2 requires no rewrite of quiz logic
- Phase 2 trigger: teacher asks "how did my students do?" or analytics become necessary
- GDPR: when accounts added, need privacy policy, data retention policy, consent flow
