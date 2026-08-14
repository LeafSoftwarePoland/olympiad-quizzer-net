# ADR-008: Excluded sources and competition types

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Many Polish platforms publish IT quiz content, and several voivodeship CS competitions exist that are not knowledge tests. Which are in scope? Recorded here so the research is not repeated.

## Considered

**Sources — commercial or unverifiable**

- **olimpus.edu.pl** — private national olympiad, paid registration, teacher login for archives. Wrong competition type.
- **okno.edu.pl** — commercial tutoring. No free archive.
- **Studocu** — paywalled, user-uploaded, provenance uncertain.
- **Kahoot (doradcazinformatyki)** — login required, no answer keys, provenance unverifiable.
- **alemozgi.pl / codeforia.com** — combined maths+IT competition, not konkurs przedmiotowy z informatyki.

**Competition types — programming, not knowledge testing**

- **Mazowieckie LOGIA** — three-stage Python programming competition, multi-hour coding sessions. No test questions at all.
- **Wielkopolskie konkurs tematyczny** — algorithmic thinking, introduced 2025/26. No public archive; 7 finalists in its first year.
- **OIJ programming tasks** (Stage I practical, Stages II–III) — write working programs judged by an online judge. Incompatible with a quiz format.

## Decision

**All of the above excluded permanently.** Content comes only from official public materials: Kuratoria Oświaty archives and the OIJ archive.

Rationale: the excluded sources are wrong competition type, paywalled, or unverifiable, and using them risks both copyright exposure and content mismatch with the real competition scope. The excluded competition types would require a sandboxed code-execution judge — a separate and much larger problem.

Accepted cons:

- Smaller content pool than if commercial archives were used.
- Programming-track students are not served by this tool.

## Remarks / Sources

- Usable official sources: Małopolskie, Kujawsko-Pomorskie, Pomorskie, Podkarpackie, Podlaskie, Łódzkie, Śląskie (2025/26), OIJ XIV–XX plus mock rounds.
- Re-evaluate only if official archives are exhausted and a content gap becomes critical.
- Far future, tracked separately: code execution via a sandboxed judge service. Not in scope.
- LOGIA task bank, reference only: https://logia.oeiizk.edu.pl/strony/bankzadan/
- OIJ programming tasks: https://oij.edu.pl/zbior_zadan/
- Full source assessment: `c:\Repositories\py-oij-quizzer\olympiads\custom\research\research-synthesis.md`
