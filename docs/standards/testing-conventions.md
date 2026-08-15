# Test naming, structure and traits

## Test naming

Pattern: **`MethodName_Reaction_WhenCondition`** — shape `DoSomething_Returns_When…`.

| Segment | Contents |
|---|---|
| `MethodName` | the method or member under test, spelled as production spells it |
| `Reaction` | what must happen. **Must begin with one of `Returns`, `Does`, `Always`, `Never`, `Throws`, `Executes`**; where none fits, another outcome verb may be used, but the segment always names an outcome. **Absence of a side-effect is a reaction too** — `DoesNothing`, `NeverLogs` |
| `WhenCondition` | the conditions, conventionally introduced with `When` |

**The order is fixed and is the part most often broken.** Reaction is second, condition is third.
`Method_Condition_Reaction` is wrong however readable it sounds.

**The condition must add information.** Restating the method in the condition slot is noise —
`Serialize_…_WhenSerializing` says nothing the first segment did not.

Real violations found in this repository, with their corrections:

| Wrong | Right | What was wrong |
|---|---|---|
| `Check_ProductionBankAndDatabase_ReturnsEmptyDelta` | `Check_ReturnsEmptyDelta_WhenProductionBankMatchesDatabase` | reaction third, noun second |
| `Check_AlreadySyncedFixture_ReturnsEmptyDelta` | `Check_ReturnsEmptyDelta_WhenFixtureIsAlreadySynced` | reaction third |
| `Check_WhenDatabaseIsMissing_ThrowsFileNotFoundException` | `Check_ThrowsFileNotFoundException_WhenDatabaseIsMissing` | segments transposed |
| `Serialize_Question_DoesUseCamelCaseKeys` | `Serialize_DoesUseCamelCaseKeys_WhenQuestionHasPascalCaseProperties` | reaction third; condition slot held a bare noun |
| `Serialize_DoesUseCamelCaseKeys_WhenSerializing` | as above | condition restates the method |

Either of the last two segments may hold more than one word group. The three-part shape must stay
recognisable: what is called, what must happen, under what conditions.

```
Grade_ReturnsFullPoints_WhenMultiCarriesAllExpectedValues
Grade_ReturnsZero_WhenShortAnswerTextIsWrong
NormalizeFreeText_ReturnsAsciiEquivalent_WhenTextHasSubscriptDigits
NormalizeChoice_DoesNotFoldSubscriptDigits_WhenComparingOptions
GetAsync_ReturnsOnlyQuestionsSatisfyingBoth_WhenCategoryAndYearAreGiven
GetQuestions_ReturnsBadRequest_WhenLimitIsAboveThirty
GetQuestions_ReturnsProblemDetails_WhenRepositoryThrows
Constructor_Throws_WhenBankFileIsMissing
```

The same schema applies at every tier.

Type names in test names use the current enum names — `Single`, `Multi`, `ShortAnswer`,
`TrueFalse`, `Ordering`, `Matching`. `SingleAbcd` and `MultiSelect` are gone; a test name
mentioning them is stale.

After writing a test, re-read the name. If it would apply equally to a different test body, rename
it.

## Test structure

- **AAA labels are mandatory in every test.** `// Arrange`, `// Act`, `// Assert`, each on its own
  line above its section, with a blank line between sections. **There is no length threshold.** The
  labels are how a reader finds the assertion boundary at a glance, and that value does not scale
  with section length. A "6+ lines" threshold once appeared here; it was invented, never asked for,
  and is gone.
- **One exception: a genuine one-liner.** A test with nothing to arrange has nothing to separate,
  so labels add noise and no structure. Two shapes qualify:
  - a single self-asserting call needing no values at all;
  - a `[Theory]` whose values arrive from `[InlineData]`, `[MemberData]` or a test-case type, with
    a single self-asserting call in the body. The parameters are named, so no-magic-values is
    already satisfied and there is no Arrange section to label.
- **No magic numbers or unexplained literals in any test.** Every value, however simple, gets a
  named variable, so the test reads as a statement about behaviour rather than a list of numbers.

  **This rule follows the data.** It does not stop at the test body. A `MemberData` source or a
  test-case type carries the same obligation: properties named `Value1`, `Item2` or `Input`/
  `Expected2` reintroduce exactly the unexplained-literal problem, relocated one file away — and
  worse there, because the test body now *looks* clean while the meaning is hidden.

  Note how the two rules interact: fixing magic values gives every value a named variable, those
  declarations **are** an Arrange section, and the test stops being a one-liner. The exception above
  therefore all but eliminates itself in practice — the surviving cases are parameterised tests and
  calls that genuinely take no arguments.
- SUT setup: a fixture via the constructor when several tests share expensive state; per-test
  `var sut = new ...` when state differs between tests. At L1 the subject is constructed by hand in
  the test — there is no host fixture to share.
- Cleanup: `IDisposable` / `IAsyncDisposable` wherever a fixture holds a file handle or a database
  file. Avoid static mutable state — a shared static makes test order significant.
- Helper methods: at the bottom of the test class, after all test methods.
- Object construction: use the shared question builder so each test states only the field it cares
  about. A schema change should touch the builder, not every test file.
- Failure messages on data-integrity tests must name the offending record. "expected true, actual
  false" across 210 questions is not a bug report — assert with a message carrying the question
  `id` and, for string comparisons, both values with invisible characters escaped.
- Grouping inside a large test class uses `#region`, never a comment banner — see
  [csharp.md](csharp.md) § Regions.

Anti-pattern: `foreach` loops inside tests → use `[Theory][InlineData]` or `[MemberData]`.
Exception: a data-integrity test asserting an invariant across the whole bank may iterate, but it
must collect all violations and fail once with the full list — never fail on the first and hide the
rest.

**No `if` in a test body.** A branch means the test proves two different things.

## Test tier traits

Tag every test with its tier so CI can filter. Use the `TestTiers` constants from
`OlympiadQuizzer.Core.Tests.Common` — never raw string literals:

```csharp
[Trait(TestTiers.Tier, TestTiers.L0)]         // unit — no I/O, no network
[Trait(TestTiers.Tier, TestTiers.L1)]         // integration — hand-constructed, real deps, no DI, no host
[Trait(TestTiers.Tier, TestTiers.L2)]         // full application, real pipeline, over HTTP
[Trait(TestTiers.Tier, TestTiers.Integrity)]  // committed artefacts, not code under test
```

The trait goes on the **class**, not on each method — a whole test class belongs to one tier by
construction, since the tier is fixed by which project the class lives in. A class in
`olympiad-quizzer-net.Core.Domain.L0` tagged `L1` is a defect.

Filtering: `dotnet test --filter "Tier=L0"`.

**L3 is not created.** Do not tag anything with it.
