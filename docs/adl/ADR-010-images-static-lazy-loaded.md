# ADR-010: Images as static lazy-loaded files

**Status:** Accepted
**Date:** 2026-08-08

## Problem

Quiz questions contain images (flowcharts, code diagrams, grid problems). How to serve them efficiently.

## Considered

- **Inline base64 in JSON** — no separate requests. Bloats JSON massively; unacceptable.
- **Static files in `wwwroot/images/`** — served from GitHub Pages CDN, loaded on demand per question.
- **External CDN (Cloudflare R2)** — off-repo storage. Needed only if repo size grows.

## Decision

**Static files in `wwwroot/images/<source>/`, referenced by relative path in question JSON. Lazy-loaded — fetched only when question is displayed.**

```json
{ "id": "oij_xx_q9", "image": "images/oij/q009_stars.png" }
```

Filename convention: `<source>_<year>_s<stage>_q<num>.<ext>`

**Pros:**
- No bloat in questions.json
- Browser fetches only images for visible questions
- GitHub Pages CDN caches images with ETag
- Direct continuity from py-oij-quizzer `static/images/` pattern

**Cons:**
- Repo size grows with images — GitHub Pages 1 GB soft limit
- Images committed to repo alongside question JSON

## Remarks / Sources

- Precedent: `c:\Repositories\py-oij-quizzer\static\images\`
- Future: Cloudflare R2 if repo approaches 1 GB (10 GB free, zero egress cost, no card required)
