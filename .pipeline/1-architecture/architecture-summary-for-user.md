# Architecture Summary — plain language

One page. Skim this; the detail lives in `solution-design.md` and `sprint-backlog.md`.

---

## What gets built

Two separate things that talk over HTTP.

**1. The website** (Blazor WASM) — lives on GitHub Pages at
`https://leafsoftwarepoland.github.io/olympiad-quizzer-net/`
It is just static files. Your browser downloads the whole app once, then runs it locally. No server involved. Never sleeps.

**2. The API** (ASP.NET Core) — lives on Render.com at
`https://olympiad-quizzer-net-api.onrender.com`
It does one useful thing: hand over the six mock questions as JSON. That is all. No database, no login, no writes.

Grading happens **in the browser**, not on the server. So the correct answers are visible to anyone who opens DevTools. Fine for a practice tool with public olympiad questions — flagged as A-10 in `assumptions.md` in case that ever changes.

## Why an API at all, when ADR-002 said "no backend"

Because ADR-007 (Render.com) was written as **"Accepted — test pending"**, and a bet you never test is a bet you find out about at the worst moment. Building the thin API now proves Render works while it is still cheap to be wrong. If Render turns out bad, the app falls back to reading a plain JSON file with a one-line change — the `IQuestionRepository` seam from ADR-003 exists for exactly this.

Written up as ADR-020.

## Three new ADRs

| ADR | What it settles |
|---|---|
| **ADR-020** | POC ships a thin API (amends ADR-002 for POC scope only) |
| **ADR-021** | A small shared code library so the API and the website cannot drift apart on the question format |
| **ADR-022** | Exactly what each question field means per type, with worked examples |

ADR-022 exists because the design spec contradicted itself. Its mock-question table said `options: null` for the true/false and matching questions, while also listing the statements and the left-hand column those questions obviously need. Left alone, the Implementor would have guessed, and the guess would have been baked into the data file, the renderer and the grader at the same time. **The fix: `options` always holds the list that answer indices point at.** ADR-022 also pins the ordering and matching index directions with worked tables, so `[1,3,0,2]` has exactly one possible reading.

## What the two pipelines do

**`deploy-frontend.yml`** — runs on every push to `main` that touches the client (or the shared code). Builds the website, points it at the API URL from the `RENDER_API_URL` secret, ships it to GitHub Pages. Fully automatic.

**`deploy-backend.yml`** — manual only, you press the button. It pings Render's deploy hook, then polls `/healthz` until it answers, up to five minutes. Manual on purpose: Render's free tier has a monthly hours budget, and automatic redeploys on every commit would waste it.

Both are written to run on your self-hosted runner, as asked. One caveat below.

## Two things to watch

**The runner may not be able to publish to Pages.** There is an open, unfixed GitHub bug: the Pages upload action uses a `tar` flag that the version of tar bundled with Git for Windows often does not support. Task **T-00** starts with a ten-second check (`tar --version` on the runner). If it says `bsdtar`, there is a PATH fix; if that fails, the workflow flips to GitHub's own Linux runners — free, since the repo is public. Written down rather than decided silently.

**Whether Render really needs no card.** Current Render docs mention a $1 verification charge (refunded). ADR-007's whole premise was "no card". Check **M11** during testing and record the answer honestly — this is the single most valuable thing the POC tells you. If a card is required, that comes back to me and ADR-007 gets amended.

## What to test after deploying

Full list is in `test-strategy.md` (M1–M16). The five that matter most:

1. The site loads at the GitHub Pages URL, and DevTools shows no 404s under `_framework/` (that is the classic Blazor-on-Pages blank-page failure).
2. No CORS error in the browser console when the site calls the API.
3. All six question types render, accept input, and grade — all-right gives `6 / 6`, all-wrong gives `0 / 6`.
4. Leave the API alone for 15 minutes, then load the quiz: you should see "Budzenie serwera…" and it should recover within a minute, not just hang.
5. Render dashboard shows $0 and you note whether a card was needed.

Record results in the table at the bottom of `docs/specs/2026-08-08-olympiad-quizzer-poc-design.md`. ADR-006 and ADR-007 both point at that table as their proof.

## What is deliberately not built

Real questions · timer · database · offline/PWA · accounts · images · Python/C++ toggle · drag-and-drop ordering (arrow buttons instead — keyboard- and touch-friendly, no JavaScript, and it satisfies the accessibility ADR).

## Effort

Fourteen tasks, one sprint. Roughly: half a day of scaffolding and models, a day of UI components, half a day of deployment plumbing. The deployment plumbing is where the surprises will be, which is why T-00 front-loads the environment checks.
