# Journal — olympiad-quizzer-net

## 2026-08-08 — Orchestrator — Pipeline adoption bootstrap

Adopted existing repo into pipeline. Project was designed from scratch in a single brainstorming session — 19 ADRs written covering all major decisions (framework, hosting, schema, accessibility, responsive design, language policy, etc.). Design spec written at `docs/specs/2026-08-08-olympiad-quizzer-poc-design.md`.

Weight class: S (POC, 1-3 days, single deliverable).

GitHub repo: https://github.com/LeafSoftwarePoland/olympiad-quizzer-net
- GitHub Pages configured (source: GitHub Actions)
- Render.com service: https://olympiad-quizzer-net-api.onrender.com (Auto-Deploy: Off, manual via deploy hook)
- Self-hosted GHA runner: Dell Latitude 5501, Windows 10, .NET 10 SDK
- Secrets configured: RENDER_API_URL, RENDER_DEPLOY_HOOK

ADRs live at `docs/adl/` (not `.pipeline/decisions/`) — existing convention preserved per adopt policy.

Dispatching Architect to formalize solution-design.md + test-strategy.md + sprint-backlog.md from existing design context.

---

## 2026-08-08 — Architect — Design formalized, sprint-01 backlog ready

Formalized the brainstorming-session design into pipeline artifacts. Weight class S, so the sprint-backlog IS the implementation plan — I wrote T-00 through T-14 with acceptance criteria, exact file paths, and the code snippets where a wrong guess costs an afternoon (Blazor base href, PowerShell BOM, Render `PORT` binding, Docker build context, CORS localhost predicate).

Discovery: scanned the global KB first — all seven notes are Python/Windows-shell/OAuth patterns, nothing transferable to a .NET web POC, so no snapshot taken and no `discovery/` folder. Ran three parallel research passes instead (Blazor WASM on GitHub Pages under .NET 10; Render.com free tier; GitHub Actions on a self-hosted Windows runner) and synthesized them into the backlog rather than storing raw output. Verified the `dotnet new` template names directly against the installed SDK instead of trusting a research answer — `blazorwasm` is the standalone template, `blazor` is the Web App one, which is not what we want.

**The design spec contradicted itself and I had to resolve it.** Its mock-question table gave `options: null` for the true/false and matching questions while also listing the statements and the left-hand column those questions plainly need. There is no `statements` or `left` field in ADR-011's schema, so the Implementor would have guessed, and the guess would have hardened simultaneously into `questions.json`, the renderer and the grader. Wrote ADR-022: `options` is the indexable list for every type, with worked tables for the ordering and matching index directions so `[1,3,0,2]` has exactly one reading. Marked it as expected to iterate with the user — answer encodings usually need two or three rounds once real questions arrive.

Two more ADRs. ADR-020 because shipping an API in the POC contradicts ADR-002 outright, and I would rather amend that in writing than let the conflict sit unremarked; the framing is that ADR-002's content-delivery decision survives and only the server of the file changes, which keeps the static-file fallback live through the ADR-003 seam. ADR-021 for the shared class library, which honestly sits close to a default — I flagged it in my own critique as the thinnest of the three.

**I found a real bug in my own grader spec before handing off.** The first draft computed `awarded = matched == total ? max : 0`, and the position-matching helper returns `(0, 0)` for an empty `correctAnswer` — so `0 == 0` awarded full marks on a malformed question, to any submission, including an empty one. Caught by running the pseudocode against an extreme edge instead of the happy path. Added a `total <= 0` guard and test 27b to keep it fixed. The draft was otherwise finished; this is exactly why the self-read rule exists.

Two risks I am handing forward rather than pretending away. The `actions/upload-pages-artifact` action calls `tar --hard-dereference`, which the bsdtar commonly bundled with Git for Windows rejects — open GitHub issue, no fix. That directly threatens the "self-hosted everywhere" instruction, so T-00 opens with a ten-second `tar --version` check and T-12 carries a documented escape hatch to `ubuntu-latest`. I chose not to raise this as a `decision-needed.md` blocker because a command answers it faster than the user could; my critique notes that a stricter reading would have asked. Separately, current Render docs mention a $1 card-verification hold, which cuts against the "no card" premise ADR-007 was built on — that is manual check M11 and it is the single most valuable thing this POC will tell us.

One thing I am not comfortable with: my two research passes disagreed on the GitHub Actions major version tags. I wrote one set into the workflows and instructed the Implementor to overwrite them from the live API in T-00. It works, but a workflow file with plausible-looking wrong tags is the kind of thing people trust on sight. Placeholders that fail loudly would have been the better call.

Handoff: Implementor, direct, no Sprint Planner. Self-critique at `.pipeline/critiques/C-001-architect-poc-design.md`.

---

## 2026-08-08 — Implementor — Sprint-01 T-00/T-12/T-13 complete

T-00 preflight results:
- tar: GNU tar 1.35 on self-hosted runner — `runs-on: self-hosted` retained in both workflow jobs, no ubuntu-latest escape hatch needed
- GHA action tags resolved via `gh api`: checkout@v7.0.1, setup-dotnet@v6.0.0, configure-pages@v6.0.0, upload-pages-artifact@v5.0.0, deploy-pages@v5.0.0

T-12: Wrote `.github/workflows/deploy-frontend.yml`. Triggers on push to main (source/client/** or source/shared/**) + workflow_dispatch. Two-job pipeline: build (self-hosted) → deploy. Encodes three known traps: trailing slash on base href (/olympiad-quizzer-net/), BOM-free appsettings.json via System.IO.File.WriteAllText with UTF8Encoding(false), source/shared/** path filter so a Shared change redeploys the client.

T-13: Wrote `.github/workflows/deploy-backend.yml`. workflow_dispatch only (ADR-020: no auto-deploy). POSTs to RENDER_DEPLOY_HOOK, waits 30s for old container to stop answering, polls /healthz for up to 5 minutes.

Sprint-01 code complete. 49 automated tests passing. Two GHA workflows written. Handoff: Repo Manager — push branch, create PR.
