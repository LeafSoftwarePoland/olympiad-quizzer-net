# GitHub Actions — CI/CD Platform

## What it is / why we use it

GitHub Actions is the CI/CD platform for this repo. It runs build, test, and deploy workflows. The repo uses a mix of a self-hosted Windows runner (for CI) and GitHub-hosted `ubuntu-latest` runners (for frontend deploy).

## Workflows

| Workflow | File | Trigger | Runner | Purpose |
|---|---|---|---|---|
| CI | `ci.yml` | push / PR to `main` | `self-hosted` | Build + test |
| Version bump | `version-bump.yml` | `pull_request` (`opened` only) | `ubuntu-latest` | Bump the patch in `Directory.Build.props` when the branch still matches base |
| Deploy backend | `deploy-backend.yml` | `workflow_dispatch` | `self-hosted` | Trigger Render deploy hook + poll `/healthz` |
| Deploy frontend | `deploy-frontend.yml` | `workflow_dispatch` | `ubuntu-latest` | Publish WASM → GitHub Pages |

**`version-bump.yml` listens to `opened` and nothing else.** It pushes a commit to the pull-request
branch, which raises `synchronize`; since no workflow listens to that, the push cannot retrigger the
bump. Changing the trigger list without re-checking this is how that becomes an infinite loop.

Neither deploy takes a version input. Both read `Directory.Build.props` from the commit being
deployed (ADR-026 amendment). The tag step skips silently when the tag already exists, so
redeploying an unchanged commit is not a failure.

## Self-hosted runner

The CI and backend deploy jobs run on a Dell Latitude laptop registered as a self-hosted GitHub Actions runner.

**Why self-hosted:**
- No GitHub-hosted runner minutes consumed for frequent CI builds.
- .NET 10 SDK pre-installed — no `setup-dotnet` step needed (and it would fail without write access to `C:\Program Files\dotnet`).
- Windows environment mirrors the local development machine.

**Registration**: runner registered via `github.com/<org>/<repo>/settings/actions/runners`. The runner service runs as a background service on the machine.

**When the machine is off**: queued workflow runs wait. CI will be blocked until the machine wakes. No auto-queuing timeout by default — jobs stay pending until the runner comes online or the workflow times out (6 h).

## Frontend deploy runner choice

Both `build` and `deploy` jobs in `deploy-frontend.yml` run on `ubuntu-latest`, not self-hosted.

**Reason**: `actions/upload-pages-artifact` calls `tar --hard-dereference`, which `bsdtar` (Git Bash on Windows) does not support. Running on GitHub-hosted Ubuntu avoids this entirely.

**Provisional, and not tech debt either**: ADR-017 rejects the PATH-reordering workaround on the self-hosted machine — that fix lives in undiffable machine state and can vanish on reboot, so it stays rejected regardless. The target is still one machine running everything, which needs the local toolchain (Docker, WSL) fixed first. Whether that happens or hosted minutes prove sufficient is the open question in ADR-017.

Public repo = GitHub-hosted runners are free. No minute budget concern.

## CI job details (`ci.yml`)

- **build-and-test** (`self-hosted`):
  - `dotnet build OlympiadQuizzer.slnx -c Release`
  - `dotnet test OlympiadQuizzer.slnx -c Release --no-build`
  - No `setup-dotnet` step — SDK pre-installed on runner.
- **build-docker** — omitted. Docker Desktop not available on the self-hosted runner (permission denied on `npipe://...`). Dockerfile is exercised on every Render deploy.

## Backend deploy job details (`deploy-backend.yml`)

- Runs on `self-hosted` (PowerShell).
- Sends HTTP POST to `$RENDER_DEPLOY_HOOK`.
- Waits 30 s, then polls `GET /healthz` every 15 s for up to 5 min.
- Fails the workflow if `/healthz` does not return 200 within the deadline.

## Secrets used

| Name | Used in workflow | Purpose |
|---|---|---|
| `RENDER_DEPLOY_HOOK` | `deploy-backend.yml` | Unauthenticated trigger for Render deploy |
| `RENDER_API_URL` | `deploy-frontend.yml` | Injected into `appsettings.json` at frontend build time |

## Action versions pinned

All actions are pinned to full `vX.Y.Z` tags (not floating `@vX`):

| Action | Version |
|---|---|
| `actions/checkout` | `v7.0.1` |
| `actions/setup-dotnet` | `v6.0.0` |
| `actions/upload-pages-artifact` | `v5.0.0` |
| `actions/deploy-pages` | `v5.0.0` |

Pinned to exact patch versions, not floating majors. Reason: the published tags for these actions were ambiguous during setup — a major-only tag resolved differently than the docs implied — so each value above was verified against a real run before being pinned. Re-verify on bump; do not float.

## Gotchas

- **Runner offline**: all `self-hosted` jobs block silently. Check if the Dell Latitude is on and the runner service is running.
- **Docker on self-hosted**: not available. `build-docker` job is intentionally omitted from `ci.yml`. Render deploy exercises the Dockerfile.
- **setup-dotnet on self-hosted**: removed from `ci.yml` because the action needs write access to `C:\Program Files\dotnet`. SDK is pre-installed globally.
- **tar/bsdtar on self-hosted Windows**: blocks `upload-pages-artifact`. Solved by running frontend deploy on `ubuntu-latest`. See ADR-017.

## Links

- ADR-004 (GitHub Pages): `docs/adl/ADR-004-github-pages-hosting.md`
- ADR-005 (Render.com): `docs/adl/ADR-005-render-com-api-hosting.md`
- ADR-015 (manual-only frontend deploy): `docs/adl/ADR-015-frontend-deploy-manual-only.md`
- ADR-017 (runner allocation): `docs/adl/ADR-017-runner-allocation.md`
- Workflows: `.github/workflows/`
