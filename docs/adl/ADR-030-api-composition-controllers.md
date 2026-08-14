# ADR-030: API composition — controllers, versioned routes, startup extensions

**Status:** Accepted
**Date:** 2026-08-14

## Problem

Every route and every piece of startup configuration lived in the entry-point type, split across partial files. The split was by file, not by unit: it was one type, so nothing was independently addressable and nothing could be constructed by name from a test. The file set grew one partial per concern.

Consequence that matters most: the routes could not be exercised without standing up the whole application over HTTP. That forces every route test to be a full-application test, which is the wrong tier for asserting a status code or a guard.

Separately, routes carried no version segment, so there was no way to change a response shape without breaking every existing caller at once.

## Considered

**Route style**

- **Route-registration lambdas in extension classes** — keeps one routing stack and one model binder. Each resource becomes a named class, but the handler itself stays an anonymous lambda: not `public`, not addressable, not constructible. A test can only reach it through the routing pipeline.
- **Classic MVC controllers** — an action is a `public` method on a `public` class, so a test constructs the class with `new`, passes real dependencies and a substitute logger, and calls the action directly. Costs a second routing and model-binding stack for four routes, and the controller attribute produces automatic validation responses.

**Versioning**

- **No version segment** — status quo. A response-shape change breaks every caller simultaneously.
- **Version in a header** — clean URLs. Invisible in a log line, unlinkable, and untestable from a browser address bar.
- **Version as the first path segment** — visible, linkable, greppable, and lets two versions coexist so callers migrate at their own pace.
- **Major and minor in the path** — more precise. Every non-breaking change then churns URLs, which defeats the purpose.

**File granularity**

- **One file per concern** — fewer files, but the boundary is a judgement call and drifts.
- **One file per top-level resource** — mechanical, no judgement, and yields a 1:1 test mirror.
- **One file per HTTP verb** — nonsense at this size.

**Startup composition**

- **Keep it in the entry point** — the status quo that created the problem.
- **Extension classes in a dedicated folder** — one unit per startup concern, entry point reduced to composition order.

## Decision

**Classic MVC controllers under `Controllers/`, one per top-level resource. Version as the first path segment. Startup configuration in extension classes under `Extensions/`. No partial entry-point files.**

### Why controllers, specifically

Testability at the right tier is the deciding reason. An action method is `public` on a `public` class, so it can be constructed by hand with real dependencies and no host, no dependency-injection container, no middleware and no routing. A route-registration lambda cannot be reached that way, so every route assertion would have to pay for a full application.

The objection previously raised against controllers — that the controller attribute's automatic model-state response would silently replace an explicit validation response — does not survive inspection. That automatic response *is* a problem-details body with a 400 status, which is exactly what the contract asks for (ADR-025). The contract pins the status and the media type, not the field set inside the body, and the automatic filter can be suppressed if a specific shape ever becomes load-bearing.

### Route shape

| Route | Controller file |
|---|---|
| `/v1/questions` | `Controllers/QuestionsController.cs` |
| `/v1/filters` | `Controllers/FiltersController.cs` |
| `/healthz` | `Controllers/HealthController.cs` |
| `/robots.txt` | `Controllers/RobotsController.cs` |

- Prefix is the version segment then the controller token. **Major version only** — `v1`, `v2`. A minor segment would churn URLs on every non-breaking change.
- **No `/api` prefix.** The API is alone on its own host (ADR-005) and namespaces nothing that needs namespacing.
- **`/healthz` and `/robots.txt` are exempt from the version prefix and sit at the host root.** Not preference — requirements. The deploy workflow polls the health route by a fixed path (ADR-026), and crawler rules are only honoured at a host root (ADR-028). Versioning either one breaks a working system.
- One-line routes get their own controller. No size exemption, because a size exemption is where the judgement call comes back.

### Startup files

`Extensions/`, static extension classes over the service container or the built application: one file for the CORS policy and its origin predicate, one for HTTP JSON serializer options, one for question-image static-file serving. Question-bank and repository registration stays in the Infrastructure project; the API project does not wrap it.

The entry point builds the host, calls the startup extensions in order, registers and maps controllers, and runs. Nothing else. It stays **`public`** — the logger factory reaches the type through it — and is **not `partial`**, because there is nothing left to split.

Accepted cons:

- A second routing and model-binding stack for four routes.
- Four controller files for four routes, two of them one line each. Deliberate.
- The automatic validation response is framework-shaped, so the exact 400 body may differ from a hand-written one. Status and media type are the contract; tests assert those.
- Log event categories live in the controllers rather than the entry point. The production log-level filter keys on the root namespace prefix, so it still matches, but a test asserting a specific category must name the controller.
- The entry point carries a block of registration calls whose **order** is load-bearing and no longer visible alongside the routes.
- Adding `v2` alongside `v1` means two controllers for one resource. That is the cost of coexistence and the reason versioning was chosen.

## Remarks / Sources

- File layout, route-attribute form, primary-constructor injection, and the test mirror rule are enforced as coding standards, not restated here. Where this ADR and the standards disagree, the standards win.
- ADR-025 (the contract these routes serve, including the pinned 200-with-empty-array), ADR-013 (read-only, stateless posture), ADR-028 (crawler rules must be served by this API at its host root), ADR-026 (the health route is polled by a fixed path), the coding standards (the entry point is public and not partial)
- Consequence: the previous route paths change. Frontend request paths, the CORS preflight tests and the API README all name the old paths and are updated in the same change.
