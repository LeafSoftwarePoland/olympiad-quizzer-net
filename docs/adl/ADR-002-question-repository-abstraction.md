# ADR-002: Question repository abstraction

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Questions are read by the browser over HTTP and by the server out of storage. Without a seam, the storage choice and the payload shape leak into every consumer, and changing either is a wide refactor.

## Considered

- **Direct HTTP calls throughout** — no indirection. Couples the URL and the response shape into every caller.
- **One repository abstraction** — single seam; the implementation is chosen at registration.

## Decision

**One repository abstraction, owned by the Domain project.**

- One query operation, taking a structured query object (categories, algorithms, years, stages, count) and a cancellation token. Server-side filtering needs multi-value predicates per tag type (ADR-025); scalar filter fields cannot express them.
- One operation returning the filter values actually present in the bank, with a count each. The UI must not offer a filter for data that does not exist, and cannot know what exists without asking.
- Implemented on **both** sides. The server implementation reads storage (ADR-029); the browser implementation calls the API. Both satisfy the same abstraction, so a schema disagreement across the wire is a compile error rather than a runtime fault in the browser.
- No static-file fallback implementation. Server unreachable means a graceful error screen, not a second data source that can disagree with the first.

Accepted cons:

- One interface plus one implementation class per backend.

## Remarks / Sources

- ADR-029 (storage), ADR-025 (filter contract)
- The abstraction lives in Domain, not in a frontend feature folder (ADR-022). Two same-named abstractions on the two sides of one HTTP call is the drift this ADR exists to prevent.
