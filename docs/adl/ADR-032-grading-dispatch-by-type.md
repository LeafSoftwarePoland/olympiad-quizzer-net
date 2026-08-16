# ADR-032: Grading dispatches by question type, one unit per type

**Status:** Accepted
**Date:** 2026-08-15

## Problem

Grading behaviour differs per question type (ADR-024): closed-list equality for `single`, set
equality for `multi`, free-text comparison for `shortAnswer`, positional comparison for
`trueFalse`, `ordering` and `matching`.

Expressed as one unit, that becomes a conditional over the type — a chain that every type must be
added to, in a method every other type already depends on. Three types are implemented and three
more are stubbed for voivodeship content, so the chain is certain to grow. Each addition then edits
code that working types rely on, and the unit accumulates every rule in the system.

The signal arrived from the mirror rule: one grading unit had accumulated **six** test files, which
is the complexity meter reporting six responsibilities in one place.

## Considered

- **One unit, conditional on type** — fewest moving parts, and correct if the set of types were
  closed forever. It is not: three types are already stubbed and voivodeship content will add more.
  Every new type edits shared code, so the risk of breaking a working type grows with each
  addition.
- **One unit per type, selected by a conditional factory** — isolates the rules. The selection
  itself is still a chain that every new type must be added to, so the open-closed problem moves
  rather than disappears.
- **One unit per type, resolved by registration** — each type's rules are self-contained and each
  unit declares which type it handles. Adding a type is a new file plus a registration line;
  nothing existing is edited. Each unit is independently substitutable, so callers above can be
  tested against a controlled grader.
- **Static units per type** — separates the rules but cannot carry a shared contract, since a
  static class implements no interface. Nothing forces a new unit to match the shape of the others,
  and nothing can substitute one in a test.

## Decision

**One grading unit per question type. Each declares the type it handles and satisfies a shared
contract. Selection is by registration, not by a conditional.**

- The contract is an abstraction in the Domain project. Every unit implements it, so a new type
  cannot silently diverge in shape.
- Adding a question type is **additive**: a new unit and a registration. No existing grading code
  is modified — open for extension, closed for modification.
- Scoring — converting matched-against-total into points, honouring partial credit — stays a single
  shared unit. It is genuinely common to every type and duplicating it would let types drift on the
  one rule they must agree about.
- Each unit is independently substitutable, so a caller can be tested against a grader that returns
  a chosen verdict or throws. The callers this matters for are **in the browser** — grading is
  client-side (ADR-013), so the session and summary components are what gain testable seams. No
  controller grades anything.

Accepted cons:

- More files and a registration step where a single method once sufficed.
- Selection failure becomes a runtime concern: an unregistered type must fail loudly, not silently
  grade as wrong. Silent mis-grading is the failure mode ADR-007 names as the worst this app has.
- Reading "how is `multi` graded" means finding its unit rather than scrolling one method — the
  price of the isolation that makes each type safe to change.

## Remarks / Sources

- ADR-024 — the per-type semantics these units implement. This ADR governs how they are organised
  and selected; it does not change any grading rule.
- ADR-007 — the answers-exist-among-options invariant, and why silent mis-grading is the failure
  mode that justifies the loud-failure requirement above.
- Trigger: the mirror rule in `docs/standards/`. Six test files against one production unit is the
  complexity meter working as designed, and this decision is the response to it rather than a
  refactor undertaken for taste.
