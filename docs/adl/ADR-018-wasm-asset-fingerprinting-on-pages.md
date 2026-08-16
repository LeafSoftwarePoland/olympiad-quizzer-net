# ADR-018: WASM asset fingerprinting on static hosting

**Status:** Accepted
**Date:** 2026-08-14

## Problem

.NET 10 fingerprints published static assets by default and rewrites the entry document's asset references to the fingerprinted names. On a static host with no content negotiation, two things break: the runtime files 404 if the document's placeholders are not rewritten, and the compressed variants are requested but never served. Result is a blank page with no error a user can act on. Reported defect: https://github.com/dotnet/aspnetcore/issues/64359

## Considered

- **Publish defaults unchanged** — nothing to maintain. Blank page on the deployed site. Rejected by evidence.
- **Disable fingerprinting entirely** — removes the failure. Also removes content-hash cache busting, so a released asset change can serve stale from CDN cache indefinitely.
- **Keep fingerprinting, force placeholder rewriting, disable compression** — keeps cache busting, matches what a no-negotiation static host can actually serve.

## Decision

**Keep fingerprinting. Force the entry document's asset placeholders to be rewritten, and disable compression at publish.**

Both flags are load-bearing and set in the frontend project. Neither is a preference:

- Without placeholder rewriting the document points at unfingerprinted names that do not exist on disk.
- With compression enabled the host is asked for encodings it will not negotiate.

Accepted cons:

- Larger transfer than a compressed publish. The host applies its own compression on the wire, so the loss is smaller than the file sizes suggest.
- Two build flags whose reason is not evident from their names, so both carry a comment against a well-meaning cleanup.
- The workaround is pinned to a framework defect; it must be re-tested when the framework is upgraded.

## Remarks / Sources

- Upstream defect: https://github.com/dotnet/aspnetcore/issues/64359
- ADR-004 (the host does no content negotiation and serves from a sub-path)
- Re-test on every framework major upgrade. If the defect is fixed, compression can be re-enabled — verify on the deployed site, not locally, because local hosting negotiates content and hides the failure.
