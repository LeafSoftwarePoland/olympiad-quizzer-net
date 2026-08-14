# ADR-028: Crawler control across two origins

**Status:** Accepted
**Date:** 2026-08-13

## Problem

The app spans two origins: the static frontend on a shared pages host, and the API on a container host. Wanted: frontend indexable except quiz routes, API not indexed at all.

Discovered while specifying it: **a crawler-rules file cannot be published for the frontend from this repository.** Crawler rules apply only to the host, protocol and port that served the file, and crawlers do not look for it in subdirectories. The frontend is served from a path *underneath* a shared host, so the authoritative file for that host sits at the host root — which belongs to a different repository. A file shipped inside this project's static assets resolves to a subdirectory path and is ignored.

## Considered

- **Ship the file in the frontend static assets and consider it done** — what was originally assumed. The file is served and ignored. Invalid.
- **Do nothing for the frontend** — quiz routes and answer content get indexed. Content is public anyway, so the harm is modest, but indexed answer pages actively degrade the tool.
- **Move the frontend to a custom domain** so the app owns its host root — solves it cleanly. Costs a domain and a DNS change. Out of scope.
- **Per-route metadata emitted by the app** — works only for crawlers that execute JavaScript. Every route serves the same shell document, so a static tag in that document cannot discriminate routes.
- **API rules via the frontend's file** — impossible, different origin.

## Decision

**Three parts, because one is not available.**

1. **API origin: the API serves its own crawler-rules file**, disallowing everything. The API is at its own host root, so this file is valid and authoritative. Serving it from the application rather than as host configuration keeps it in the deployable (ADR-030).
2. **Frontend origin: a manual change in the user-pages repository.** The disallow rules for quiz and settings paths go in that repo's root file, recorded in `docs/integrations/github-pages.md` with the exact lines, because it is outside this repository and will otherwise be forgotten.
3. **Belt, inside the app: per-route metadata** on quiz and settings routes. This is the belt and not the trousers — it reaches only crawlers that execute JavaScript.

A crawler-rules file also ships in the frontend's static assets even though it is inert today. It costs nothing and becomes authoritative the day the site moves to a custom domain, at which point part 2 disappears.

Accepted cons:

- Frontend crawler control depends on a change in a repository this project does not own, and nothing here can verify it stayed in place.
- Per-route metadata is unreliable by nature.
- An inert file ships in the frontend assets and will read as a mistake to a future reader. Hence this ADR.

## Remarks / Sources

- File scope rules, verified 2026-08-13: https://developers.google.com/search/docs/crawling-indexing/robots/robots_txt — must sit in the top-level directory; subdirectory files are not checked; rules apply only to the host, protocol and port that served the file.
- ADR-004 (pages hosting and the project sub-path), ADR-030 (the route is served by the API and is exempt from the version prefix), ADR-009 (no accounts — nothing private is exposed either way)
