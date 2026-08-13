# olympiad-quizzer-net

Polish quiz app for the OIJ (Olimpiada Informatyczna Juniorów) — built for a child preparing for the national junior informatics olympiad.

**Tech stack:** Blazor WASM (.NET 10) frontend + ASP.NET Core API backend. Custom CSS, no frameworks. Deployed on GitHub Pages (frontend) and Render.com (API).

## Why it exists

The OIJ publishes past exam questions as PDFs. There is no interactive practice tool. This app imports those questions and lets students drill them in exam-simulation or free-learning mode, with a timer, explanations, server-side filtering by category, algorithm, year and stage, and answer feedback.

## How to run locally

**Prerequisites:** .NET 10 SDK.

**1. Run the API**

```
dotnet run --project source/App/olympiad-quizzer-net.API
```

The API listens on `http://localhost:10000` by default. Endpoints: `GET /healthz`, `GET /api/filters`, `GET /api/questions`.

In development mode (the default for `dotnet run`), the API automatically loads `appsettings.Development.json`, which points at a small 6-question fixture (`Data/dev-questions.json`) instead of the full 210-question bank. This keeps startup fast and covers all three question types — single choice, multiple choice, and short answer.

To use the full question bank locally, delete or rename `appsettings.Development.json`, or override the path:

```
dotnet run --project source/App/olympiad-quizzer-net.API -- --QuestionBank:FilePath=Data/questions.json
```

**2. Run the frontend**

```
dotnet run --project source/App/olympiad-quizzer-net.Client
```

Opens at `http://localhost:<port>`. The frontend reads `wwwroot/appsettings.json` for `ApiBaseUrl`; in development this defaults to `http://localhost:10000`.

**3. Run tests**

```
dotnet test OlympiadQuizzer.slnx -c Release
```

L0 (unit) and L1 (integration) tests — 199 total. Tests always use their own fixture files, not the dev or production question bank.

## Dev fixture

`source/App/olympiad-quizzer-net.API/Data/dev-questions.json` — 6 questions:

| # | Type | Topic |
|---|---|---|
| 1 | single | Python `def` keyword |
| 2 | single | Code tracing — arithmetic expression |
| 3 | single | Recursion — factorial |
| 4 | multi | Prime numbers (partial credit) |
| 5 | multi | Binary representation (partial credit) |
| 6 | short\_answer | GCD (NWD) calculation |

## Project structure

```
source/
  Core/
    olympiad-quizzer-net.Domain/      — domain types, grader, session logic, IQuestionRepository
  Infrastructure/
    olympiad-quizzer-net.SQLite/      — JSON question bank loader, filtering, shuffling
  App/
    olympiad-quizzer-net.API/         — ASP.NET Core minimal API, CORS, Dockerfile
    olympiad-quizzer-net.Client/      — Blazor WASM frontend, feature folders
    olympiad-quizzer-net.Domain.L0/   — domain unit tests (xUnit)
    olympiad-quizzer-net.API.L1/      — integration tests via WebApplicationFactory (xUnit)
docs/
  adl/          — Architecture Decision Log
  integrations/ — GitHub Pages, Render.com, GitHub Actions documentation
  rules/        — Competition rules (machine-readable)
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

## Documentation

- [Architecture Decision Log](docs/adl/INDEX.md) — all architectural decisions
- [Architecture guide](docs/architecture-guide.md) — layer rules, test levels, document types
- [Integrations](docs/integrations/INDEX.md) — GitHub Pages, Render.com, GitHub Actions
