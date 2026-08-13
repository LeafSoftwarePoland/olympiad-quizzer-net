# ADR-029: Scalability posture — one concurrent user

**Status:** Shell
**Date:** 2026-08-12
**See also:** `.pipeline/1-architecture/assumptions.md` — A-16

## Problem

POC and v1.0 are designed for one concurrent user. No load testing, no concurrency design. Render free tier: 0.1 CPU, 512 MB RAM. The moment a class of 30 students uses the URL simultaneously, the free tier will buckle. No explicit decision has been made about what to do when that happens.

Decision needed: at what user count does the architecture change? What is the upgrade path — Render paid tier, Oracle Cloud free ARM (ADR-008), or other?

## Considered

_To be discussed._

## Decision

_Not decided yet._

## Remarks / Sources

_None yet._
