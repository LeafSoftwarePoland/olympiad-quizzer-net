# ADR-016: Responsive design — mobile, tablet, desktop

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Students may use school PCs, home laptops, tablets, or phones. App must work on all form factors.

## Considered

- **Desktop-only** — simpler build. School PCs assumed. Risk: students practice at home on phones, app breaks.
- **Responsive from day one** — Bootstrap 5 included in Blazor WASM default template handles ~90% automatically. Remaining work: touch targets, ordering widget, code block overflow.

## Decision

**Responsive from day one. Mobile + tablet + desktop.**

Cost if done during initial build: ~0.5–1 dev-day.
Cost if retrofitted: 2–3× more expensive.

Specific requirements:
- Ordering / drag-and-drop: pointer events (not mouse-only) for touch support
- Code blocks: horizontal scroll within container, never page-level overflow
- ABCD tap targets: minimum 44×44 px (WCAG AA recommendation)
- Font sizes: minimum 16px body text on mobile

**Pros:**
- Students can practice on any device
- No separate mobile build or app store
- Bootstrap 5 grid handles layout with minimal custom CSS

**Cons:**
- Touch-friendly ordering widget requires more thought than desktop drag-and-drop

## Remarks / Sources

- Bootstrap 5: https://getbootstrap.com/
- Pointer Events API (touch + mouse + stylus unified): https://developer.mozilla.org/en-US/docs/Web/API/Pointer_events
- WCAG tap target: https://www.w3.org/WAI/WCAG21/Understanding/target-size.html
