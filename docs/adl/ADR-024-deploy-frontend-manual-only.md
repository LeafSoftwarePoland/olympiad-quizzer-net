# ADR-024: deploy-frontend is manual-only (workflow_dispatch)

**Status:** Accepted
**Date:** 2026-08-09

## Problem

`deploy-frontend.yml` originally triggered on push to `main` (`source/client/**`). Caused accidental deploys during iterative polishing — user wants explicit control.

## Decision

Remove push trigger. `workflow_dispatch` only. Both `deploy-frontend` and `deploy-backend` are now fully manual.

**To deploy frontend:** GitHub Actions → deploy-frontend → Run workflow → main.

## Rule

No automatic frontend deploys. Push to `main` is not a deploy signal. Deploys are intentional, manual acts.
