# ADR-005: Render.com for API hosting

**Status:** Accepted
**Date:** 2026-08-08

## Problem

The API ships as a container. Need a host. Hard constraint: no credit or debit card.

## Considered

| Platform | Card | RAM | Sleeps | Verdict |
|---|---|---|---|---|
| **Render.com** | no | 512 MB | 15 min idle | Chosen. Stops at limit, never auto-charges. |
| Northflank | no | 256 MB | never | Tight for .NET. |
| Back4App Containers | no | 256 MB | unclear | 600 active hours/month. |
| Railway | no | any | no | 30-day trial, then paid. |
| Fly.io | **yes** | — | — | Eliminated. |
| Oracle Cloud Always Free | physical card | 24 GB ARM | never | Deferred (ADR-006). |
| Azure App Service F1 | **yes** | — | — | No Docker. Eliminated. |

Cyclic.sh shut down May 2024. Adaptable.io free tier ended Sep 2024. Koyeb closed to new users Feb 2026.

## Decision

**Render.com free plan, kept warm by a 5-minute external ping.** No card, Docker-native, 512 MB is enough for a lean ASP.NET Core process, and the plan stops rather than billing.

Verified in practice: no card and no verification hold. Cold start ~35 s measured. Deploy hook plus health-endpoint poll worked first try.

Accepted cons:

- Sleeps after 15 min idle. External ping reduces but does not eliminate cold starts (overnight, weekends) — hence the client timeout (ADR-019).
- 750 instance-hours/month, roughly one instance running continuously.
- **No persistent disk.** This is the constraint that forces the question bank into the image (ADR-029).
- Free plan terms can change; the fallback is ADR-006.

## Remarks / Sources

- Live URL: `https://olympiad-quizzer-net-api.onrender.com`
- Free plan limits: https://render.com/docs/free
- The host builds the image from the repository itself in response to a deploy trigger, so the pipeline cannot inject build arguments. Consequence for versioning: ADR-026.
- Re-evaluate ADR-006 if 512 MB becomes insufficient.
