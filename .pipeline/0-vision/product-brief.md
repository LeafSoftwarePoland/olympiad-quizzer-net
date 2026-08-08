# Product Brief — olympiad-quizzer-net

**PM note**: Written by orchestrator from session context. Interactive PM session skipped — design was fully established in brainstorming session before pipeline adoption.

## Product

Quiz training app for Polish primary school students (grades 4-8) preparing for IT olympiads and voivodeship subject competitions.

## Target audience

- **Primary users**: students grades 4-8 preparing for OIJ (national algorithmics olympiad) or VEA regional IT competitions
- **Secondary**: teachers recommending a preparation tool

## Purpose

Self-practice tool. Students work through archived olympiad questions, see immediate feedback, and learn correct answers. No classroom management, no teacher accounts.

## Phase 1 (this POC)

Prove the full stack: Blazor WASM on GitHub Pages + ASP.NET Core API on Render.com. One mock question per type (6 types), no real content, no timer.

## Must-have (Phase 1 POC)

- All 6 question types render and grade: multiSelect, singleAbcd, shortAnswer, trueFalse, ordering, matching
- Terminal hacker aesthetic matching predecessor Python app (#0d0d0d / #00ff41 / monospace)
- GitHub Pages deployment via GHA
- Render.com API deployment via GHA manual trigger (deploy hook)
- CORS working between the two
- Polish UI labels

## Nice-to-have (deferred)

- Real questions from combined.json
- Contest simulation mode (100 min timer, 20 questions)
- Free practice mode (topic filter)
- OIJ Python/C++ language toggle
- PWA / offline support
- User accounts + progress tracking

## Deployment

- Frontend: GitHub Pages (https://leafsoftwarepoland.github.io/olympiad-quizzer-net)
- Backend API: Render.com free tier (https://olympiad-quizzer-net-api.onrender.com)
- Cost target: $0, no credit card attached

## Lifetime

Long-term product. POC is phase 1 of a multi-phase plan.
