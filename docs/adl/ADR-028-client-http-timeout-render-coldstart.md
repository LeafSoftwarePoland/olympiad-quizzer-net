# ADR-028: Client HTTP timeout for Render cold start

**Status:** Shell
**Date:** 2026-08-12
**See also:** `.pipeline/1-architecture/assumptions.md` — A-15

## Problem

Render free tier spins down after 15 min idle. Cold start is ~35 s measured (ADR-007 amendment). Client must not time out during cold start. Current assumption: 90 s timeout covers worst-case spin-up. UptimeRobot 5-min pings reduce cold-start frequency but do not eliminate it (overnight, weekends).

Decision needed: what is the correct client-side HTTP timeout, and should the UI show a "waking up the server…" spinner with a countdown?

## Considered

_To be discussed._

## Decision

_Not decided yet._

## Remarks / Sources

_None yet._
