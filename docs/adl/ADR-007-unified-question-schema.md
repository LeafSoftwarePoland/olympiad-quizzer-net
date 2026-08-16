# ADR-007: Unified question schema

**Status:** Accepted
**Date:** 2026-08-13

## Problem

OIJ and voivodeship konkursy carry different question types, grading rules and metadata, and one app serves both (ADR-003). Need one record shape covering both, stable enough to migrate ~200 hand-checked questions against.

## Considered

- **Separate schema per competition family** — clean isolation. Duplicates shared fields and makes cross-family queries awkward.
- **Unified schema, type-discriminated, optional fields per type** — one record type; a type value selects which optional fields apply.
- **Free-form tag array** — flexible. A tag means nothing to a filter that must group by axis, so filtering degrades to substring matching. Rejected.
- **Typed tag fields, one per classification axis** — a new axis costs a new field, which is the honest price. Filters can group by axis.

## Decision

**One unified, type-discriminated record. Typed tag fields. camelCase keys throughout, enums as camelCase strings.**

### v1.0 field list — frozen

| key | type | required | note |
|---|---|---|---|
| `id` | `int` | yes | stable, unique across the bank, never reused. Images are named after it. |
| `category` | `string[]` | yes, non-empty | vocabulary in `docs/tags.md` |
| `algorithms` | `string[]` | no | may be empty |
| `olympiad` | `string` | yes | `"OIJ"` for the current corpus. Not an enum — a new family must not need a schema change. |
| `stage` | `string` | yes | `E1` / `E2` / `E3` |
| `year` | `int \| null` | no | |
| `difficulty` | `int \| null` | no | 1–5 |
| `source` | `string` | recommended | origin code, e.g. `OIJ-2024-E1` |
| `sourceUrl` | `string` | recommended | one link to the original |
| `sourceRaw` | `string` | no | source PDF filename, for traceability. Not displayed, not filterable. |
| `explanationSource` | `string` | no | free text: `AI generated`, `official`, `documentation`, `community` |
| `type` | enum | yes | `single` / `multi` / `shortAnswer` / `trueFalse` / `ordering` / `matching` |
| `content` | block array | yes, non-empty | |
| `contentCpp` | block array `\| null` | no | C++ variant of `content`; UI shows a language toggle when present |
| `options` | `string[] \| null` | per type | the indexable item list. `null` only for `shortAnswer`. |
| `matchOptions` | `string[] \| null` | per type | right-hand pool, non-null only for `matching` |
| `explanation` | block array `\| null` | no | shown after grading, same block format as `content`, same renderer |
| `correctAnswer` | `string \| string[]` | yes | option **text**, never an index — ADR-024 |
| `points` | `int` | defaults to 1 | |
| `partialCredit` | `bool` | defaults to false | |

### Content blocks

Question text, code and images are ordered arrays of typed blocks, not flat strings, matching the scraper's output so import needs no structural conversion. Block types: `text`, `code`, `image`.

An `image` block carries a file reference and **mandatory** Polish `alt` text describing the image in enough detail to answer the question. A decorative-level `alt` is a content bug (ADR-010).

### Unicode

Question text is Unicode by design: Polish diacritics, mathematical italics, superscripts, subscripts, middle dot. Stored as UTF-8, never stripped or "cleaned". Grading normalisation is **two rules, not one** — see ADR-024.

### Invariant this creates

For every closed-list question, each `correctAnswer` value must appear among `options` — or among `matchOptions` for `matching` — after normalisation. Not expressible in a type, so it is asserted by a data-integrity test over the real bank. Violating it grades a correct answer as wrong, silently, which is the worst failure this app can have.

Accepted cons:

- Optional fields per type require validation the type system cannot provide.
- `options` is semantically overloaded — a reader must consult `type` to know what the list *is*. Per-type meaning is recorded here and in ADR-024; it is not restated in code comments.
- `contentCpp` duplicates the non-code blocks of `content`. Simpler than block-level language variants.
- A new classification axis costs a field and therefore an amendment.

## Remarks / Sources

- ADR-024 (answer semantics, normalisation, grading), ADR-010 (mandatory `alt`), ADR-029 (where the bank lives), ADR-025 (filter parameters mirror these key names exactly)
- Removed from earlier drafts and **not** to be reintroduced without an amendment: flat `tags[]`, `sourceUrls[]`, `competition`, `voivodeship`. The last two return as their own fields when voivodeship content is imported.
- `shortAnswer` carries exactly one canonical answer. Mechanical variants are handled by normalisation; genuinely different valid answers need a new field, which is an amendment, not a patch.
- Scraper output format: `c:\Repositories\py-pdf-scraper\sample\output\combined.json`
- Question-type catalogue: `c:\Repositories\py-oij-quizzer\olympiads\custom\research\research-synthesis.md` — Addendum A
