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
- `[ApiController]` produces an automatic problem-details response on model-state failure. **That is
  intended and is left alone.** It rejects requests our own client cannot produce, so it sits
  outside the coded contract by design — see § Error handling § Scope. Do not suppress the filter to
  make every response carry a code; that builds a user-facing experience for a state the UI cannot
  reach.

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

## Error handling — two layers, no gaps

The requirement is that failures reach the user *gracefully*. A graceful screen is impossible
unless the API returns something structured, so the API's obligation is to **never emit an
unshaped failure** — not a raw stack trace, not an empty body, not a bare 500.

Two layers, and both are required. Neither substitutes for the other.

**1. Per-action, in the controller.** Every action wraps its work in `try`/`catch` and catches only
the failures it **accounts for** — the ones it knows can happen and knows what they mean. Log with
full detail: exception, parameters, whatever identifies the request. Then return a shaped response,
which may legitimately be a 500, carrying enough for the UI to render something useful.

**2. Global exception middleware.** Catches **everything** — literally everything the first layer
did not account for. Logs it as an unhandled-exception occurrence with full detail. Returns a
shaped 500.

The division is the point. Layer 1 handles the *known*; layer 2 guarantees that the *unknown* never
escapes unshaped. An empty `catch (Exception)` in a controller defeats both and is a defect —
see [csharp.md](csharp.md) § Error handling.

### The error contract

One response shape for every failure **the application itself emits**:

- a **stable machine code**
- the framework's request identifier, so a report can be traced to a log line
- **nothing else**

**Scope — read this before adding a code.** The contract exists to serve *our client*: a failure
arrives, the client maps the code to Polish, the student sees something useful. It does not extend
to requests our client cannot produce.

A malformed request — `?limit=abc` — is rejected by `[ApiController]` before any of our code runs,
and that response carries no code. **That is correct and is not a gap.** Our client builds its URLs
from typed values, so if it ever sent that, the bug is in the client and the fix is in the client;
giving the malformed request a well-designed error would be building a user experience for a state
our UI cannot reach. A request hand-crafted in Postman is outside our contract by definition, and
the framework's standard rejection is the right answer to it.

**Codes are added when a real failure needs one, never in advance.** Two speculative codes were
removed after review found neither could occur: a bank-unavailable code, unreachable because a
missing or mismatched database is fatal at startup so a running API always has a working bank; and
an invalid-limit code, unreachable because that rejection is the framework's, not ours. A code
nobody can trigger is documentation that lies.

Today there is exactly one: **`UNEXPECTED`**, which the client's unrecognised-code fallback also
maps to, so both share one Polish string.

**Codes, not prose.** The API is not user-facing: it is read by a program. Polish belongs to the
client, which owns every other user-facing string. Sending a Polish sentence from the backend would
put UI copy in the wrong project, make rewording a message a backend redeploy, and split
responsibility for what the user reads across two rings.

Codes are declared as constants in the **Domain** project, so the API emits them and the client maps
them to Polish text against the same definitions — a mismatch becomes a compile error rather than a
mystery on screen. The client keeps a generic fallback for a code it does not recognise, which also
covers version skew between two independently deployed sides.

**No technical detail leaves the process.** No exception type, no message, no stack, no SQL, no
file path, no connection string. The request identifier is the only technical value that may cross
the wire, and it is safe precisely because it means nothing outside the logs.

### What this obliges each tier to prove

Derived, not restated — see § How these rules compose in [INDEX.md](INDEX.md):

- **L0** — the middleware shapes and logs correctly; each controller action returns the right code
  for each accounted-for failure, including when its repository **throws**.
- **L1** — controllers and repositories **bubble** unaccounted failures rather than swallowing
  them. Middleware is unreachable at L1 by construction, so this is the half L1 owns.
- **L2** — the chain end to end: a fault thrown deep in the app surfaces over HTTP as a shaped
  response with the right status and code, and nothing in between swallowed it.
