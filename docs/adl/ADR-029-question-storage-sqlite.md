# ADR-029: Question storage — SQLite in `data/`, Dapper, generated from committed JSON

**Status:** Accepted
**Date:** 2026-08-14

## Problem

Three coupled questions, answered together because answering any one alone forces the others.

Where does the question bank live? It sat inside the API project tree. It is content, not code: edited on a different cadence by a different workflow, reviewed for correct Polish and correct answer keys rather than for compilation. Keeping it there made every content edit look like a code change and coupled the content path to the project name.

In what form is it stored? A flat JSON file read whole at startup was the delivery-today shortcut. It has no indexes, no constraints, and no way to express the answers-exist-among-options invariant (ADR-007) other than a test.

What reads it? The Infrastructure project has been named for SQLite since the layout was set (ADR-023) while containing no SQLite, which reads as wrong until you know why.

## Considered

**Where the data lives**

- **Inside the API project** — zero migration. Content edits keep touching the code tree; the path changes on every project rename.
- **Repository root `data/`** — top-level, obviously not code, versioned with the app so a deploy is reproducible.
- **Outside the repository, volume-mounted** — cleanest separation. **Infeasible: the API host's free plan has no persistent disk (ADR-005).** Would force a paid tier for a single-user tool.
- **Fetched at boot from object storage** — decouples release cadence from content cadence. Adds a network call to a startup path whose whole design is fail-fast, plus a bucket and a credential. Disproportionate.
- **Separate repository or submodule** — adds a checkout to CI and to the container build context. Buys nothing at this size.

**Storage form**

- **JSON file, read whole at startup** — no schema, no query engine, no constraints. Works at 210 questions and stops being defensible the moment filtering gets more selective than a linear scan.
- **SQLite file** — real schema, real constraints, real indexes, single file, no server process, no credential, and content-addressable by a path. The engine the Infrastructure project was already named for.
- **Hosted relational database** — overkill, costs money, and the host's free database offer expires.

**Which artefact is the source of truth**

- **The database file, authored directly with a SQLite tool** — one artefact, no generation step. **A committed binary has an opaque diff.** A content pull request would show "binary file changed" and nothing else, so the answer-key review that is the whole point of a content PR becomes impossible.
- **The JSON file only, database built at container build** — fully reviewable. The database is then not in the repository, so nothing in a pull request proves the two agree, and the build becomes the only place the schema is exercised.
- **JSON authored and committed; database generated from it and also committed; CI asserts they agree** — one source of truth, one derived artefact, and staleness is caught by a test rather than by trust.

**Data access**

- **Full ORM** — generates SQL, tracks entities, ships a migrations toolchain. Tens of megabytes of extra RAM on a 512 MB plan, and hidden query behaviour.
- **Micro-ORM, hand-written SQL** — ~2 MB, full query control, parameterised by default, queries readable and debuggable. Costs mapping boilerplate.
- **Hybrid — ORM for schema only, micro-ORM at runtime** — two toolchains for one small read-only schema.

## Decision

**SQLite is the runtime store, in `data/`, committed. `questions.json` stays the authored source of truth, also committed, and the database is generated from it. Dapper reads it. Both files are baked into the container image and located purely by configuration.**

### Layout

- `data/questions.json` — the **authored** bank. Hand-edited, diff-reviewable, the thing a content pull request is reviewed against.
- `data/questions.db` — the **generated** SQLite bank. Committed, because it is what ships and what the tests run against.
- `data/images/` — question images.
- Lowercase and kebab-cased, matching the repo convention for content and static assets. Not a namespace-bearing code folder, so the PascalCase folder rule does not apply.

### Why both files rather than one

A committed binary alone cannot be reviewed. A JSON file alone cannot be proved to match what ships. Keeping the authored JSON and the generated database side by side gives one editable source, one shipped artefact, and a cheap CI assertion that the second was regenerated from the first. **Regenerating the database is part of a content change, not a separate chore** — a pull request whose JSON and database disagree fails CI.

This is the one point in this ADR not dictated by an external constraint. It exists because losing answer-key reviewability on a hand-maintained bank of ~210 questions, where a wrong answer grades a correct response as wrong silently (ADR-007), is a worse outcome than carrying one generated file.

### Synchronisation is automated and reconciling

The generator is **not** a one-shot import and **not** a full rebuild. It reconciles the database against the authored JSON and applies only what differs:

| Delta | Detected by | Action |
|---|---|---|
| added | identifier present in JSON, absent from the database | insert |
| changed | identifier in both, row content hash differs | update in place |
| removed | identifier present in the database, absent from JSON | delete |

The question identifier is what makes this possible: ADR-007 requires it stable, unique across the bank, and never reused. Every row also carries a content hash of its authored fields, so "changed" is an exact single-column comparison rather than a field-by-field diff.

**The sync emits a delta report — counts and identifiers for added, changed and removed.** That report is the review mechanism for the binary artefact: a reviewer cannot read a database diff, but can read "changed: 3 (ids 19, 131, 136)" and check those three against the JSON diff sitting in the same pull request.

**CI runs the sync in check mode and fails on any non-empty delta.** A non-empty delta means the committed database was not regenerated after the JSON changed. This is deliberately not a byte comparison of two files — SQLite page layout is not stable across writes, so byte equality would fail on databases whose contents agree.

A full rebuild on every content edit was rejected: it rewrites the entire blob for a one-question fix, and it produces no delta report, which forfeits the only reviewability a binary artefact can have.

### Schema and its lifecycle

- The schema is a checked-in SQL script. It is the definition of the database, and the generator applies it to an empty file.
- **No runtime migration mechanism.** The shipped database is pre-built with the schema the code expects, so there is nothing to migrate at startup. A schema change is a script change plus a regeneration.
- The database carries a schema-version value. Startup compares it against the version the code requires and **fails fast on mismatch**, so a stale committed database cannot be served.
- Constraints the JSON could only assert in a test become real: non-null columns, the closed-list answer relationship, uniqueness of the question identifier.

### Encoding

The authored JSON is **UTF-8 without BOM** and stays that way. SQLite stores `TEXT` as UTF-8 natively, so import is a straight read with no transcoding step and no place for a re-encoding bug. Question text is Unicode by design (ADR-007) and is never stripped, escaped or "cleaned" at any point in this path.

### How the API finds it

- Configuration keys, each accepting an absolute path or a path relative to the application base directory: one for the database, one for the image directory. Defaults point at `data/`.
- Missing image directory is non-fatal; the static-file mapping is skipped. **Missing or schema-mismatched database is fatal at startup.** An API that boots and serves an empty array forever is worse than one that refuses to boot, because a health check catches the second and a student catches the first.
- Case sensitivity: the deployed filesystem is Linux. Configured paths must match the on-disk case exactly.

### Data access

Dapper for every query. No ORM at runtime. Parameterised queries only — never string interpolation into SQL. Implemented in the Infrastructure project, which stops being named for something it does not contain.

### Container

The runtime image copies `data/` next to the published application, so the base directory resolves the relative defaults with no absolute path and no environment variable. The copy happens in the **runtime** stage: the data is not a build item, so publishing would not carry it, and copying it last means a content-only change rebuilds one layer instead of the application.

### Git and CI

- `data/` is committed, as a top-level sibling of `source/` and `docs/`. Content changes are commits and pull requests like any other change.
- Content changes use the `data` commit scope, so a bank edit is distinguishable from a code edit in history.
- The Infrastructure test project runs against the real database file. CI therefore validates every bank edit against the schema and the ADR-007 invariant before merge. **This is the reason `data/` is committed rather than mounted.**
- CI asserts the committed database matches the committed JSON.
- The container image is the unit of release. **A content-only change still requires a backend release** to reach production; the bank is not hot-swappable. Accepted: content changes are rare, and an immutable image means a rollback restores code and content together.
- The container ignore file must not exclude `data/`, and must not exclude `source/`.

Accepted cons:

- Two files describing one bank, with a regeneration step between them. Mitigated by CI, not by discipline.
- The sync is production code and must be tested like it: a wrong delta silently ships the wrong bank, which is the same failure class as a wrong answer key.
- A content hash column is derived data living inside the store. It must be recomputed by the sync and never hand-edited.
- A binary file in git. Repository size grows with every content revision, and the database's own diff is opaque — the JSON's is not, which is the point.
- Bank edits require a backend deploy. No live content updates.
- Manual schema script, no auto-generated diff. Trivial for a read-only bank; would not be for a mutable one.
- Images live in git, so repository size also grows with the bank. Acceptable at planned size; revisit if images reach hundreds of megabytes.
- Mapping boilerplate for any query more complex than a projection.

## Remarks / Sources

- **Migration state:** the current corpus is 210 questions in `data/questions.json`, UTF-8 without BOM, CRLF. Three questions (ids 19, 131, 136) contain Hangul-block glyphs that are near-certainly mis-scraped Greek letters; compatibility normalisation (ADR-024) cannot fold those, so they are content bugs and must be corrected before or during the migration. Ligature and letterlike-symbol occurrences elsewhere in the bank are folded correctly by normalisation and need no edit.
- **Follow-up risk:** nothing yet asserts that every image referenced by the bank exists in `data/images/`. With the two in one folder the check is cheap and belongs in the integrity suite.
- ADR-005 (no persistent disk — the constraint that rules out volume-mounting; revisit this ADR if the plan changes), ADR-007 (the record shape, the wire format and the integrity invariant), ADR-023 (the Infrastructure project is named for this engine), ADR-024 (normalisation rules the import must respect), ADR-013 (read-only posture — no write path to this store)
- Injection safety: always parameterised, never interpolated.

## Amendment — 2026-08-15 — the read path, the sync entry point, and a missing file in § Layout

**Overrides:** nothing in the Decision text, which never specified how a query executes. Fills the
gap it left.
**Adds:** `data/schema.sql` to § Layout; the regeneration entry point.

**Trigger.** The first implementation loaded the whole bank with one unfiltered query at
construction and filtered it in memory. The three indexes in the schema were never used at read
time — so § Considered's argument against flat JSON ("no indexes… stops being defensible the moment
filtering gets more selective than a linear scan") was **untrue of the code that replaced it.** The
storage engine was delivering constraints only.

**Read path, in three steps:**

1. **SQL filters, returning identifiers only.** Tag predicates execute in the database, so the
   indexes are used and the ADR's stated motivation becomes true.
2. **The shuffle stays in application code**, seeded. `ORDER BY RANDOM()` would move it into SQL and
   read as simpler, but SQLite's random function **cannot be seeded** — that would destroy test
   determinism and with it every assertion about draw order. ADR-025 pins *shuffle, then cap*
   precisely because capping first makes the result deterministic by bank order; that rule needs a
   test, and a test needs a seed.
3. **Fetch only the selected rows.** At the cap of 30 that materialises 30 records rather than the
   whole bank.

Consequence worth stating: this keeps the logic most worth testing — filtering semantics, clamping,
shuffle-then-cap — above the persistence seam and reachable without a database, which is what the
ADR-023 amendment's seam exists to buy.

**Regeneration has an entry point.** The sync routine previously had no production caller: its only
callers were tests, so the workflow this ADR calls "part of a content change, not a separate chore"
could not actually be performed. It is now a console tool in `source/Solution/`, beside the suites
that validate the artefacts it produces. Defaults resolve relative to the repository root; paths may
be overridden by argument; failure exits non-zero and prints the exception, because the audience is
a developer and a stack trace is the most useful thing it can emit. That is the opposite of the
API's rule against leaking technical detail, and deliberately so — different audience, different
correct answer.

**`data/schema.sql`** belongs in § Layout beside `questions.json`, `questions.db` and `images/`. It
is required by § Schema and its lifecycle and exists on disk; the file list omitted it.
