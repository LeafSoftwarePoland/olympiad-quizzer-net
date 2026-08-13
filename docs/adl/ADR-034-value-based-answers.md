# ADR-034: Answers are values, not option indices

**Status:** Accepted
**Date:** 2026-08-13
**Overrides:** ADR-011 `correct_answer shape by type` (the index-based table), and its
FormC-for-everything normalisation note as it applies to grading.

## Problem

POC modelled `correctAnswer` as option **indices** and carried a submission shape with a
separate collection per question type. The real scraped corpus stores answers as option
**text**:

```json
{ "id": 2, "type": "single",
  "options": ["y >= x", "y >= z", "x >= z", "y < z"],
  "correct_answers": ["y >= z"] }
```

Every import is then a text-to-index conversion, off-by-one-prone, and the resulting index
means nothing if options are ever reordered or one is inserted. The v1.0 plan also states an
answer is always a string or an array of strings — there is no third shape.

## Considered

- **Keep indices, convert at ingestion** — smallest code change; grading stays integer
  comparison. Conversion is a lossy one-way step done by hand over ~200 questions, and a wrong
  index grades a correct answer as wrong **silently**. Rejected.
- **Store both text and index** — belt and braces. Two sources of truth for one fact, which
  drift. Rejected.
- **Value-based: the stored answer is the option text; the submission is the option text** —
  the data needs no conversion, indices cannot rot, the submission shape collapses to one
  collection.
- **One normalisation rule for all types, canonical composition only** — simple. Then a stored
  answer written with subscript digits never matches a student typing plain digits. Rejected.
- **One normalisation rule for all types, compatibility folding** — fixes free text, but folds
  characters inside closed-list options too, so two options differing only by a superscript
  could both match. Rejected.

## Decision

**One answer shape everywhere: an ordered list of strings. Two normalisation rules, chosen by
whether the answer came from a closed list or from a keyboard.**

### Wire shape

`correctAnswer` is either a bare string or an array of strings. Reading accepts both; writing
emits a bare string for a single value and an array otherwise, so a round trip stays faithful
to the documented schema. Submissions carry a single ordered list of strings for every type.

| `type` | `correctAnswer` | How it is graded |
|---|---|---|
| `single` | one string | exactly one submitted value, closed-list equality |
| `multi` | array | set equality, closed-list, duplicates collapsed |
| `shortAnswer` | one string | free-text equality |
| `trueFalse` | array of `"true"`/`"false"` | positional, one entry per `options` entry |
| `ordering` | array of option values in correct order | positional |
| `matching` | array of `matchOptions` values | positional, aligned to `options` by index |

### Two normalisation rules

- **Closed-list comparison** (`single`, `multi`, `trueFalse`, `ordering`, `matching`): trim,
  canonical composition, lowercase invariant. Deliberately does **not** fold compatibility
  characters — the submitted value came from `options`, so only case, surrounding whitespace and
  composed-versus-decomposed accents can differ, and folding could make two distinct options
  equal.
- **Free-text comparison** (`shortAnswer` only): trim, canonical **compatibility** composition,
  lowercase invariant, internal whitespace runs collapsed. Compatibility composition folds
  exactly the characters the source PDFs are full of — subscript and superscript digits,
  mathematical italic letters, non-breaking spaces. It does **not** strip Polish diacritics.

### Partial credit

Positional types award a proportional score when `partialCredit` is set. `single`, `multi` and
`shortAnswer` are all-or-nothing. `multi` has no agreed over-selection penalty and OIJ stage E1
sets partial points off (`docs/rules/oij.md`), so no formula is invented here.

### The invariant this creates

Value-based grading is only correct if every stored answer value actually appears in `options`.
A stray typographic character makes a right answer wrong, **silently** — the worst failure this
app can have. Therefore an integration-level data-integrity test asserts, for every closed-list
question, that each stored answer value matches an `options` entry under the closed-list rule.
The data migration is not finished until it is green.

**Pros:**
- The real corpus needs no answer conversion — the largest source of migration error removed
- Indices cannot rot when options are edited or reordered
- One submission shape instead of five parallel collections
- The dynamic-JSON element and its accessor helpers disappear from the domain model
- Free-text folding is specified and tested rather than accidental

**Cons:**
- Grading now depends on exact option text, so a content error becomes a grading error —
  mitigated only by the integrity test
- Duplicate option text within one question makes the answer ambiguous (tested, treated as a
  content bug)
- `shortAnswer` accepts exactly one canonical string, losing ADR-011's original "multiple valid
  forms" array. Free-text folding covers the mechanical variants; genuinely different valid
  answers would need an additional accepted-answers field, which is a schema change and
  therefore a future ADR — **not** a patch
- A `multi` answer that happens to hold one value serialises back out as a bare string. Legal
  per the schema, harmless on read, and a single-answer `multi` question is a content bug anyway

## Remarks / Sources

- ADR-011 and its 2026-08-12 amendment (b) — the string/array shape per type; this ADR carries
  that into grading semantics and overrides the index table
- ADR-022 (POC field bindings) — the index semantics described there no longer apply
- Real corpus: `.resources/output-fixed/*.json` — the answer field holds option text
- Unicode normalisation forms: https://learn.microsoft.com/dotnet/api/system.text.normalizationform
- v1.0 solution design §3.6–§3.8 for the concrete types, converter and worked-example table;
  v1.0 test strategy §2.1–§2.6 and the data-integrity suite
