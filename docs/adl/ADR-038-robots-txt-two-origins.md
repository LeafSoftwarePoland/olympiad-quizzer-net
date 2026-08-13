# ADR-038: Crawler control across two origins

**Status:** Accepted
**Date:** 2026-08-13

## Problem

The app spans two origins: the static frontend on the project pages host, and the API on the
container host. The plan calls for two `robots.txt` files — frontend indexable except quiz
routes, API not indexed at all.

Discovered while specifying it: **a `robots.txt` cannot be published for the frontend from this
repository.** Crawler rules apply only to the host, protocol and port serving the file, and
crawlers do not look for the file in subdirectories
(verified 2026-08-13 — https://developers.google.com/search/docs/crawling-indexing/robots/robots_txt).

The frontend is served from a path *underneath* a shared host. The authoritative file for that
host sits at the host root, which belongs to a **different repository** — the user-pages repo.
A file shipped inside this project's static assets resolves to a subdirectory path and is
ignored.

## Considered

- **Ship a `robots.txt` in the frontend static assets and consider it done** — what the plan
  assumed. Invalid for the reason above: the file is served, and ignored.
- **Do nothing for the frontend** — quiz routes and answer content get indexed. The content is
  public anyway, so the harm is modest, but indexed answer pages actively degrade the tool.
- **Move the frontend to a custom domain** so the app owns its host root — solves it properly
  and cleanly. Costs a domain and a DNS change; out of scope for v1.0.
- **Per-route `noindex` metadata emitted by the app** — works for crawlers that render
  JavaScript, worthless for those that do not. Every route serves the same shell document, so a
  static tag in that document cannot discriminate routes; only a per-page one can.
- **API rules via the frontend's file** — impossible, different origin.

## Decision

**Three parts, because one is not available.**

1. **API origin: serve crawler rules from the API itself**, disallowing everything. The API is at
   the root of its own host, so this file is valid and authoritative. Serving it from the
   application rather than as a static file keeps it in the deployable and out of the host
   configuration.
2. **Frontend origin: document a manual change in the user-pages repository.** The disallow rules
   for the quiz and settings paths go in that repo's root file. Recorded in
   `docs/integrations/github-pages.md` as a manual step with the exact lines, because it is
   outside this repository and will otherwise be forgotten.
3. **Belt, inside the app: per-route `noindex` metadata** on the quiz and settings routes, emitted
   through the framework's head-content mechanism. This is the belt and not the trousers — it
   only reaches crawlers that execute JavaScript.

A crawler-rules file is also shipped in the frontend's static assets even though it is inert
today. It costs nothing and becomes authoritative the day the site moves to a custom domain,
at which point part 2 disappears.

The free question-browsing page stays deferred, so the plan's open question about its
indexability does not arise in v1.0.

**Pros:**
- The API, where the rule is actually enforceable, is fully covered
- The un-implementable part is documented as a manual out-of-repo step instead of silently
  shipping a file that does nothing
- Moving to a custom domain later collapses three parts into one with no code change

**Cons:**
- Frontend crawler control depends on a change in a repository this project does not own, and
  nothing here can verify it stayed in place
- Per-route metadata is unreliable by nature
- An inert file ships in the frontend assets, which will look like a mistake to a future reader —
  hence this ADR

## Remarks / Sources

- Crawler-rules file scope — https://developers.google.com/search/docs/crawling-indexing/robots/robots_txt
  (verified 2026-08-13): must sit in the top-level directory; subdirectory files are not checked;
  rules apply only to the host, protocol and port that served the file
- ADR-006 (pages hosting and the project sub-path), ADR-007 (API host), ADR-015 (no accounts —
  nothing private is exposed either way)
- Verification note: `.pipeline/1-architecture/discovery/research-platform-constraints-2026-08-13.md`
- v1.0 solution design §5.5 for the exact file contents and the manual step
