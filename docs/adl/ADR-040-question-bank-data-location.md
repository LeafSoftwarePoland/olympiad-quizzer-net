# ADR-040: Question bank data lives outside the API project

**Status:** Accepted
**Date:** 2026-08-14

## Problem

`questions.json` and the question images live inside the API project tree
(`source/App/olympiad-quizzer-net.API/Data/`). They are content, not code: edited by a different
workflow, on a different cadence, reviewed for correctness of Polish text and answer keys rather than
for compilation. Keeping them there makes every content edit look like a code change, grows the
project folder with binary assets, and couples the content path to the project name — which
ADR-039 is about to change.

## Considered

**Where the data lives**

- **Keep inside the API project** — zero migration. Content edits keep touching the code tree; the
  path changes again on every project rename.
- **Repository root `data/`** — top-level, obviously not code, versioned with the app so a deploy is
  reproducible. Costs a container-build and configuration change.
- **Outside the repository, volume-mounted only** — cleanest separation. Infeasible: the backend runs
  on the Render free plan (ADR-007) which has no persistent disk. Would force a paid tier for a
  ~single-user tool.
- **Fetched at boot from object storage** — decouples release cadence from content cadence. Adds a
  network dependency to a startup path whose whole design is fail-fast, plus a bucket, plus a
  credential. Rejected as disproportionate.
- **Separate repository or git submodule** — adds a submodule checkout to CI and to the container
  build context; buys nothing at this size.

**How the API finds it**

- **Copy the data into the build output via a project item** — keeps runtime resolution untouched but
  re-couples the project file to the content path, which is the thing being decoupled.
- **Configuration only** — the existing key already accepts an absolute path or a path relative to the
  application base directory. Container layout is chosen so the relative form keeps working.

## Decision

**`data/` at the repository root, committed, baked into the container image at build time, located
purely by configuration.**

### Layout

- `data/questions.json` — the production question bank.
- `data/images/` — question images.
- Lowercase and kebab-cased, matching the repo convention for content and static assets. It is not a
  namespace-bearing code folder, so the PascalCase folder rule does not apply.
- `source/App/…API/Data/dev-questions.json` stays where it is. It is a development fixture with no
  production role, it is small, and it must travel with the project so a fresh clone runs without
  extra setup.

### How the API finds it

- Two configuration keys, both accepting an absolute path or a path relative to the application base
  directory: `QuestionBank:FilePath` for the bank, `QuestionBank:ImagesPath` for the image directory.
- The image directory location becomes configuration. Today it is a compiled-in path, which is the
  reason the image folder cannot move without a code change.
- Defaults, set in `appsettings.json`: `data/questions.json` and `data/images`.
- `appsettings.Development.json` overrides `QuestionBank:FilePath` to the in-project dev fixture.
  Development therefore never reads `data/` and never needs it copied anywhere.
- Missing image directory stays non-fatal — the static-file mapping is skipped. Missing bank file
  stays fatal at startup. Unchanged from today.
- Case sensitivity: the deployed filesystem is Linux. `data/` in configuration must match `data/` on
  disk exactly. The development override points at `Data/` inside the project, which is Windows-local.
  Each value is set explicitly; neither is derived from the other.

### Container

The runtime image copies `data/` next to the published application, so the application base directory
resolves the relative defaults with no absolute path and no environment variable. The copy happens in
the runtime stage, not the build stage: the data is not an MSBuild item any more, so publishing would
not carry it, and copying it last means a content-only change rebuilds one layer instead of the
application.

### Git and CI

- `data/` is committed to this repository, as a top-level sibling of `source/` and `docs/`.
- Content changes use the existing `data` commit scope, so a bank edit is distinguishable from a code
  edit in history without a second repository.
- The L1 test project keeps linking the real bank as its integrity fixture, now from `data/` instead
  of from the API project. CI therefore validates every bank edit against the schema and invariant
  suite before merge — this is the reason `data/` is committed rather than mounted.
- The container image is the unit of release. **A content-only change still requires a backend
  release** to reach production; the bank is not hot-swappable in v1.0. Accepted: content changes are
  rare, and an immutable image means a rollback restores code and content together.
- `.dockerignore` must not exclude `data/`.

**Pros:**
- Content is a top-level concern in the tree, visibly separate from code.
- Path no longer changes when the API project is renamed (ADR-039).
- Deploys are reproducible — image and bank version together; rollback is atomic.
- No persistent disk, no bucket, no credential, no startup network call.
- Image locations become configurable, which was previously impossible without a code change.

**Cons:**
- Bank edits require a backend deploy. No live content updates.
- Two configuration keys instead of one hard-coded path plus one key.
- A case-sensitivity trap: `data/` on Linux, `Data/` for the Windows dev fixture.
- Images live in git; repository size grows with the bank. Acceptable at the planned bank size,
  revisit if images reach hundreds of megabytes.

## Remarks / Sources

- **Live defect found while writing this ADR, unrelated to the move:** `.dockerignore` excludes
  `source/`, while the container build copies from `source/`. The exclusion predates the migration
  into `source/` and was correct when project folders sat at the repository root. Any container build
  from the repository root fails on the first copy. Must be fixed in the same change; it is not
  caused by this ADR and was not caused by ADR-039.
- ADR-007 (Render.com hosting) — free plan, no persistent disk. The constraint that rules out the
  volume-mount option. Revisit this ADR if the plan changes.
- ADR-009 / ADR-011 — the JSON document is the canonical question format; this ADR moves it, it does
  not change it.
- ADR-039 — same structural change; shares the container-build edit.
- Follow-up risk: nothing yet asserts that every image referenced by the bank exists in `data/images/`.
  With the two now in the same folder that check is cheap and belongs in the L1 integrity suite.
