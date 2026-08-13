# ADR-002: No backend in Phase 1

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Questions need to be served to WASM client. Options: static file vs API server.

## Considered

- **API server (ASP.NET Core)** — questions in DB, served via REST. Requires hosting, ops, cold starts, card or account setup.
- **Static JSON file** — `questions.json` in `wwwroot/data/`, served from GitHub Pages CDN. No server process.

## Decision

**Static JSON file. No backend server in Phase 1.**

**Pros:**
- Zero hosting cost, zero ops
- GitHub Pages CDN: no cold starts, always-on
- Questions are public olympiad material — no secrecy needed
- Simpler architecture

**Cons:**
- Answers visible in JSON (DevTools) — acceptable for practice tool
- Admin changes require repo push + deploy (minutes, not instant)
- No per-user analytics

## Remarks / Sources

- Seam for backend: `IQuestionRepository` interface (ADR-003) — WASM code unchanged when API added
- Backend added in Phase 2 when accounts or admin panel needed (ADR-015)

## Amendment — 2026-08-12 — v1.0 ships backend; static-JSON delivery superseded

**Overrides:** "No backend server in Phase 1" decision.

v1.0 ships with the ASP.NET Core API (proved in POC per ADR-020). Questions are served server-side with filtering. Static `questions.json` in `wwwroot` is no longer the delivery mechanism. ADR-002 is superseded for question delivery strategy. The `IQuestionRepository` seam (ADR-003) remains; `ApiQuestionRepository` is the live implementation.
