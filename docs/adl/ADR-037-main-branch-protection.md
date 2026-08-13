# ADR-037: Protected `main` with a required CI check

**Status:** Accepted
**Date:** 2026-08-13

## Problem

POC pushed straight to `main`. CI existed but could not block anything, so a red build reached
`main` more than once and was fixed forward. v1.0 wants `main` protected: pull request required,
CI required, no force pushes.

Complication that has to be solved before switching it on: the repo has exactly one human. A
required-approval rule and a single maintainer do not obviously coexist.

## Considered

- **No protection, discipline only** — status quo. Discipline failed during the POC; that is the
  evidence base.
- **Legacy branch protection screen** — familiar. No bypass list, so the single-maintainer
  approval problem has no clean answer there. Rejected.
- **Branch ruleset** — same rules plus a bypass list, and rulesets are the direction the platform
  is moving.
- **Required approvals = 1, no bypass** — closest to the stated intent. Pull request authors
  cannot approve their own pull requests
  (verified 2026-08-13 — https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/approving-a-pull-request-with-required-reviews),
  so with one human **nothing could ever merge**. Not viable as written.
- **Required approvals = 0, CI check only** — honest for a one-person repo. Loses the review
  requirement the moment a collaborator appears.
- **Required approvals = 1 with the owner on the bypass list** — a future collaborator's pull
  request needs a review; the owner can merge their own. The review discipline becomes social
  rather than enforced, for the owner only.

## Decision

**A branch ruleset targeting `main`, active, with:**

- pull request required before merging
- required approvals: **1**, with the repository owner on the ruleset **bypass list**
- stale approvals dismissed when new commits are pushed
- required status check: the CI build-and-test job, sourced from the Actions provider, with
  "require branches to be up to date" on
- force pushes blocked
- branch deletion restricted

The self-approval constraint above is why the bypass entry exists. It is recorded here so the
first person to wonder why an owner can merge unreviewed finds the answer instead of "fixing" it
and deadlocking the repo. The alternative (zero required approvals) remains available and is a
preference, not an architectural change.

**The required check is matched by job name.** Renaming the CI job silently stops the rule
matching, and the failure mode is a pull request that appears to satisfy protection. Renaming
that job is a protection change and must be treated as one.

**No ruleset targeting tags.** The deploy workflows push release tags (ADR-036); a tag ruleset
without a bypass entry for the Actions identity would fail every deploy at its last step.

Supporting templates, decided together because they are the same "make the process visible"
change: a pull request template (what changed, what it relates to, the local build/test/ADR
checklist) and two issue templates (bug, feature request). Plain markdown, not issue forms —
forms earn their keep on high-volume public repos, and this repo has one maintainer.

**Pros:**
- A red build cannot reach `main`
- History stays linear and unrewritten
- The pull request becomes the place where the ADR-hygiene checklist is actually read
- Ready for a collaborator without a further configuration change

**Cons:**
- The owner's own changes are effectively unreviewed — the bypass makes that explicit rather
  than accidental
- Every change needs a pull request, which is friction on a one-line docs fix
- The required check depends on the self-hosted runner being online; when the machine is off,
  nothing can merge except through the bypass (`docs/integrations/github-actions.md`, ADR-026)
- Configuration lives in the platform UI, not in the repository, so it is not reviewable in a
  diff and has to be documented instead

## Remarks / Sources

- Self-approval rule — https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/approving-a-pull-request-with-required-reviews
  (verified 2026-08-13)
- ADR-036 (tag pushes — why no tag ruleset), ADR-026 (self-hosted runner availability),
  ADR-024 (deploys stay manual and are unaffected)
- v1.0 solution design §7.4 for the click-by-click configuration and §7.3 for the templates
- Open user preference on the approval count recorded in the pipeline's decision-needed note;
  the recommended option above is what gets configured absent a different instruction
