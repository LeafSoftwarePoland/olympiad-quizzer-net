# ADR-025: Question filtering endpoint contract

**Status:** Accepted
**Date:** 2026-08-14

## Problem

Filtering happens server-side (ADR-013). The semantics were fixed — OR within a tag type, AND across types, random subset up to a requested count, cap 30, empty result handled gracefully, filter options computed from real data — but not the wire contract. Without one fixed, the frontend and the API disagree about parameter names, casing and error codes, and every disagreement looks like a bug in the other side.

## Considered

- **Filter in the browser over the whole bank** — no endpoint work. The client downloads everything, which is what server-side filtering exists to stop. Rejected.
- **`POST` with a filter object in the body** — expresses complex filters naturally, no URL length limit. Not cacheable, not linkable, and a `POST` on a read is a semantic lie. Rejected.
- **`GET` with comma-joined values** — shorter URLs. Needs an escaping rule for values containing a comma, and every client must implement the same join. Rejected.
- **`GET` with repeated parameters** — native in the framework and in every HTTP client, no custom escaping, individually readable in a log line.
- **Hardcode filter option lists in the UI from the vocabulary doc** — no second endpoint. Offers filters for data that may not exist. Rejected.
- **A second endpoint exposing available filter values with counts** — one small aggregate over data already in memory; keeps the UI honest.

## Decision

**Two read-only `GET` endpoints under a version segment. Repeated query parameters. Server shuffles, server caps.**

### `GET /v1/questions`

All parameters optional, all repeatable except the count:

| Parameter | Type | Notes |
|---|---|---|
| `category` | string, repeatable | value from the standardised vocabulary |
| `algorithms` | string, repeatable | plural, matching the wire field name |
| `year` | integer, repeatable | |
| `stage` | string, repeatable | `E1` / `E2` / `E3` |
| `limit` | integer | 1–30, default 30 |

Parameter names mirror the question record's wire field names exactly (ADR-007). No aliases.

Semantics:

- **OR within a tag type.** Two `category` values match a question carrying either.
- **AND across tag types.** A `category` and a `year` match only a question satisfying both.
- **No selection on a type means all values of that type**, not an empty result.
- Comparison is ordinal, case-insensitive, surrounding whitespace trimmed. The vocabulary is diacritic-free Latin snake_case (ADR-012), so ordinal folding is sufficient and locale-independent.
- **Shuffle, then cap.** Capping first would make the result deterministic by bank order.
- Fewer matches than requested returns all matches, shuffled. No padding, no error.
- **Zero matches returns HTTP 200 with an empty array.** Not 404, not 400, and **not 204** — the client deserialises an array, and a bodyless response breaks it. This is pinned; do not "improve" it.
- The cap of 30 is enforced twice: the endpoint rejects an out-of-range count, and the data layer clamps regardless of caller.

Errors:

- `limit` outside 1–30, or a non-numeric `limit` or `year`, returns **400 with a problem-details body**. Some of these are produced by the framework's automatic model-state response rather than by explicit code (ADR-030); the status and the problem-details media type are the contract, the exact field set inside the body is not.
- An **unknown tag value is not an error.** It matches nothing and is logged as a warning. The frontend gets its options from the filter endpoint, so an unknown value means a stale client or a hand-typed URL — neither justifies a status code a well-behaved client can never trigger.
- An unknown query parameter is ignored, so an older client keeps working.

Response carries the **full** question payload including correct answers and explanations, because grading is client-side (ADR-013).

### `GET /v1/filters`

Returns, per tag type, the values actually present in the bank with a count each, plus the total question count. The frontend renders only these, so a filter is never offered for data that does not exist.

Accepted cons:

- Long URLs with many selections. Far inside practical limits at 30 questions and a small vocabulary.
- Two endpoints to keep consistent. Mitigated by a round-trip test feeding a value from the filter endpoint into the questions endpoint.
- No repeat suppression across draws and no range predicates, so a question can reappear in a later quiz.
- Unknown tag values fail silently from the caller's point of view; only a server-side warning records it.

## Remarks / Sources

- ADR-030 (route shape, version segment, controller composition), ADR-013 (why the API is read-only and answers travel), ADR-007 (field names these parameters mirror), ADR-016 (the browser caches the returned order), ADR-020 (one concurrent user — no caching layer or rate limiting)
- `docs/tags.md` (vocabulary), `docs/rules/oij.md` (stage identifiers)
