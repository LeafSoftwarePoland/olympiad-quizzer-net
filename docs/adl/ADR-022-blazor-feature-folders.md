# ADR-022: Feature-based folders in the frontend

**Status:** Accepted
**Date:** 2026-08-13

## Problem

The frontend grouped files by technical kind — pages, components, services, layout. At six pages that worked. v1.0 adds a filter picker, a three-page quiz flow, a settings page, a toast host, session persistence and a mode catalogue. Every change would touch three folders, and no folder name would say what the app does.

## Considered

- **Keep technical-kind folders** — matches the framework template. Related code drifts apart as the count grows; a feature cannot be read, reviewed or deleted in one place.
- **One folder per page, no grouping** — obvious at six pages, unnavigable at twenty. No home for code shared by exactly two features.
- **Feature folders** — one folder per feature holds its pages, state, services and private components; a shared folder holds only what two or more features use.
- **One class library per feature** — strongest isolation. Six extra projects, split payload, longer builds. Disproportionate.

## Decision

**Feature folders.**

```
Features/
  Home/        landing page, not-found page
  Quiz/        setup, run and summary pages; session state; HTTP question access;
               Components/ for this feature's private components
  Settings/    settings page, user-preference state
  Info/        static informational pages per competition
Shared/
  Components/  used by two or more features
  Services/    used by two or more features
Layout/        app shell — not a feature
wwwroot/       static assets
```

Rules:

- A feature owns its pages, its state, its services and its private components.
- A components folder nested inside a feature holds that feature's private components. The location **is** the access modifier.
- Promote into the shared folder only on the **second** consumer. One consumer means it stays inside the feature.
- Folder path mirrors the namespace.
- The app shell stays outside the features folder. It is not a feature.
- Domain abstractions do **not** move into a feature folder. The frontend contributes only its HTTP implementation of the repository abstraction (ADR-002).

Accepted cons:

- Deviates from the framework template, so it reads as unfamiliar at first glance.
- "Is this shared yet?" is a judgement call, bounded by the second-consumer rule.
- Cross-feature navigation still couples features through route strings. Unavoidable.

## Remarks / Sources

- ADR-023 (the solution layout this sits inside — the frontend is the Presentation ring), ADR-014 (components own their CSS), ADR-002 (one abstraction, one home)
