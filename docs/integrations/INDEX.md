# Integrations

External services this app talks to at runtime.

| Service | File | Purpose |
|---|---|---|
| GitHub Pages | [github-pages.md](github-pages.md) | Frontend static hosting |
| GitHub Actions | [github-actions.md](github-actions.md) | CI/CD platform + self-hosted runner |
| Render.com | [render-com.md](render-com.md) | API hosting |

## Not yet documented (pending setup)

- **UptimeRobot** (or equivalent) — keep-alive pings against Render free-tier spin-down. Document once account is set up and approach decided.

## Future (do not document until live)

- **Piston** — code execution API for open-answer questions. Not a live dependency yet.

## Not integrations

- `py-pdf-scraper` — situational, run-by-hand tool. Not a live dependency. Document under `docs/tooling/` if ever needed.
