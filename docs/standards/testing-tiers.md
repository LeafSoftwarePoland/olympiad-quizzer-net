# Test tiers, projects and mocking

## Test levels

Authoritative. A test sitting at the wrong level is a defect, not a style preference. Tiers are
created when needed — understand all four so you can judge which applies; do not create a tier
nobody asked for.

The purpose of tiering: each higher tier tests more of the real system. L0 is maximally granular;
L3 is maximally real. As tiers increase, substitutes are replaced with real implementations. The
number of tests also shifts: many small L0s that pin individual behaviours, few large L3s that
confirm the whole system works end-to-end.

### L0 — unit, everything mocked

- One class, or one method, under test. Every collaborator is substituted.
- **Black-box on the contract.** Test every meaningful value the actual type can carry — not
  imagined future values. `int` has min, max, `0`, `-1`, `1`. `double` also has `NaN`, `+∞`,
  `-∞`, `±0` — those exist and can reach the method, so they are tested. The rule is: test what
  the type has, but test all of it that matters.
- **Type complexity is a signal.** If a type forces you to test cases you do not actually care
  about, the type may be wrong for the job. An enum, record, or struct with restricted values
  eliminates those irrelevant cases at the source — and the reduction in test count is evidence
  the type is better suited. The 1:1 mirror rule makes this visible.
- **Error handling is part of the contract.** When something unexpected happens — an absurd
  question count, a negative size — the unit logs a description of what it saw and returns a
  **safe** value: `0`, `null`, an empty collection, or a status the UI can render. It does not
  hand a surprise to its caller.
- **One method with forty tests means the method does too much.** Split the responsibility. Do not
  delete the tests.
- Test count measures the complexity of the **code**, not the complexity of the product.
- No filesystem, no network, no DI container, no configuration, no host.

### L1 — integration, manually constructed, no DI and no middleware

- The subject is constructed **by hand**: `new QuestionsController(realRepository, mockLogger)`.
  **No `WebApplicationFactory`. No service-collection registration. No middleware, no routing, no
  model binding, no filters, no `appsettings`.**
- Loggers are always substituted — `NullLogger<T>` by default, a capturing logger when the test
  asserts a specific log event was emitted.
- External dependencies (database, filesystem, network) follow § Mocking below.
- **Three valid L1 entry points, all valuable and often coexisting:**
  - **Controller-to-bottom with real external** (default): construct the controller with all real
    in-app layers wired to the real external. Covers happy paths, error paths unrelated to the
    external, and error paths where the real external can be coerced to emit the needed data
    (e.g. a SQLite row with a bad value). Prefer this whenever the real external can produce the
    scenario under test.
  - **Controller-to-bottom with mocked external** (targeted, narrow): use only when a specific
    scenario requires external behaviour that cannot easily be produced with the real external —
    e.g. a message broker delivering a malformed body. The mock covers just that scenario, not
    the external generally. This is not a substitute for the real-external tests; it is additive.
  - **Repository layer only against the real external** (always valuable, always separate):
    exercises the actual connection, discovers behaviours the controller-to-bottom path cannot
    reach, and covers what can be covered against the live external even when some controller
    tests use a mock. No controller, no service above it.
  - There is no "service-to-bottom" L1 — that range is already covered by controller-to-bottom.
- **Valid inputs and valid requests only.** L1 may assert that a guard rejects one invalid input
  and returns the correct status. Exhaustive validation coverage is L0's job.

### L2 — full application, DI and every registration

- The entire application is built with its real service registrations and its real middleware
  pipeline, in-process: `WebApplicationFactory<Program>` or equivalent.
- Driven over HTTP — the only level that exercises routing, model binding, content negotiation,
  CORS, problem-details bodies, status codes and headers end-to-end.
- Real externals preferred. Substitutes only when the external is genuinely impossible to
  provision in the test environment.

### L3 — end-to-end with a browser

- A real backend and a real frontend, driven by an automated browser.
- Real externals required. L3 is the highest-confidence tier; substitutes defeat its purpose.

**L2 and L3 are not created in v1.0.** Do not tag anything with them and do not write one because
it seems convenient. A `WebApplicationFactory` appearing in an L1 project is a defect, not a
shortcut.

---

## Test projects

| Project | Tier | Subject | How the subject is obtained |
|---|---|---|---|
| `Core/olympiad-quizzer-net.Core.Domain.L0` | L0 | Domain classes | hand-authored objects, collaborators substituted |
| `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L1` | L1 | classes that talk to the database or the filesystem | `new`, over a real test database |
| `App/olympiad-quizzer-net.App.API.L1` | L1 | controllers | `new Controller(realDependencies)` |

A test project has exactly **one** production counterpart, named by its `{FolderName}.{SubName}`
prefix. A test whose subject lives in another production project belongs in **that** project's
test project. Infrastructure tests inside an API test project are a filing defect; **there is no
qualifier-folder escape hatch.**

`Core.Tests.Common` is shared support code, not a test project, and carries no tier. Any helper,
builder, fixture, capturing logger, or mock factory used across **two or more** test projects
belongs here — not duplicated, not inlined per project.

Every test project is listed in `OlympiadQuizzer.slnx`, so `dotnet test OlympiadQuizzer.slnx`
runs every level in one invocation and CI needs no per-project step.

### Test mirror rule

- Test file name = production file name with `Tests` appended before the extension.
  `QuestionsController.cs` → `QuestionsControllerTests.cs`. Mechanical: no re-wording, no
  singular/plural adjustment.
- Test file folder = the production file's folder, relative to its project root. A test for a file
  in `Controllers/` lives in `Controllers/`.
- Test class name matches its file name. Namespace follows its folder.
- **The same production file has the same test file name at every tier.** A
  `QuestionsControllerTests.cs` in an L1 project and one in an L0 project are the same file name
  in different projects; the project name carries the tier.
- **Strictly 1:1.** One production file, one test file. No aspect splits, no concern suffixes. If
  a test file grows too large, the production class has too many responsibilities — split the
  class, and the test files follow naturally.
- Support code — fixtures, builders, harnesses, capturing loggers — has no production counterpart
  and lives under `Harness/`, `Builders/`, or `Fixtures/`, or in `Core.Tests.Common` if shared.
- Cross-cutting suites with no production counterpart — question-bank integrity, mode-definition
  drift, the architecture-guard test — keep descriptive names and live in the test project of the
  ring whose content they validate.

The mirror rule serves two purposes, and the second is the important one. **Navigation:** a
developer finds `Extensions/CorsExtensions.cs` from `Extensions/CorsExtensionsTests.cs` without
opening either file. **Complexity meter:** the total lines across the test file for a class is a
gauge — when it keeps growing, the class should be split. Allowing aspect splits would let a class
grow without limit while its tests stayed tidy, which destroys the gauge. That is why the 1:1 rule
has no exceptions.

---

## Mocking

Preferred library: **Moq**. Use AutoFaker (or a custom builder) for values irrelevant to the
test — keeps test intent legible and constructors honest. Hand-written fakes are allowed when
justified, but Moq is preferred over reinventing one. There is no ban on mocking at any tier.

**The principle:** prefer real implementations at every tier. As tiers increase, substitutes are
replaced with real things. Mocking is a deliberate trade-off — state why in a comment when the
reason is not obvious.

**Mocking is justified when:**

- The real thing is genuinely impractical for reasons unrelated to setup effort — e.g. a 3-hour
  timer, where waiting adds nothing to confidence; the reaction to expiry is what matters, not
  the wait.
- The real external cannot be provisioned in any test environment — true impossibility, not
  inconvenience.

**Difficulty of setup is not justification for mocking.** An L1 repository test that needs a real
CosmosDB: provision one. L2 and L3 need live externals; substituting there defeats the purpose of
the tier.

### What is substituted at each tier

**L0:** every collaborator. Nothing real except the subject itself.

**L1:** exactly two categories, substituted through the constructor — no DI container involved:

- **Loggers** → `NullLogger<T>` by default; a capturing logger when the test asserts a specific
  log event was emitted.
- **Externals** — strategy depends on how faithfully the external can be replicated:
  - *Easy and faithful*: use a real instance — a live SQLite file created on demand and deleted
    after the test. Nothing mocked at controller or repository level.
  - *Expensive or not faithfully replicable*: mock the repository boundary in controller L1 tests,
    and test the repository itself against the real external in its own Infrastructure L1 project.

**In-memory databases are not automatically acceptable.** They do not enforce SQL constraints,
triggers, functions, or database-level errors — they produce false positives and hide bugs that
surface only in production. Use in-memory only where its behaviour is genuinely equivalent to the
live external for the scenarios under test, and document why. **For SQLite specifically: use a
live SQLite file on disk, not in-memory SQLite.**

**L2 and L3:** real externals. Substitutes only on true impossibility, and if one is used, record
the reduced confidence — external-specific behaviours will not be covered and may surface only in
production.
