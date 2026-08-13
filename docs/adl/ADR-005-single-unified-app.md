# ADR-005: Single unified app for OIJ + voivodeship konkursy

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Two competition types: OIJ (national algorithmics) and custom voivodeship konkursy (broad IT curriculum). One app or two?

## Considered

- **Two separate apps** — clean separation, independent deploy. Doubles hosting, maintenance, deploy pipeline.
- **One app, two tracks** — competition selector at quiz start, shared infrastructure, shared question schema.

## Decision

**One unified app.**

**Pros:**
- Shared codebase, one deploy
- Student can practice both without switching apps
- Shared question schema (ADR-011) works for both

**Cons:**
- Schema must accommodate both competition types — more complex than OIJ-only
- UI needs competition/track selector

## Remarks / Sources

- OIJ: multi-select + short-answer, strict grading, code language toggle (Python/C++)
- Custom konkursy: adds single-select ABCD, True/False multi-statement, ordering, matching, partial credit
- Śląskie voivodeship is widest question-type superset — its types drive schema design
- Research synthesis: `c:\Repositories\py-oij-quizzer\olympiads\custom\research\research-synthesis.md`
