# ADR-006: Oracle Cloud Always Free — deferred

**Status:** Deferred
**Date:** 2026-08-08

## Problem

Oracle Cloud Always Free offers 4 ARM cores, 24 GB RAM and 200 GB storage, permanently free and never sleeping. Large headroom over ADR-005. Take it now or not?

## Considered

- **Always Free tier** — permanent, separate from the 30-day trial credits. Resources persist after the trial.
- **Inactivity reclaim risk** — CPU below the 20th percentile over a 7-day window makes the VM eligible for reclaim; a fully idle account risks suspension. Mitigation is a synthetic-load cron job.
- **Card requirement** — a physical card is required. Virtual, prepaid and single-use cards are rejected.
- **Physical card with a low internet-payment limit** — pre-authorisation of ~1 PLN passes, larger charges decline. Pre-auth reverses within 14 days.

## Decision

**Deferred.** Current scale does not justify attaching a card plus running keep-alive ops.

## Remarks / Sources

- Revisit if ADR-005 proves insufficient, traffic grows, or a physical card becomes available.
- Always Free resources: https://docs.oracle.com/en-us/iaas/Content/FreeTier/freetier_topic-Always_Free_Resources.htm
- Inactivity reclaim reports: https://lowendtalk.com/discussion/184161/oracle-may-reclaim-your-idle-vps
