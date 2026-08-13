# ADR-022: POC schema field bindings and answer semantics

**Status:** Accepted
**Date:** 2026-08-08
**Clarifies:** ADR-011 (does not change the schema)
**Expected to iterate with user** — this pins user-visible answer semantics; 2–3 rounds of refinement are normal once real questions land.

## Problem

ADR-011 fixes the field list but leaves two types under-specified, and the POC design spec's mock table contradicts itself on them:

- **`trueFalse`** — the question carries N statements, each judged true/false. ADR-011 has no `statements` field. The POC mock table shows `options: null` while also listing three statements.
- **`matching`** — ADR-011 says `correctAnswer` maps "left[i] → index in `matchOptions`". There is no `left` field in the schema. The POC mock table shows `options: null` while also listing a left column.

Left unresolved, the Implementor guesses, and the guess bakes into `questions.json`, the renderer, and the grader simultaneously.

A third, smaller ambiguity: `ordering`'s `correctAnswer` is described as "option indices in correct order" — which could be read in either direction (position→item, or item→position).

## Considered

- **Add dedicated fields** (`statements: string[]`, `left: string[]`) — most self-documenting. Grows the schema by two nullable arrays that only ever apply to one type each, and forces a schema revision (ADR-011) plus a re-import mapping for py-pdf-scraper output before any real content exists.
- **Model statements / left column as `content` blocks** — no new fields. Renderer must then parse positional meaning out of a prose block array, and the grader loses any index it can trust. Rejected.
- **Reuse `options` as "the left-hand / enumerated items" for every type** — zero schema change. `options` already means "the indexable list this question's `correctAnswer` refers to" for `multiSelect`, `singleAbcd`, and `ordering`. Extending that meaning to `trueFalse` statements and `matching` left column is consistent, not a hack.

## Decision

**`options` is the indexable item list for every type. `matchOptions` is the right-hand pool for `matching` only.**

| Type | `options` | `matchOptions` | `correctAnswer` | Meaning |
|---|---|---|---|---|
| `multiSelect` | choices | null | `int[]` | set of correct choice indices, order-insensitive |
| `singleAbcd` | choices | null | `int[]` len 1 | the correct choice index |
| `shortAnswer` | **null** | null | `string[]` | any listed form is accepted |
| `trueFalse` | **statements** | null | `bool[]` | `correctAnswer[i]` is the verdict for `options[i]`; lengths must match |
| `ordering` | items **as displayed** | null | `int[]` | `correctAnswer[k]` = index in `options` of the item belonging at position `k` |
| `matching` | **left column** | right column | `int[]` | `correctAnswer[i]` = index in `matchOptions` paired with `options[i]` |

`shortAnswer` is the only type with `options: null`. The POC design spec's mock table showing `options: null` for `trueFalse` and `matching` is superseded by this table.

### Worked examples (these are the POC fixtures — `questions.json` must match exactly)

`ordering`, `poc-5`: `options = ["C","A","D","B"]`, target order A→B→C→D.

| position k | correct item | its index in `options` | `correctAnswer[k]` |
|---|---|---|---|
| 0 | "A" | 1 | 1 |
| 1 | "B" | 3 | 3 |
| 2 | "C" | 0 | 0 |
| 3 | "D" | 2 | 2 |

→ `correctAnswer = [1, 3, 0, 2]`

`matching`, `poc-6`: `options = ["Kot","Pies","Ryba"]`, `matchOptions = ["Woda","Trawa","Mleko"]`.

| i | `options[i]` | pairs with | its index in `matchOptions` | `correctAnswer[i]` |
|---|---|---|---|---|
| 0 | Kot | Mleko | 2 | 2 |
| 1 | Pies | Trawa | 1 | 1 |
| 2 | Ryba | Woda | 0 | 0 |

→ `correctAnswer = [2, 1, 0]`

The generic examples in ADR-011's shape table (`ordering: [1,2,0]`, `matching: [2,0,3,1]`) are illustrative only and are not the POC fixtures.

### Answer submission and grading

One `AnswerSubmission` type, one populated field per question type:

| Type | Submitted | Correct when |
|---|---|---|
| `multiSelect` | `int[] SelectedIndices` | set-equal to `correctAnswer` |
| `singleAbcd` | `int[] SelectedIndices` (len ≤ 1) | equal to `correctAnswer` |
| `shortAnswer` | `string Text` | normalized `Text` equals any normalized entry of `correctAnswer` |
| `trueFalse` | `bool?[] Booleans` | every element non-null and element-wise equal |
| `ordering` | `int[] Order` (user's arrangement, as option indices) | sequence-equal to `correctAnswer` |
| `matching` | `int[] Matches` (len = `options.Length`, `-1` = unanswered) | element-wise equal |

**Short-answer normalization** (ADR-011 mandates NFC; this pins the full pipeline and its order):

```
normalize(s) = s.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant()
```

Applied to both the user input and each expected answer. Case-insensitive by `ToLowerInvariant` — invariant culture avoids the Turkish-I trap. Internal whitespace is **not** collapsed; multi-word answers must be listed in `correctAnswer` in the form(s) expected.

**Partial credit** — `partialCredit` is `false` on all six POC fixtures, so everything is all-or-nothing in the POC. The grader still implements the partial branch, because ADR-011 names `trueFalse` / `ordering` / `matching` as partial-credit types and the branch is cheap to write now and awkward to retrofit:

```
partial types (trueFalse, ordering, matching) with partialCredit == true:
    pointsAwarded = points * (matching positions / total positions)
all other cases:
    pointsAwarded = isCorrect ? points : 0
isCorrect  = (pointsAwarded == maxPoints)
```

`multiSelect` and `singleAbcd` and `shortAnswer` never award partial credit even if `partialCredit` is set true — flagged as a validation warning, not an error.

**Unknown `type` value** deserializes to `QuestionType.Unknown`; the grader returns `(false, 0, points)` and the renderer shows a Polish placeholder rather than throwing.

## Consequences

**Pros:**
- No schema change, no ADR-011 revision, no re-import mapping
- One indexing rule to remember across all six types
- Grader and renderer share one source of truth for what an index means

**Cons:**
- `options` is semantically overloaded — a reader must consult `type` to know what the list *is*. Mitigated by this table and by naming the C# property `Options` with an XML doc comment stating the per-type meaning
- If a future type needs both an enumerated list *and* statements, `options` runs out and the schema change deferred here comes due

## Remarks / Sources

- ADR-011 (unified schema, camelCase, NFC requirement), ADR-019 (Polish UI / English code)
- POC mock table being superseded: `docs/pocs/2026-08-08-olympiad-quizzer-poc-design.md` §"Mock questions"
- Unicode NFC: https://unicode.org/reports/tr15/
- `string.Normalize`: https://learn.microsoft.com/en-us/dotnet/api/system.string.normalize
