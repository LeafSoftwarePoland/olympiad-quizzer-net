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
| Tag vocabulary | `docs/tags.md` | The controlled `category[]` / `algorithms[]` values. Authoritative — the integrity suite validates the bank against it | See file header |
| Architecture guide | `docs/architecture-guide.md` | Layer definitions, test levels, document hierarchy | Self-describing |
| Development guide | `docs/development.md` | Local run, project structure, deploy. English, for contributors — the root `README.md` is Polish and for users | Self-describing |
| Coding standards | `docs/standards/` | How code is written and reviewed. Enforced at every PR. | `docs/standards/INDEX.md` |

`docs/standards/INDEX.md` is a **map, not a substitute** — anyone writing code reads every file it lists, in full.

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
2. Update the "Last updated" line in `docs/adl/INDEX.md`, and the Status column if it changed.
3. Never edit the original body.

A new ADR beats an amendment when the decision reverses entirely, the technology changes, or the amendment would run longer than the original.

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

---

## Change-impact checklist

**Run this after any change that alters how something works, not just what it does.** It exists
because the same omission kept recurring: the code changed, one document was updated, and three
others went on describing the old behaviour. A document that confidently states something untrue is
worse than a missing one — the next reader believes it.

Work top to bottom. Most rows will not apply; the point is to have looked.

| # | Check | Where |
|---|---|---|
| 1 | Does an ADR record the decision you just changed? | `docs/adl/` — append an amendment, never edit the body |
| 2 | Did the ADR's one-line description in the index go stale? | `docs/adl/INDEX.md` — row text, Status, and the "Last updated" line |
| 3 | Does another ADR cite the one you amended? | `grep -rn "ADR-0NN" docs/` — a citation may now point at superseded text |
| 4 | Did a workflow, trigger, runner or permission change? | `docs/integrations/github-actions.md` — the workflow table |
| 5 | Did anything about hosting, the deploy path or an endpoint change? | `docs/integrations/github-pages.md`, `render-com.md` |
| 6 | Is there a user-visible behaviour change? | `docs/functionalities.md` — entry plus a Changelog row |
| 7 | Did a rule, convention or enforced practice change? | `docs/standards/` — and `INDEX.md` if a file was added or its scope moved |
| 8 | Did a layer, project, test tier or dependency direction change? | `docs/architecture-guide.md` |
| 9 | Did a term change meaning, or a new one appear? | `docs/Glossary.md` |
| 10 | Did the local-run steps, project list, or `data/` contents change? | `docs/development.md` |
| 11 | Did a new top-level file or folder appear? | `docs/development.md` structure block, and `.gitignore` / `.dockerignore` if it should not ship |
| 11a | Did anything change that a **user** would notice — privacy, accessibility, what is stored in their browser? | root `README.md` (Polish, user-facing) |
| 12 | Did a config key, path or default change? | `appsettings*.json`, the `.csproj` content items, and every doc that names the old key |
| 13 | Is any test asserting the old behaviour — or now asserting nothing? | A rule change usually needs a test change; a test that cannot fail is the defect to look for |
| 14 | Does a code comment or failure message name the thing you changed? | `grep` for the old name — messages rot silently because nothing compiles against them |

**The two that catch the most.** Row 3, because ADRs cite each other and an amendment does not
update its citers. Row 14, because a string is invisible to the compiler: an assertion message
claiming more than the assertion tests reads as a green check over an open question.

**Scope note.** `docs/pocs/` is deliberately exempt. A POC document records what was true during
that experiment; updating it would falsify the record rather than maintain it.
