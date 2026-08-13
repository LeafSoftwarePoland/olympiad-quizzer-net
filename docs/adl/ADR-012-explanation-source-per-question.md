# ADR-012: source_urls + explanation per question

**Status:** Accepted
**Date:** 2026-08-08
**Updated:** 2026-08-08

## Problem

After answering, student needs context: why is this correct, where to learn more.

## Considered

- **No explanation** — minimal build cost. Student must look up answers independently.
- **Inline explanation** — `explanation: ContentBlock[]`. Shown as expandable card after grading. Supports text + code + image blocks (same format as question content).
- **Separate topic/lesson pages** — Brilliant-like. Topic entities, questions link to topics, full explanatory pages. High content + engineering cost.
- **source_urls only** — links to official PDFs, no inline text.
- **Single source_url** — limits to one reference link per question.

## Decision

**Both `source_urls: string[]` and `explanation: ContentBlock[] | null` on question record.**

- `explanation` — structured content blocks (text/code/image), optional, manually authored. Same block format as question `content` — renderer reuses same component.
- `source_urls` — **array** of links (question may reference both question PDF and answer key PDF). Links to official VEA PDFs, OIJ archive, ZPE articles, etc.

**Pros:**
- Near-zero engineering cost (fields in schema, ADR-011)
- Student sees context immediately after answering
- Explanation supports code snippets and images (same renderer as questions)
- Multiple source links — e.g., question PDF + answer key PDF as separate entries

**Cons:**
- Explanation must be manually authored per question — content bottleneck
- Not as deep as topic pages — no cross-question concepts

## Remarks / Sources

- Brilliant-like topic pages: deferred — separate sub-project
- Official sources: kuratorium PDFs, oij.edu.pl archive, zpe.gov.pl, dyzurnet.pl
- Recommended sources per voivodeship scope docs: `research-synthesis.md` — Addendum B

## Override history

| Date | What changed | Why |
|---|---|---|
| 2026-08-08 | `source_url: string` → `source_urls: string[]`; `explanation: markdown string` → `ContentBlock[]` | Multiple reference links needed per question; explanation block format matches content renderer |
