# ADR-016: Quiz session persisted in browser storage

**Status:** Accepted
**Date:** 2026-08-12

## Problem

Quiz state is lost on browser refresh. In timed exam-simulation mode that is punishing: correct answers vanish and the timer resets to full.

## Considered

- **In-memory only** — simplest. Refresh restarts the quiz. Unacceptable in timed mode.
- **Session storage** — survives refresh, lost on tab close. Insufficient for timer continuity across an OS-level tab restore.
- **Local storage** — survives refresh, tab close and browser restart. Sufficient.
- **Server-side session** — requires identity, which ADR-009 forbids.

## Decision

**Browser local storage, active session only.**

Cached: the full quiz payload (up to 30 questions with answers, explanations and image references), the timer start timestamp, the current question index, and every answer given so far.

Behaviour:

- Questions are shuffled once by the server (ADR-025); the browser caches that order and iterates it sequentially.
- Timer is wall-clock strict: remaining = limit − (now − start). Non-pausable. Refreshing buys no extra time.
- Navigating away mid-quiz and returning resumes at the cached question. Not treated as session end.
- Session ends when time expires or the user cancels. The summary screen is shown, and the cache is cleared on leaving it or on starting a new quiz.
- Applies to exam-simulation mode. Free-learning modes are untimed by default.

Accepted cons:

- The user can inspect and edit the stored state. Acceptable: this is a self-practice tool with no stakes, and the worst outcome for someone who edits it is that they cheat themselves.
- Storage quota (~5 MB) bounds the cached payload. Sufficient at 30 questions.

## Remarks / Sources

- Validation and discard-on-tamper requirements for everything read from browser storage are enforced as a security coding standard, as is the prohibition on state import/export. Not restated here.
- ADR-025 (server fixes the order; an empty result must not start a quiz), ADR-009 (no server-side session), ADR-021 (long-term progress tracking is a separate, undecided question)
