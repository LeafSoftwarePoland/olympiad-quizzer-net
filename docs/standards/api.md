# API file layout

**Classic MVC controllers.** One controller per top-level resource, one file each, under
`Controllers/`. Each is a `public sealed class` deriving from `ControllerBase` and carrying
`[ApiController]`. No minimal-API route registration, no `MapGet`, no `Endpoints/` folder, no
`Program.*.cs`.

Controllers are the required style here because they make L1 tests straightforward without hacks:
an action method is `public` on a `public` class, so an L1 test constructs the controller with
`new` and calls the action directly — no host, no container, no middleware. A minimal-API lambda
cannot be reached that way, which would force every route assertion up to a full-application tier.

| Route | Controller |
|---|---|
| `/v1/questions` | `Controllers/QuestionsController.cs` |
| `/v1/filters` | `Controllers/FiltersController.cs` |
| `/healthz` | `Controllers/HealthController.cs` |
| `/robots.txt` | `Controllers/RobotsController.cs` |

## Routes

- Route prefix is `v{n}/[controller]` — version segment first, then the `[controller]` token.
  **Major version only** (`v1`, `v2`); a minor segment would churn URLs on every non-breaking
  change. Versioning lets versions coexist so old callers migrate at their own pace.
- **No `/api` prefix.** The API is alone on its own host and namespaces nothing.
- Verb attributes distinguish operations on one resource (`v1/questions` for both `GET` and
  `POST`). An action slug is added only when the operations are not CRUD on one resource:
  `[Route("v1/[controller]/[action]")]`. Route templates with parameters (`{id}`) are orthogonal
  and work under either form.
- **Carve-out — operational and well-known routes are exempt from the version prefix and sit at
  the host root:** `/healthz` and `/robots.txt`. Not preference: the deploy workflow polls the
  health route by fixed path, and crawler rules are honoured only at a host root. Versioning
  either one breaks a working system. Any future route in this class needs the same exemption
  stated here.
- **API route segments are English. Client route segments are Polish.** They are consistent with
  the audience rule, not with each other — see [naming-and-comments.md](naming-and-comments.md).
- The `[controller]` token is used. A class rename is a larger refactor by definition — the URL
  changes with it and both are updated together.

## Controllers

- Dependencies arrive through a **primary constructor**, never through property or method
  injection, so hand-construction in L1 is the same call the container makes.
- A controller contains request validation, the call into the repository and the result. No
  filtering logic, no data access, no domain rules.
- One-line routes get their own controller too. No size exemption — a size exemption is where the
  judgement call creeps back in.
- `[ApiController]` produces an automatic problem-details response on model-state failure. That
  is intended: the status code and the problem-details media type are the wire contract, the exact
  field set inside the body is not. Suppress the filter only if a specific body shape ever becomes
  load-bearing, and say so where you suppress it.

## Startup

Startup configuration lives one file per concern under `Extensions/`, as static extension classes
over the service container or the built application. CORS, HTTP JSON options and static-asset
serving are startup concerns, not endpoints, and stay there:

| Concern | File |
|---|---|
| CORS policy and its origin predicate | `Extensions/CorsExtensions.cs` |
| HTTP JSON serializer options | `Extensions/JsonExtensions.cs` |
| Question image static-file serving | `Extensions/StaticAssetsExtensions.cs` |

`Main` builds the host, calls the startup extensions in order, registers controllers, maps them,
and runs. Nothing else. The **order** of those calls is load-bearing and is no longer visible
alongside the routes, so do not reorder them casually.
