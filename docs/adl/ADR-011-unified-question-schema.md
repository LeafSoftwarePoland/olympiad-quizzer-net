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
