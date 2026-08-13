# ADR-004: Dapper over EF Core

**Status:** Accepted
**Date:** 2026-08-08

## Problem

When backend DB added in Phase 2, need data access layer. EF Core (full ORM) vs Dapper (micro-ORM).

## Considered

- **EF Core** — generates SQL, tracks entities, runs migrations. ~20–50 MB extra RAM overhead. Migrations toolchain is convenient.
- **Dapper** — write SQL manually, Dapper maps results to C# objects. ~2 MB RAM. Parameterized by default (injection-safe when using `@param` syntax — never string-concatenate into queries).
- **Hybrid** — EF Core for `dotnet ef migrations` only, Dapper for runtime queries.

## Decision

**Dapper for all runtime queries. DbUp (or manual SQL scripts) for migrations. No EF Core runtime dependency.**

**Pros:**
- Lean RAM — relevant for Render.com free tier (512 MB shared)
- Full SQL control — no surprise N+1 or over-fetching
- Parameterized by default — injection-safe
- Queries readable and debuggable

**Cons:**
- Manual migration scripts — no auto-generated schema diff
- Boilerplate mapping for complex queries
- Requires SQL knowledge

## Remarks / Sources

- Phase 1 has no backend — applies from Phase 2 onward
- Schema managed from VS Code + SQLite extension in Phase 1
- DbUp: lightweight migration runner, numbered `.sql` files applied in order
- Re-evaluate if team grows and SQL expertise becomes bottleneck
- Injection safety: always `@param`, never string interpolation in SQL
