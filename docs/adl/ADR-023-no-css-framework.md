# ADR-023: Custom CSS only — no Bootstrap or utility framework

**Status:** Accepted
**Date:** 2026-08-09

## Problem

Bootstrap CDN included for responsive grid. Bootstrap defines `.progress { height:1rem; background:#e9ecef }` — silently overwrote our `.progress` text counter with a white bar. Class name collision, no warning.

## Considered

- **Keep Bootstrap, rename our class** — fragile; any Bootstrap update can re-collide.
- **Remove Bootstrap, hand-roll CSS** — more work upfront, full control.

## Decision

Remove Bootstrap. Custom CSS only in `wwwroot/css/app.css`. Custom properties (`--bg`, `--accent`, `--text`, etc.) for theming.

**Pros:** No collisions. No ~50KB CDN payload. Full theme control.  
**Cons:** Must write responsive utilities by hand — done.

## Rule

Never add Bootstrap, Tailwind, or other utility-first CSS CDN to this project. `app.css` is the single source of style truth. If a missing utility is needed, add it there.

**Also fixed on Bootstrap removal:** `fieldset { border: none }` (Bootstrap provided implicitly). Add explicit reset in `app.css` when removing any CSS framework.
