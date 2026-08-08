# Architecture Phase — Index

**Project**: olympiad-quizzer-net · **Weight class**: S · **Phase**: architecture-design → complete
**Author**: Architect · **Date**: 2026-08-08
**Mode**: brownfield adopt — design pre-existed in a brainstorming session; this phase formalized it into pipeline artifacts.

## Artifacts

| File | Purpose | Audience |
|---|---|---|
| [`solution-design.md`](solution-design.md) | ~1-page design: components, data flow, interfaces, errors, observability, security, NFRs | Implementor, Reviewer |
| [`test-strategy.md`](test-strategy.md) | L0 + L1 test inventory (42 tests), manual checklist M1–M16, `execution_mode: local`, CI `build_only` | Implementor, Reviewer |
| [`sprint-backlog.md`](sprint-backlog.md) | **The implementation plan.** T-00…T-14, ordered, with acceptance criteria, file paths and load-bearing code | Implementor |
| [`architecture-summary-for-user.md`](architecture-summary-for-user.md) | Plain-language one-pager | User |
| [`assumptions.md`](assumptions.md) | A-01…A-16, each with an invalidation trigger | Everyone |

No `archaeology-report.md` — the repo contains only a design spec, ADRs and empty `source/` folders; nothing to excavate.
No `decision-needed.md` — no user-decidable question is blocking. The one candidate (self-hosted vs GitHub-hosted runner for the Pages job) is resolved by a preflight check in T-00 with a documented escape hatch, so it needs no answer up front.
No `discovery-progress.md` — Discovery closed within the session.
No `component-specs/` or `interface-contracts/` — weight class S; interfaces are inline in `solution-design.md` §4 and `sprint-backlog.md` T-02.

## Decisions produced this phase

ADRs live at `docs/adl/` (existing project convention, preserved per adopt policy).

| ADR | Title | Note |
|---|---|---|
| [ADR-020](../../docs/adl/ADR-020-poc-ships-thin-api.md) | POC ships a thin API instead of static JSON | **Amends ADR-002** for POC scope |
| [ADR-021](../../docs/adl/ADR-021-shared-class-library.md) | Shared class library for models and grader | Deviates from the design spec's repo layout |
| [ADR-022](../../docs/adl/ADR-022-poc-schema-field-bindings.md) | POC schema field bindings and answer semantics | **Clarifies ADR-011**; expected to iterate with user |

## Discovery

Global KB (`~/.claude/kb/INDEX.md`) scanned — all seven notes are Python / Windows-shell / OAuth patterns with no bearing on a .NET web POC. **No KB snapshot taken**, so no `discovery/` folder.

External verification performed (three parallel research passes, findings synthesized into `sprint-backlog.md` and `assumptions.md` rather than stored raw):
- Blazor WASM .NET 10 → GitHub Pages: base href, `.nojekyll`, SPA 404 fallback, static-asset fingerprinting
- Render.com free tier: `PORT` binding, deploy hooks, health checks, limits, card requirement
- GitHub Actions on a self-hosted Windows runner: `upload-pages-artifact` tar incompatibility, current action tags

Locally verified against the installed SDK: `dotnet new` template short names, .NET 10 SDK presence (10.0.300 / 10.0.301).

## Handoff

Weight class S → **Sprint Planner skipped**. `sprint-backlog.md` goes directly to the Implementor as sprint-01.
Auto-Critic not triggered (L/XL only). Critic available on request.

## Self-critique

`.pipeline/critiques/C-001-architect-poc-design.md` (gitignored).
