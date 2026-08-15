# Architecture Decision Log

Last updated: 2026-08-15 — ADR-031 and ADR-032 added; ADR-012, ADR-023 and ADR-029 amended.

The 2026-08-14 cleanup rewrote bodies, folded in amendments, deleted dead decisions and renumbered the log with no gaps. That was a one-time hard fix. **Amendments are now the only way to change an ADR.**

## How to change an ADR

Append `## Amendment — YYYY-MM-DD — one-line reason`. State `**Overrides:** <section>` for anything that changes existing text, `**Adds:** <topic>` for net-new facts. Caveman-terse, one line per point. **Never edit the original decision body.** Multiple amendments are multiple sections, newest last.

New ADR beats amendment when the decision reverses entirely, the technology changes, or the amendment would be longer than the original.

Full template and rules: [ADR-SCHEMA.md](ADR-SCHEMA.md).

## Content rule

ADRs state **WHAT** was decided and **WHY** — not **HOW**. No class, method, interface or property names; no code listings of production types; no project-file snippets. Paths, external URLs, secret names, configuration keys and wire-format field names are allowed. Implementation detail belongs in the design document; enforcement detail belongs in `docs/standards/`. Where an ADR and the coding standards disagree, **the standards win**.

## Log

| ADR | Title | Status | Date |
|---|---|---|---|
| [ADR-001](ADR-001-blazor-wasm-frontend.md) | Blazor WASM as frontend framework | Accepted | 2026-08-08 |
| [ADR-002](ADR-002-question-repository-abstraction.md) | Question repository abstraction | Accepted | 2026-08-08 |
| [ADR-003](ADR-003-single-unified-app.md) | Single unified app for OIJ and voivodeship konkursy | Accepted | 2026-08-08 |
| [ADR-004](ADR-004-github-pages-hosting.md) | GitHub Pages for WASM static hosting | Accepted | 2026-08-08 |
| [ADR-005](ADR-005-render-com-api-hosting.md) | Render.com for API hosting | Accepted | 2026-08-08 |
| [ADR-006](ADR-006-oracle-cloud-deferred.md) | Oracle Cloud Always Free — deferred | Deferred | 2026-08-08 |
| [ADR-007](ADR-007-unified-question-schema.md) | Unified question schema | Accepted | 2026-08-13 |
| [ADR-008](ADR-008-excluded-sources-and-competitions.md) | Excluded sources and competition types | Accepted | 2026-08-08 |
| [ADR-009](ADR-009-no-accounts.md) | No user accounts | Accepted | 2026-08-08 |
| [ADR-010](ADR-010-responsive-and-accessible-upfront.md) | Responsive and accessible from day one | Accepted | 2026-08-08 |
| [ADR-011](ADR-011-pwa-deferred.md) | PWA / Service Worker — deferred | Deferred | 2026-08-08 |
| [ADR-012](ADR-012-language-policy.md) | Language policy | Accepted | 2026-08-08 |
| [ADR-013](ADR-013-api-posture-read-only.md) | API posture — read-only, stateless, grading in the browser | Accepted | 2026-08-12 |
| [ADR-014](ADR-014-no-css-framework.md) | Custom CSS only — no Bootstrap or utility framework | Accepted | 2026-08-09 |
| [ADR-015](ADR-015-frontend-deploy-manual-only.md) | Frontend deploy is manual only | Accepted | 2026-08-09 |
| [ADR-016](ADR-016-localstorage-quiz-session.md) | Quiz session persisted in browser storage | Accepted | 2026-08-12 |
| [ADR-017](ADR-017-runner-allocation.md) | Runner allocation — hosted for the Pages path | Accepted | 2026-08-14 |
| [ADR-018](ADR-018-wasm-asset-fingerprinting-on-pages.md) | WASM asset fingerprinting on static hosting | Accepted | 2026-08-14 |
| [ADR-019](ADR-019-client-http-timeout-cold-start.md) | Client HTTP timeout covers API cold start | Accepted | 2026-08-14 |
| [ADR-020](ADR-020-scalability-posture.md) | Scalability posture — one concurrent user | Shell | 2026-08-12 |
| [ADR-021](ADR-021-progress-tracking-browser-side.md) | Long-term progress tracking — browser-side | Shell | 2026-08-12 |
| [ADR-022](ADR-022-blazor-feature-folders.md) | Feature-based folders in the frontend | Accepted | 2026-08-13 |
| [ADR-023](ADR-023-solution-layout-and-project-naming.md) | Solution layout and project naming | Accepted | 2026-08-13 |
| [ADR-024](ADR-024-value-based-answers.md) | Answers are values, not option indices | Accepted | 2026-08-13 |
| [ADR-025](ADR-025-questions-filter-endpoint-contract.md) | Question filtering endpoint contract | Accepted | 2026-08-14 |
| [ADR-026](ADR-026-versioning-via-git-tags.md) | Release versioning via git tags, auto patch bump | Accepted | 2026-08-13 |
| [ADR-027](ADR-027-main-branch-protection.md) | Protected main branch with a required CI check | Accepted | 2026-08-13 |
| [ADR-028](ADR-028-robots-txt-two-origins.md) | Crawler control across two origins | Accepted | 2026-08-13 |
| [ADR-029](ADR-029-question-storage-sqlite.md) | Question storage — SQLite in `data/`, Dapper | Accepted | 2026-08-14 |
| [ADR-030](ADR-030-api-composition-controllers.md) | API composition — controllers, versioned routes, startup extensions | Accepted | 2026-08-14 |
| [ADR-031](ADR-031-api-error-handling-boundary.md) | API error handling — two layers and a coded contract | Accepted | 2026-08-15 |
| [ADR-032](ADR-032-grading-dispatch-by-type.md) | Grading dispatches by question type, one unit per type | Accepted | 2026-08-15 |

## Open decisions

| Where | What is not decided |
|---|---|
| [ADR-020](ADR-020-scalability-posture.md) | At what concurrent-user count the architecture changes, and the upgrade path. |
| [ADR-021](ADR-021-progress-tracking-browser-side.md) | Whether long-term progress tracking is built, its UX, and how data-loss risk is communicated. |
| [ADR-017](ADR-017-runner-allocation.md) | Overall runner strategy — whether hosted minutes suffice, or the local machine gets its full toolchain fixed. The Pages-path allocation is decided and provisional. |

## Cleanup note — 2026-08-14

The log was renumbered. Prior numbering does not map to current numbering, and prior ADR numbers appearing in git history, commit messages or pull requests refer to the pre-cleanup log.

Decisions deleted as fully dead, with no successor: no-backend-Phase-1, versioned-static-JSON question delivery, static image paths and filename convention, `sourceUrls[]` explanation bindings, and the index-based answer semantics. Their live residue, where any existed, was absorbed into ADR-007, ADR-024 and ADR-029.

Also deleted, as a duplicate rather than as dead: the C#-language-posture ADR. Nullable-disabled, no top-level statements, implicit usings, `var` policy and warnings-as-errors are all stated with their reasoning in `docs/standards/projects-and-solution.md` and `docs/standards/csharp.md`. An ADR restating a rule that lives there earns nothing.

Decisions merged: the shared-class-library, Onion-layout and project-naming ADRs into [ADR-023](ADR-023-solution-layout-and-project-naming.md); the data-access-library and bank-location ADRs into [ADR-029](ADR-029-question-storage-sqlite.md); the two exclusion lists into [ADR-008](ADR-008-excluded-sources-and-competitions.md); the responsive and accessibility ADRs into [ADR-010](ADR-010-responsive-and-accessible-upfront.md).

Two decisions reversed: [ADR-030](ADR-030-api-composition-controllers.md) chooses MVC controllers and versioned routes, replacing minimal-API route registration on unversioned paths. [ADR-029](ADR-029-question-storage-sqlite.md) chooses SQLite as the runtime store, replacing the flat-JSON read that existed only to ship sooner.
