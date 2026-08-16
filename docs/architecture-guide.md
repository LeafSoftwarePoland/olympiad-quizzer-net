# Architecture Guide

General architecture rules for this repo. Supplements the ADL — does not replace it.

## Architecture style

**Clean Architecture / Onion Architecture.**

Dependency direction: inner layers know nothing about outer layers. The reference graph enforces
it, so a violation is a compile error rather than a review finding (ADR-023).

| Layer | Project | Contents | Rules |
|---|---|---|---|
| Domain | `Core/olympiad-quizzer-net.Core.Domain` | question and answer types; the grading contract and its per-type units (ADR-032); query and filter types; the repository abstraction and the persistence seam (ADR-023); error-code constants (ADR-031); session state and logic; serialization contract | References **nothing** — no project references, no package references. No I/O, no HTTP, no DI, no logging. One bounded exception: the platform JSON serializer in `Domain/Serialization` (ADR-023). |
| Test support | `Core/olympiad-quizzer-net.Core.Tests.Common` | tier constants, shared builders, fixtures, capturing loggers | References Domain. Used by two or more test projects. Ships nothing, carries no tier. |
| Infrastructure | `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite` | question storage, filtering, shuffling, DI extension | References Domain only. Reads `data/questions.db` with Dapper (ADR-029). **I/O only** — no validation, no conversion; data-level faults bubble (ADR-023). |
| Application / API | `App/olympiad-quizzer-net.App.API` | `Program` (class-based, non-partial), controllers under `Controllers/`, startup extensions under `Extensions/`, exception middleware, Dockerfile | References Domain + Infrastructure. Stateless, read-only (ADR-013). |
| Presentation / Client | `App/olympiad-quizzer-net.App.Client` | Blazor WASM, feature folders, HTTP repository implementation, browser-storage services, error-code to Polish mapping | References Domain only. HTTP is the Client's own infrastructure boundary. |
| Tests — Domain L0 | `Core/olympiad-quizzer-net.Core.Domain.L0` | Domain unit tests | References Domain + Tests.Common. Cannot touch the filesystem — enforced by the project graph. |
| Tests — Infrastructure L0 | `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L0` | logic above the persistence seam, seam mocked | References Domain + Infrastructure + Tests.Common. |
| Tests — Infrastructure L1 | `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L1` | storage tests against a real database file | References Domain + Infrastructure + Tests.Common. |
| Tests — API L0 | `App/olympiad-quizzer-net.App.API.L0` | controller tests with a mocked repository | References Domain + API + Tests.Common. |
| Tests — API L1 | `App/olympiad-quizzer-net.App.API.L1` | controller tests, hand-constructed over real infrastructure | References Domain + Infrastructure + API + Tests.Common. |
| Tests — API L2 | `App/olympiad-quizzer-net.App.API.L2` | whole application, real pipeline, over HTTP | References Domain + Infrastructure + API + Tests.Common. |
| Tests — Integrity | `Solution/olympiad-quizzer-net.Solution.DataIntegrityTests` | the committed artefacts, not code | References Domain + Tests.Common. **`Solution` is not a ring** — the one recorded exception (ADR-023). |
| Tooling | `Solution/olympiad-quizzer-net.Solution.BankSync` | console tool: regenerates `data/questions.db` from `data/questions.json`, prints the delta report | References Domain + Infrastructure. Not shipped. Run locally before committing a content change; CI runs it in check mode (ADR-029). |

Reference graph, `.csproj` settings and the naming rule: [standards/projects-and-solution.md](standards/projects-and-solution.md).
All code conventions: [standards/INDEX.md](standards/INDEX.md) — read every file it lists.

**Precedence, in order:** coding standards → ADRs → code. The standards say how things are
written; ADRs say what was decided and why; the code is the result. When the code conflicts with
an ADR, exactly one of them is wrong — decide which, deliberately. Do not assume the code is
right because it exists, and do not assume the ADR is right because it is written down.

## Test levels

Defined authoritatively in [standards/testing-tiers.md](standards/testing-tiers.md). Restated
here for orientation only; where the two disagree, the standards win.

| Level | Scope | Project |
|---|---|---|
| L0 — unit | One class or method. Every collaborator substituted, each also given a throwing case. No filesystem, network, DI or host. Any ring qualifies where collaborators can be substituted. | `Core/…Core.Domain.L0`, `Infrastructure/…SQLite.L0`, `App/…App.API.L0` |
| L1 — in-app integration | Subject constructed **by hand** with real in-app layers over a real external. No `WebApplicationFactory`, no DI container, no middleware, no routing. | `Infrastructure/…SQLite.L1`, `App/…App.API.L1` |
| L2 — full application | Whole app built with real registrations and the real middleware pipeline, driven over HTTP. **Narrowly scoped** to what no lower tier can reach: the exception-middleware chain, routing, model binding, content negotiation, CORS preflight. | `App/…App.API.L2` |
| L3 — end-to-end | Browser-driven against a real backend. | *not created* |
| Integrity | The repository's committed **output** — question bank, generated database, rule blocks. No code under test, no production counterpart. | `Solution/…Solution.DataIntegrityTests` |

**L2 exists because a rule-derived obligation landed in it**, not because a host is convenient.
Exception middleware is unreachable at L1 by construction, so proving the chain end to end requires
the full pipeline. A `WebApplicationFactory` in an L1 project is still a defect.

**L3 is deliberately not created.** Do not tag anything with it.

The derivation that produced L2, and the six-point test that justified the Integrity exception, are
in `docs/standards/INDEX.md` § How these rules compose. Read it before proposing a new tier or a
new exception.

Every test project is in `OlympiadQuizzer.slnx`, so `dotnet test OlympiadQuizzer.slnx` runs every
level in one invocation and CI needs no per-project step.

**Real-data validation.** The real question bank is an L1 fixture, not just production data. An
integrity suite asserts the invariants the type system cannot express — mandatory `category[]`,
every stored answer existing among `options` after normalisation, mandatory image `alt`, tag
values drawn from `docs/tags.md` — and CI additionally asserts that the committed database was
regenerated from the committed JSON (ADR-029). It re-runs on every bank refresh, followed by a
manual spot-check: an invariant cannot catch an answer that is simply mis-transcribed.

## Document types in this repo

| Type | Location | Schema ref |
|---|---|---|
| ADR | `docs/adl/` | `docs/adl/ADR-SCHEMA.md` |
| Coding standards | `docs/standards/` | `docs/standards/INDEX.md` |
| Integration doc | `docs/integrations/` | `docs/integrations/INDEX.md` |
| POC doc | `docs/pocs/` | Standalone — no fixed schema |
| Functionality registry | `docs/functionalities.md` | See file header |
| Glossary | `docs/Glossary.md` | — |
| Competition rules | `docs/rules/` | `docs/rules/README.md` |
| Architecture guide | `docs/architecture-guide.md` | This file |

**Hierarchy (no circular references):**

- POC docs are standalone.
- ADRs may reference POC docs and the standards.
- Everything else (architecture guide, functionalities, rules, integrations) may reference ADRs.
- Nothing references up to functionalities or rules.
- **Nothing committed may reference a `.pipeline/` path** — that folder is gitignored and
  deletable by design, so any such link is a dangling reference waiting to happen.

## ADR hygiene

Whenever an ADR is added:

- Track it in git in the same commit.
- Add it to `docs/adl/INDEX.md` in the same commit.

To change one, append `## Amendment — YYYY-MM-DD — reason`. Never edit the original body. Full
rules in `docs/adl/ADR-SCHEMA.md` and [standards/process.md](standards/process.md).

Same hygiene applies to integration docs (`docs/integrations/INDEX.md`) and competition rules
(`docs/rules/README.md`). No orphaned docs.
