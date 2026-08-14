# ADR-011: PWA / Service Worker — deferred

**Status:** Deferred
**Date:** 2026-08-08

## Problem

Should the app work offline after first load, and be installable to a home screen?

## Considered

- **Ship a PWA now** — the framework template generates the service worker and manifest; roughly half a dev-day to wire up. Offline after first load, installable.
- **Defer** — ship online-only, add once the question bank and the delivery path are stable.

## Decision

**Deferred.**

Two reasons, the second decisive:

1. The question bank is not stable yet, so caching it would cache content that is still changing.
2. Questions are filtered server-side (ADR-013). Offline mode therefore requires **duplicating the filtering logic client-side** and designing a caching strategy for the whole bank. That is a significant feature, not a template flag.

Accepted cons:

- No offline use, no home-screen install, no faster repeat loads until this is revisited.

## Remarks / Sources

- Active-quiz caching in the browser (ADR-016) is **not** an offline solution — it covers one session's payload, nothing else.
- Revisit once the bank is stable and a full-bank caching strategy is designed. Adding the service worker itself remains additive; the filtering duplication is the real cost.
