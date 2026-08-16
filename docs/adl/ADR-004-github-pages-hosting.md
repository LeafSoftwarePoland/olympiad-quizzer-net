# ADR-004: GitHub Pages for WASM static hosting

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Where to host the published WASM output: HTML, runtime files, CSS, static assets. Constraint: no payment card.

## Considered

- **GitHub Pages** — free, CDN-backed, no card, built into the repo host, never sleeps.
- **Netlify / Vercel** — better build UX, but a card may be required.
- **Fly.io** — card required. Eliminated.
- **Self-hosted** — ops burden, no benefit at this scale.

## Decision

**GitHub Pages.** Free permanently with no card, CDN-backed, and never sleeps, so the UI layer has no cold start even when the API does (ADR-005).

Accepted cons:

- 1 GB repo soft limit. Images live in the repo (ADR-029); revisit if they approach it.
- 100 GB/month bandwidth soft limit. Sufficient at planned traffic; the host warns before cutting off.
- Static only, no server-side logic. Expected — that is what the API is for.
- The site is served from a path under a shared host, not its own host root. Consequence: crawler rules cannot be published from this repo (ADR-028).

## Remarks / Sources

- Live URL: `https://leafsoftwarepoland.github.io/olympiad-quizzer-net/`
- Base href must be `/olympiad-quizzer-net/` **with** trailing slash, or the router 404s on a direct URL.
- The SDK-install CI step fails on the self-hosted Windows runner (no write access to the global install path). Removed — the SDK is already installed machine-wide.
- Deploys are manual only (ADR-015).
- Future: move to a custom domain if crawler control or a clean host root becomes worth a domain and a DNS change.
