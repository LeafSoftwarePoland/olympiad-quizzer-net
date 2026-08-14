# ADR-012: Language policy

**Status:** Accepted
**Date:** 2026-08-08

## Problem

App targets Polish primary-school students. Which language for UI, content, code, docs and identifiers?

## Considered

- **Polish throughout, identifiers included** — consistent for the maintainer. Polish characters in identifiers cause encoding friction and break international convention.
- **English throughout** — conventional. Target users cannot read the UI.
- **Split by audience** — Polish where a student reads it, English where a developer reads it.

## Decision

**Split by audience.**

| Scope | Language |
|---|---|
| User-facing UI — labels, buttons, messages, errors, page titles | Polish |
| Question text, explanations, image alt text | Polish, as-is from source PDFs |
| Code examples inside questions (Python/C++) | as-is from the original |
| **Client route segments** — the path a student reads in the address bar | **Polish** |
| **API route segments** — `/v1/questions`, `/v1/filters` | **English** |
| C# code, JSON keys, CSS classes, HTML attributes | English |
| ADRs, docs, commit messages, branch names, file names | English |

The two route rows are the one place this split is not obvious, so they are stated rather than derived. A client URL is read by a student, so it is user-facing text and goes Polish. An API URL is read by a program, is a frozen wire contract (ADR-025), and goes English. Do not "make them consistent" — they are consistent, with the audience rule rather than with each other.

**No i18n infrastructure.** Polish-only, no locale switching.

**One exception — tag vocabulary.** Tag identifiers (`category[]`, `algorithms[]` values) are Polish words in Latin letters with diacritics dropped: ą→a, ę→e, ó→o, ś→s, ł→l, ź/ż→z, ć→c, ń→n. So `złożoność` becomes `zlozonosc`. Justified because tags are Polish domain concepts shown directly in the UI — English equivalents are less recognisable to Polish educators — and because they are data values, not code identifiers.

Accepted cons:

- Adding an English UI later needs a refactor. Acceptable for this audience.
- One documented exception to the English-identifier rule, which a reader must know about.

## Remarks / Sources

- Full tag vocabulary: `docs/tags.md`
- If localisation is ever wanted, the platform's string-localiser abstraction is additive and needs no rewrite.
- Predecessor convention: `c:\Repositories\py-oij-quizzer\CLAUDE.md`
