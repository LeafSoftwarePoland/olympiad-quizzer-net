# ADR-026: Release versioning — one version in the repository, auto patch bump

**Status:** Accepted
**Date:** 2026-08-13

**Title and filename corrected 2026-08-16.** Both read "via git tags", which the amendment at the
foot of this file reverses. A heading that contradicts its own decision misleads before anyone
reaches the text that corrects it. The Problem, Considered and Decision sections below are
unedited and describe the **superseded** scheme — read the amendment for what holds today.

## Problem

The version was a hand-edited file in the frontend's static assets. Nobody bumped it, so the displayed version lied. Wanted: three-segment versions from `1.0.0`, automatic patch bump on deploy, an option to name a version explicitly, and the version visible on the deploy run.

Complication: the main branch forbids direct pushes (ADR-027), so anything that writes a version back into the repository must get past branch protection.

## Considered

- **Keep hand-editing the file** — zero machinery. Nobody does it, and a lying version is worse than none.
- **Workflow commits the bumped file back to the main branch** — the file stays the single source of truth. Requires granting push-to-protected-branch rights to a manually triggered workflow in order to increment an integer. The blast radius of that permission dwarfs the benefit. Rejected.
- **Version derived from commit SHA or build date** — free and always unique. Meaningless to a human reading a footer, and cannot express intent. Rejected as the primary scheme, retained as the backend's self-reported identity.
- **Version from a git tag, bumped by the deploy workflow** — tags are not blocked by branch protection and need only content-write permission, and a tag is an immutable release marker for free.
- **A versioning tool** — richer semantics, pre-release labels. A dependency and a config file for a two-deployable hobby project. Disproportionate.

## Decision

**Git tags are the source of truth. Two independent version lines, one per deployable, distinguished by tag prefix.**

- Format `major.minor.patch`. First release `1.0.0`.
- Each deploy workflow is manually triggered (ADR-015) and takes an optional version input:
  - empty input, no prior tag → `1.0.0`
  - empty input, prior tag exists → patch + 1
  - input given → used as-is, after validating the three-segment format and rejecting a version whose tag already exists
  - malformed input → the job fails before anything is built
- The tag is pushed **after** a successful deploy, so a failed deploy consumes no version.
- The frontend build writes both version values into a static version file at publish time. That file is generated, never committed.
- The API cannot be versioned by the pipeline: the host builds the container from the repository itself in response to a trigger, so no build argument can be injected (ADR-005). The API therefore self-reports the **commit** supplied by the host's environment through its health endpoint, and the tag is the human-facing release label.

Accepted cons:

- Two version lines to reason about instead of one.
- **The run title cannot carry an auto-bumped version.** The title is evaluated before any job runs, so job outputs are unavailable to it. The title uses the input when present and a stable placeholder otherwise; the resolved version goes to the run summary and becomes the pushed tag. No workaround produces a computed title.
- **The displayed backend version can lag.** The frontend's version file records the latest backend tag at frontend build time; deploy the backend afterwards and the footer under-reports until the next frontend deploy. The alternative — querying the health endpoint on load — puts a cold-start-blocking request on the landing page for a cosmetic string. The health endpoint always reports the truth for anyone who needs it.
- Deploy workflows need content-write permission to push tags. A smaller grant than branch push, but not nothing.
- **No tag protection rule may be added**, or every deploy fails at its last step.

## Remarks / Sources

- ADR-027 (branch protection — the constraint that rules out commit-back, and why no tag ruleset exists), ADR-015 (deploys are manual), ADR-005 (the host builds its own image)
- Run-title contexts: https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax#run-name
- Revisit if pre-release or preview channels are ever wanted — that is where a versioning tool starts paying for itself.

## Amendment — 2026-08-16 — the version moves into the repository; tags become release markers

**Overrides:** "Git tags are the source of truth"; the two-version-line model; the per-workflow
version input and its auto-bump; the con "the API cannot be versioned by the pipeline"; the con
"the displayed backend version can lag".
**Adds:** `Directory.Build.props` as the version's home; automatic patch bump when a pull request
opens.

**Trigger.** Two defects in the tag scheme, both found in use. A missing generated version file
made the frontend display nothing and left a fresh clone unable to say what it was — and because
the next version was computed from the newest tag, a repository with no tags would resolve `1.0.0`
again rather than failing. Nothing in the working tree stated the version, so nothing could be
reviewed in a pull request.

**One version for the whole solution, in `Directory.Build.props` at the repository root.** MSBuild
reads that file automatically and every project inherits it, so the number is compiled into each
assembly rather than applied from outside at deploy time.

Two versions were considered and rejected. Separate lines per deployable earn their keep when the
deployables live in separate repositories and move independently; here one commit produces both,
so two numbers describing one tree is bookkeeping without a reader.

The solution file cannot hold this. `.slnx` lists projects and configurations and carries no
MSBuild properties, so a version written there would never reach a build.

**The patch bumps when a pull request opens**, and only when the branch's version still equals the
base branch's. A version that already differs was set deliberately and is left alone — that is how
a minor or major is taken: edit the file, and the automation stays out of the way.

The bump is a commit pushed to the pull request branch, so **a developer must pull before
continuing work on that branch**. The workflow listens to `opened` only; the push it makes raises
`synchronize`, which nothing listens to, so it cannot retrigger itself.

**Consequences that resolve earlier cons:**

- The API is now versioned like everything else. It no longer needs a build argument the host
  cannot supply, because the version is already inside the assembly it builds.
- The displayed version cannot lag behind a deploy — the client reads its own assembly rather than
  a file written by a workflow, so it is correct in local development too.
- The generated version file is gone, and with it the class of failure where its absence showed a
  blank version or silently reset the sequence.

**Tags survive as release markers, not as truth.** Each deploy still tags what it shipped, prefixed
per deployable, so "which version is live on each side" stays answerable. A tag that already exists
is now skipped rather than failing the run, because redeploying an unchanged commit is legitimate
once the version no longer comes from the tag.

Accepted cons:

- A bot commit lands on the branch, so a stale local checkout is a pull away from a conflict on one
  line of one file.
- The run name still cannot carry the version — it is evaluated before any job runs and cannot read
  a file. The resolved version goes to the run summary, as before.
- Frontend and backend can be deployed from different commits and therefore report different
  versions despite sharing one line. The tags record which is which.
