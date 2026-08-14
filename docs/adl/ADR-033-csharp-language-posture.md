# ADR-033: C# language posture — nullable disabled, no top-level statements

**Status:** Accepted
**Date:** 2026-08-13

## Problem

POC used nullable reference types enabled and top-level statements — both framework template
defaults. Both fought the codebase. The question schema has genuinely optional fields
throughout (`year`, `difficulty`, `options`, `matchOptions`, `explanation`, `contentCpp`), so
almost every property acquired a nullable annotation carrying no information, plus
null-forgiving operators wherever the code knew better than the compiler. Top-level statements
produce an internal entry-point type, which needs a trailing partial declaration for the
integration test host and cannot be used as a logger category without ceremony.

## Considered

- **Nullable enabled, annotate honestly** — best compile-time safety in principle. In practice
  the schema's optional fields make the annotations near-universal and therefore
  information-free, and warnings become background noise, which is worse than no warnings.
- **Annotations without warnings** (`Nullable: annotations`) — half measure. Annotations the
  compiler does not check are documentation that rots.
- **Nullable disabled, guard with code** — explicit null checks at boundaries. Loses compiler
  help; gains a codebase where every null check is visible.
- **Top-level statements** — fewer lines. Costs the trailing partial declaration, no injectable
  entry-point type for logging, and no way to split startup by concern.
- **Explicit entry-point class with an explicit `Main`** — six more lines; public by choice,
  splittable across files.

## Decision

**Nullable reference types disabled in every project. No top-level statements anywhere.**

Nullable posture:

- No nullable annotations on reference types. No null-forgiving operator. No pragmas
  re-enabling nullability locally.
- Nullability on **value** types stays — an optional year is a nullable `int` and is unrelated.
- Nulls are handled by guard clauses and by validation at boundaries (query string, browser
  storage, the question bank file), not by annotations.
- `TreatWarningsAsErrors` is enabled. With nullable noise gone a warning means something, so it
  can be fatal. Suppress specific diagnostic IDs with a stated reason; never disable the
  property.

Entry points:

- Explicit class with an explicit `Main`, declared **public** so the integration test host and
  the logger factory both reach it without a friend-assembly attribute.
- Declared **partial** so startup splits by concern across files rather than becoming one long
  method. With an explicitly public class the partial keyword is not *required* by the test
  host — that requirement belongs to top-level statements, which emit an internal type. It is
  kept for the file split, and stating this prevents the myth being copied forward.
- The frontend entry point follows the same shape for consistency, though nothing drives it the
  way the test host drives the API's.

Also settled here because it travels with the same decision:

- Implicit usings stay **enabled** — that setting concerns using directives, not type
  inference, and it removes real noise.
- Explicit types are preferred over inferred locals except where the type already appears on the
  same line. Detail in `docs/coding-standards.md`.

**Pros:**
- No annotation noise on a schema that is legitimately full of optional fields
- Warnings-as-errors becomes usable, which matters more in a one-maintainer repo
- Injectable, splittable, testable entry point
- Every null check is visible in the code rather than implied by a type

**Cons:**
- Loses compiler null-flow analysis entirely — a real safety cost, paid knowingly
- New contributors must unlearn the modern default
- Migration work: every inherited annotation and null-forgiving operator must be stripped, and
  with warnings-as-errors that is a build break rather than a warning, which is the point
- Nullable-enabled is the ecosystem direction; re-enabling later is a large mechanical change

## Amendment — 2026-08-14 — entry point no longer partial (ADR-041)

**Overrides:** Decision → Entry points → the `partial` bullet.

- Entry-point types are **not** declared `partial`. ADR-041 moves routes and startup configuration
  out of the entry point into their own units, so there is nothing left to split across files.
- The **public** requirement is unchanged and is the load-bearing one — the integration test host and
  the logger factory both reach the type through it.
- The bullet's clarification stands and is worth keeping: `partial` was never *required* by the test
  host. That requirement belongs to top-level statements, which emit an internal type.
- Nullable posture, warnings-as-errors, implicit usings and `var` policy: all unchanged.

## Remarks / Sources

- Reversal trigger: if the question schema ever becomes mostly-required — for example after a
  content editor tool enforces completeness — the noise argument weakens and this is revisited.
- ADR-011 and its 2026-08-12 amendments — the optional fields that drive this
- ADR-032 (solution layout), `docs/coding-standards.md`
- v1.0 solution design §2.4 (project settings), §5.1 (entry point), §8.3 (migration trap)
