# ADR-027: Protected main branch with a required CI check

**Status:** Accepted
**Date:** 2026-08-13

## Problem

Commits went straight to the main branch. CI existed but could block nothing, so a red build reached main more than once and was fixed forward. Wanted: pull request required, CI required, no force pushes.

Complication to solve first: the repository has exactly one human. A required-approval rule and a single maintainer do not obviously coexist — a pull request author cannot approve their own pull request, so with one human nothing could ever merge.

## Considered

- **No protection, discipline only** — status quo. Discipline already failed; that is the evidence base.
- **Legacy branch protection** — familiar. No bypass list, so the single-maintainer approval problem has no clean answer there. Rejected.
- **Required approvals 1, no bypass** — closest to the stated intent. Deadlocks the repository. Not viable.
- **Required approvals 0, CI check only** — honest for a one-person repo. Loses the review requirement the moment a collaborator appears.
- **Required approvals 1 with the owner on a bypass list** — a future collaborator's pull request needs review; the owner can merge their own. Review discipline becomes social rather than enforced, for the owner only.

## Decision

**A branch ruleset targeting the main branch, active, with:**

- pull request required before merging
- required approvals **1**, with the repository owner on the ruleset **bypass list**
- stale approvals dismissed when new commits are pushed
- required status check: the CI build-and-test job, with "require branches to be up to date" on
- force pushes blocked
- branch deletion restricted

The bypass entry exists because of the self-approval constraint above. It is recorded here so the first person to wonder why an owner can merge unreviewed finds the answer instead of "fixing" it and deadlocking the repository.

**The required check is matched by job name.** Renaming the CI job silently stops the rule matching, and the failure mode is a pull request that *appears* to satisfy protection. Renaming that job is a protection change and must be treated as one. This is also why the solution file is not renamed (ADR-023).

**No ruleset targets tags.** Deploy workflows push release tags (ADR-026); a tag ruleset without a bypass for the Actions identity would fail every deploy at its last step.

Decided together because it is the same "make the process visible" change: a pull request template carrying the local build/test/ADR checklist, plus bug and feature issue templates. Plain markdown, not issue forms — forms earn their keep on high-volume public repos.

Accepted cons:

- The owner's own changes are effectively unreviewed. The bypass makes that explicit rather than accidental.
- Every change needs a pull request, which is friction on a one-line docs fix.
- The required check depends on the self-hosted runner being online. When that machine is off, nothing merges except through the bypass.
- Ruleset configuration lives in the platform UI, not in the repository, so it is not reviewable in a diff and must be documented instead.

## Remarks / Sources

- Self-approval rule: https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/approving-a-pull-request-with-required-reviews
- ADR-026 (tag pushes — why no tag ruleset), ADR-017 (runner availability), ADR-015 (deploys stay manual and are unaffected)
- `docs/integrations/github-actions.md`
- The approval count remains a preference, not an architectural property. Zero required approvals is available and changes nothing else here.
