# Architecture Decision Log

Last updated: 2026-08-08 (ADR-020/021/022 added — architecture phase)

| ADR | Title | Status | Date |
|---|---|---|---|
| [ADR-001](ADR-001-blazor-wasm-frontend.md) | Blazor WASM as frontend framework | Accepted | 2026-08-08 |
| [ADR-002](ADR-002-no-backend-phase1.md) | No backend in Phase 1 — static JSON | Accepted | 2026-08-08 |
| [ADR-003](ADR-003-question-repository-interface.md) | IQuestionRepository abstraction | Accepted | 2026-08-08 |
| [ADR-004](ADR-004-dapper-over-ef-core.md) | Dapper over EF Core | Accepted | 2026-08-08 |
| [ADR-005](ADR-005-single-unified-app.md) | Single app for OIJ + voivodeship konkursy | Accepted | 2026-08-08 |
| [ADR-006](ADR-006-github-pages-hosting.md) | GitHub Pages for WASM static hosting | Accepted | 2026-08-08 |
| [ADR-007](ADR-007-render-com-api-hosting.md) | Render.com for Phase 2 API hosting | Accepted (test pending) | 2026-08-08 |
| [ADR-008](ADR-008-oracle-cloud-deferred.md) | Oracle Cloud Always Free — deferred | Deferred | 2026-08-08 |
| [ADR-009](ADR-009-versioned-static-json-questions.md) | Questions as versioned static JSON | Accepted | 2026-08-08 |
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

## Amendment graph

- **ADR-002** (no backend Phase 1) — amended for POC scope by **ADR-020**. Not superseded: static-JSON delivery stays the fallback via the ADR-003 seam.
- **ADR-011** (unified schema) — clarified, not changed, by **ADR-022**. ADR-022 pins per-type meaning of `options` / `matchOptions` / `correctAnswer` and the short-answer normalization pipeline.
- **ADR-007** (Render.com) — still `Accepted (test pending)`. The POC is the test; update after manual check M11.
