# ADR-017: Runner allocation — hosted for the Pages path, overall strategy open

**Status:** Accepted
**Date:** 2026-08-14

## Problem

The intent is for the self-hosted machine to run every workflow: it is already paid for, its SDK and package caches are warm, and hosted minutes are free only while they last.

One workflow cannot run there. The Pages artifact-upload action invokes `tar --hard-dereference`. On the self-hosted Windows runner the shell resolves `tar` to the BSD build shipped with Git for Windows, which does not support that flag, so the upload step fails. Known upstream issue: https://github.com/actions/upload-pages-artifact/issues/95

## Considered

- **Reorder the system PATH** so the GNU tar in Git's `bin` directory is found before the BSD tar in `usr/bin`. Fixes it in place and keeps every job self-hosted. The fix lives in machine state, not in the repository: invisible in a diff, unverifiable in review, and liable to vanish on a runner reboot or a Git for Windows update.
- **Vendor a GNU tar and prepend it per job** — repository-controlled and diffable. Ships a binary and a PATH mutation for one action's benefit.
- **Run the frontend deploy on hosted Linux runners** — the tool is correct there by default, no machine state involved. Spends hosted minutes and forfeits the warm local caches.

## Decision

**The frontend deploy workflow runs on hosted Linux runners. The self-hosted Windows runner keeps the CI build-and-test job and the backend health-poll job, neither of which touches `tar`.**

The PATH workaround is rejected. A fix that lives in undiffable machine state and can disappear on reboot is worse than a runner label, because its failure mode is a workflow that worked yesterday and does not today for no reason visible in the repository.

Accepted cons:

- The frontend build forfeits the self-hosted machine's warm SDK and package caches.
- Two runner classes in one repository, so a workflow author must know which label a job needs.
- Hosted minutes are consumed, and are free only while the repository stays public.

## Remarks / Sources

- **Open, and deliberately not decided here: the overall runner strategy.** The target remains one machine running everything, but that requires the local machine to handle the full toolchain — Docker and WSL included — which it does not today. The decision to make is whether hosted minutes prove sufficient for this project's volume, or whether the local machine gets fixed. Until then the split above stands. This is a provisional allocation, not the end state, and it is not tech debt either: the PATH workaround is rejected on its own merits and stays rejected regardless of how the strategy question resolves.
- The upstream issue is open. If it is fixed, the narrow decision above still holds on its own reasoning; only the strategy question is affected.
- ADR-027 (the required CI check runs on the self-hosted runner, so nothing merges while that machine is off)
- `docs/integrations/github-actions.md`
