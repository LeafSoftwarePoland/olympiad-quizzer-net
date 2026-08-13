# ADR-001: Blazor WASM as frontend framework

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Need frontend framework. Current prototype in Flask (Python). App requires interactive elements (ordering, matching questions). Developer's main language is C#. Long-term maintainability priority.

## Considered

- **Flask (Python, current)** — working prototype, fast start. No compile-time safety. Grows messy for complex interactive UIs. Interpreted — errors only at runtime.
- **Blazor Server** — full C#, server renders HTML, WebSocket for interactivity. Per-connection ~250–500 KB RAM. Stateful — restart kills sessions. Cold start = blank page for 30–60s.
- **Blazor WASM** — C# compiled to WebAssembly, runs in browser. Stateless. Static files on CDN. ~3–4 MB first-load (compressed, cached after).
- **TypeScript + React/Vue** — strong typing, large ecosystem. Separate language from backend. Two codebases to maintain.

## Decision

**Blazor WASM.**

**Pros:**
- C# throughout — compile-time errors, familiar tooling
- Stateless — no per-user server RAM
- Static output — GitHub Pages CDN, no server in Phase 1
- PWA-capable (ADR-018)
- Blazor component model handles drag-and-drop natively

**Cons:**
- ~3–4 MB first load (cached after first visit, mitigated by loading screen)
- Some browser API calls need JS interop
- WASM cold load slower than server-rendered HTML on very slow connections

## Remarks / Sources

- .NET 8 + PublishTrimmed + Brotli: ~3–4 MB total transfer
- Previous Flask prototype: `c:\Repositories\py-oij-quizzer`
- Re-investigate if: project needs SSR for SEO, or team unfamiliar with WASM

## Amendment — 2026-08-09 — POC gotchas

**Adds:** Runtime traps confirmed in POC.

- **Bootstrap CDN: don't use.** Bootstrap `.progress` overwrote our text counter. Removed entirely → ADR-023.
- **Sticky footer: flex on `#app`, not `body`.** Blazor renders `main` into `div#app` (child of body), not directly into body.
- **`Nav.NavigateTo("/")`**: navigates to domain root on GitHub Pages. Use `Nav.BaseUri`.
- **JS interop confirmed needed:** localStorage (theme persist), `document.documentElement` mutation (font size, `data-theme`), `FocusAsync` on element refs.
- **`GetFromJsonAsync` can silently fail** before component fully initialises. Use `Http.GetAsync` + explicit status check instead.
