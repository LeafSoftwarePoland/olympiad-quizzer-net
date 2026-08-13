# ADR-027: .NET 10 WASM asset fingerprinting on GitHub Pages

**Status:** Shell
**Date:** 2026-08-12
**See also:** `.pipeline/1-architecture/assumptions.md` — A-05

## Problem

.NET 10 fingerprints static assets by default. A reported standalone-WASM defect (https://github.com/dotnet/aspnetcore/issues/64359) may cause `_framework` runtime files to 404 on GitHub Pages, producing a blank page. GitHub Pages does no content negotiation. Current mitigation: `<CompressionEnabled>false</CompressionEnabled>` in the client project. If fingerprinting causes 404s, the fallback is toggling `OverrideHtmlAssetPlaceholders`.

Decision needed: is compression-disabled publish sufficient, or do we need additional build flags to work around the fingerprinting defect?

## Considered

_To be discussed._

## Decision

_Not decided yet._

## Remarks / Sources

_None yet._
