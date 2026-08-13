# ADR-035: Question filtering endpoint contract

**Status:** Accepted
**Date:** 2026-08-13
**Amends:** ADR-020 (filtering is v1.0 scope, named there but not specified)

## Problem

v1.0 moves question filtering to the server (plan §3, ADR-020 amendment). The plan fixes the
semantics — OR within a tag type, AND across types, random subset up to a requested count, cap
30, empty result handled gracefully, filter options computed from real data — but not the wire
contract. Without one fixed now, the frontend and the API disagree about parameter names,
casing and error codes, and every disagreement looks like a bug in the other side.

## Considered

- **Client-side filtering over the whole bank** — no endpoint work. The client downloads
  everything, which is what server-side filtering was chosen to stop (ADR-020 amendment,
  ADR-009 amendment). Rejected.
- **`POST` with a filter object in the body** — expresses complex filters naturally, no
  parameter-repetition question, no URL length limit. Not cacheable, not linkable, and a `POST`
  on a read is a semantic lie. Rejected.
- **`GET` with comma-joined values** (`category=a,b`) — shorter URLs. Needs an escaping rule for
  a value containing a comma, and every client must implement the same join. Rejected.
- **`GET` with repeated parameters** (`category=a&category=b`) — native support in the framework
  and in every HTTP client, no custom escaping rule, individually readable in a log line.
- **Hardcoded filter option lists in the UI, taken from the tag vocabulary doc** — no second
  endpoint. Offers filters for data that may not exist, which the plan explicitly forbids.
  Rejected.
- **A second endpoint exposing available filter values with counts** — one small aggregate over
  the already-loaded bank; keeps the UI honest.

## Decision

**Two read-only `GET` endpoints. Repeated query parameters. Server shuffles, server caps.**

### `GET /api/questions`

Parameters, all optional, all repeatable except the count:

| Parameter | Type | Notes |
|---|---|---|
| `category` | string, repeatable | value from the standardized vocabulary |
| `algorithms` | string, repeatable | plural, matching the wire field name |
| `year` | integer, repeatable | |
| `stage` | string, repeatable | `E1` / `E2` / `E3` |
| `limit` | integer | 1–30, default 30 |

Parameter names mirror the wire field names of the question record exactly. No aliases.

Semantics:

- **OR within a tag type.** Two `category` values match a question carrying either.
- **AND across tag types.** A `category` and a `year` match only a question satisfying both.
- **No selection on a type means all values of that type** — not an empty result.
- Comparison is ordinal, case-insensitive, with surrounding whitespace trimmed. The vocabulary
  is diacritic-free Latin snake_case (`docs/tags.md`), so ordinal folding is sufficient and
  locale-independent.
- **Shuffle, then cap.** Capping first would make the result deterministic by bank order.
- Fewer matches than requested → return all matches, shuffled. No padding, no error.
- Zero matches → **HTTP 200 with an empty array.** Not 404, not 400. The frontend turns this
  into "no questions for these filters" and refuses to start a quiz (ADR-025).
- The cap of 30 exists so no single request can drain the bank, and it is enforced twice: the
  endpoint rejects an out-of-range count, and the data layer clamps regardless of caller.

Errors:

- `limit` outside 1–30, or any non-numeric `limit` or `year` → **400** with a problem-details body.
- An **unknown tag value is not an error.** It matches nothing and is logged as a warning. The
  frontend gets its options from the filter endpoint, so an unknown value means a stale client
  or a hand-typed URL — neither justifies a status code a well-behaved client can never trigger.
- An unknown query parameter is ignored, so an older client keeps working.

Response carries the **full** question payload — text, options, correct answers, explanations
and image references. Answers travel to the browser by design because grading is client-side
(ADR-020 amendment, ADR-025). The API does not grade, holds no session, and has no write
endpoints.

### `GET /api/filters`

Returns, for each tag type, the values actually present in the bank with a count each, plus the
total question count. The frontend renders only these, so a filter is never offered for data
that does not exist (plan §3, F-04).

**Scope addition, stated plainly:** the dispatch brief named the questions endpoint as the one
real feature. This second endpoint is added because the "filter options are computed
dynamically" requirement is otherwise unimplementable without hardcoding the vocabulary in the
UI, which the plan forbids. It is a projection over data already in memory — no new storage, no
new dependency.

**Pros:**
- Repeated parameters need no custom join or escape rule on either side
- Cacheable and linkable; a filter combination can be shared as a URL
- Server-side shuffle means the client cannot bias the draw, and the order is fixed once for the
  cached session (ADR-025)
- Empty-result-as-200 keeps "no matches" out of the error path, where it does not belong
- The filter endpoint makes a naming or casing mismatch between the two endpoints testable

**Cons:**
- Long URLs with many selections; still far inside practical limits at 30 questions and a
  16-value vocabulary
- Two endpoints to keep consistent — mitigated by a round-trip test that feeds a value from the
  filter endpoint into the questions endpoint
- No repeat suppression across draws and no range predicates (both explicitly out of scope per
  the plan), so a question can reappear in a later quiz
- Unknown tag values fail silently from the caller's point of view; only a server-side warning
  records it

## Remarks / Sources

- ADR-020 amendment (server-side filtering is the v1.0 decision), ADR-009 and ADR-002
  amendments (static JSON delivery superseded), ADR-003 amendment (repository seam shape)
- ADR-025 (session caches the returned order; empty result must not start a quiz)
- ADR-029 (one concurrent user — no caching layer or rate limiting needed)
- `docs/tags.md` (the vocabulary), `docs/rules/oij.md` (stage identifiers), F-04
- v1.0 solution design §4.4 (filtering rules and implementation) and §5.1 (endpoint definitions)
