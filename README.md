# olympiad-quizzer-net

Polish quiz app for the OIJ (Olimpiada Informatyczna Juniorów) — built for a child preparing for the national junior informatics olympiad.

**Tech stack:** Blazor WASM (.NET 10) frontend + ASP.NET Core API backend. Custom CSS, no frameworks. Deployed on GitHub Pages (frontend) and Render.com (API).

## Why it exists

The OIJ publishes past exam questions as PDFs. There is no interactive practice tool. This app imports those questions and lets students drill them in exam-simulation or free-learning mode, with a timer, explanations, server-side filtering by category, algorithm, year and stage, and answer feedback.

## How to run locally

Prerequisites: .NET 10 SDK.

**Run the API:**
```
dotnet run --project App/olympiad-quizzer-net.API
```
API listens on `http://localhost:10000` by default (port injected via `PORT` environment variable on Render). Endpoints: `GET /healthz`, `GET /api/filters`, `GET /api/questions`.

**Run the frontend:**
```
dotnet run --project App/olympiad-quizzer-net.Client
```
Opens at `http://localhost:<port>`. The frontend reads `wwwroot/appsettings.json` for `ApiBaseUrl`; in development this defaults to `http://localhost:10000`.

**Run tests:**
```
dotnet test OlympiadQuizzer.slnx -c Release
```

## Project structure

```
Core/
  olympiad-quizzer-net.Domain/      — domain types, grader, session logic, IQuestionRepository
Infrastructure/
  olympiad-quizzer-net.SQLite/      — JSON question bank loader, filtering, shuffling (named for future SQLite)
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
