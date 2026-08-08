# POC: Olympiad Quizzer .NET

**Date:** 2026-08-08
**Scope:** Phase 1 POC — prove Blazor WASM + Render.com deployment end-to-end with all question types rendered
**Development approach:** Agentic — designed and implemented using Claude Code (claude-sonnet-4-6) with human review at decision points. ADRs serve as the decision record.
**Purpose of this document:** POC plan + test results. Referenced from ADRs as deployment proof.

---

## Goal

Prove the full stack works before committing to full content entry:
- Blazor WASM builds and deploys to GitHub Pages
- ASP.NET Core minimal API deploys to Render.com via GHA deploy hook
- CORS between the two works
- All 6 question types render and grade correctly
- No hidden hosting costs

---

## Architecture

```
Browser
  └── Blazor WASM (GitHub Pages: LeafSoftwarePoland.github.io/olympiad-quizzer-net)
        └── HTTP GET /api/questions
              └── ASP.NET Core API (Render.com: olympiad-quizzer-net-api.onrender.com)
                    └── returns mock questions.json (embedded resource)
```

Stateless. No database in POC. Questions hardcoded in API.

---

## Repo layout

```
/
├── source/
│   ├── api/                          ← ASP.NET Core minimal API
│   │   ├── Dockerfile
│   │   ├── OlympiadQuizzer.Api.csproj
│   │   ├── Program.cs
│   │   └── Data/questions.json
│   └── client/                       ← Blazor WASM
│       ├── OlympiadQuizzer.Client.csproj
│       ├── Program.cs
│       ├── Pages/
│       └── wwwroot/
├── docs/
│   ├── adl/
│   └── superpowers/specs/
├── .github/workflows/
│   ├── deploy-frontend.yml
│   └── deploy-backend.yml
└── OlympiadQuizzer.sln
```

---

## Question schema

JSON keys: **camelCase**. Full schema in ADR-011.

```json
{
  "id": "string",
  "source": "oij | vea | other",
  "competition": "string",
  "voivodeship": "string | null",
  "stage": "int | null",
  "year": "string",
  "type": "multiSelect | shortAnswer | singleAbcd | trueFalse | ordering | matching",
  "content": [{ "type": "text | code | image", "text": "..." }],
  "contentCpp": null,
  "options": ["string"] | null,
  "matchOptions": ["string"] | null,
  "correctAnswer": "(varies by type)",
  "points": 1,
  "partialCredit": false,
  "tags": [],
  "sourceUrls": [],
  "explanation": null
}
```

---

## Mock questions (1 per type)

| Type | Polish label | correctAnswer |
|---|---|---|
| `multiSelect` | "Wielokrotny wybór — zaznacz A i B" | `[0, 1]` |
| `singleAbcd` | "Jednokrotny wybór — poprawna C" | `[2]` |
| `shortAnswer` | "Odpowiedź otwarta — wpisz: kajak" | `["kajak"]` |
| `trueFalse` | "Prawda/fałsz — 3 twierdzenia" | `[true, false, true]` |
| `ordering` | "Kolejność — ułóż elementy" | `[2, 0, 3, 1]` |
| `matching` | "Dopasowanie — 3 pary" | `[1, 2, 0]` |

No real content. Labels describe the question type so the POC is self-documenting.

---

## App screens

1. **Landing** — app title, "Rozpocznij quiz" button
2. **Question** — progress indicator (`1 / 6`), question text, type-appropriate input
3. **Answer reveal** — verdict (green POPRAWNIE / red BŁĄD), correct answer shown
4. **Results** — score (`X / 6`), per-question breakdown table, restart button

No timer in POC.

---

## Styling

Port of Python app (`python/static/css/style.css`):

| Token | Value |
|---|---|
| `--bg` | `#0d0d0d` |
| `--bg-code` | `#141414` |
| `--accent` | `#00ff41` |
| `--text` | `#d8d8d8` |
| `--text-dim` | `#888` |
| `--verdict-red` | `#ff4444` |
| `--font` | `ui-monospace, "Cascadia Code", Consolas, monospace` |

Buttons: transparent border, invert on hover. Code blocks: dark bg + green left border. No gradients, no images.

Bootstrap 5 for responsive grid only — all colours/fonts override Bootstrap tokens.

---

## API endpoints

```
GET /healthz         → 200 { "ok": true }
GET /api/questions   → 200 Question[]
```

CORS: allow `https://leafsoftwarepoland.github.io` and `http://localhost:*` (dev).

---

## GitHub Actions pipelines

### deploy-frontend.yml
```
on: push to main (source/client/**)
jobs:
  build:
    - dotnet publish source/client -c Release
    - inject RENDER_API_URL into wwwroot/appsettings.json
    - upload pages artifact
  deploy:
    - deploy to GitHub Pages
```

### deploy-backend.yml
```
on: workflow_dispatch (manual only)
jobs:
  deploy:
    - POST to RENDER_DEPLOY_HOOK secret
    - wait for health check https://olympiad-quizzer-net-api.onrender.com/healthz
```

---

## Deployment targets

| | Frontend | Backend |
|---|---|---|
| **Host** | GitHub Pages | Render.com |
| **URL** | `LeafSoftwarePoland.github.io/olympiad-quizzer-net` | `olympiad-quizzer-net-api.onrender.com` |
| **Cost** | $0, no card | $0, no card |
| **Deploy trigger** | Push to main | Manual (`workflow_dispatch`) |
| **Sleep** | Never | 15 min idle (free tier) |

---

## What this POC proves

- [ ] Blazor WASM compiles and serves from GitHub Pages
- [ ] All 6 question types render correctly
- [ ] All 6 question types grade correctly
- [ ] Render.com accepts Docker deploy via GHA
- [ ] CORS between GitHub Pages → Render.com works
- [ ] No credit card required, no surprise costs
- [ ] GHA pipelines work independently for frontend and backend

---

## Out of scope for POC

- Real questions
- Timer / contest simulation mode
- SQLite / Dapper
- PWA / service worker
- User accounts
- Image blocks
- OIJ Python/C++ toggle

---

## POC test results

*Fill in after deployment. Attach link to this file from ADR-006 and ADR-007 as deployment proof.*

**Date tested:**
**Frontend URL:**
**Backend URL:**

| Check | Result | Notes |
|---|---|---|
| Blazor WASM builds and deploys to GitHub Pages | | |
| All 6 question types render | | |
| All 6 question types grade correctly | | |
| Render.com Docker deploy via GHA succeeds | | |
| CORS frontend → backend works | | |
| `/healthz` returns 200 | | |
| No credit card required / $0 cost confirmed | | |
| Self-hosted runner executed pipelines | | |

**Issues found:**

**Verdict:** PASS / FAIL
