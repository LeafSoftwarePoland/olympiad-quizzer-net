# ADR-008: Oracle Cloud Always Free — deferred

**Status:** Deferred
**Date:** 2026-08-08

## Problem

Oracle Cloud Always Free offers 4 ARM A1 cores + 24 GB RAM + 200 GB storage — permanently free after 30-day/$300 trial ends. Significant headroom vs Render.com (ADR-007).

## Considered

- **Always Free tier** — permanent (not 30-day). Trial credits are separate. Resources persist indefinitely after trial.
- **Inactivity risk** — CPU below 20th percentile for 7-day window → VM eligible for reclaim. Account fully idle 30+ days → suspension risk. Workaround: cron job with synthetic load.
- **Card requirement** — physical debit/credit card required. Oracle explicitly rejects virtual, prepaid, single-use cards. mBank eKarta wirtualna = rejected.
- **mBank physical card strategy** — set internet payment limit to 1–2 PLN: pre-auth (~1 PLN) passes, any charge above limit declined. Physical card pre-auth reverses within 14 days.

## Decision

**Deferred.** Current scale doesn't justify card attachment + keep-alive ops overhead.

## Remarks / Sources

- Revisit if: Render.com proves insufficient, traffic grows, physical card available
- Official docs: https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier.htm
- Always Free resources: https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm
- Inactivity reclaim reports: https://lowendtalk.com/discussion/184161/oracle-may-reclaim-your-idle-vps
- mBank internet limits: https://www.mbank.pl/indywidualny/karty/pytania-i-odpowiedzi/limity-autoryzacyjne-dla-platnosci/
