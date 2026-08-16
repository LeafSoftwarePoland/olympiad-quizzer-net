# ADR-010: Responsive and accessible from day one

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Students use school PCs, home laptops, tablets and phones, and some use screen readers. Build for that now, retrofit later, or skip?

## Considered

- **Desktop-only, sighted-only** — cheapest build. Breaks for students practising at home on a phone, and excludes students with visual impairment.
- **Retrofit later** — deferred cost. Both responsive layout and accessible markup cost 2–3× more to add after components exist, because both are properties of the markup itself.
- **Build both during initial component work** — roughly one dev-day for responsive plus ~4h for accessibility, spread across component work.

## Decision

**Both built during initial component work. Mobile, tablet and desktop; screen-reader usable throughout.**

Retrofit is 2–3× the cost for the same result, and Polish public-sector digital accessibility law applies to this content category.

Load-bearing targets that constrain component design:

- Tap targets minimum 44×44 px; body text minimum 16 px on mobile.
- Code blocks scroll horizontally **inside their container**, never at page level.
- Pointer events, not mouse-only handlers, so ordering and matching work on touch.
- Ordering and matching need a keyboard interaction path, not only drag-and-drop.
- WCAG 2.1 AA for quiz content.

Per-component requirements — accessible names, roles, live regions, focus management, label association, visible focus — are enforced as a coding standard, not restated here.

Accepted cons:

- Touch-friendly ordering and matching widgets need more design than desktop drag-and-drop.
- Keyboard reorder is non-trivial, though a documented pattern exists.

## Remarks / Sources

- WCAG 2.1 AA: https://www.w3.org/TR/WCAG21/
- Tap target size: https://www.w3.org/WAI/WCAG21/Understanding/target-size.html
- Keyboard reorder pattern: https://www.w3.org/WAI/ARIA/apg/patterns/listbox/
- Polish law: Ustawa z dnia 4 kwietnia 2019 r. o dostępności cyfrowej stron internetowych i aplikacji mobilnych podmiotów publicznych
- Pointer Events API: https://developer.mozilla.org/en-US/docs/Web/API/Pointer_events
- Responsive layout is hand-written CSS (ADR-014), not a framework grid.
- Image `alt` text is mandatory and content-bearing (ADR-007).
