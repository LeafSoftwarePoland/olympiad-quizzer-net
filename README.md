# olympiad-quizzer-net

Polish quiz app for the OIJ (Olimpiada Informatyczna Juniorów) — built for a child preparing for the national junior informatics olympiad.

**Tech stack:** Blazor WASM (.NET 10) frontend + ASP.NET Core API backend. SQLite question store read with Dapper. Custom CSS, no frameworks. Deployed on GitHub Pages (frontend) and Render.com (API).

## Why it exists

The OIJ publishes past exam questions as PDFs. There is no interactive practice tool. This app imports those questions and lets students drill them in exam-simulation or free-learning mode, with a timer, explanations, server-side filtering by category, algorithm, year and stage, and answer feedback.

## How to run locally

**Prerequisites:** .NET 10 SDK.

**1. Run the API**

```
dotnet run --project source/App/olympiad-quizzer-net.App.API
```

The API listens on `http://localhost:10000` by default. Routes: `GET /healthz`, `GET /v1/filters`, `GET /v1/questions`.

In development mode (the default for `dotnet run`), the API loads `appsettings.Development.json`, which points at a small 6-question dev bank inside the API project instead of the full production bank. Keeps startup fast and covers all three live question types — single choice, multiple choice, short answer.

To use the full bank locally, delete or rename `appsettings.Development.json`, or override the path (relative paths resolve against the build output directory, not the repo root):

```
dotnet run --project source/App/olympiad-quizzer-net.App.API -- --QuestionBank:FilePath="$(pwd)/data/questions.db"
```

**2. Run the frontend**

```
dotnet run --project source/App/olympiad-quizzer-net.App.Client
```

Opens at `http://localhost:<port>`. The frontend reads `wwwroot/appsettings.json` for `ApiBaseUrl`; in development this defaults to `http://localhost:10000`.

**3. Run tests**

```
dotnet test OlympiadQuizzer.slnx -c Release
```

Runs L0 (unit) and L1 (integration) in one invocation. L1 includes bank-integrity checks against the real bank in `data/`, and asserts that the committed database was regenerated from the committed JSON. All other tests use their own fixture files.

## The question bank

`data/` holds both artefacts, and both are committed:

| File | Role |
|---|---|
| `data/questions.json` | **Authored source of truth.** Hand-edited, diff-reviewable. What a content PR is reviewed against. |
| `data/questions.db` | **Generated SQLite bank.** What the API actually reads. |
| `data/images/` | Question images, named after the question `id`. |

A content change edits the JSON and regenerates the database in the same commit. The sync is reconciling — it reports added, changed and removed questions by `id`, and that report is how a binary artefact stays reviewable. CI fails if the two disagree. See [ADR-029](docs/adl/ADR-029-question-storage-sqlite.md).

## Project structure

```
data/                                       — question bank: questions.json, questions.db, images/
source/
  Core/
    olympiad-quizzer-net.Core.Domain/         — domain types, grading units, session logic, abstractions, error codes
    olympiad-quizzer-net.Core.Domain.L0/      — domain unit tests (xUnit)
    olympiad-quizzer-net.Core.Tests.Common/   — shared test constants, builders, fixtures
  Infrastructure/
    olympiad-quizzer-net.Infrastructure.SQLite/    — SQLite store, filtering, shuffling, DI extension
    olympiad-quizzer-net.Infrastructure.SQLite.L0/ — logic above the persistence seam, seam mocked
    olympiad-quizzer-net.Infrastructure.SQLite.L1/ — storage tests against a real database file
  App/
    olympiad-quizzer-net.App.API/             — ASP.NET Core API, Controllers/, Extensions/, middleware, Dockerfile
    olympiad-quizzer-net.App.Client/          — Blazor WASM frontend, feature folders
    olympiad-quizzer-net.App.API.L0/          — controller tests, repository mocked
    olympiad-quizzer-net.App.API.L1/          — controller tests, hand-constructed (no test host)
    olympiad-quizzer-net.App.API.L2/          — whole app, real pipeline, over HTTP
  Solution/
    olympiad-quizzer-net.Solution.DataIntegrityTests/ — the committed artefacts, not code
    olympiad-quizzer-net.Solution.BankSync/           — console tool: regenerates questions.db from questions.json
docs/
  adl/          — Architecture Decision Log
  standards/    — coding standards; read every file
  integrations/ — GitHub Pages, Render.com, GitHub Actions
  rules/        — competition rules (machine-readable)
.github/workflows/ — ci.yml, deploy-backend.yml, deploy-frontend.yml
```

## How it deploys

Both deploys are **manual** (`workflow_dispatch`) — no automatic deploys on push.

| Component | Platform | Workflow |
|---|---|---|
| Frontend (Blazor WASM) | GitHub Pages | `.github/workflows/deploy-frontend.yml` |
| API (ASP.NET Core) | Render.com (Docker) | `.github/workflows/deploy-backend.yml` |

Live frontend: `https://leafsoftwarepoland.github.io/olympiad-quizzer-net/`
Live API: `https://olympiad-quizzer-net-api.onrender.com`

A content-only change still needs a backend deploy — the bank ships inside the container image.

## Documentation

- [Coding standards](docs/standards/INDEX.md) — read every file listed, in full, before writing code
- [Architecture Decision Log](docs/adl/INDEX.md) — all architectural decisions
- [Architecture guide](docs/architecture-guide.md) — layer rules, test levels, document types
- [Integrations](docs/integrations/INDEX.md) — GitHub Pages, Render.com, GitHub Actions
