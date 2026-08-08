# ADR-006: GitHub Pages for WASM static hosting

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Where to host Blazor WASM static files (HTML, WASM runtime, CSS, images, JSON data).

## Considered

- **GitHub Pages** — free, CDN-backed, no card, built into GitHub repo, never sleeps
- **Netlify / Vercel** — free tiers, better build UX, but card may be required
- **Fly.io** — card required; eliminated (ADR-007)
- **Self-hosted** — ops burden; overkill

## Decision

**GitHub Pages.**

**Pros:**
- Free permanently, no card
- CDN-backed — fast globally
- GitHub Actions built-in: push → `dotnet publish` → deploy in one workflow
- Never sleeps — no cold start for UI layer

**Cons:**
- 1 GB repo size soft limit — images may grow; mitigated by external image hosting if needed (ADR-010)
- 100 GB/month bandwidth soft limit — sufficient for low-traffic
- Static only — no server-side logic (expected by design)

## Remarks / Sources

- Deploy workflow: `dotnet publish` → copy `wwwroot/` to `gh-pages` branch
- GitHub bandwidth limits are soft — GitHub contacts before cutting off
- Future: Cloudflare R2 if images grow large (10 GB free, zero egress cost)
