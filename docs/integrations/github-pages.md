# GitHub Pages — Frontend Hosting

## What it is / why we use it

GitHub Pages hosts the Blazor WASM frontend as a static site, served from the `leafsoftwarepoland/olympiad-quizzer-net` repo. Chosen because it is free permanently, CDN-backed, and requires no card. See ADR-006 for the full platform comparison.

Live frontend: `https://leafsoftwarepoland.github.io/olympiad-quizzer-net/`

## How deployment works

Deploy is manual only (`workflow_dispatch`). No push trigger — see ADR-024.

The `deploy-frontend.yml` workflow runs two jobs:

**Job: build** (`ubuntu-latest`)
1. `actions/checkout@v7.0.1`
2. `actions/setup-dotnet@v6.0.0` (dotnet 10.0.x)
3. `dotnet publish source/App/olympiad-quizzer-net.Client/olympiad-quizzer-net.Client.csproj -c Release -o publish-out`
4. Post-publish fixups (PowerShell):
   - Rewrites `<base href>` in `index.html` to `/olympiad-quizzer-net/` (required for Blazor Router on a subpath)
   - Copies `index.html` → `404.html` (GitHub Pages serves 404.html on unknown routes; Blazor Router handles the rest client-side)
   - Creates `.nojekyll` (empty file — prevents GitHub from running Jekyll on the output)
   - Writes `appsettings.json` with `ApiBaseUrl` injected from `RENDER_API_URL` secret
5. `actions/upload-pages-artifact@v5.0.0` — packages `publish-out/wwwroot`

**Job: deploy** (`ubuntu-latest`, needs: build)
1. `actions/deploy-pages@v5.0.0` — pushes the artifact to the GitHub Pages environment

Both jobs run on `ubuntu-latest`. Self-hosted runner is NOT used for this workflow — see Gotchas.

## GitHub Pages setup (what was required)

1. Repo Settings → Pages → Source: GitHub Actions (not branch deploy).
2. `pages` write permission + `id-token` write permission declared in workflow.
3. `concurrency: group: pages` with `cancel-in-progress: true` — prevents overlapping deploys.
4. Environment named `github-pages` in the deploy job — required by `deploy-pages` action.

## Secrets used

| Name | Purpose | Where it lives | How to rotate |
|---|---|---|---|
| `RENDER_API_URL` | Injected into `appsettings.json` at build time | GitHub repo secrets | Update if Render URL changes |

No deploy hook — GitHub Pages deploy is fully managed by the Actions workflow + GitHub token.

## Gotchas

- **Base href**: must be `/olympiad-quizzer-net/` with trailing slash. Without the slash, Blazor Router 404s on direct URL access (e.g. refresh on a non-root route).
- **404.html**: GitHub Pages serves `404.html` when a path is not found. Copying `index.html` to `404.html` lets Blazor Router handle client-side routing after the initial load.
- **`.nojekyll`**: when Pages source is "GitHub Actions", Jekyll does not run — but the file is harmless and Microsoft guidance still recommends it. See A-06 in assumptions.
- **Self-hosted runner**: `upload-pages-artifact` calls `tar --hard-dereference`, which fails on the self-hosted Windows runner (bsdtar). Both jobs run on `ubuntu-latest` to avoid this. Tech debt: see ADR-026.
- **setup-dotnet on self-hosted**: would fail (no write access to `C:\Program Files\dotnet`). Irrelevant here since both jobs run on `ubuntu-latest`, but noted for CI (`ci.yml` omits setup-dotnet on self-hosted).
- **Fingerprinting**: `<CompressionEnabled>false</CompressionEnabled>` is set in the client project to avoid a .NET 10 WASM fingerprinting defect. See ADR-027.
- **Manual deploy only**: push to `main` does NOT trigger a frontend deploy. See ADR-024.

## Org-level robots.txt (one-time setup)

The Blazor app is deployed at a subpath (`/olympiad-quizzer-net/`). Search engines treat
`robots.txt` as authoritative only from the domain root — the app's own `wwwroot/robots.txt`
is served at `/olympiad-quizzer-net/robots.txt` and is therefore **not** domain-root authoritative.
The `<meta name="robots" content="noindex">` tags on quiz/settings pages are more reliable,
but adding the domain-root robots.txt is belt-and-suspenders.

**Step-by-step (one time only):**

1. Open `github.com/LeafSoftwarePoland/leafsoftwarepoland.github.io` in a browser.
   - If the repo does not exist: create it — the repo name must be exactly `leafsoftwarepoland.github.io` (all lowercase). Enable GitHub Pages: Settings → Pages → Source: Deploy from branch → `main` / `/ (root)`.
2. Create (or edit) the file `robots.txt` at the repo root with this content:
   ```
   User-agent: *
   Disallow: /olympiad-quizzer-net/quiz
   Disallow: /olympiad-quizzer-net/quiz/
   Disallow: /olympiad-quizzer-net/settings
   ```
3. Commit to `main`. Pages picks it up immediately (no workflow needed).
4. Verify: open `https://leafsoftwarepoland.github.io/robots.txt` in a browser and confirm the file is live.

Note: expand the `Disallow` list here whenever new app paths should be excluded from search indexing.

## Links

- Live URL: `https://leafsoftwarepoland.github.io/olympiad-quizzer-net/`
- ADR-006 (platform decision): `docs/adl/ADR-006-github-pages-hosting.md`
- ADR-024 (manual-only deploy): `docs/adl/ADR-024-deploy-frontend-manual-only.md`
- Deploy workflow: `.github/workflows/deploy-frontend.yml`
