# Assumptions & Risks — olympiad-quizzer-net (Phase 1 POC)

Cumulative. Each entry: what is assumed, and what would invalidate it.

---

## A-01 — Render.com free tier needs no payment card ⚠ **highest risk**

**Assumed**: signing up and running a free web service requires no credit/debit card. This is the load-bearing premise of ADR-007 and the reason Fly.io and Azure F1 were eliminated.

**Contradicting evidence found during Discovery**: current Render documentation references a **$1 card-verification transaction** (refunded) for account verification. Sources are ambiguous about whether this applies to all free accounts or only some sign-up paths.

**Invalidated if**: the POC deploy (check M11 in `test-strategy.md`) shows a card or hold is required.
**Then**: ADR-007 must be amended, ADR-008 (Oracle Cloud Always Free — also needs a physical card) reconsidered, and the fallback is ADR-002's static-JSON delivery with no backend at all, which the POC keeps viable by design (ADR-020). Route as upstream feedback to Architect, do not absorb silently.

---

## A-02 — Render free tier limits hold

**Assumed**: 512 MB RAM, 0.1 CPU, 750 instance-hours/month, spin-down after 15 min idle, ~1 min spin-up.
**Source**: https://render.com/docs/free (verified 2026-08-08)
**Invalidated if**: Render changes free-tier terms, or the .NET container OOMs at 512 MB.
**Then**: the UI cold-start copy and the 90 s client timeout need re-tuning; at worst, ADR-008 is revisited.

---

## A-03 — `actions/upload-pages-artifact` works on the self-hosted Windows runner

**Assumed**: the Pages artifact upload runs on `self-hosted` as the dispatch requires.
**Known contradicting evidence**: open issue https://github.com/actions/upload-pages-artifact/issues/95 — the action calls `tar --hard-dereference`, which `bsdtar` (often what Git Bash resolves to on Windows) does not support. No official fix. Workaround: order `C:\Program Files\Git\bin` above `C:\Program Files\Git\usr\bin` in system PATH.
**Invalidated if**: T-00's `tar --version` check reports `bsdtar` and the PATH fix does not stick.
**Then**: T-12 documents the escape hatch — flip both jobs to `ubuntu-latest` (public repo, free minutes). This is a deliberate, recorded deviation from the "self-hosted everywhere" instruction, not a silent one.

---

## A-04 — GitHub Actions major version tags in T-12/T-13 are current

**Assumed**: `checkout@v7`, `setup-dotnet@v6`, `configure-pages@v6`, `upload-pages-artifact@v5`, `deploy-pages@v5`.
**Confidence: low.** Two independent Discovery passes returned *different* answers (`upload-pages-artifact` v3 vs v5, `configure-pages` v5 vs v6, `deploy-pages` v4 vs v5). One or both read stale or synthesised sources.
**Invalidated if**: any tag does not resolve.
**Then**: T-00 step 3 resolves them against the live API before the workflows are committed. Treat the numbers written in T-12 as placeholders.

---

## A-05 — .NET 10 Blazor WASM serves correctly from GitHub Pages

**Assumed**: with `<CompressionEnabled>false</CompressionEnabled>`, a corrected `<base href>`, `.nojekyll`, and `404.html`, the published output loads from a project subpath.
**Risk**: .NET 10 fingerprints static assets by default, and there is a reported standalone-WASM fingerprinting defect around `_framework` runtime files (https://github.com/dotnet/aspnetcore/issues/64359). GitHub Pages does no content negotiation, so any mismatch between what `index.html` requests and what was published is a hard 404 and a blank page.
**Invalidated if**: manual check M7 shows 404s under `_framework/`.
**Then**: first suspects, in order — missing trailing slash on `base href`; `.br`/`.gz`-only assets (should be impossible with compression off); the fingerprinting defect above, which may need `OverrideHtmlAssetPlaceholders` toggled.

Disabling compression is itself a trade: the WASM payload ships uncompressed, so first load is larger than ADR-001's ~3–4 MB Brotli estimate. Acceptable for a POC; revisit before real users.

---

## A-06 — `.nojekyll` is still needed

**Assumed**: harmless to add. When Pages source is "GitHub Actions" (as configured here), the artifact is served as-is and Jekyll never runs, so the file is probably redundant — but Microsoft's guidance still calls for it and it costs one line.
**No action either way.** Listed so nobody spends time debugging it.

---

## A-07 — Move-up/move-down buttons satisfy the ordering requirement

**Assumed**: ADR-016 (pointer-event drag) and ADR-017 (keyboard reorder) are both satisfied by `▲`/`▼` buttons, which need no JS interop and are keyboard- and touch-native.
**Invalidated if**: the user judges the POC ordering UX unacceptable, or real OIJ questions need long lists where button-clicking becomes tedious.
**Then**: drag-and-drop becomes a Phase 2 enhancement layered on top; the buttons stay as the accessible fallback.

---

## A-08 — Documented deviations from the POC design spec

All four are deliberate and reversible:

| # | Deviation | Why |
|---|---|---|
| 1 | Extra `source/shared/` project (spec's layout showed only `api/` + `client/`) | ADR-021 — one wire-format definition, fast grader tests |
| 2 | `trueFalse` and `matching` have non-null `options` (spec table said `null`) | ADR-022 — the spec table contradicted its own statement/left-column lists |
| 3 | `poc-2` carries an extra `code` content block | Otherwise the code-block styling ships untested |
| 4 | `questions.json` is a copied content file, not an embedded resource | Test 28 asserts against the shipped artifact directly |

**Invalidated if**: the user wants the spec's layout honoured literally.
**Then**: 1 and 4 are cheap to reverse; 2 is not (it is a correctness fix).

---

## A-09 — ADR-002 amended, not superseded

**Assumed**: shipping an API in the POC (ADR-020) does not overturn ADR-002's content-delivery decision — questions remain a flat JSON file, and the static-file path stays a live fallback via the ADR-003 seam.
**Invalidated if**: Phase 2 puts questions in a database and the static path is abandoned.
**Then**: ADR-002 gets a proper superseded status at that point, not now.

---

## A-10 — Client-side grading is acceptable

**Assumed**: answers travelling to the browser is fine — public olympiad material, self-practice tool, no stakes (ADR-002 rationale, ADR-015 no accounts).
**Invalidated if**: the app is ever used for scored or competitive assessment.
**Then**: grading moves server-side. `Grader` lives in `Shared` (ADR-021) specifically so that move is a routing change, not a rewrite.

---

## A-11 — Losing quiz state on browser refresh is acceptable

**Assumed**: `QuizSession` is in-memory only. Refresh mid-quiz restarts it.
**Invalidated if**: user testing shows students refresh often, or a timer mode arrives where losing progress is punitive.
**Then**: `sessionStorage` persistence via JS interop, or Blazor's `PersistentComponentState`.

---

## A-12 — `dotnet new blazorwasm -f net10.0` is the right scaffold ✅ **verified 2026-08-08**

Checked directly against the installed SDK (10.0.301) rather than assumed:
`dotnet new list blazor` → `blazorwasm` = "Blazor WebAssembly Standalone App"; `blazor` = "Blazor Web App" (the server-interactive one, **not** what we want). `dotnet new list xunit` → `xunit` present.
No longer a risk.

---

## A-13 — Short-answer normalization matches how students actually type

**Assumed**: `Trim → NFC → ToLowerInvariant`, no internal-whitespace collapsing, is enough for POC (`"kajak"`).
**Invalidated if**: real questions need multi-word answers, numeric tolerance, alternate Unicode forms (`AF₁₆` vs `AF16` — already handled by listing both), or students routinely double-space.
**Then**: extend `Grader.Normalize` and add the forms to `correctAnswer`. ADR-022 is marked as expected to iterate with the user for this reason.

---

## A-14 — Polish-only UI, no i18n

Per ADR-019. Invalidated only if a non-Polish audience appears. Then `IStringLocalizer<T>`, which is additive.

---

## A-15 — 90 s client HTTP timeout covers Render cold start

**Assumed**: ~60 s worst-case spin-up plus margin.
**Invalidated if**: M10 shows cold starts exceeding 90 s.
**Then**: raise the timeout, or adopt the UptimeRobot keep-alive ping ADR-007 already anticipates.

---

## A-16 — One concurrent user

POC. No load testing, no concurrency design. Invalidated the moment the URL is shared with a class; at that point Render's 0.1 CPU is the first thing to buckle.
