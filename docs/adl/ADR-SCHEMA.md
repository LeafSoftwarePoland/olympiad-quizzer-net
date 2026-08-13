# ADR Schema Reference

Template and rules for ADR authors in this repo.

## File naming

`docs/adl/ADR-NNN-kebab-title.md` — three-digit number, lowercase kebab title. Register in `INDEX.md` in the same commit.

## Required fields (header block)

```
# ADR-NNN: Title

**Status:** Accepted | Deferred | Superseded | Shell
**Date:** YYYY-MM-DD
```

Optional header fields: `**Amends:**`, `**Clarifies:**`, `**Updated:**` (for in-body corrections only — not amendments).

## Required sections

| Section | Content |
|---|---|
| `## Problem` | What question this decision answers. One paragraph max. |
| `## Considered` | Options evaluated, with a one-line verdict on each. |
| `## Decision` | The choice and its rationale. May include sub-sections for details. |
| `## Remarks / Sources` | Links, cross-references, follow-up risks. May be empty ("_None._"). |

## Optional section — Amendment

Append after `## Remarks / Sources`. Never edit the original body.

```
## Amendment — YYYY-MM-DD — one-line reason

**Overrides:** <section or field name> — what changed.
**Adds:** <topic> — net-new information.

- Bullet per changed or added point. Caveman-terse.
```

Rules:
- Multiple amendments = multiple sections, newest last.
- `**Overrides:**` for anything that changes existing text. `**Adds:**` for net-new facts.
- One section per date/reason. Never combine unrelated amendments.
- New ADR preferred over amendment when: decision reverses entirely, new technology, or amendment length exceeds original.

## Tone

Caveman-terse. One line per point. No filler. Prose only where a table or list would be worse.

## Status values

| Status | Meaning |
|---|---|
| `Accepted` | Live decision. Follow it. |
| `Deferred` | Intentionally not decided. Will revisit. |
| `Superseded` | Replaced by a newer ADR. See amendment or successor. |
| `Shell` | Problem captured; decision not yet made. Fill in Considered + Decision when ready. |

## Example

See [ADR-001](ADR-001-blazor-wasm-frontend.md) — the cleanest complete example in this ADL.
