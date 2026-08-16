# ADR-014: Custom CSS only — no Bootstrap or utility framework

**Status:** Accepted
**Date:** 2026-08-09

## Problem

Bootstrap was pulled from a CDN for its responsive grid. Bootstrap defines a `.progress` rule that silently overwrote our progress-counter element with a blank bar. Class-name collision, no warning, no build error.

## Considered

- **Keep Bootstrap, rename our class** — smallest change. Any Bootstrap update can collide again on a different name, and nothing warns.
- **Keep Bootstrap, namespace every one of our classes** — defends against the collision class permanently. Verbose, and still ships ~50 KB for a grid we barely use.
- **Remove Bootstrap, hand-write the CSS** — more work upfront, no collision surface, full theme control.

## Decision

**Remove Bootstrap. Hand-written CSS only, one stylesheet as the single source of style truth.** Theming through CSS custom properties, switched by a data attribute on the root element.

Removing a CSS framework also removes the resets it provided implicitly — the fieldset border reset was the one that bit here. Any framework removal needs its implicit resets made explicit.

Accepted cons:

- Responsive utilities are written by hand (ADR-010).
- No component library to draw on.

## Remarks / Sources

- The standing prohibition on adding Bootstrap, Tailwind or any utility-first CSS framework is enforced as a coding standard, not restated here.
- ADR-010 (responsive layout is hand-written), ADR-022 (components own their CSS)
