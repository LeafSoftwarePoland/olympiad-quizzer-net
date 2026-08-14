# ADR-020: Scalability posture — one concurrent user

**Status:** Shell
**Date:** 2026-08-12

## Problem

v1.0 is designed for one concurrent user. No load testing, no concurrency design, no caching layer, no rate limiting — all deliberate (ADR-013). The API host provides 0.1 CPU and 512 MB (ADR-005). A class of 30 students hitting the URL at once will exhaust it. No decision has been made about what happens then.

Open questions: at what concurrent-user count does the architecture have to change, and what is the upgrade path — a paid plan on the current host, the deferred free ARM option (ADR-006), or something else?

## Considered

_To be discussed._

## Decision

_Not decided yet._

## Remarks / Sources

- ADR-005 (host resource limits), ADR-006 (deferred alternative with large headroom), ADR-013 (no caching layer or rate limiting, on the strength of this posture)
