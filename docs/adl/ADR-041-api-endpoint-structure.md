# ADR-041: API endpoints and startup composition live outside the entry point

**Status:** Accepted
**Date:** 2026-08-14
**Amends in effect:** ADR-033 (the "split startup across partial files" rationale)

## Problem

Every route and every piece of startup configuration lives in the entry-point type, split across
`Program.cs`, `Program.Endpoints.cs` and `Program.Cors.cs`. The split is by file, not by unit: it is
one type, so nothing is independently addressable, nothing can be referenced by name from a test, and
the file set grows one partial per concern. There is also no stated rule linking an integration test
file to the production file it exercises, so the L1 folder layout is drifting from the API layout.

## Considered

**Endpoint style**

- **Classic MVC controllers** — familiar, attribute routing, per-resource classes by construction.
  Brings a second routing and model-binding stack for four routes. Decisive objection: the
  controller attribute's automatic model-state responses would replace the explicit validation
  response this API already returns, changing a wire contract frozen in ADR-035 as a side effect of
  a file reorganisation.
- **Minimal-API route groups registered by extension classes** — keeps the current routing stack,
  the current model binding and the current response shapes byte-identical. Each resource becomes a
  named, independently referenceable unit. No inheritance, no base class.
- **Status quo — partial entry-point files** — rejected by requirement, and it is what created the
  problem.

**File granularity**

- **One file per concern** (all reads in one file, all operational routes in another) — fewer files,
  but the boundary is a judgement call and drifts.
- **One file per top-level route** — mechanical, no judgement, and yields a 1:1 test mirror.
- **One file per HTTP verb** — nonsense at this size.

**Startup composition**

- **Keep in the entry point** — that is the status quo.
- **Extension classes in a dedicated folder** — one unit per startup concern, each independently
  testable, entry point reduced to composition order.

## Decision

**No `Program.*.cs` partial files. Routes live in per-route endpoint classes under `Endpoints/`;
startup configuration lives in extension classes under `Extensions/`. Minimal API throughout —
no MVC, no controller base type.**

### What stays in the entry point

Build, call the startup extensions in order, call the endpoint registrations, run. Nothing else. The
entry-point type stays `public` — the integration test host resolves the application through it — but
is no longer `partial`, because there is nothing left to split.

### Endpoint files — one per top-level route

| Route | File |
|---|---|
| `/api/questions` | `Endpoints/QuestionsEndpoints.cs` |
| `/api/filters` | `Endpoints/FiltersEndpoints.cs` |
| `/healthz` | `Endpoints/HealthEndpoints.cs` |
| `/robots.txt` | `Endpoints/RobotsEndpoints.cs` |

A file owns one top-level route and every verb and sub-path beneath it. `/healthz` and `/robots.txt`
get their own files despite being one line each — the rule has no size exemption, because a size
exemption is where the judgement call comes back.

Routes, query parameter names, status codes and response bodies are unchanged. ADR-035's contract is
not touched by this ADR.

### Startup files — one per concern

`Extensions/`, static extension classes over the service container or the built application:

| Concern | File |
|---|---|
| CORS policy and its origin predicate | `Extensions/CorsExtensions.cs` |
| HTTP JSON serializer options | `Extensions/JsonExtensions.cs` |
| Question image static-file serving | `Extensions/StaticAssetsExtensions.cs` |

Question-bank and repository registration stays in the Infrastructure project, where it already is.
The API project does not get a wrapper for it.

### L1 test mirror rule

- Test file name = production file name with `Tests` appended before the extension.
  `QuestionsEndpoints.cs` → `QuestionsEndpointsTests.cs`. Fully mechanical, no re-wording, no
  singular/plural adjustment.
- Test file folder = the production file's folder, relative to its project root. A test for a file in
  `Endpoints/` lives in `Endpoints/`.
- Test class name matches its file name, as everywhere else in the repo.
- Applies to files that have a production counterpart. Cross-cutting suites — bank integrity, mode
  definition drift — have no counterpart and keep descriptive names in their own folder.
- Test *method* naming is unchanged: `MethodName_Scenario_ExpectedResult`.

Consequence: the existing L1 `Api/` and `Infrastructure/` folders and several file names no longer
comply and are renamed in the same change.

**Pros:**
- Each route and each startup concern is a named unit, addressable from a test and from a stack trace.
- Adding a route adds a file; it never grows an existing one.
- The mirror rule makes "is this endpoint tested?" a directory listing, not a search.
- No MVC stack, no behavioural change to the frozen response contract, no new package.

**Cons:**
- Four files for four routes, two of which are one line. Deliberate.
- More files than the partial split it replaces.
- Log event categories move from the entry point to the endpoint units. The production log-level
  filter keys on the root namespace prefix, so it still matches — but any test asserting a specific
  category must be updated.
- `Program.cs` grows a short block of registration calls whose *order* is now load-bearing and no
  longer visible in one glance at the routes.

## Remarks / Sources

- ADR-033 (language posture) required the entry-point type to be `public` *and* `partial`, the latter
  specifically so startup could be split across files. The `public` requirement stands — the test host
  needs it. The `partial` requirement is dropped here; the reason for it no longer exists.
  `docs/coding-standards.md` § Program entry points updated in the same change.
- ADR-035 (filtering endpoint contract) — unchanged by this ADR, and the reason MVC controllers were
  rejected.
- ADR-020 (thin API) — the posture that makes a second routing stack disproportionate.
- ADR-038 (crawler control) — `/robots.txt` is host-scoped and must keep being served by this API.
- ADR-039 — same structural change; endpoint and extension paths above are relative to the renamed
  API project.
- `docs/architecture-guide.md` names endpoints and CORS as contents of the API ring; the layout
  detail belongs to the solution design, not to that table.

---

## Amendment — 2026-08-14: the mirror rule gains an aspect suffix and a project qualifier

**Trigger:** a review pass found that the `Endpoints/`/`Extensions/` half of the rename landed, but
the `Infrastructure/` half did not, and that executing it literally would have made the suite worse.
Investigating why produced two separate defects in the rule as written above. Both are amended here;
the decision, the endpoint layout and the startup layout are unchanged.

### Defect 1 — the rule forbids splitting one unit's tests across focused files

Taken literally, "test file name = production file name with `Tests` appended" allows exactly one
test file per production file. Three focused files covering distinct concerns of one repository unit
would have to collapse into one large file. That is a worse suite: the merged file loses the
concern boundary that currently makes a failure's subject obvious from the file name alone, and it
grows without bound as concerns are added, which is precisely the failure mode this ADR set out to
fix on the production side.

The rule also contradicted a convention the repository had already adopted before this ADR was
written: the L0 project splits one production grading unit across five files by concern, and has
done so since that unit was written. The mechanical rule would have made established, deliberate
practice non-compliant by accident.

**Ruling: an aspect suffix is permitted.** A test file name is the production file name with an
optional aspect word inserted before `Tests`. The production stem must come first and must be
spelled exactly as the production file spells it, so that a directory listing still sorts every test
for one unit together and still answers "is this unit tested?" without a search. The aspect word
names a concern, not a scenario — scenarios stay in method names, where the existing three-part
pattern already puts them. Splitting is a judgement call by design; the guard against drift is that
the stem is not a judgement call.

One file per production file remains the default. The split is justified only when the concerns are
genuinely distinct, and never as a way to name a test file after something that has no production
counterpart. Suites that legitimately have no counterpart — bank integrity, mode definition drift,
round-trip suites spanning several units — keep descriptive names under the existing carve-out
above; that carve-out is here relaxed to allow them to sit in the folder of the concern they span
rather than requiring a folder of their own.

### Defect 2 — "folder relative to its project root" is ambiguous when one test project covers two production projects

The rule assumes a test project has one production counterpart. This one does not: the L1 project
holds integration tests for both the API project and the Infrastructure project, because the tests
share a host fixture and a fixture bank. Two production projects can own same-named subfolders — both
already have folders whose names would collide, and a bare mirrored folder at the test project root
silently reads as belonging to the API, because the test project is named after the API.

So the surviving `Infrastructure/` folder was not the violation the review took it for. It was an
undocumented project qualifier doing necessary work that the rule had no vocabulary for.

**Ruling: the mirrored path is qualified by production project when, and only when, the subject is
not the test project's own counterpart.**

- Tests for the test project's own counterpart mirror the production subfolder at the test project
  root, unqualified. The project name already declares the subject. `Endpoints/` and `Extensions/`
  as they stand today are correct and are not touched.
- Tests for any other production project sit under a qualifier folder, and the production subfolder
  is mirrored beneath it. The qualifier is that project's solution-folder segment in the sense of
  ADR-039's naming form — `Infrastructure`, `Core`. Should a solution folder ever hold more than one
  production project, the qualifier extends with the distinguishing sub-name; today none does, so it
  does not.
- Support code — host fixtures, builders, fixture data — has no production counterpart and is
  unaffected.

### What the Infrastructure tests must look like

The folder keeps its name and gains the mirrored production subfolders beneath it. Files move; five
of the six keep their names.

| Current path (relative to the L1 project root) | Required path |
|---|---|
| `Infrastructure/JsonQuestionRepositoryFilteringTests.cs` | `Infrastructure/Json/JsonQuestionRepositoryFilteringTests.cs` |
| `Infrastructure/JsonQuestionRepositoryLimitTests.cs` | `Infrastructure/Json/JsonQuestionRepositoryLimitTests.cs` |
| `Infrastructure/JsonQuestionRepositoryShuffleTests.cs` | `Infrastructure/Json/JsonQuestionRepositoryShuffleTests.cs` |
| `Infrastructure/QuestionBankLoaderTests.cs` | `Infrastructure/Json/QuestionBankLoaderTests.cs` |
| `Infrastructure/FilterOptionsTests.cs` | `Infrastructure/Json/JsonQuestionRepositoryFilterOptionsTests.cs` |
| `Infrastructure/InfrastructureRegistrationTests.cs` | `Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensionsTests.cs` |

The last two are substantive corrections, not bookkeeping:

- The filter-options file is named after a result type declared in the Domain project, but it
  exercises the Infrastructure repository — the type it is named after has no test of its own
  anywhere. The current name therefore both hides its real subject and implies coverage that does
  not exist. Renamed, it becomes the fourth aspect file of the repository unit, which is what it is.
- The registration file is named for what it does rather than for the unit it exercises, and its
  production counterpart is not at the Infrastructure project root but in that project's
  dependency-injection folder. Both the name and the folder were wrong, in the same file.

Test class names follow their file names, and namespaces follow their folders, as elsewhere in the
repo; neither is a new rule and neither is restated here.

**Cost accepted:** deeper nesting in the L1 project, and one long file name. Both are consequences of
mirroring a real layout rather than a flattened summary of it, which is the property the rule exists
to buy.

### Remarks

- `docs/coding-standards.md` § API file layout restates the mirror rule in short form and is updated
  in the same change; the convention lives there, this ADR owns the structural decision.
- The L0 grading split is retroactively compliant under the aspect rule and is not renamed.
- With the mirror complete, one gap becomes visible as a directory listing rather than a search: the
  Infrastructure project's randomisation folder has no mirrored test folder. Its behaviour is
  currently asserted only indirectly, through the repository's shuffle aspect and through
  registration. Recorded here as a consequence of the rule working as intended; closing it is
  backlog scope, not this amendment's.
