# ADR-017: ARIA accessibility built upfront

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Should app support screen readers for blind/visually impaired students?

## Considered

- **Skip entirely** — zero cost. Excludes students with visual impairment.
- **Retrofit later** — deferred. 2–3× more expensive than adding during initial component build.
- **Add during component build** — ~4h overhead spread across component work. Permanent benefit.

## Decision

**Add ARIA during initial component build.**

Required per component:
- `aria-label` on all form inputs and icon-only buttons
- `role="radiogroup"` + `aria-checked` on ABCD option groups
- `aria-live="polite"` on score/feedback region (announces result to screen reader without focus change)
- `FocusAsync()` or JS interop for focus management after page navigation
- `<label>` elements properly linked to inputs via `for` / `id`

**Pros:**
- ~4h total cost if added during build
- WCAG 2.1 AA compliance for quiz content
- Polish public institution accessibility mandate applies (Ustawa 2019)

**Cons:**
- Ordering/drag-and-drop requires additional keyboard interaction (arrow keys to reorder) — non-trivial but documented pattern exists

## Remarks / Sources

- WCAG 2.1 AA: https://www.w3.org/TR/WCAG21/
- Polish digital accessibility law: Ustawa z dnia 4 kwietnia 2019 r. o dostępności cyfrowej stron internetowych i aplikacji mobilnych podmiotów publicznych
- Keyboard reorder pattern: https://www.w3.org/WAI/ARIA/apg/patterns/listbox/
