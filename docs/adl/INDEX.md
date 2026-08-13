# Architecture Decision Log

Last updated: 2026-08-13 (v1.0 architecture: ADR-031..038 added; amendments to ADR-003/011/021)

## How to amend an ADR

Append `## Amendment — YYYY-MM-DD — one-line reason` to existing file. State `**Overrides:** <section>` for anything that changes; `**Adds:** <topic>` for net-new facts. Write caveman-terse — one line per point. Never edit original decision body. Multiple amendments = multiple sections, newest last.

New ADR > amendment when: decision reverses entirely, new technology, or amendment length exceeds original.

| ADR | Title | Status | Date |
|---|---|---|---|
| [ADR-001](ADR-001-blazor-wasm-frontend.md) | Blazor WASM as frontend framework | Accepted | 2026-08-08 |
| [ADR-002](ADR-002-no-backend-phase1.md) | No backend in Phase 1 — static JSON | Superseded for question delivery | 2026-08-08 |
| [ADR-003](ADR-003-question-repository-interface.md) | IQuestionRepository abstraction | Accepted | 2026-08-08 |
| [ADR-004](ADR-004-dapper-over-ef-core.md) | Dapper over EF Core | Accepted | 2026-08-08 |
| [ADR-005](ADR-005-single-unified-app.md) | Single app for OIJ + voivodeship konkursy | Accepted | 2026-08-08 |
| [ADR-006](ADR-006-github-pages-hosting.md) | GitHub Pages for WASM static hosting | Accepted | 2026-08-08 |
| [ADR-007](ADR-007-render-com-api-hosting.md) | Render.com for Phase 2 API hosting | Accepted | 2026-08-08 |
| [ADR-008](ADR-008-oracle-cloud-deferred.md) | Oracle Cloud Always Free — deferred | Deferred | 2026-08-08 |
| [ADR-009](ADR-009-versioned-static-json-questions.md) | Questions as versioned static JSON | Superseded for question delivery | 2026-08-08 |
| [ADR-010](ADR-010-images-static-lazy-loaded.md) | Images as static lazy-loaded files | Accepted | 2026-08-08 |
| [ADR-011](ADR-011-unified-question-schema.md) | Unified question schema | Accepted | 2026-08-08 |
| [ADR-012](ADR-012-explanation-source-per-question.md) | source_urls[] + explanation ContentBlocks per question | Accepted | 2026-08-08 |
| [ADR-013](ADR-013-excluded-paid-sources.md) | Excluded sources — private/paid olympiads | Accepted | 2026-08-08 |
| [ADR-014](ADR-014-excluded-programming-competitions.md) | Excluded competition types — pure programming | Accepted | 2026-08-08 |
| [ADR-015](ADR-015-no-accounts-phase1.md) | No user accounts in Phase 1 | Accepted | 2026-08-08 |
| [ADR-016](ADR-016-responsive-design.md) | Responsive design — mobile/tablet/desktop | Accepted | 2026-08-08 |
| [ADR-017](ADR-017-aria-accessibility-upfront.md) | ARIA accessibility built upfront | Accepted | 2026-08-08 |
| [ADR-018](ADR-018-pwa-deferred.md) | PWA / Service Worker — deferred | Deferred | 2026-08-08 |
| [ADR-019](ADR-019-language-policy.md) | Language policy | Accepted | 2026-08-08 |
| [ADR-020](ADR-020-poc-ships-thin-api.md) | POC ships a thin API (amends ADR-002) | Accepted | 2026-08-08 |
| [ADR-021](ADR-021-shared-class-library.md) | Shared class library for models and grader | Accepted | 2026-08-08 |
| [ADR-022](ADR-022-poc-schema-field-bindings.md) | POC schema field bindings and answer semantics (clarifies ADR-011) | Accepted | 2026-08-08 |
| [ADR-023](ADR-023-no-css-framework.md) | Custom CSS only — no Bootstrap or utility framework | Accepted | 2026-08-09 |
| [ADR-024](ADR-024-deploy-frontend-manual-only.md) | deploy-frontend manual-only (workflow_dispatch) | Accepted | 2026-08-09 |
| [ADR-025](ADR-025-localstorage-quiz-session.md) | Quiz session persisted in localStorage | Accepted | 2026-08-12 |
| [ADR-026](ADR-026-self-hosted-runner-tar-workaround.md) | Self-hosted Windows runner — tar/bsdtar PATH workaround | Shell | 2026-08-12 |
| [ADR-027](ADR-027-dotnet10-wasm-fingerprinting-pages.md) | .NET 10 WASM asset fingerprinting on GitHub Pages | Shell | 2026-08-12 |
| [ADR-028](ADR-028-client-http-timeout-render-coldstart.md) | Client HTTP timeout for Render cold start | Shell | 2026-08-12 |
| [ADR-029](ADR-029-scalability-posture.md) | Scalability posture — one concurrent user | Shell | 2026-08-12 |
| [ADR-030](ADR-030-progress-tracking-browser-side.md) | Progress and history tracking — browser-side, lost on cache clear | Shell | 2026-08-12 |
| [ADR-031](ADR-031-blazor-feature-folders.md) | Blazor WASM feature-based folder structure | Accepted | 2026-08-13 |
| [ADR-032](ADR-032-onion-solution-layout.md) | Onion solution layout — Core / Infrastructure / App | Accepted | 2026-08-13 |
| [ADR-033](ADR-033-csharp-language-posture.md) | C# language posture — nullable disabled, no top-level statements | Accepted | 2026-08-13 |
| [ADR-034](ADR-034-value-based-answers.md) | Answers are values, not option indices | Accepted | 2026-08-13 |
| [ADR-035](ADR-035-questions-filter-endpoint-contract.md) | Question filtering endpoint contract | Accepted | 2026-08-13 |
| [ADR-036](ADR-036-versioning-via-git-tags.md) | Release versioning via git tags, auto patch bump | Accepted | 2026-08-13 |
| [ADR-037](ADR-037-main-branch-protection.md) | Protected `main` with a required CI check | Accepted | 2026-08-13 |
| [ADR-038](ADR-038-robots-txt-two-origins.md) | Crawler control across two origins | Accepted | 2026-08-13 |

## Amendment graph

- **ADR-002** (no backend Phase 1) — amended 2026-08-09 for POC scope by **ADR-020**; amended 2026-08-12: v1.0 ships backend, static-JSON delivery superseded. Status: Superseded for question delivery; `IQuestionRepository` seam (ADR-003) remains.
- **ADR-009** (versioned static JSON) — amended 2026-08-12: server-side filtering supersedes static-JSON delivery for questions. `manifest.json`/versioned filenames no longer used for question data.
- **ADR-011** (unified schema) — clarified by **ADR-022** (2026-08-08); amended 2026-08-12 (a): typed tag fields replace flat `tags[]` (breaking — `category`, `algorithms`, `source`, `sourceUrl`, `year`, `difficulty`, `explanationSource`, `source_raw`); amended 2026-08-12 (b): type enum renames (`single`/`multi` replace `singleAbcd`/`multiSelect`), `correctAnswer` shape per type.
- **ADR-018** (PWA deferred) — amended 2026-08-12: offline mode deferred further; server-side filtering dependency adds complexity.
- **ADR-019** (language policy) — amended 2026-08-12: Polish snake_case exception for tag vocabulary (`category[]`, `algorithms[]` values use diacritic-stripped Polish words).
- **ADR-020** (POC thin API) — amended 2026-08-12: grading confirmed client-side; static-JSON fallback dropped; server-side filtering is v1.0 decision.
- **ADR-007** (Render.com) — updated `Accepted (test pending)` → `Accepted`. M11 resolved: no card required. See ADR-007 amendment section.
- **ADR-001** (Blazor WASM) — POC gotchas appended: Bootstrap conflict, `#app` sticky footer, `Nav.BaseUri`, JS interop needs.
- **ADR-006** (GitHub Pages) — POC confirmed. Push trigger removed. See ADR-006 amendment + ADR-024.
- **ADR-003** (repository seam) — amended 2026-08-13: query abstraction takes a structured query object + cancellation token; second operation added for available filter values; POC filter type deleted; static-JSON fallback implementation dropped; abstraction moves to the Domain project. Seam itself unchanged and now implemented on both sides.
- **ADR-011** (unified schema) — amended 2026-08-13: v1.0 field list frozen. Adds `olympiad`, changes `stage` to string, `id` to int, `year` to nullable int, normalises `source_raw` → `sourceRaw`, removes `competition`/`voivodeship`, adds mandatory `alt` on image blocks, splits grading normalisation (see ADR-034), records the answers-exist-among-options invariant.
- **ADR-021** (shared class library) — amended 2026-08-13: project superseded by `Core/olympiad-quizzer-net.Domain` (ADR-032); reasoning kept; contents updated for the v1.0 schema and for session logic moved inward; "no I/O, no DI" rule now structurally enforced with one bounded serializer exception.
- **ADR-020** (POC thin API) — further amended in effect by **ADR-035**, which specifies the filtering contract ADR-020's amendment only named.
- **ADR-032** supersedes the *layout* in ADR-021. ADR-031 sits inside ADR-032's Presentation ring.
- **ADR-034** overrides ADR-011's index-based `correct_answer` table and ADR-022's index semantics.
- **ADR-036** is constrained by **ADR-037** (protected `main` rules out committing a version back) and depends on it staying that way — no tag protection rule may be added.

## ADR content rule (from 2026-08-13, ADR-031 onward)

ADRs state WHAT was decided and WHY, not HOW. No class, method, interface or property names in
an ADR body; no code listings of production types; no project-file snippets. Paths, external
URLs, secret names, configuration keys and wire-format field names are allowed. Implementation
detail belongs in the design document. ADRs numbered below 031 predate this rule and are not
retro-edited — the rule protects new content. Full text: `docs/coding-standards.md`.
