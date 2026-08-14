---
project: olympiad-quizzer-net
language: C#
framework: .NET 10 / Blazor WebAssembly / ASP.NET Core
test_framework: xUnit 2.9
---

# Coding Standards

Agent instruction: Read in full before writing or reviewing any code. All rules enforced at every PR. Violations are blocking findings.

Scope: this file covers conventions specific to this repo. Where it is silent, follow the
[.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
and the [Framework Design Guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/).

Related: `docs/architecture-guide.md` (layers, test levels, doc hygiene), ADR-019 (language
policy), ADR-031 (feature folders), ADR-032 (solution layout), ADR-033 (language posture).

---

## Project settings

Every `.csproj` in this repo sets all five properties:

```xml
<AssemblyName>olympiad-quizzer-net.<FolderName>.<SubName></AssemblyName>
<RootNamespace>OlympiadQuizzer.<FolderName>.<SubName></RootNamespace>
<Nullable>disable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

### Naming rule — `{SolutionName}.{FolderName}[.{SubName}]`

Applies to the project folder, the `.csproj` file name, the `AssemblyName` **and** the
`RootNamespace`. All four agree. ADR-039.

| Token | In a file/assembly name | In a namespace |
|---|---|---|
| `{SolutionName}` | `olympiad-quizzer-net` | `OlympiadQuizzer` — dashes are illegal, `-net` is a repo-name artefact |
| `{FolderName}` | the ring folder under `source/` | same |
| `{SubName}` | component, plus `.L0` / `.L1` for test projects | same, acronym casing per the Naming table |

| Project folder | AssemblyName | RootNamespace |
|---|---|---|
| `source/Core/olympiad-quizzer-net.Core.Domain` | `olympiad-quizzer-net.Core.Domain` | `OlympiadQuizzer.Core.Domain` |
| `source/Core/olympiad-quizzer-net.Core.Domain.L0` | `olympiad-quizzer-net.Core.Domain.L0` | `OlympiadQuizzer.Core.Domain.L0` |
| `source/Core/olympiad-quizzer-net.Core.Tests.Common` | `olympiad-quizzer-net.Core.Tests.Common` | `OlympiadQuizzer.Core.Tests.Common` |
| `source/Infrastructure/olympiad-quizzer-net.Infrastructure.SQLite` | `olympiad-quizzer-net.Infrastructure.SQLite` | `OlympiadQuizzer.Infrastructure.SQLite` |
| `source/App/olympiad-quizzer-net.App.API` | `olympiad-quizzer-net.App.API` | `OlympiadQuizzer.App.Api` |
| `source/App/olympiad-quizzer-net.App.API.L1` | `olympiad-quizzer-net.App.API.L1` | `OlympiadQuizzer.App.Api.L1` |
| `source/App/olympiad-quizzer-net.App.Client` | `olympiad-quizzer-net.App.Client` | `OlympiadQuizzer.App.Client` |

`API` in the dashed name, `Api` in the namespace — the dashed name mirrors the folder, the
namespace follows the acronym rule in the Naming table below.

A test project lives in the folder of the ring it exercises, so its `{FolderName}` is that ring's:
L0 tests Domain and sits in `Core/`; L1 tests the API and sits in `App/`.

The solution file stays `OlympiadQuizzer.slnx` — a deliberate exception, see ADR-039.

`AssemblyName` and `RootNamespace` are **always explicit** even though the rule now makes them
derivable. Project file names contain dashes; namespaces cannot. Left to MSBuild's default, the
root namespace becomes `olympiad_quizzer_net_Core_Domain`.

`TreatWarningsAsErrors` is on. To suppress a specific diagnostic, suppress that ID with a
one-line comment stating why. Never turn the property off.

## Program entry points

**No top-level statements.** Explicit class, explicit `Main`, declared `public` so the test host
and the logger factory can both reach it. **Not `partial`** — routes and startup configuration
live in their own units, so there is nothing to split. No `Program.*.cs` file may exist.
See ADR-033 (amended) and ADR-041.

## API file layout

Routes live one file per top-level route under `Endpoints/`. Startup configuration lives one file
per concern under `Extensions/`, as static extension classes over the service container or the
built application. Minimal API only — no MVC, no controller base type. ADR-041.

Test mirror rule: test file name = production file name with `Tests` appended
(`QuestionsEndpoints.cs` → `QuestionsEndpointsTests.cs`), in the same folder path relative to its
project root.

One file per production file is the default. When one production unit has several genuinely distinct
testing concerns, an aspect word may be inserted before `Tests`
(`JsonQuestionRepository.cs` → `JsonQuestionRepositoryShuffleTests.cs`). The production stem comes
first and is spelled exactly as the production file spells it. The aspect names a concern, not a
scenario — scenarios belong in method names.

When a test project covers a production project other than its own counterpart, the mirrored path
is prefixed with a qualifier folder named after that project's solution folder — for example
`Infrastructure/Json/`. Tests for the test project's own counterpart stay unqualified at its root.

Cross-cutting suites with no production counterpart keep descriptive names, and may sit in the
folder of the concern they span. ADR-041 and its amendment.

---

## Test naming

Pattern: `MethodName_Scenario_ExpectedResult`

The `Scenario` segment may itself contain underscores when a scenario needs more than one word
group. The three-part shape must stay recognisable: what is called, under what conditions, what
must happen.

The `ExpectedResult` segment must use one of: `Returns`, `Throws`, `Does`, `Executes`, or `Is`
to make the outcome explicit.

Examples from this repo:

- `Grade_Multi_AllExpectedValues_ReturnsFullPoints`
- `Grade_ShortAnswer_WrongText_ReturnsZero`
- `Grade_Single_TwoSubmittedValues_ReturnsIncorrect`
- `NormalizeFreeText_WithSubscriptDigits_ReturnsAsciiEquivalent`
- `NormalizeChoice_WithSubscriptDigit_DoesNotFoldIt`
- `GetAsync_WithCategoryAndYear_ReturnsOnlyQuestionsSatisfyingBoth`
- `GetQuestions_WithLimitAboveThirty_ReturnsBadRequest`
- `Grade_UnknownType_ReturnsIncorrectAndZeroPoints`

Type names in test names use the **v1.0** enum names — `Single`, `Multi`, `ShortAnswer`,
`TrueFalse`, `Ordering`, `Matching`. `SingleAbcd` and `MultiSelect` are gone; a test name
mentioning them is stale.

After writing a test, re-read the name. If it applies to a different test body → rename.

---

## Test structure

- AAA sections: Arrange / Act / Assert with blank lines between. No comment labels for simple
  tests; add `// Arrange` / `// Act` / `// Assert` only when a section body reaches 6+ lines.
- SUT setup: constructor for shared fixtures (`IClassFixture<T>` for the API test host);
  per-test `var sut = new ...` when state differs between tests.
- Cleanup: `IDisposable` / `IAsyncDisposable` where a fixture holds a host or a file handle.
  Avoid static mutable state — a shared static would make test order significant.
- Helper methods: at the bottom of the test class, after all test methods.
- Magic strings → named constants. A repeated `"sledzenie_kodu"` becomes
  `private const string _categoryCodeTracing = "sledzenie_kodu";`.
- Object construction: use the `QuestionBuilder` test helper so each test states only the field
  it cares about. A schema change should touch the builder, not every test file.
- Failure messages on data-integrity tests must name the offending record. "expected true,
  actual false" across 200 questions is not a bug report — assert with a message carrying the
  question `id` and, for string comparisons, both values with invisible characters escaped.

Anti-pattern: `foreach` loops inside tests → use `[Theory][InlineData]` or `[MemberData]`.
Exception: a data-integrity test that must assert an invariant across the whole bank may
iterate, but it must collect all violations and fail once with the full list — never fail on
the first and hide the rest.

No `if` in a test body. A branch means the test proves two different things.

## Test tier traits

Tag every test with its tier so CI can filter. Use the `TestTiers` constants from
`OlympiadQuizzer.Core.Tests.Common` — never raw string literals:

```csharp
[Trait(TestTiers.Tier, TestTiers.L0)]   // unit — no I/O, no network
[Trait(TestTiers.Tier, TestTiers.L1)]   // integration — real file I/O, real JSON, WebApplicationFactory
```

The trait goes on the **class**, not on each method — a whole test class belongs to one tier by
construction, since the tier is determined by which test project it lives in. A class in
`olympiad-quizzer-net.Core.Domain.L0` tagged `L1` is a defect.

Filtering: `dotnet test --filter "Tier=L0"`.

Tiers L2 and L3 are defined in `docs/architecture-guide.md` but not created in v1.0. Do not
tag anything with them.

---

## Mocking

**No mocking library in L0.** The Domain layer has no external dependencies — no I/O, no HTTP,
no DI, no logging — so there is nothing to mock. Hand-written fakes or plain in-line
construction suffice, and the absence of a mocking framework is a useful signal: if an L0 test
feels like it needs a mock, the class under test has an outward dependency it should not have.

**L1 substitutes exactly two things**, both by DI replacement in the test host rather than by a
mocking library:

- the shuffler → a seed-driven implementation, so "the server shuffles" is assertable and the
  suite is not intermittently red;
- loggers → the default test-host logging, or a capturing provider when a test asserts that a
  specific event was logged.

Everything else in L1 is the real implementation, including the real question bank file.

If a mock library is needed for L1+, use **Moq**.

---

## Null safety

`<Nullable>disable</Nullable>` in all `.csproj` files. See ADR-033 for why.

- No `?` on reference types. No `!` null-forgiving operator. No `#nullable enable` pragmas.
- `?` on **value** types stays — `int? Year` is correct and unrelated.
- Null checks at system boundaries only: API query parameters, JSON deserialisation results,
  `localStorage` reads, configuration values. Internal code trusts its own invariants.
- Guard clauses over nested `if`. Return early.
- Collections that a caller will iterate are initialised at declaration
  (`List<string> Category { get; set; } = new();`) so no downstream null guard is needed.

---

## Types and `var`

Prefer `var` where the type is obvious from context — the right-hand side makes the type
unambiguous without looking elsewhere:

```csharp
// Yes — type is explicit on the right-hand side.
var repository = new JsonQuestionRepository(loader, shuffler, logger);
var builder = new StringBuilder(value.Length);
var port = Environment.GetEnvironmentVariable("PORT");

// No — type is not evident from the method name alone.
GradeResult result = Grade(question, answer);

// No — numeric literal where the exact type matters.
int matched = 0;
bool positional = question.Type == QuestionType.TrueFalse;
```

Keep explicit type when:
- The literal is numeric or `bool` and the exact type matters (`int x = 0`, `bool flag = true`).
- The right-hand side is a method call and the return type is not self-evident from the name.
- The declaration widens to an interface deliberately (`IEnumerable<Question> candidates = ...`).

Never `var` for the result of a LINQ chain where the concrete type is opaque.
Target-typed `new()` requires the explicit type on the left — do not use `var` with it.

---

## Initializers and expressions

- Use primary constructors where applicable.
- Prefer simplified initializers:
  - `new()` (target-typed new) when the type is on the left.
  - `[]` for an empty collection when the element type is evident from context.
  - `[.. existingCollection]` for spread / copy.
- Prefer collection expressions over `new List<T> { }` or `new T[] { }`.

```csharp
// Preferred
List<string> tags = [];
List<Question> copy = [.. original];

// Avoid
List<string> tags = new List<string>();
Question[] arr = new Question[0];
```

---

## Comment policy

### Banned (PR-blocker)

- ADR references in code: `// See ADR-012`, `// per ADR-021`
- Issue/PR numbers in code: `// #13`, `// issue #36`
- Task references: `// added in task-12`, `// TODO task-15`
- Foreign repo names in comments
- `// TODO` / `// FIXME` / `// HACK` not cleaned up before commit
- Commented-out code. Git remembers.

Traceability belongs in the ADL, the functionality registry and the commit message — not in the
source. A reader who needs the decision history greps `docs/`, and a comment naming an ADR
number rots the moment that ADR is amended.

### No self-explaining comments

Comment ONLY when the WHY is non-obvious: a hidden constraint, a subtle invariant, a specific
bug workaround, non-obvious API behavior.

If a comment could be deleted and replaced by reading the method name → delete it.

```csharp
// Bad — restates the next line.
// Set the limit to the default
limit = DefaultLimit;

// Good — states what the code cannot.
// Shuffle before capping: capping first would make the result deterministic by bank order.

// Good — pins a load-bearing constraint against a well-meaning cleanup.
// Compression is disabled to work around a .NET 10 WASM asset-fingerprinting defect on
// static hosting. Do not remove.
```

### Allowed

- Numbered orchestrator steps `// 1. ...`, `// 2. ...` (spec for orchestration logic)
- Interop / P-Invoke / JS-interop rationale
- Non-obvious serializer or framework config — e.g. why the question bank is read with
  `File.ReadAllText` and handed to the deserialiser as a string rather than as a byte stream
  (the former strips a UTF-8 BOM, the latter does not, and the resulting error never mentions
  the BOM)
- Non-obvious PowerShell in workflows — e.g. why a file is written with an explicit
  no-BOM UTF-8 encoder rather than the shell's default cmdlet

XML doc comments: none on internal code. A one-line `///` on a cross-project public member only
where behaviour is not obvious from the signature — two normalisation helpers that differ only
in strictness are exactly such a case.

---

## Identifier language

C# code, JSON keys, CSS classes, HTML attributes, test names, file names, branch names, commit
messages, ADRs: **English**.

User-facing UI (labels, buttons, errors, page titles), question text, explanations, and image
alt text: **Polish**.

Tag identifiers (`category[]`, `algorithms[]` values): **Polish snake_case, Latin letters only,
diacritics dropped** (ą→a, ę→e, ó→o, ś→s, ł→l, ź/ż→z, ć→c, ń→n). `sledzenie_kodu`,
`zlozonosc`. This is the one exception to the English rule — ADR-019 amendment, vocabulary in
`docs/tags.md`.

---

## Naming

| Thing | Convention | Example |
|---|---|---|
| Types, methods, properties, constants, enum members | PascalCase | `QuestionQuery`, `MaxLimit` |
| Locals, parameters | camelCase | `matchedCount`, `cancellationToken` |
| Private fields | `_camelCase` | `_shuffler` |
| File-scoped private constants | `_camelCase` | `_corsPolicyName` |
| Interfaces | `I` + PascalCase | `IQuestionRepository` |
| Acronyms | two letters upper, three-plus PascalCase | `IO`, `ID`, but `Api`, `Json`, `Html`, `Url` |
| `.cs` / `.razor` files | match the single type they contain | one public type per file |
| Folders | PascalCase, matching the namespace segment | `Features/Quiz/Components/` |
| Docs, markdown, static assets, workflows, CSS | kebab-case | `coding-standards.md`, `deploy-frontend.yml` |
| JSON keys | camelCase | `correctAnswer`, `sourceRaw` |
| Git branches | kebab-case, English, `type/` prefix | `feature/server-side-filtering` |
| localStorage keys | `oqn.<area>.v<n>` | `oqn.session.v1` |

`sourceRaw`, not `source_raw`. The ADR-011 amendment text writes it snake_case, which
contradicts the same ADR's camelCase rule. camelCase wins.

### Using directives

Do not use fully-qualified type names when a `using` directive covers it. Add the `using` and
shorten the reference. Fully-qualified names are load-bearing only when two namespaces export
the same simple name and a `using` alias would be less clear than the qualification.

---

## JSON

- One shared serializer options instance, owned by the Domain. Never construct a second one at
  a call site — both ends of the wire must use the same one or camelCase gets configured on
  one side only.
- Keys are camelCase via the camelCase naming policy.
- **Never hand-write `[JsonPropertyName]`.** If a C# property name and its JSON key differ by
  more than casing, the C# name is wrong — rename the property.
- `[JsonConverter]` on a member is allowed where a shape genuinely varies. That is not the same
  thing as renaming a key.
- Enums serialise as camelCase strings, never as integers.
- Read JSON files as text, not as a raw byte stream (BOM — see the comment policy example).
- Write JSON files as **UTF-8 without BOM**. In PowerShell:
  `New-Object System.Text.UTF8Encoding($false)`. A BOM in an `appsettings.json` fails startup
  in Production with an error that never mentions the BOM.
- Question text is Unicode by design — Polish diacritics, mathematical italics (𝑥), subscripts
  (₁₆), middle dot. Never strip, escape or "clean" it. The relaxed JSON escaping this repo uses
  is safe **only** because rendered text never reaches a raw-HTML sink — see Security rules.

---

## Error handling

- **No empty `catch` blocks.** Ever.
- **No `catch (Exception)`** that swallows. Catch the specific type you can handle.
- A `catch` either handles, or logs and rethrows, or translates to a domain-meaningful result.
  Doing none of the three is a defect.
- Fail fast at startup. A missing or empty question bank throws and the process does not start.
  An API that boots and then serves an empty array forever is worse than one that refuses to
  boot, because the health check catches the second and a student catches the first.
- Untrusted input is validated at the boundary and **discarded** on failure, never repaired.
- Do not use exceptions for expected outcomes. "No questions matched these filters" is an empty
  list and HTTP 200 — not an exception, not a 404.

---

## Logging

- `ILogger<T>` only. No `Console.WriteLine`, no external sink.
- **Structured templates with named placeholders.** Never interpolate into the message:

```csharp
// Yes
_logger.LogInformation("Question query served: matched={MatchedCount} limit={Limit}", n, limit);

// No — destroys the structure and defeats log search
_logger.LogInformation($"Question query served: matched={n} limit={limit}");
```

- Levels: `Information` for lifecycle and served requests; `Warning` for a recoverable oddity
  (empty result, unknown tag, rejected parameter); `Error` for an unhandled failure. No `Debug`
  or `Trace` in committed code.
- Never log question text, answers, or a whole payload. The host keeps 7 days of logs; a
  30-question dump makes that window useless.

---

## Blazor

- One component per file. Component file name = component name.
- Feature-first folders (ADR-031). A page, its state, its service and its private components
  live together. Promote to `Shared/` only on the **second** consumer.
- Navigate using the injected navigation manager's base URI, never a literal `"/"`. The app is
  served from a sub-path on static hosting, and a literal root path escapes the app.
- `HttpClient.BaseAddress` ends with `/`; request URIs do **not** start with `/`.
- Anything holding a timer or a subscription implements `IDisposable` / `IAsyncDisposable` and
  actually disposes. A periodic timer left running leaks one loop per navigation.
- Every interactive element gets an accessible name and visible focus. ARIA and
  `:focus-visible` are written as the component is written, not retrofitted (ADR-017).
- CSS is ours — no framework (ADR-023). Theming through CSS custom properties on `:root`,
  switched by `data-*` attributes on `<html>`.

---

## Long method decomposition

Extract numbered private methods with a leading step comment when a method exceeds ~20 lines:

```csharp
private void BuildQuiz()
{
    // 1. Load questions
    // 2. Apply filters
    // 3. Shuffle
}
```

Prefer clear method names over inline comments. File length is a smell above ~300 lines;
method length above ~40. Not hard limits — prompts to look.

No regions.

---

## PR format

Title: `type(scope): description` — e.g. `feat(api): server-side question filtering`

Types: `feat`, `fix`, `refactor`, `test`, `ci`, `docs`, `chore`

Scope is the layer or area: `domain`, `infra`, `api`, `client`, `l0`, `l1`, `ci`, `docs`, `data`.

If a pull request template exists in `.github/` (`PULL_REQUEST_TEMPLATE.md` or
`pull_request_template.md` — GitHub accepts either casing), **it overrides this section**.
Repo Manager reads it first.

Commit messages follow the same `type(scope): description` shape.

---

## ADR content rules

ADRs state **WHAT** was decided and **WHY** — not **HOW** it was implemented.

Forbidden in an ADR body: class names, method names, interface names, property names, converter
logic, code listings of production types, `.csproj` snippets.

Write the decision in domain terms. "Answers are compared by option text, not by option
position" belongs in an ADR. The name of the type that does the comparing belongs in the
solution design.

Allowed in an ADR body: file and folder **paths** when the decision is itself structural (a
folder layout decision has to name folders), external URLs, secret **names**, configuration
**keys**, and wire-format field names when the ADR is about the wire format.

Other ADR rules: caveman-terse, one line per point, required sections per
`docs/adl/ADR-SCHEMA.md`, never edit a decision body — append an amendment. Every new ADR is
tracked in git and added to `docs/adl/INDEX.md` **in the same commit**.

Note: ADRs numbered below 031 predate this rule and contain code listings. Do not retro-edit
them — the rule protects new content, and an ADR body is not rewritten for style.

---

## Security rules

1. **Secrets**: Never write a secret value in any file (code, doc, comment, config, test
   fixture, commit message, log line). For every secret: name it, state its purpose, state where
   it lives (GitHub repo secret / Render dashboard / runner machine), state how to rotate it.
   Never the value. **Never a full deploy hook URL** — a deploy hook is an unauthenticated
   trigger, so the URL *is* the credential, and a partially redacted one still leaks the rest.
   In code, secrets come from the environment or injected configuration with **no literal
   fallback and no plausible-looking placeholder**.

2. **localStorage validation**: Everything read from `localStorage` must be sanitized and
   validated before use. Parse inside a `catch` for the specific parse exception; validate the
   result against an explicit predicate (schema version, non-empty collections, consistent
   counts, in-range indices, timestamps not in the future, sane limits, known enum values); on
   any failure **discard and clear the key** and return the user to a safe screen. Never repair,
   never partially trust, never default a missing field. Keys are version-suffixed so a schema
   change is a discard, not a migration. No tampered value may put the app into a broken or
   exploitable state — the worst outcome for a user who edits it is that they cheat themselves.

3. **No settings import/export**: Deliberate design decision, not a missing feature. No settings
   export, no settings import, no state import, no restore-from-file, no share link carrying
   state. Importing user-provided JSON is an attack surface — arbitrary shapes, sizes and nesting
   depth into the one place the app trusts its own data — and it buys nothing for a tool with no
   accounts and no sync. Reject any feature request that asks for it and point to this rule.
   Reopening it requires an ADR that addresses the attack surface, not a PR that adds a file
   picker.

4. **No XSS from question content**: Question text, option text, explanation text and image alt
   text must render as **text**, not raw HTML. Do not use `MarkupString` unless the content is
   explicitly sanitized first — and on bank or `localStorage` content, never. Code blocks render
   inside `<pre><code>` as text; if syntax highlighting is ever added it must work on a parsed
   token model, never by building HTML from the question string. This rule is also the
   precondition for the relaxed JSON escaping used on the wire — the two decisions are coupled
   and must not be separated.

---

## Pipeline artifacts — git policy

`.pipeline/` is **entirely gitignored** in this project — no pipeline artifacts are committed
to git. This is a deliberate project constraint, not a temporary scaffold. Do NOT modify the
`.pipeline/` entry in `.gitignore`, not during T-01, not during any other task.

What IS committed:

- `docs/` — all documentation (ADRs, architecture guide, integrations, rules, tags, etc.)
- `.github/` — workflows, PR template, issue templates

What is NOT committed (entirely gitignored via `.pipeline/`):

- All pipeline state, design, plan, and task files — `STATE.md`, `JOURNAL.md`, task plans,
  solution-design, test-strategy, feedback, critiques, and every other `.pipeline/` artifact

Design and task artifacts live in `.pipeline/` for local reference only. They are not version
history — the commit messages, ADRs, and `docs/` are the durable record.
