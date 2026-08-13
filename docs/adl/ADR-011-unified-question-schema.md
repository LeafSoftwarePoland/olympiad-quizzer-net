# ADR-011: Unified question schema

**Status:** Accepted
**Date:** 2026-08-08
**Updated:** 2026-08-08 (camelCase keys)

## Problem

OIJ and voivodeship konkursy have different question types, grading rules, and metadata. Need one JSON schema for both.

## Considered

- **Separate schemas per competition** — clean isolation. Duplicates shared fields. Hard to query across competitions.
- **Unified schema with optional fields** — one record type, type-discriminated optional fields.

## Decision

**Unified schema, type-discriminated.**

### Source enum

`source`: `"oij" | "vea" | "other"`

- `oij` — Olimpiada Informatyczna Juniorów (national)
- `vea` — Voivodeship Educational Authority (Kuratorium) konkurs przedmiotowy
- `other` — reserved

### Content blocks

Question text, code, and images are **not flat strings**. They are ordered arrays of typed blocks matching the py-pdf-scraper output format:

```json
[
  { "type": "text",  "text": "Ile gwiazdek wypisze poniższy kod?" },
  { "type": "code",  "text": "for i in range(3):\n    print('*')" },
  { "type": "image", "file": "images/oij/q18.png" }
]
```

Block types: `text` | `code` | `image`

### Full field list

```json
{
  "id":            "string",
  "source":        "oij | vea | other",
  "competition":   "OIJ | Śląskie | Małopolskie | ...",
  "voivodeship":   "string | null",
  "stage":         "int | null",
  "year":          "string",
  "type":          "multiSelect | shortAnswer | singleAbcd | trueFalse | ordering | matching",
  "content":       "[ContentBlock]",
  "contentCpp":    "[ContentBlock] | null",
  "options":       ["string"] | null,
  "matchOptions":  ["string"] | null,
  "correctAnswer": "(varies by type — see below)",
  "points":        "int",
  "partialCredit": "bool",
  "tags":          ["string"],
  "sourceUrls":    ["string"],
  "explanation":   "[ContentBlock] | null"
}
```

JSON keys: **camelCase** throughout. Enum values: camelCase (`multiSelect`, `shortAnswer`, `singleAbcd`, `trueFalse`). C# property names: PascalCase — `JsonNamingPolicy.CamelCase` handles serialization automatically.

Removed: `text` (replaced by `content`), `image` (embedded in content blocks), `source_url` (replaced by `sourceUrls[]`), `code_py` / `code_cpp` (replaced by `content` / `contentCpp`).

### correct_answer shape by type

| Type | Shape | Notes |
|---|---|---|
| `multiSelect` | `[0, 1]` | option indices |
| `singleAbcd` | `[2]` | single-element array |
| `shortAnswer` | `["kajak"]` or `["AF₁₆", "AF16"]` | **array** — multiple valid forms |
| `trueFalse` | `[true, false, true]` | bool per statement |
| `ordering` | `[1, 2, 0]` | option indices in correct order |
| `matching` | `[2, 0, 3, 1]` | for left[i] → index in matchOptions |

### OIJ language toggle

`content` = Python (default). `contentCpp` = C++ variant (non-null only for OIJ questions with code). UI shows toggle when `contentCpp != null`.

### Unicode

Question text contains Polish diacritics, mathematical italic Unicode (𝑥 𝑎 𝑛 from PDF rendering), superscripts (²⁶), subscripts (₁₆), middle dot (·). All stored as UTF-8 strings — no special encoding. `short_answer` grader must apply `string.Normalize(NormalizationForm.FormC)` before comparison.

**Pros:**
- Single `questions.json` for all content
- Matches py-pdf-scraper output format directly — no import conversion for content structure
- Filter by any field (competition, voivodeship, type, tag)
- Multiple valid short answers supported natively

**Cons:**
- Nullable fields for type-specific data — requires validation
- `correct_answer` shape varies — discriminated union pattern in C# needed
- `content_cpp` duplicates non-code text blocks (acceptable — simpler than block-level language variants)

## Remarks / Sources

- Śląskie voivodeship = widest type superset — schema satisfies its types = satisfies all others
- Partial credit types: `true_false`, `ordering`, `matching`
- py-pdf-scraper output format: `c:\Repositories\py-pdf-scraper\sample\output\combined.json`
- Research synthesis (question type catalogue): `c:\Repositories\py-oij-quizzer\olympiads\custom\research\research-synthesis.md` — Addendum A

## Override history

| Date | What changed | Why |
|---|---|---|
| 2026-08-08 | `text: string` → `content: ContentBlock[]`; `explanation: string` → `ContentBlock[]`; `source_url` → `source_urls[]`; `code_py`/`code_cpp` → `content_cpp: ContentBlock[]`; `source: custom` → `vea`, added `other`; `short_answer` correct_answer → `string[]` | Scraper output uses block arrays; multiple valid answers needed; enum renamed for clarity |
| 2026-08-08 | All JSON keys → camelCase (`content_cpp` → `contentCpp`, `match_options` → `matchOptions`, `correct_answer` → `correctAnswer`, `partial_credit` → `partialCredit`, `source_urls` → `sourceUrls`); enum values → camelCase (`multi_select` → `multiSelect`, etc.) | C# convention — `JsonNamingPolicy.CamelCase` is default; snake_case is Python, not .NET |

## Amendment — 2026-08-12 — typed tag fields replace flat tags[] (breaking)

**Overrides:** Full field list — `tags` field removed; replaced by separate typed arrays below.

**Adds:** Typed tag schema.

- `category: string[]` — mandatory, never empty. Standardized vocabulary. See `docs/tags.md`.
- `algorithms: string[]` — optional. Named algorithms only. See `docs/tags.md`.
- `source: string` — origin code, format `"OIJ-2024-E1"`. Plain string, not enum.
- `sourceUrl: string` — link to original source (e.g. oij.edu.pl). Per question.
- `year: int | null` — year question appeared. null if unknown.
- `difficulty: int | null` — 1–5 scale. null if not assessed. See `docs/tags.md`.
- `explanationSource: string` — free text. Values in use: `"AI generated"`, `"official"`, `"documentation"`, `"community"`.
- `source_raw: string` — raw PDF filename for traceability. Not for display or filtering.
- Old `sourceUrls: string[]` field removed — replaced by `sourceUrl: string` (single URL per question).
- Because tag fields are typed, free-form extra tags are no longer the model. New classification axes get their own field.
- `category` is the only required tag field. `year`, `difficulty` are nullable. Others are optional but recommended.

## Amendment — 2026-08-12 — question type renames and correctAnswer shape per type

**Overrides:** `type` enum, `correctAnswer shape by type` table.

Type enum values:
- `single` (replaces `singleAbcd`) — exactly one correct answer. `correctAnswer: string`.
- `multi` (replaces `multiSelect`) — multiple correct answers. `correctAnswer: string[]`.
- `shortAnswer` — short text answer. `correctAnswer: string`.
- `trueFalse`, `ordering`, `matching` — unchanged. Kept for future VEA content. No current OIJ questions use them. UI does not need to render them yet.
- `open` from scraper output → map to `shortAnswer`. Do not import as `open`.

Answer shape: `string` for `single` and `shortAnswer`; `string[]` for `multi`. No third shape.

## Amendment — 2026-08-13 — v1.0 field list frozen

**Overrides:** Full field list, and the `correct_answer shape by type` table.

Final v1.0 record. Keys camelCase throughout, as this ADR already requires.

| key | type | required | note |
|---|---|---|---|
| `id` | `int` | yes | was `string`. Stable, unique across the bank, never reused. Images are named after it. |
| `category` | `string[]` | yes, non-empty | vocabulary in `docs/tags.md` |
| `algorithms` | `string[]` | no | may be `[]` |
| `olympiad` | `string` | yes | **new** — see below |
| `stage` | `string` | yes | **changed** from `int \| null`. `E1`/`E2`/`E3` |
| `year` | `int \| null` | no | was `string` |
| `difficulty` | `int \| null` | no | 1–5 |
| `source` | `string` | recommended | `OIJ-2024-E1` |
| `sourceUrl` | `string` | recommended | single URL |
| `sourceRaw` | `string` | no | **key normalised** from `source_raw` |
| `explanationSource` | `string` | no | free text |
| `type` | enum | yes | `single`/`multi`/`shortAnswer`/`trueFalse`/`ordering`/`matching` |
| `content` | `ContentBlock[]` | yes, non-empty | |
| `contentCpp` | `ContentBlock[] \| null` | no | |
| `options` | `string[] \| null` | per type | `null` for `shortAnswer` |
| `matchOptions` | `string[] \| null` | per type | non-null only for `matching` |
| `explanation` | `ContentBlock[] \| null` | no | |
| `correctAnswer` | `string \| string[]` | yes | option **text**, not an index — ADR-034 |
| `points` | `int` | defaulted 1 | |
| `partialCredit` | `bool` | defaulted false | |

Content block gains `alt` — Polish alt text, **mandatory** on `image` blocks, describing the
image in enough detail to answer the question. A decorative-level alt is a content bug
(ADR-017, F-08).

**Removed:** `competition`, `voivodeship`. Both return as their own fields when VEA content is
imported — a new classification axis gets a new field, per this ADR's 2026-08-12 amendment.
Also gone, as already amended: `tags`, `sourceUrls`.

**Adds:** two fields and one key normalisation, with reasons.

- `stage: string` — the v1.0 filter contract accepts a stage. The stage is otherwise encoded only
  inside `source`, whose optional trailing part makes the last segment ambiguous, so parsing it
  at query time is fragile and puts string parsing on the request path. An explicit field is set
  once during migration.
- `olympiad: string` — not needed by any v1.0 filter. Added because multi-olympiad support is the
  plan's long-term driver and back-filling a field across a hand-maintained bank later is the
  expensive path. `"OIJ"` for the whole current corpus. Not an enum, for the same reason `source`
  is not.
- `sourceRaw`, not `source_raw`. The 2026-08-12 amendment wrote it snake_case, contradicting this
  ADR's own camelCase rule. camelCase wins.

**Overrides:** the Unicode section's normalisation guidance for grading.

- The single FormC rule is replaced by two rules — canonical composition for closed-list answers,
  canonical **compatibility** composition plus whitespace collapsing for typed free text. See
  ADR-034 for why one rule cannot serve both.
- Consequence of the string answer shape, recorded so it is not lost: a `shortAnswer` question now
  carries exactly one canonical answer, where this ADR originally allowed an array of valid forms.
  Mechanical variants are handled by normalisation; genuinely different valid answers would need a
  new field and therefore a new ADR.

**Adds:** the invariant that value-based answers create.

- For every closed-list question, each `correctAnswer` value must appear among `options` (or among
  `matchOptions` for `matching`) after normalisation. Not expressible in a type, so it is asserted
  by an integration-level data-integrity test over the real bank. Violating it grades a correct
  answer as wrong, silently.
