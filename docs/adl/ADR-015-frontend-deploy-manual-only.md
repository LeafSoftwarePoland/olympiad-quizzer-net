# ADR-015: Frontend deploy is manual only

**Status:** Accepted
**Date:** 2026-08-09

## Problem

The frontend deploy workflow triggered on push to the main branch. Iterative UI polishing therefore deployed repeatedly and unintentionally.

## Considered

- **Keep the push trigger** — deploys are always current. Every commit is a deploy, including half-finished ones.
- **Push trigger with a path filter** — narrower. Still fires on any frontend commit, which is exactly what the polishing case does.
- **Manual trigger only** — deploys become deliberate acts. Costs one click and the risk of forgetting to deploy.

## Decision

**Manual trigger only, for both frontend and backend deploys.** A push to the main branch is not a deploy signal.

Accepted cons:

- A merged change is not live until someone clicks. Forgetting is possible.
- No continuous deployment.

## Remarks / Sources

- Deploy path: repository Actions tab → the deploy workflow → run it against the main branch.
- ADR-026 (the deploy run resolves and pushes the release tag), ADR-027 (deploys are unaffected by branch protection)
