# ADR-036: Release versioning via git tags, auto patch bump

**Status:** Accepted
**Date:** 2026-08-13

## Problem

POC carries a hand-edited `version.json` in the frontend static assets, holding a frontend and
a backend version. Nobody bumps it. v1.0 wants: three-segment versions starting at `1.0.0`, an
automatic patch bump on deploy, an option to name any version explicitly at deploy time, and the
version visible in the deploy run.

Complication: `main` becomes a protected branch that forbids direct pushes (ADR-037). Anything
that writes a version back into the repository has to get past that.

## Considered

- **Keep hand-editing the file** — zero machinery. Nobody does it, so the displayed version
  lies, which is worse than no version.
- **Workflow commits the bumped file back to `main`** — obvious approach, and the file stays the
  single source of truth. Requires granting push-to-protected-branch rights to a manually
  triggered workflow in order to increment an integer. Rejected: the blast radius of that
  permission dwarfs the benefit.
- **Version derived from the commit SHA or the build date** — free, always unique, never
  ambiguous. Meaningless to a human reading a footer, and cannot express intent (was this a
  feature release or a typo fix?). Rejected as the primary scheme; retained as the backend's
  self-reported identity.
- **Version from a git tag, bumped by the deploy workflow** — tags are not blocked by branch
  protection, need only content-write permission, and give an immutable release marker for free.
- **A versioning tool (GitVersion / MinVer)** — richer semantics, pre-release labels, CI
  integration. A dependency and a config file to learn for a two-deployable hobby project.
  Rejected as disproportionate.

## Decision

**Git tags are the source of truth. Two independent version lines, one per deployable.**

- Format `major.minor.patch`. First release `1.0.0`.
- Tag prefixes distinguish the deployables, since the frontend and the API deploy separately and
  on different cadences.
- Each deploy workflow is manually triggered and takes an optional version input:
  - input empty, no prior tag → `1.0.0`
  - input empty, prior tag exists → patch + 1
  - input given → used as-is, after validating the three-segment format and rejecting a version
    whose tag already exists
  - input malformed → the job fails before anything is built
- The tag is pushed **after** a successful deploy, so a failed deploy does not consume a version.
- The frontend build writes both version values into its static version file at publish time.
  The file is generated, never committed.
- The API cannot be versioned by the build: the host builds the container from the repository
  itself in response to a deploy trigger, so the pipeline cannot inject a build argument. The
  API therefore self-reports the **commit** provided by the host's environment through its
  health endpoint, and the tag is the human-facing release label.

### Accepted limitation — the run title cannot show a computed version

The workflow run title is evaluated **before** any job runs, and only the trigger and
repository contexts are available to it; job outputs are not
(verified 2026-08-13 — https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax#run-name).

So "deploy run titles include the version" holds when the user supplies the version, and cannot
hold for an auto-bumped one. Resolution: the title uses the input when present and a stable
placeholder otherwise; the resolved version is written to the run summary — the first thing on
the run page — and becomes the pushed tag. No workaround produces a computed title; claiming
otherwise would be wrong.

### Accepted limitation — the displayed backend version can lag

The frontend's version file records the latest backend tag **at frontend build time**. Deploy
the backend afterwards and the footer under-reports until the next frontend deploy. The
alternative — the frontend querying the API's health endpoint on load — would put a
cold-start-blocking request on the landing page for a cosmetic string. Not worth it. The health
endpoint always reports the truth for anyone who needs it.

**Pros:**
- No write access to the protected branch is ever needed
- Tags are immutable release markers, greppable and comparable, for free
- A failed deploy consumes no version
- Explicit override covers minor and major bumps without a second mechanism
- No new tool, no config file

**Cons:**
- Two version lines to think about instead of one
- The displayed backend version can lag (above)
- The run title cannot carry an auto-bumped version (above)
- Tags require content-write permission on the deploy workflows — a smaller grant than branch
  push, but not nothing
- **No tag protection rule may be added**, or every deploy fails at its last step

## Remarks / Sources

- ADR-024 (frontend deploy is manual only), ADR-037 (branch protection — the constraint that
  rules out commit-back), ADR-007 (the API host builds the image itself)
- Host-provided commit environment variable — https://render.com/docs/environment-variables
  (verified 2026-08-13)
- Run-title contexts — https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax#run-name
  (verified 2026-08-13)
- v1.0 solution design §7.2 for the exact workflow steps and the resolution rules
- Revisit if pre-release or preview channels are ever wanted — that is the point where a
  versioning tool starts paying for itself
