# ADR-003: Single unified app for OIJ and voivodeship konkursy

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Two competition families: OIJ (national algorithmics) and voivodeship konkursy przedmiotowe (broad IT curriculum). Different question types, grading rules and metadata. One app or two?

## Considered

- **Two separate apps** — clean isolation, independent deploy. Doubles hosting, maintenance and deploy pipeline for one maintainer.
- **One app, two tracks** — competition selected at quiz start; shared schema, shared infrastructure.

## Decision

**One unified app.** One codebase, one deploy, one question schema (ADR-007) covering both families. A student practises both without switching tools.

Accepted cons:

- Schema must accommodate both families, so it is broader than OIJ alone requires.
- UI needs a competition/track selector.

## Remarks / Sources

- OIJ needs: multi-select, short answer, strict grading, Python/C++ code toggle.
- Voivodeship konkursy add: single-select, true/false over multiple statements, ordering, matching, partial credit.
- Śląskie voivodeship is the widest question-type superset, so its types drive the schema.
- Research synthesis: `c:\Repositories\py-oij-quizzer\olympiads\custom\research\research-synthesis.md`
