# Test naming, structure and traits

## Test naming

Pattern: `MethodName_Reaction_WhenCondition` — shape `DoSomething_Returns_When…`.

| Segment | Contents |
|---|---|
| `MethodName` | the method or member under test, spelled as production spells it |
| `Reaction` | what must happen. **Must begin with one of `Returns`, `Does`, `Always`, `Never`, `Throws`, `Executes`**; where none fits, another outcome verb may be used, but the segment always names an outcome. **Absence of a side-effect is a reaction too** — `DoesNothing`, `NeverLogs` |
| `WhenCondition` | the conditions, conventionally introduced with `When` |

Either of the last two segments may contain more than one word group. The three-part shape must
stay recognisable: what is called, what must happen, under what conditions.

```
Grade_ReturnsFullPoints_WhenMultiCarriesAllExpectedValues
Grade_ReturnsZero_WhenShortAnswerTextIsWrong
Grade_ReturnsIncorrect_WhenSingleCarriesTwoSubmittedValues
NormalizeFreeText_ReturnsAsciiEquivalent_WhenTextHasSubscriptDigits
NormalizeChoice_DoesNotFoldSubscriptDigits_WhenComparingOptions
GetAsync_ReturnsOnlyQuestionsSatisfyingBoth_WhenCategoryAndYearAreGiven
GetQuestions_ReturnsBadRequest_WhenLimitIsAboveThirty
Constructor_Throws_WhenBankFileIsMissing
```

The same schema applies at every tier.

Type names in test names use the current enum names — `Single`, `Multi`, `ShortAnswer`,
`TrueFalse`, `Ordering`, `Matching`. `SingleAbcd` and `MultiSelect` are gone; a test name
mentioning them is stale.

After writing a test, re-read the name. If it would apply equally to a different test body,
rename it.

## Test structure

- AAA sections: Arrange / Act / Assert with blank lines between. Add `// Arrange` / `// Act` /
  `// Assert` labels when a section body reaches 6+ lines.
- **True one-liner tests skip AAA entirely** — labels add noise and no structure. A test stops
  being a one-liner the moment any value needs its own named variable, which is required below.
  In practice, true one-liners are rare.
- **No magic numbers or unexplained literals in any test.** Every value, however simple, gets a
  named variable so the test reads as a statement about behaviour, not a list of numbers.
- SUT setup: a fixture via the constructor when several tests share expensive state; per-test
  `var sut = new ...` when state differs between tests. At L1 the subject is constructed by hand
  in the test — there is no host fixture to share.
- Cleanup: `IDisposable` / `IAsyncDisposable` wherever a fixture holds a file handle or a
  database file. Avoid static mutable state — a shared static makes test order significant.
- Helper methods: at the bottom of the test class, after all test methods.
- Object construction: use the shared question builder so each test states only the field it
  cares about. A schema change should touch the builder, not every test file.
- Failure messages on data-integrity tests must name the offending record. "expected true, actual
  false" across 210 questions is not a bug report — assert with a message carrying the question
  `id` and, for string comparisons, both values with invisible characters escaped.

Anti-pattern: `foreach` loops inside tests → use `[Theory][InlineData]` or `[MemberData]`.
Exception: a data-integrity test asserting an invariant across the whole bank may iterate, but it
must collect all violations and fail once with the full list — never fail on the first and hide
the rest.

**No `if` in a test body.** A branch means the test proves two different things.

## Test tier traits

Tag every test with its tier so CI can filter. Use the `TestTiers` constants from
`OlympiadQuizzer.Core.Tests.Common` — never raw string literals:

```csharp
[Trait(TestTiers.Tier, TestTiers.L0)]   // unit — no I/O, no network
[Trait(TestTiers.Tier, TestTiers.L1)]   // integration — hand-constructed, real deps, no DI, no host
```

The trait goes on the **class**, not on each method — a whole test class belongs to one tier by
construction, since the tier is fixed by which project the class lives in. A class in
`olympiad-quizzer-net.Core.Domain.L0` tagged `L1` is a defect.

Filtering: `dotnet test --filter "Tier=L0"`.

Tiers L2 and L3 are not created in v1.0. Do not tag anything with them.
