# Documentation System

What lives in `docs/` and how to maintain it.

## Document types

| Type | Location | Purpose | Schema |
|---|---|---|---|
| ADR | `docs/adl/` | Record an architectural or product decision with rationale | `docs/adl/ADR-SCHEMA.md` |
| Integration doc | `docs/integrations/` | Document an external service this app talks to at runtime | `docs/integrations/INDEX.md` |
| POC doc | `docs/pocs/` | Standalone proof-of-concept write-up; records experiment setup and findings | No fixed schema |
| Functionality registry | `docs/functionalities.md` | Registry of user-facing features with status and ADR pointers | See file header |
| Glossary | `docs/Glossary.md` | Project-specific terms. Authoritative — do not duplicate in pipeline artifacts. | — |
| Competition rules | `docs/rules/` | Machine-readable + human-readable competition rules per olympiad | `docs/rules/README.md` |
| Architecture guide | `docs/architecture-guide.md` | Layer definitions, test levels, document hierarchy | This file |

## Hierarchy (no circular references)

```
POC docs        ←  standalone, no inbound refs required
ADRs            ←  may reference POC docs
Everything else ←  may reference ADRs
                   (arch guide, functionalities, rules, integrations)
```

Nothing should reference up to functionalities or rules from an ADR — that creates circular dependencies.

## How to maintain

### Adding an ADR
1. Create `docs/adl/ADR-NNN-kebab-title.md` using the schema in `ADR-SCHEMA.md`.
2. Add a row to `docs/adl/INDEX.md`.
3. Commit both files in the same git commit.

### Amending an ADR
1. Append `## Amendment — YYYY-MM-DD — one-line reason` to the ADR file.
2. Update the "Last updated" line and amendment graph in `docs/adl/INDEX.md`.
3. Never edit the original body.

### Adding an integration doc
1. Create `docs/integrations/<service>.md`.
2. Add a row to `docs/integrations/INDEX.md`.
3. Commit both in the same commit.

### Adding a competition rules file
1. Create `docs/rules/<id>.md` following the schema in `docs/rules/README.md`.
2. Add a row to `docs/rules/README.md`.
3. Commit both in the same commit.

### Adding a functionality
1. Add an entry to `docs/functionalities.md`.
2. Add a row to the Changelog section at the bottom.
3. No separate commit requirement — may be part of the feature commit.
