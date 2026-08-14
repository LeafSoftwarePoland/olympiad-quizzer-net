# ADR-009: No user accounts

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Should the app identify individual students and persist their scores and progress server-side?

## Considered

- **Full auth and accounts** — login, per-student progress, teacher dashboard. Requires a database, GDPR handling, email verification, session management, a privacy policy.
- **No accounts** — stateless tool. A quiz runs, results are shown, nothing is persisted server-side.

## Decision

**No accounts.**

The app stores no personal data, so there are no GDPR obligations, no consent flow and no retention policy to write. Registration friction is removed for the target audience.

Accepted cons:

- No cross-session progress tracking server-side. Browser-side only, and lost on cache clear (ADR-021).
- No per-student analytics, no teacher dashboard.
- Server-side quiz session is not viable, so session state lives in the browser (ADR-016).

## Remarks / Sources

- Trigger to revisit: a teacher asks "how did my students do?", or analytics become necessary.
- Adding identity later does not require rewriting quiz logic; it requires the GDPR work listed above.
