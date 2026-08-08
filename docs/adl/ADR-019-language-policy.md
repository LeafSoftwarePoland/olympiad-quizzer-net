# ADR-019: Language policy

**Status:** Accepted
**Date:** 2026-08-08

## Problem

App targets Polish primary-school students. What language for UI, code, docs, and configuration?

## Decision

| Scope | Language |
|---|---|
| User-facing UI (labels, buttons, messages, errors) | Polish |
| Question text and explanations | Polish (as-is from source PDFs) |
| Code examples within questions (Python/C++) | As-is from original |
| C# code, JSON keys, CSS classes, HTML attributes | English |
| Pipeline artifacts, ADRs, specs, commit messages | English |
| Git branch names, file names | English |

**No i18n infrastructure needed.** Polish-only. No locale switching.

**Pros:**
- Students read Polish — no comprehension barrier
- Code/config in English — international conventions, no encoding issues with Polish characters in identifiers
- Consistent with CLAUDE.md convention from predecessor project

**Cons:**
- Cannot add English UI without refactor — acceptable given target audience

## Remarks / Sources

- If localization ever needed: `IStringLocalizer<T>` from `Microsoft.Extensions.Localization` — additive, no rewrite
- OIJ question text is in Polish; code blocks are Python/C++ (language-neutral)
- Predecessor convention: `c:\Repositories\py-oij-quizzer\CLAUDE.md`
