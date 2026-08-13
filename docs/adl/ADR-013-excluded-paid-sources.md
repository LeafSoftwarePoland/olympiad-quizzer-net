# ADR-013: Excluded sources — private/paid olympiads

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Multiple commercial platforms publish IT quiz content for Polish students. Include?

## Considered

- **olimpus.edu.pl** — private national olympiad, 16 PLN/student registration, teacher login for archives. Wrong competition type (not a voivodeship konkurs przedmiotowy).
- **okno.edu.pl** — commercial tutoring, 99 PLN diagnosis + 1 699 PLN course. No free archive.
- **Studocu** — paywall, user-uploaded unverified content, provenance uncertain.
- **Kahoot (doradcazinformatyki)** — login required, no answer keys, unverifiable provenance.
- **alemozgi.pl / codeforia.com** — different competition (combined maths+IT, not informatyka konkurs przedmiotowy).

## Decision

**All excluded permanently.** App uses only official public materials from Kuratoria Oświaty and OIJ.

**Rationale:** These are either wrong competition type, paywalled, or unverifiable. Using them risks copyright issues and content mismatch with actual competition scope.

## Remarks / Sources

- Re-evaluate only if official archives exhausted and content gap is critical
- Usable official sources: Małopolskie, Kujawsko-Pomorskie, Pomorskie, Podkarpackie, Podlaskie, Łódzkie, Śląskie (2025/26), OIJ XIV–XX + mock
- Full source assessment: `c:\Repositories\py-oij-quizzer\olympiads\custom\research\research-synthesis.md` — Sources assessment section
