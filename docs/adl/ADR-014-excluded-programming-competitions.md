# ADR-014: Excluded competition types — pure programming competitions

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Some voivodeship CS competitions are programming-only. Include in quiz engine?

## Considered

- **Mazowieckie LOGIA** — 3-stage Python programming competition (150–180 min coding sessions). No ABCD, no knowledge test questions. Task bank at `logia.oeiizk.edu.pl/strony/bankzadan/`.
- **Wielkopolskie konkurs tematyczny** — 2-stage algorithmic thinking competition, introduced 2025/26. No public question archive. Only 7 finalists in inaugural year.
- **OIJ programming tasks** (Stages II, III; Stage I remote + practical) — write working programs judged by SIO2. Not compatible with quiz format.

## Decision

**All excluded. Listed here to prevent re-research.**

Quiz engine targets knowledge tests (ABCD, T/F, ordering, matching, short-answer) only. Programming task runners are a separate, much larger sub-problem (sandboxed execution, judge infrastructure).

## Remarks / Sources

- LOGIA task bank (reference only): https://logia.oeiizk.edu.pl/strony/bankzadan/
- OIJ programming tasks: https://oij.edu.pl/zbior_zadan/
- Far future: code execution via Piston API (free, sandboxed) or Judge0 (self-hosted) — tracked separately, not part of current scope
