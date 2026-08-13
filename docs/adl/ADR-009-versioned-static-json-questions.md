# ADR-009: Questions as versioned static JSON

**Status:** Accepted
**Date:** 2026-08-08

## Problem

How to serve questions to WASM client and handle updates without stale cache serving wrong answers.

## Considered

- **Single `questions.json`** — simple. Browser may cache old version until manual cache clear.
- **Versioned filenames** (`questions-v3.json`) + `manifest.json` pointer — client reads manifest first, fetches correct version.
- **Cache-busting query string** (`questions.json?v=date`) — CDNs may not cache query-string URLs consistently.

## Decision

**`manifest.json` (always network-fresh) + versioned `questions-v<N>.json` filenames.**

```json
// manifest.json — fetched network-first on every app start
{ "questionsVersion": "2026-08-08", "questionsFile": "questions-v3.json" }
```

Cache strategy (applied now via HTTP headers, enforced later via Service Worker in ADR-018):

| File | Strategy |
|---|---|
| `manifest.json` | network-first |
| `questions-v*.json` | cache-forever (new filename = new file) |
| Images | cache-first |

**Pros:**
- Instant propagation when questions change — new filename, no cache ambiguity
- Old version cached but unreferenced — safe
- Works correctly offline (last-known version)

**Cons:**
- Old question files accumulate until Service Worker quota clears them
- Two fetches on app start (manifest + questions file)

## Remarks / Sources

- Related: ADR-018 (PWA / Service Worker for offline enforcement)

## Amendment — 2026-08-12 — server-side filtering supersedes static-JSON delivery

**Overrides:** Entire delivery strategy.

Questions are no longer served as a static JSON file from `wwwroot`. Server-side filtering in the API handles question delivery: client sends filter parameters + requested count, API returns a random subset. Versioned filenames and `manifest.json` approach is no longer used for question delivery.

Cache strategy for the active quiz: full quiz payload (questions, answers, explanations, images) is cached in `localStorage` for the duration of the active session. Images come from the API alongside questions, not as separately fetched static files.

The manifest.json + versioned file approach may still apply to WASM app shell assets, but not to question data.
