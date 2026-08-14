# ADR-001: Blazor WASM as frontend framework

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Need a frontend framework. Prototype was Flask/Python. App needs interactive question types (ordering, matching). Maintainer's language is C#. Long-term maintainability over fast start.

## Considered

- **Flask (Python, the prototype)** — works today. No compile-time safety; errors surface at runtime only; grows messy on complex interactive UI.
- **Blazor Server** — full C#, server renders. Per-connection RAM, stateful, restart kills live sessions, cold start shows a blank page.
- **Blazor WASM** — C# compiled to WebAssembly, runs in the browser. Stateless, publishes as static files. Multi-MB first load.
- **TypeScript + React/Vue** — large ecosystem, strong typing. Second language, two codebases to maintain.

## Decision

**Blazor WASM.**

C# on both ends means a wire-format disagreement is a compile error, not a browser surprise. Stateless output makes static hosting viable (ADR-004) and costs no per-user server RAM.

Accepted cons:

- Multi-MB first load. Cached after first visit, masked by a loading screen.
- Browser APIs need JS interop: `localStorage`, root-element mutation for theme and font size, focus management.
- Slower cold load than server-rendered HTML on slow connections.

## Remarks / Sources

- Confirmed runtime traps: base href must carry the hosting sub-path with a trailing slash or the router 404s on a direct URL (ADR-004); the app shell's sticky footer needs flex on the app root element, not `body`, because the framework renders into a child of `body`; navigate via the injected base URI, never a literal `/`; the typed JSON convenience fetch can fail silently before a component finishes initialising, so issue the request and check status explicitly.
- Predecessor prototype: `c:\Repositories\py-oij-quizzer`
- Revisit if server-side rendering for SEO becomes a requirement.
