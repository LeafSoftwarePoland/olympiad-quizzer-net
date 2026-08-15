# Test tiers, projects and mocking

## Test levels

Authoritative. A test sitting at the wrong level is a defect, not a style preference.

The purpose of tiering: each higher tier tests more of the real system. L0 is maximally granular;
L3 is maximally real. As tiers increase, substitutes are replaced with real implementations. The
number of tests shifts with it: many small L0s pinning individual behaviours, few large L3s
confirming the whole system works.

**A tier is created when a rule-derived obligation lands in it** — not because it seems useful and
not because it is convenient. See § How these rules compose in [INDEX.md](INDEX.md) for the worked
derivation that brought L2 into existence here.

### L0 — unit, everything mocked

- One class, or one method, under test. Every collaborator is substituted.
- **Black-box on the contract.** Test every meaningful value the actual type can carry — not
  imagined future values. `int` has min, max, `0`, `-1`, `1`. `double` also has `NaN`, `+∞`,
  `-∞`, `±0` — those exist and can reach the method, so they are tested. Test what the type has,
  but test all of it that matters.
- **Type complexity is a signal.** If a type forces you to test cases you do not care about, the
  type may be wrong for the job. An enum, record or struct with restricted values eliminates those
  cases at the source — and the reduction in test count is evidence the type is better suited.
- **Every substituted collaborator is tested for throwing, not only for returning.** Good data,
  weird data and nulls are half the contract. The other half is: what happens when the call itself
  fails? A mock that only ever returns is half a mock, and the untested half is where production
  breaks.

  Two kinds of throwing case, and both are wanted:

  - **Anticipated exceptions, derived from the collaborator's real contract.** Not invented — read
    what the dependency actually throws. A file seam gives `IOException`,
    `FileNotFoundException`, `UnauthorizedAccessException`; a parser gives `FormatException`; a
    guarded API gives `ArgumentNullException`; SQLite gives its own. **One test each**, because
    each has a distinct correct reaction and lumping them proves nothing about which one you
    actually handled.
  - **One unanticipated case**, proving the unknown path: nothing swallows it and it reaches the
    layer that owns unknowns. A generic `Exception` works; so does a test-owned
    `UnexpectedException`, which reads as intentional where an esoteric real type reads as
    someone's arbitrary pick. Optional — use it where it earns clarity.

  **A deliberate bubble is a valid asserted outcome.** A test that shows "the store threw and this
  layer let it through" is not a weak test — it records that *not* handling was a decision. Two
  things follow: a later change that starts swallowing that exception fails a test instead of
  silently disabling the layer above; and it is the L1 obligation from § How these rules compose
  ([INDEX.md](INDEX.md)) made concrete, since middleware never fires if something lower swallowed
  the fault first. It also completes the rule in [csharp.md](csharp.md): a `catch` must handle, log
  and rethrow, or translate — and **not catching at all is the fourth valid outcome**, which this
  test is what makes deliberate rather than accidental.
- **Error handling is part of the contract.** When something unexpected happens, the unit logs what
  it saw and returns a **safe** value — `0`, `null`, an empty collection, a status the UI can
  render — or it bubbles deliberately. It does not hand a surprise to its caller.
- **One method with forty tests means the method does too much.** Split the responsibility. Do not
  delete the tests.
- Test count measures the complexity of the **code**, not of the product.
- No filesystem, no network, no DI container, no configuration, no host.

**L0 is not only for Domain.** Any ring gets L0 tests where its collaborators can be substituted.
Infrastructure qualifies when the external sits behind a **mockable seam** — a thin pass-through
interface doing I/O and nothing else. The logic above that seam (query shaping, limit clamping,
ordering, mapping) is then testable with no database at all. If a class cannot be L0-tested because
its I/O is welded to its logic, that is a design finding, not a reason to skip the tier.

### L1 — integration, manually constructed, no DI and no middleware

- The subject is constructed **by hand**: `new QuestionsController(realRepository, mockLogger)`.
  **No `WebApplicationFactory`. No service-collection registration. No middleware, no routing, no
  model binding, no filters, no `appsettings`.**
- Loggers are always substituted — `NullLogger<T>` by default, a capturing logger when the test
  asserts a specific log event was emitted.
- External dependencies follow § Mocking below.
- **Three valid L1 entry points, all valuable and often coexisting:**
  - **Controller-to-bottom with real external** (default): all real in-app layers wired to the real
    external. Covers happy paths, error paths unrelated to the external, and error paths where the
    real external can be coerced into emitting the needed data. Prefer this whenever the real
    external can produce the scenario.
  - **Controller-to-bottom with mocked external** (targeted, narrow): only when a scenario needs
    external behaviour the real external cannot easily produce. Covers that scenario, not the
    external generally. Additive, never a replacement for the real-external tests.
  - **Repository layer only against the real external** (always valuable, always separate):
    exercises the actual connection and finds behaviours the controller-to-bottom path cannot
    reach. No controller, no service above it.
  - There is no "service-to-bottom" L1 — controller-to-bottom already covers that range.
- **Valid inputs and valid requests only.** L1 may assert that a guard rejects one invalid input
  and returns the correct status. Exhaustive validation coverage is L0's job.

### L2 — full application, DI and every registration

- The entire application built with its real service registrations and its real middleware
  pipeline, in-process: `WebApplicationFactory<Program>` or equivalent. Driven over HTTP.
- **Scoped narrowly to what no lower tier can reach.** L2 is not "L1 with a host" and is not where
  behaviour that fits L0 or L1 goes because a host makes it easier to write.

  What legitimately lives here:
  - the exception-handling middleware chain end to end — thrown deep, surfacing as a shaped
    response ([api.md](api.md) § Error handling)
  - routing, model binding and content negotiation
  - CORS preflight over real HTTP
  - problem-details bodies, status codes and headers as actually emitted

- Real externals preferred. **Mocking an external at L2 is legitimate exactly where the real one
  cannot produce the scenario** — the canonical case being "make the store throw an arbitrary
  fault" to prove nothing swallows it. That is § Mocking authorising the substitute, not an
  exception to it.

### L3 — end-to-end with a browser

- A real backend and a real frontend, driven by an automated browser.
- Real externals required. L3 is the highest-confidence tier; substitutes defeat its purpose.
- **Not created.** Do not tag anything with it and do not write one because it seems convenient.

### Integrity — the produced artefact, not the code

A deliberate, recorded exception to the L0–L3 scheme. See § Breaking a rule with sense in
[INDEX.md](INDEX.md) for why it exists and why the deviation is confined to two axes.

- Validates **what the repository produces** — the committed question bank, the generated database,
  the machine-readable rule blocks — not code under test.
- Has **no production counterpart**, which is precisely why it cannot live in any tiered project.
- Reads committed artefacts directly. No controller, no repository, no application.
- Lives in `Solution/olympiad-quizzer-net.Solution.DataIntegrityTests`, tagged `Integrity`.

---

## Test projects

| Project | Tier | Subject |
|---|---|---|
| `Core/olympiad-quizzer-net.Core.Domain.L0` | L0 | Domain classes, collaborators substituted |
| `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L0` | L0 | Infrastructure logic above a mocked persistence seam |
| `Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite.L1` | L1 | classes talking to a real database file |
| `App/olympiad-quizzer-net.App.API.L0` | L0 | controllers with a mocked repository |
| `App/olympiad-quizzer-net.App.API.L1` | L1 | controllers hand-constructed over real infrastructure |
| `App/olympiad-quizzer-net.App.API.L2` | L2 | whole application, real pipeline, over HTTP |
| `Solution/olympiad-quizzer-net.Solution.DataIntegrityTests` | Integrity | committed artefacts |

A test project has exactly **one** production counterpart, named by its `{FolderName}.{SubName}`
prefix, and one tier. A test whose subject lives in another production project belongs in **that**
project's test project. Infrastructure tests inside an API test project are a filing defect;
**there is no qualifier-folder escape hatch.**

The Integrity project is the single documented exception: it has no production counterpart, which
is the reason it exists separately rather than the reason to file it somewhere convenient.

`Core.Tests.Common` is shared support code, not a test project, and carries no tier. Any helper,
builder, fixture, capturing logger or mock factory used across **two or more** test projects
belongs here — not duplicated, not inlined per project.

Every test project is listed in `OlympiadQuizzer.slnx`, so `dotnet test OlympiadQuizzer.slnx` runs
every level in one invocation and CI needs no per-project step.

### Test mirror rule

- Test file name = production file name with `Tests` appended before the extension.
  `QuestionsController.cs` → `QuestionsControllerTests.cs`. Mechanical: no re-wording, no
  singular/plural adjustment.
- Test file folder = the production file's folder, relative to its project root.
- Test class name matches its file name. Namespace follows its folder.
- **The same production file has the same test file name at every tier.** The project name carries
  the tier.
- **Strictly 1:1.** One production file, one test file **per tier**. No aspect splits, no concern
  suffixes. If a test file grows too large, the production class has too many responsibilities —
  split the class, and the test files follow naturally.
- Support code — fixtures, builders, harnesses, capturing loggers — has no production counterpart
  and lives under `Harness/`, `Builders/` or `Fixtures/`, or in `Core.Tests.Common` if shared.

The mirror rule serves two purposes, and the second is the important one. **Navigation:** a
developer finds `Extensions/CorsExtensions.cs` from `Extensions/CorsExtensionsTests.cs` without
opening either file. **Complexity meter:** the total lines across the test file for a class is a
gauge — when it keeps growing, the class should be split. Allowing aspect splits would let a class
grow without limit while its tests stayed tidy, which destroys the gauge. That is why the 1:1 rule
has no exceptions, and it is why this rule has twice found real design defects here.

---

## Mocking

Preferred library: **Moq**. Use AutoFaker (or a custom builder) for values irrelevant to the test —
keeps intent legible and constructors honest. Hand-written fakes are allowed when justified, but
Moq is preferred over reinventing one. **There is no ban on mocking at any tier.**

**The principle:** prefer real implementations at every tier. As tiers increase, substitutes are
replaced with real things. Mocking is a deliberate trade-off — state why in a comment when the
reason is not obvious.

**Mocking is justified when:**

- The real thing is impractical for reasons unrelated to setup effort — a 3-hour timer, where
  waiting adds nothing; the reaction to expiry is what matters, not the wait.
- **The real external cannot produce the scenario under test.** Making a healthy database throw an
  arbitrary fault is the canonical case, and it is why mocking is legitimate even at L2.
- The real external cannot be provisioned in any test environment — true impossibility, not
  inconvenience.

**Difficulty of setup is not justification.** An L1 repository test that needs a real CosmosDB:
provision one.

### What is substituted at each tier

**L0:** every collaborator. Nothing real except the subject itself. Each one also gets a throwing
case.

**L1:** exactly two categories, substituted through the constructor — no DI container involved:

- **Loggers** → `NullLogger<T>` by default; a capturing logger when asserting a log event.
- **Externals** — by how faithfully they can be replicated:
  - *Easy and faithful*: use a real instance — a live SQLite file created on demand and deleted
    after the test. Nothing mocked at controller or repository level.
  - *Expensive or not faithfully replicable*: mock the repository boundary in controller L1 tests,
    and test the repository itself against the real external in its own Infrastructure L1 project.

**In-memory databases are not automatically acceptable.** They do not enforce SQL constraints,
triggers, functions or database-level errors — they produce false positives and hide bugs that
surface only in production. Use in-memory only where behaviour is genuinely equivalent for the
scenarios under test, and document why. **For SQLite specifically: use a live file on disk.**

**L2:** real externals, except where the scenario demands a fault the real one cannot produce.
Record the reduced confidence when substituting.

**Integrity:** nothing is mocked. The point is the real committed artefact.
