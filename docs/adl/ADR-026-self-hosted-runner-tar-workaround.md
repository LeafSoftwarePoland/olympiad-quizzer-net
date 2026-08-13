# ADR-026: Self-hosted Windows runner — tar/bsdtar PATH workaround

**Status:** Shell
**Date:** 2026-08-12
**See also:** `.pipeline/1-architecture/assumptions.md` — A-03

## Problem

`actions/upload-pages-artifact` calls `tar --hard-dereference`. On Windows, Git Bash resolves `tar` to `bsdtar` (from `C:\Program Files\Git\usr\bin`), which does not support `--hard-dereference`. Known open issue: https://github.com/actions/upload-pages-artifact/issues/95. Workaround: place `C:\Program Files\Git\bin` above `C:\Program Files\Git\usr\bin` in the system PATH so the GNU tar is found first. The workaround may not persist across runner reboots.

Decision needed: is the PATH fix sufficient and stable, or should the upload job permanently run on `ubuntu-latest` (public repo = free minutes)?

## Considered

_To be discussed._

## Decision

_Not decided yet._

## Remarks / Sources

_None yet._
