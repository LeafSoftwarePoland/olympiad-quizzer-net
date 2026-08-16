# ADR-019: Client HTTP timeout covers API cold start

**Status:** Accepted
**Date:** 2026-08-14

## Problem

The API host spins the service down after 15 minutes idle, and cold start measures ~35 s (ADR-005). The default client timeout is far shorter than that, so the first request after an idle period fails and the user sees an error for a service that is merely waking.

## Considered

- **Default timeout** — no configuration. First request after idle fails. Rejected by evidence.
- **Timeout just above measured cold start** (~45 s) — tight. A slow start or a slow connection lands on the wrong side of it, and the failure looks identical to a real outage.
- **Generous fixed timeout (90 s)** — covers measured worst case with headroom. A genuinely dead server makes the user wait 90 s before being told.
- **Retry with backoff instead of a long timeout** — more responsive feedback. Multiple requests to a waking service do not make it wake faster, and each retry restarts the wait.

## Decision

**Single client timeout of 90 s, no retry.**

Cold start is a wait, not a failure, and the only correct response to a wait is to wait. 90 s is roughly 2.5× measured cold start, so a slow start still succeeds. An external 5-minute ping reduces how often this path is hit but cannot eliminate it — overnight and weekend gaps exceed the idle window.

Accepted cons:

- A genuinely unreachable API makes the user wait the full 90 s before seeing an error.
- The number is empirical and tied to one host's behaviour; a host change invalidates it.
- The UI must tell the user something is happening for up to 90 s, or the wait reads as a hang.

## Remarks / Sources

- ADR-005 (idle spin-down, ~35 s measured cold start, external ping), ADR-013 (the frontend has a hard runtime dependency on this service)
- `docs/integrations/render-com.md`
- Revisit if the host plan changes to one that does not sleep.
