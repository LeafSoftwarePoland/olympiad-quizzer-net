# ADR-031: Blazor WASM feature-based folder structure

**Status:** Accepted
**Date:** 2026-08-13

## Problem

POC frontend groups files by technical kind — `Pages/`, `Components/`, `Services/`, `Layout/`.
At six pages that worked. v1.0 adds a filter picker, a three-page quiz flow, a settings page, a
toast host, session persistence and a mode catalogue. Every change would touch three folders,
and no folder would say what the app does.

## Considered

- **Keep technical-kind folders** — familiar, matches the framework template. Related code
  drifts apart as the count grows; a feature cannot be read, reviewed or deleted in one place.
- **One folder per page, no grouping** — obvious at six pages, unnavigable at twenty. No home
  for code shared between exactly two features.
- **Feature folders** — one folder per feature holds that feature's pages, state, services and
  private components. A shared folder holds only what two or more features genuinely both use.
- **One Razor class library per feature** — strongest isolation. Six extra projects, split
  payload, longer builds. Absurd at this size.

## Decision

**Feature-based folders in the frontend project.**

```
Features/
  Home/        landing page, not-found page
  Quiz/        setup, run and summary pages; session state; HTTP question access;
               Components/ for this feature's private components
  Settings/    settings page, user-preference state
  Info/        static informational pages per competition
Shared/
  Components/  components used by two or more features
  Services/    services used by two or more features
Layout/        app shell — not a feature
wwwroot/       static assets
```

Rules:

- A feature owns its pages, its state, its services and its private components.
- A `Components/` folder nested inside a feature is that feature's private components. Nesting
  is not a contradiction — the location is the access modifier.
- Promote into the shared folder only on the **second** consumer. One consumer means it stays
  inside the feature.
- Folder path mirrors the namespace.
- The app shell stays outside `Features/` — it is not a feature.

Domain abstractions do **not** move into a feature folder. The question-repository abstraction
lives in the Domain project; the frontend contributes only its HTTP implementation of that
abstraction. Two same-named abstractions on the two sides of one HTTP call is exactly the drift
ADR-021 exists to prevent.

**Pros:**
- A feature is readable, reviewable and deletable in one place
- Change locality — a quiz change touches one folder
- A folder listing describes the product, not the framework

**Cons:**
- Deviates from the framework template, so unfamiliar at first glance
- "Is this shared yet?" needs a judgement call — mitigated by the second-consumer rule
- Cross-feature navigation still couples features through route strings (unavoidable)

## Remarks / Sources

- ADR-021 (one definition of the wire format), ADR-023 (no CSS framework — components own their CSS),
  ADR-032 (solution layout this sits inside)
- v1.0 solution design §6 for the concrete tree, service registrations and the quiz flow
- `iterates_with_user: true` — the structure is settled; the user-facing route strings and
  Polish labels inside these folders are not, and are expected to change with review
