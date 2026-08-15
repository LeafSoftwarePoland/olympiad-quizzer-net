# ADR-031: API error handling — two layers and a coded contract

**Status:** Accepted
**Date:** 2026-08-15

## Problem

Three ADRs require failures to reach the student *gracefully* — a graceful error screen when the
server is unreachable (ADR-002, ADR-013), an empty result handled gracefully (ADR-025). None of
them says where the API catches, what it returns, or what happens to a failure nobody anticipated.

A graceful screen is impossible unless the API returns something structured. Left unspecified, each
action invents its own handling, unanticipated faults escape as raw stack traces or bare 500s, and
the client cannot distinguish "no questions matched" from "the database is gone".

## Considered

**Where the catch sits**

- **Per-action only** — the action knows what its failures mean and can respond precisely. Leaves
  every unanticipated fault to escape unshaped, which is the failure that actually reaches users.
- **Global middleware only** — nothing escapes. Loses the specific meaning of anticipated failures;
  everything collapses to one generic response, so the UI cannot react usefully.
- **Both layers** — per-action for the known, middleware as the floor for the unknown. Two
  mechanisms to keep honest.

**What crosses the wire**

- **Exception detail** — most useful when debugging. Leaks types, stacks, SQL and paths to anyone
  with a browser. Rejected outright.
- **A translated sentence** — the client can display it directly. Puts user-facing copy in a ring
  that is not user-facing, and makes rewording a message a backend release.
- **A stable machine code plus the request identifier** — the client decides what a student reads;
  the identifier ties a report to a log line.

## Decision

**Two layers, both required, plus a coded contract. Neither layer substitutes for the other.**

### Layer 1 — per action

Every action wraps its work and catches only the failures it **accounts for**: the ones it knows
can occur and knows the meaning of. It logs with full detail, then returns a shaped response —
which may legitimately be a 500 — carrying enough for the UI to render something useful.

### Layer 2 — global middleware

Catches **everything** layer 1 did not account for. Logs it as an unhandled-exception occurrence
with full detail. Returns a shaped 500.

The division is the whole point: layer 1 gives the *known* its specific meaning, layer 2 guarantees
the *unknown* never escapes unshaped. An empty catch in an action defeats both.

### The contract

Every failure **the application itself emits** carries a **stable machine code** and the framework's
request identifier. Nothing else.

**The contract is scoped to our own client**, and deliberately does not extend to requests that
client cannot produce. A malformed request is rejected by the framework before application code
runs, and that rejection carries no code. That is the boundary of the contract, not a hole in it:
our client builds its URLs from typed values, so a malformed one is a client bug to fix rather than
a failure mode to design a student-facing experience for. A hand-crafted request is outside our
contract by definition.

**Codes are added when a real failure needs one, never in advance.** Speculative codes are
documentation that lies — two were written and both proved unreachable, one because a missing
database is fatal at startup so a running API always has a working bank, the other because that
rejection belongs to the framework. Today the set is one code plus the client's fallback for an
unrecognised one, and both map to the same Polish string.

- Codes are constants in the Domain project, so API and client compile against one definition
  (ADR-012 amendment).
- **No technical detail leaves the process** — no exception type, message, stack, SQL, path or
  connection string. The request identifier is the only technical value permitted across the wire,
  and it is safe precisely because it is meaningless outside the logs.
- The request identifier is the framework's own; nothing is generated for it.

Accepted cons:

- Two mechanisms rather than one, and a per-action `try` that is easy to forget. Its absence is
  invisible until an anticipated failure arrives as a generic 500.
- A code-to-text map on the client that must stay in step with the codes — mitigated by the shared
  constants and a generic fallback for unknown codes.
- The request identifier is only useful while logs exist. The host retains seven days and there is
  no log sink, so an older report cannot be traced. Accepted: the identifier costs nothing, and a
  sink can be added later without changing this contract.
- Debugging from a response alone is impossible by design. The log is the only place detail lives.

## Remarks / Sources

- ADR-002, ADR-013, ADR-025 — the graceful-handling requirements that this fills the server half of.
- ADR-012 amendment — codes, not prose, and why the tag-vocabulary exception does not extend here.
- **This decision brought L2 into existence.** Middleware is unreachable at L1 by construction, so
  proving the chain end to end requires the full pipeline over HTTP. The tier obligations for each
  layer are derived in `docs/standards/` — this ADR does not restate them.
- Mocking the store to force an arbitrary fault is legitimate at L2: the standards permit a
  substitute exactly where the real external cannot produce the scenario, and a healthy database
  will not throw on command.
