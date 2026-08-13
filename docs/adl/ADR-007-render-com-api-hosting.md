# ADR-007: Render.com for Phase 2 API hosting

**Status:** Accepted (test pending)
**Date:** 2026-08-08

## Problem

When backend API added (Phase 2), need hosting for ASP.NET Core Docker container. Constraint: no credit/debit card required.

## Considered

| Platform | Card? | RAM | Sleep? | Notes |
|---|---|---|---|---|
| **Render.com** | No | 512 MB | 15 min idle | Stops on limit, no auto-charge |
| Northflank | No | 256 MB | Never | Tight for .NET |
| Back4App Containers | No | 256 MB | Unclear | 600 active hours/month |
| Railway | No | any | No | 30-day trial only, then $1/mo |
| Fly.io | **Yes** | — | — | Eliminated |
| Oracle Cloud Always Free | Physical card | 24 GB ARM | Never | Deferred (ADR-008) |
| Azure App Service F1 | **Yes** | — | — | No Docker; eliminated |

Cyclic.sh: shut down May 2024. Adaptable.io: free tier ended Sep 2024. Koyeb: closed to new users Feb 2026.

## Decision

**Render.com + UptimeRobot (free) ping every 5 min to prevent sleep.**

**Pros:**
- No card
- 512 MB — sufficient for lean ASP.NET Core + Dapper
- Stops on limit — never auto-charges
- Docker-native

**Cons:**
- Sleeps after 15 min idle without UptimeRobot
- Cold start ~30–60s if not kept alive
- 750 instance-hours/month (~31 days at 24/7)
- No persistent disk on free tier

**Must test before relying on it.** Free tier availability can change.

## Remarks / Sources

- UptimeRobot: free, 50 monitors, 5-min interval
- Render free PostgreSQL expires after 30 days — use SQLite volume or paid DB
- Re-evaluate Oracle Cloud (ADR-008) if 512 MB becomes insufficient

## Amendment — 2026-08-09 — POC confirmed, M11 resolved

**Overrides:** Status → Accepted (POC confirmed).  
**Resolves:** M11 open item from assumptions.md.

Live URL: `https://olympiad-quizzer-net-api.onrender.com`

**M11 result:** No card required. No verification hold. Free tier activated without payment details.  
**Cold start:** ~35s measured. UptimeRobot 5-min ping keeps warm.  
**Deploy hook + healthz poll:** worked first try. `deploy-backend.yml` waits 30s then polls `/healthz` for 5 min.
