# ADR-024: Answers are values, not option indices

**Status:** Accepted
**Date:** 2026-08-13

## Problem

The prototype modelled the correct answer as option **indices**, with a separate submission shape per question type. The real scraped corpus stores answers as option **text**. Every import is then a text-to-index conversion, done by hand over ~200 questions, off-by-one-prone, and the resulting index means nothing if options are ever reordered or one is inserted.

## Considered

- **Keep indices, convert at ingestion** — smallest code change; grading stays integer comparison. Conversion is a lossy manual step, and a wrong index grades a correct answer as wrong **silently**. Rejected.
- **Store both text and index** — belt and braces. Two sources of truth for one fact, which drift. Rejected.
- **Value-based: stored answer is the option text, submission is the option text** — the data needs no conversion, indices cannot rot, and the submission shape collapses to one collection.
- **One normalisation rule for all types, canonical composition only** — simple. A stored answer written with subscript digits then never matches a student typing plain digits. Rejected.
- **One normalisation rule for all types, compatibility folding** — fixes free text, but folds characters inside closed-list options too, so two options differing only by a superscript could both match. Rejected.

## Decision

**One answer shape everywhere: an ordered list of strings. Two normalisation rules, selected by whether the answer came from a closed list or from a keyboard.**

### Wire shape

`correctAnswer` is a bare string or an array of strings. Reading accepts both; writing emits a bare string for a single value and an array otherwise, so a round trip stays faithful to the schema. Submissions carry one ordered list of strings for every type.

| `type` | `correctAnswer` | Graded as |
|---|---|---|
| `single` | one string | exactly one submitted value, closed-list equality |
| `multi` | array | set equality, closed-list, duplicates collapsed |
| `shortAnswer` | one string | free-text equality |
| `trueFalse` | array of `"true"`/`"false"` | positional, one entry per `options` entry |
| `ordering` | option values in correct order | positional |
| `matching` | `matchOptions` values | positional, aligned to `options` by index |

### Two normalisation rules

- **Closed list** (`single`, `multi`, `trueFalse`, `ordering`, `matching`): trim, canonical composition, lowercase invariant. Deliberately does **not** fold compatibility characters — the submitted value came from `options`, so only case, surrounding whitespace and composed-versus-decomposed accents can differ, and folding could make two distinct options equal.
- **Free text** (`shortAnswer` only): trim, canonical **compatibility** composition, lowercase invariant, internal whitespace runs collapsed. Compatibility composition folds exactly what the source PDFs are full of — subscript and superscript digits, mathematical italics, non-breaking spaces. It does **not** strip Polish diacritics.

Lowercasing uses invariant culture, which avoids the Turkish-I trap.

### Partial credit

Positional types award a proportional score when `partialCredit` is set. `single`, `multi` and `shortAnswer` are all-or-nothing. `multi` has no agreed over-selection penalty and OIJ stage E1 turns partial points off, so no formula is invented here.

Accepted cons:

- Grading depends on exact option text, so a content error becomes a grading error. Mitigated only by the integrity test in ADR-007, which is why that test is not optional.
- Duplicate option text within one question makes the answer ambiguous. Tested, and treated as a content bug.
- `shortAnswer` accepts exactly one canonical string. Genuinely different valid answers need a new field, which is an amendment, not a patch.
- A `multi` answer holding one value serialises back out as a bare string. Legal per the schema, harmless on read, and a single-answer `multi` question is a content bug anyway.

## Remarks / Sources

- ADR-007 (the field list, and the answers-exist-among-options invariant this decision creates)
- Unicode normalisation forms: https://learn.microsoft.com/dotnet/api/system.text.normalizationform
- Real corpus, answers as option text: `.resources/output-fixed/*.json`
