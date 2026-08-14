# Render.com — API Hosting

## What it is / why we use it

Render.com hosts the ASP.NET Core API (`OlympiadQuizzer.Api`) as a Docker container on its free tier. Chosen because it requires no credit/debit card, is Docker-native, and provides 512 MB RAM — sufficient for a lean Dapper-based API. See ADR-005 for the full platform comparison.

Live API: `https://olympiad-quizzer-net-api.onrender.com`

## How deployment works

Deploy is triggered manually via `deploy-backend.yml` (`workflow_dispatch`). The workflow:

1. Sends an HTTP POST to the Render deploy hook URL (stored as `RENDER_DEPLOY_HOOK` secret).
2. Render pulls the repo, runs the multi-stage Dockerfile, and starts the new container.
3. The workflow polls `GET /healthz` every 15 s (up to 5 min) until it returns HTTP 200.

Render never auto-deploys from git push — deploy hook only.

### Dockerfile (multi-stage)

```
Build stage:  mcr.microsoft.com/dotnet/sdk:10.0
              dotnet publish source/App/olympiad-quizzer-net.App.API/olympiad-quizzer-net.App.API.csproj -c Release
Runtime stage: mcr.microsoft.com/dotnet/aspnet:10.0
              Port: 10000 (Render default for free tier)
              ASPNETCORE_ENVIRONMENT=Production
```

`source/Core/` and `source/Infrastructure/` are copied into the build context alongside `source/App/olympiad-quizzer-net.App.API/` so all project references build inside Docker. `data/` is copied in the **runtime** stage, not the build stage, so a content-only change rebuilds one layer instead of the application — the API then resolves `data/questions.db` relative to the application base directory. The Dockerfile lives at `source/App/olympiad-quizzer-net.App.API/Dockerfile`.

### What the API serves

- `GET /healthz` — plain 200, and the commit the host built. Used by the deploy workflow and by the keep-alive ping. Unversioned by requirement — the workflow polls a fixed path.
- `GET /robots.txt` — disallows everything. Unversioned: crawler rules are only honoured at a host root.
- `GET /v1/questions` — filtered, shuffled, capped question payload as JSON.
- `GET /v1/filters` — filter values present in the bank, with counts.

CORS is configured to allow requests from the GitHub Pages origin (`https://leafsoftwarepoland.github.io`).

## Free tier limits

| Limit | Value |
|---|---|
| RAM | 512 MB |
| CPU | 0.1 shared |
| Instance hours | 750/month (~31 days continuous) |
| Idle spin-down | 15 min |
| Cold start | ~35 s measured |
| Persistent disk | No |
| Card required | No — verified 2026-08-09 (M11) |

UptimeRobot pings every 5 min to prevent spin-down. (UptimeRobot has not been implemented, it simply stays as a suggesting for now to keep the API always on, not needed now).

## Setup (what was required)

1. Render account created at render.com (no card).
2. New Web Service created → Docker runtime → connected to this GitHub repo.
3. Build command: leave blank (Dockerfile handles everything).
4. Port set to `10000` (Render default).
5. Deploy hook URL generated in Render dashboard → stored as `RENDER_DEPLOY_HOOK` in GitHub repo secrets.
6. API base URL stored as `RENDER_API_URL` in GitHub repo secrets (used by frontend deploy to inject `appsettings.json`).

## Secrets

| Name | Purpose | Where it lives | How to rotate |
|---|---|---|---|
| `RENDER_DEPLOY_HOOK` | Triggers a Render deploy | GitHub repo secrets | Render dashboard → Service → Settings → Deploy Hook → regenerate → update secret |
| `RENDER_API_URL` | API base URL injected into frontend `appsettings.json` at deploy time | GitHub repo secrets | Update if Render URL changes (unlikely on free tier) |

Never commit the full deploy hook URL — it is an unauthenticated trigger.

## Gotchas

- **Cold start**: ~35 s. UI must show a "waking up…" state. UptimeRobot mitigates but does not eliminate (overnight, weekends). See ADR-019.
- **No persistent disk**: `questions.json` is baked into the Docker image at build time, not mounted. Updating questions requires a redeploy.
- **750 instance-hours/month**: at 24/7 continuous that is ~31 days. UptimeRobot keeps the instance warm, which counts against the limit. Free PostgreSQL expires after 30 days — not used (SQLite or flat JSON only).
- **Port 10000**: Render free tier expects the app on port 10000. `ASPNETCORE_URLS` must match or Render will report unhealthy.

## Links

- Render dashboard: https://dashboard.render.com
- Free tier docs: https://render.com/docs/free
- ADR-005 (platform decision): `docs/adl/ADR-005-render-com-api-hosting.md`
- ADR-013 (API posture): `docs/adl/ADR-013-api-posture-read-only.md`
- Deploy workflow: `.github/workflows/deploy-backend.yml`
