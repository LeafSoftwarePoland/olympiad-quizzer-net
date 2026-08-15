# Functionality Registry

One entry per user-facing feature. Add new entries here when planning, not after shipping.

## Schema

| Field | Values | Meaning |
|---|---|---|
| ID | `F-NN` | Stable identifier. Never reuse. |
| Status | `active` / `planned` / `obsolete` | `active` = shipped and live; `planned` = decided, not yet built; `obsolete` = removed |
| Superseded by | pointer | Only for `obsolete` entries |
| ADR | link | Primary ADR governing this feature |

---

## Registry

### F-01 — Multiple question types

**Status:** active  
**ADR:** ADR-007, ADR-024

Types in use: `single`, `multi`, `shortAnswer`. Types stubbed for future VEA content: `trueFalse`, `ordering`, `matching`. UI does not need to render the latter three yet.

---

### F-02 — Exam simulation mode with timer

**Status:** active  
**ADR:** ADR-016

Timed quiz. OIJ default: 30 questions, 90 minutes. Timer is wall-clock strict — refresh does not reset it. State persisted in `localStorage`. See ADR-016.

---

### F-03 — Free-learning mode (no timer)

**Status:** active  
**ADR:** ADR-016

Quiz mode with the timer checkbox unchecked. No countdown — questions are answered at the student's own pace. Session ends on explicit navigation away or summary page.

---

### F-04 — Server-side question filtering by category, algorithm, year, stage

**Status:** active  
**ADR:** ADR-025, ADR-013

Client sends filter parameters + requested count. API returns a random subset. Client never downloads the full bank.

---

### F-05 — Session persistence via localStorage (quiz state, timer)

**Status:** active  
**ADR:** ADR-016

Full quiz payload cached in `localStorage` for the active session. Navigate away and return = resume. Session ends on timeout or explicit cancel. Cache cleared on leaving summary.

---

### F-06 — Responsive design (phone/tablet/desktop)

**Status:** active  
**ADR:** ADR-010

Custom CSS, no framework (ADR-014). Breakpoints hand-rolled in `app.css`.

---

### F-07 — Dark/light theme + font size settings (localStorage)

**Status:** active  
**ADR:** ADR-016 (localStorage), ADR-014 (custom CSS custom properties)

Theme toggle and font size preference stored in `localStorage`. Survives browser restart.

---

### F-08 — WCAG AA accessibility (ARIA, focus-visible, contrast)

**Status:** active  
**ADR:** ADR-010

ARIA labels, focus-visible, sufficient contrast. Built upfront, not retrofitted.

---

### F-09 — Explanation per question (with explanationSource distinction)

**Status:** active  
**ADR:** ADR-007

Each question carries an `explanation: [ContentBlock]` and `explanationSource: string`. Values in use: `"AI generated"`, `"official"`, `"documentation"`, `"community"`. UI shows source label.

---

### F-10 — OIJ mode (30 questions, 90 minutes, stage rules)

**Status:** active  
**ADR:** ADR-025, docs/rules/oij.md

Rules defined in `docs/rules/oij.md`. Client reads the machine-readable rule block via `ModeCatalog` to pre-fill filters and time limit. Default: 30 questions, 90 minutes, E1 stage pre-selected.

---

### F-11 — Question browsing without quiz (free browse mode)

**Status:** planned, deferred  
**ADR:** none yet

Browse full question bank by category/tag without starting a timed session.

---

### F-12 — Code-execution questions via Piston

**Status:** planned, far future  
**ADR:** none yet

Open-answer questions where the student writes code that is executed against test cases. Depends on Piston API integration. Not a live dependency.

---

### F-13 — PWA / offline mode

**Status:** planned, deferred  
**ADR:** ADR-011 (deferred)

Server-side filtering makes offline mode significantly more complex. Deferred until filtering is stable. See ADR-011.

---

### F-14 — Graceful failure handling (toasts, error screens, coded messages)

**Status:** active  
**ADR:** ADR-031, ADR-012, ADR-013, ADR-025

A child who hits a failure gets a comprehensible Polish message and an obvious next step — never a blank screen, a raw error, or a spinner that never resolves. The purpose is not robustness for its own sake: a confused or frustrated student stops practising, and this app has no teacher standing next to it to explain what went wrong.

Covers:

- **Toasts** for recoverable problems the student can act on or ignore.
- **Server-unreachable screen** when the API is down or still waking (ADR-013, ADR-019 — cold start reaches ~35 s, so "slow" must not read as "broken").
- **Empty result** handled as a normal outcome, not an error — "no questions for these filters", and the quiz refuses to start rather than starting empty (ADR-025).
- **Coded errors mapped to Polish client-side.** The API returns a stable machine code, the client owns every string a student reads (ADR-012 amendment), with a generic fallback for an unrecognised code. Today one code exists, `UNEXPECTED`, and the fallback shares its text:

  > **Coś poszło nie tak.**
  > Spróbuj ponownie wykonać ostatnią akcję. Jeżeli błąd nadal występuje, skontaktuj się proszę z twórcą aplikacji wraz z zrzutami ekranu.

  Written for a ten-to-fourteen-year-old: plain register, and a next step rather than a dead end. A code is added only when a real failure needs its own message — see ADR-031.
- **Nothing technical reaches the screen.** No exception type, message, stack or path (ADR-031).

Toasts, the unreachable screen and empty-result handling ship today. The middleware-backed coded contract lands with ADR-031.

---

## Changelog

| Date | Change |
|---|---|
| 2026-08-12 | Initial registry — F-01..F-13 seeded from plan and ADLs |
| 2026-08-13 | F-03, F-10 marked active — delivered by v1.0 |
| 2026-08-15 | ADR pointers remapped after the ADL renumber; F-04 and F-10 corrected to cite the filtering-contract ADR |
| 2026-08-15 | F-14 added — graceful failure handling. Registers toast, unreachable-screen and empty-result behaviour that already shipped but was never recorded, plus the coded error contract from ADR-031 |
