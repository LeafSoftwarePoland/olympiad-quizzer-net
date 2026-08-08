# ADR-018: PWA / Service Worker — deferred

**Status:** Deferred
**Date:** 2026-08-08

## Problem

Should app work offline after first load (Progressive Web App)?

## Considered

- **Phase 1 PWA** — `dotnet new blazorwasm --pwa` flag generates Service Worker + `manifest.json`. Offline after first load. Students can install to home screen. ~0.5 dev-day to wire up.
- **Deferred** — ship online-only first, add when app is stable and question data is settled.

## Decision

**Deferred.** Ship Phase 1 online-only. Add PWA after initial stable release.

Architecture is already compatible — static JSON + images in `wwwroot` = cacheable by design. Adding Service Worker is purely additive, no changes to app logic.

Cache strategy when implemented:
- WASM runtime + app shell → `cache-forever` (content-hashed filenames auto-invalidate)
- `manifest.json` → `network-first`
- `questions-v*.json` → `cache-forever` (per ADR-009)
- Images → `cache-first`

PWA also enables:
- "Install to home screen" on mobile/desktop
- Works on school bus without WiFi
- Faster repeat loads (all assets from local cache)

## Remarks / Sources

- `dotnet new blazorwasm --pwa`: adds `service-worker.js` + `service-worker.published.js` scaffolding
- Workbox integration optional for more complex cache strategies
- Re-activate once: initial question bank is stable, app tested online
