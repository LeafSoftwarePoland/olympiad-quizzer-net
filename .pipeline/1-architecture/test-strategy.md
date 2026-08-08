# Test Strategy — olympiad-quizzer-net (Phase 1 POC)

**Weight class**: S
**Framework**: xUnit (`xunit.v3` if available on .NET 10, else `xunit` 2.x — Implementor picks whichever `dotnet new xunit` scaffolds)
**Project**: `source/tests/OlympiadQuizzer.Tests.csproj` — one test project, references `Shared` + `Api`
**execution_mode**: `local`
**CI**: `build_only` — GHA workflows deploy; they do not run tests. Free-tier / self-hosted-runner minutes are not spent on a test gate for a POC. Implementor runs `dotnet test` locally before handing off.

Run: `dotnet test OlympiadQuizzer.sln`

---

## Levels in scope

| Level | In POC? | Why |
|---|---|---|
| L0 unit | **yes** | Grader is the only real logic in the build. Cheap, fast, catches the index-semantics mistakes ADR-022 exists to prevent |
| L1 integration (in-process API) | **yes** | Two endpoints. `WebApplicationFactory` in-process, no network, no container |
| L2 (containers / DB) | **no** | No database (ADR-020 scope guard). Nothing to stand up |
| L3 (end-to-end browser) | **no** | Automated E2E costs more than the POC is worth. Covered by the manual checklist below |
| Contract | **no** | One producer, one consumer, one repo, both compile against the same `Shared` types (ADR-021). The compiler *is* the contract test |

No L2 suite → no L2 README required.

---

## L0 — unit tests

`source/tests/GraderTests.cs`

Per type, at minimum: one correct, one incorrect, one edge.

| # | Test | Expect |
|---|---|---|
| 1 | `MultiSelect_ExactSet_IsCorrect` — submit `[0,1]` for `correctAnswer [0,1]` | correct, 1.0 pts |
| 2 | `MultiSelect_OrderIrrelevant` — submit `[1,0]` | correct |
| 3 | `MultiSelect_Subset_IsIncorrect` — submit `[0]` | incorrect, 0 pts |
| 4 | `MultiSelect_Superset_IsIncorrect` — submit `[0,1,2]` | incorrect |
| 5 | `SingleAbcd_Correct` — submit `[2]` for `[2]` | correct |
| 6 | `SingleAbcd_Wrong` — submit `[0]` | incorrect |
| 7 | `SingleAbcd_Empty_IsIncorrect` — submit `[]` | incorrect, not an exception |
| 8 | `ShortAnswer_ExactMatch` — `"kajak"` | correct |
| 9 | `ShortAnswer_CaseInsensitive` — `"KAJAK"`, `"Kajak"` | correct |
| 10 | `ShortAnswer_TrimsWhitespace` — `"  kajak  "` | correct |
| 11 | `ShortAnswer_NfcNormalization` — decomposed Polish input (`"ł"`, `"ó"` as combining sequences, e.g. `"ó"` vs `"ó"`) matches precomposed expected | correct |
| 12 | `ShortAnswer_AcceptsAnyListedForm` — `correctAnswer ["AF₁₆","AF16"]`, submit `"af16"` | correct |
| 13 | `ShortAnswer_Wrong` — `"rower"` | incorrect |
| 14 | `ShortAnswer_NullOrEmpty_IsIncorrect` | incorrect, no exception |
| 15 | `TrueFalse_AllCorrect` — `[true,false,true]` | correct |
| 16 | `TrueFalse_OneWrong` — `[true,true,true]`, `partialCredit=false` | incorrect, **0 pts** |
| 17 | `TrueFalse_OneWrong_WithPartialCredit` — same, `partialCredit=true` | incorrect, **2/3 pts** |
| 18 | `TrueFalse_Unanswered_IsIncorrect` — a `null` element | incorrect |
| 19 | `Ordering_Correct` — submit `[1,3,0,2]` for `poc-5` | correct |
| 20 | `Ordering_TwoSwapped` — `[3,1,0,2]`, `partialCredit=false` | incorrect, 0 pts |
| 21 | `Ordering_TwoSwapped_WithPartialCredit` | 2/4 pts |
| 22 | `Ordering_WrongLength_IsIncorrect` | incorrect, no exception |
| 23 | `Matching_Correct` — `[2,1,0]` for `poc-6` | correct |
| 24 | `Matching_Unanswered_IsIncorrect` — contains `-1` | incorrect |
| 25 | `Matching_Partial_WithPartialCredit` — one right of three | 1/3 pts |
| 26 | `UnknownType_ReturnsIncorrect_DoesNotThrow` | `(false, 0, points)` |
| 27 | `Grade_NullSubmission_IsIncorrect` | incorrect, no exception |
| 27b | `MalformedQuestion_EmptyCorrectAnswer_IsIncorrect` — `ordering` with `correctAnswer: []`, submit `[]` | **incorrect, 0 pts** — not correct. Guards the `total <= 0` branch in T-03; without it `0 == 0` grades an unanswerable question as right |

`source/tests/NormalizationTests.cs` — direct tests of the normalize helper (see ADR-022 for the exact pipeline and order): trim, NFC, `ToLowerInvariant`, idempotence (`normalize(normalize(x)) == normalize(x)`).

`source/tests/QuestionLoadingTests.cs`

| # | Test | Expect |
|---|---|---|
| 28 | `QuestionsJson_Deserializes` — load the real `source/api/Data/questions.json` through the shared `JsonOptions` | 6 questions, no exception |
| 29 | `QuestionsJson_AllTypesPresent` | one each of the 6 `QuestionType` values, no `Unknown` |
| 30 | `QuestionsJson_IdsUnique` | 6 distinct ids |
| 31 | `QuestionsJson_MatchesAdr022Bindings` | `shortAnswer` has `options == null`; every other type has non-null `options`; only `matching` has non-null `matchOptions` |
| 32 | `QuestionsJson_CorrectAnswerShapesValid` | per-type shape + length rules from ADR-022 hold (`trueFalse` bool count == `options` count; `ordering`/`matching` index arrays in range and, for `ordering`, a permutation) |
| 33 | `QuestionsJson_FixturesMatchAdr022WorkedExamples` | `poc-5.correctAnswer == [1,3,0,2]`, `poc-6.correctAnswer == [2,1,0]` — guards against the exact drift ADR-022 was written to stop |
| 34 | `Serialization_UsesCamelCase` | round-trip a `Question`, assert the JSON contains `"correctAnswer"` and `"matchOptions"`, not `"CorrectAnswer"` / `"correct_answer"` (ADR-011) |

Test 28 reads the shipped data file, not a copy — link it into the test project as a `Content`/`None` item with `CopyToOutputDirectory`, or resolve it by relative path from the test assembly. Copying it would let the real file rot while tests stay green.

---

## L1 — API integration tests

`source/tests/ApiEndpointTests.cs`, in-process via `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`). Minimal-API `Program.cs` needs `public partial class Program;` at the end to be addressable as a generic argument.

| # | Test | Expect |
|---|---|---|
| 35 | `Healthz_Returns200AndOkTrue` | 200, body `{"ok":true}` |
| 36 | `Questions_Returns200` | 200, `application/json` |
| 37 | `Questions_ReturnsSixQuestions` | 6 items |
| 38 | `Questions_PayloadIsCamelCase` | raw body contains `"correctAnswer"`, `"partialCredit"`, `"matchOptions"` |
| 39 | `Questions_DeserializesIntoSharedModel` | round-trips into `List<Question>` with no `Unknown` type — the real client/server contract check |
| 40 | `UnknownRoute_Returns404` | 404 |
| 41 | `Cors_AllowsGitHubPagesOrigin` — preflight `OPTIONS /api/questions` with `Origin: https://leafsoftwarepoland.github.io` | `Access-Control-Allow-Origin` echoes the origin |
| 42 | `Cors_RejectsUnknownOrigin` — `Origin: https://evil.example` | no `Access-Control-Allow-Origin` header |

Tests 41–42 matter more than they look: CORS is the single most likely cause of a POC that "works locally, blank page in production", and it is the one thing the manual browser check can only tell you *after* a full deploy cycle.

**Not tested at L1**: Blazor components. No bUnit in the POC — component behaviour is thin dispatch over the grader, which is already covered at L0. Revisit if component logic grows.

---

## Manual verification checklist

Run after the first successful deploy of both halves. Record results in the table at the bottom of `docs/specs/2026-08-08-olympiad-quizzer-poc-design.md` — that spec is cited by ADR-006 and ADR-007 as deployment proof.

**Local (before pushing):**

| # | Check |
|---|---|
| M1 | `dotnet run` API → `curl http://localhost:<port>/healthz` returns `{"ok":true}` |
| M2 | `dotnet run` client → landing page renders in terminal style (`#0d0d0d` bg, `#00ff41` accent, monospace) |
| M3 | Full quiz run: all 6 types render, accept input, grade, and reach the result page with a plausible score |
| M4 | Answer everything wrong → score `0 / 6`; answer everything right → `6 / 6` |
| M5 | Stop the API, reload the quiz → Polish error panel, not a stack trace or blank screen |

**Deployed:**

| # | Check |
|---|---|
| M6 | `https://leafsoftwarepoland.github.io/olympiad-quizzer-net/` loads the app (not a 404, not a blank page) |
| M7 | Browser DevTools → Network: `_framework/*` all 200, no 404s (guards the .NET 10 fingerprinting risk in `assumptions.md`) |
| M8 | Deep link `.../olympiad-quizzer-net/quiz` reloaded directly resolves (404.html SPA fallback works) |
| M9 | DevTools → Console: no CORS error; `/api/questions` returns 200 from the Render origin |
| M10 | Cold start: leave API idle > 15 min, then load the quiz — loading copy appears, request eventually succeeds within 90 s |
| M11 | Render dashboard: service is on the free plan, billing shows $0, and it is recorded whether a card or card-verification hold was required (this is the ADR-007 test) |
| M12 | `deploy-backend.yml` run via `workflow_dispatch` → deploy hook fires, health poll goes green |
| M13 | `deploy-frontend.yml` runs on push to `main` touching `source/client/**` and completes on the self-hosted runner |
| M14 | Mobile viewport (360 px, DevTools device emulation): no horizontal page scroll; ABCD tap targets ≥ 44×44 px (ADR-016) |
| M15 | Keyboard only: tab through a question, submit, advance — focus never lost (ADR-017) |
| M16 | Screen reader spot-check (NVDA or Windows Narrator) on one question: options announced, verdict announced via `aria-live` (ADR-017) |

M11 is the commercially load-bearing check. M16 is the one most likely to be skipped under time pressure — if it is skipped, say so in the results table rather than leaving it blank.

---

## Exit criteria

POC passes when: all L0 + L1 green locally, M1–M14 pass, M15–M16 pass or are explicitly recorded as deferred, and the results table in the design spec is filled in with a PASS/FAIL verdict.
