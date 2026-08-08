# Sprint Backlog — olympiad-quizzer-net

**Weight class S → this file IS the implementation plan. No Sprint Planner.**
Implementor executes T-00 → T-14 in order. Anything not written here is out of scope; do not invent abstractions, feature flags, or compat shims.

**Sprint 01 — "POC end-to-end"** · Goal: both halves deployed, all 6 question types render and grade, Render cost confirmed $0.
Leans on: ADR-001, ADR-003, ADR-006, ADR-007, ADR-011, ADR-016, ADR-017, ADR-019, **ADR-020**, **ADR-021**, **ADR-022** · `solution-design.md` · `test-strategy.md`

## Dependency order

```
T-00 preflight
 └─ T-01 solution scaffold
     └─ T-02 shared models
         ├─ T-03 grader ─────────────┬─ T-10 L0 tests
         ├─ T-04 API ── T-05 Docker ─┴─ T-11 L1 tests
         └─ T-06 client bootstrap
             ├─ T-07 css
             └─ T-08 shell + pages
                 └─ T-09 question components
                                        └─ T-12 / T-13 workflows ─ T-14 manual verification
```

T-07 is independent of T-08/T-09 and can be done any time after T-06.

---

## T-00 — Preflight checks

**Why**: three environment facts below have burned other projects. Ten seconds each now, hours later.

1. `dotnet --list-sdks` → confirm a `10.0.*` SDK. (Verified present on this machine 2026-08-08: 10.0.300, 10.0.301.)
2. **On the self-hosted runner machine**, in a Git Bash shell: `tar --version`.
   - Prints `bsdtar` → `actions/upload-pages-artifact` **will fail** with `tar.exe: Option --hard-dereference is not supported` (open issue, no fix: https://github.com/actions/upload-pages-artifact/issues/95). Fix by putting `C:\Program Files\Git\bin` **above** `C:\Program Files\Git\usr\bin` in system PATH, restart the runner, re-check.
   - Prints `GNU tar` → proceed.
   - If the PATH fix does not take, use the escape hatch documented in T-12 (flip the Pages job to `ubuntu-latest`) and record it in `assumptions.md`. Do not silently change it — note it in the JOURNAL.
3. Confirm the action tags used in T-12/T-13 actually exist before committing:
   ```powershell
   'checkout','setup-dotnet','configure-pages','upload-pages-artifact','deploy-pages' |
     ForEach-Object { "$_ -> " + (gh api "repos/actions/$_/releases/latest" --jq .tag_name) }
   ```
   Two independent research passes disagreed on these versions (`upload-pages-artifact` v3 vs v5, `configure-pages` v5 vs v6, `deploy-pages` v4 vs v5). **Trust this command, not the tags written in T-12.** Pin to the highest existing major.

**Acceptance**: SDK 10 confirmed; tar flavour on the runner known and recorded; five action tags resolved and written into T-12/T-13 before those files are committed.

---

## T-01 — Solution + project scaffold

Run from repo root `C:\Repositories\olympiad-quizzer-net`.

```powershell
dotnet new sln -n OlympiadQuizzer

dotnet new classlib   -n OlympiadQuizzer.Shared -o source/shared -f net10.0
dotnet new web        -n OlympiadQuizzer.Api    -o source/api    -f net10.0
dotnet new blazorwasm -n OlympiadQuizzer.Client -o source/client -f net10.0
dotnet new xunit      -n OlympiadQuizzer.Tests  -o source/tests  -f net10.0

dotnet sln add source/shared/OlympiadQuizzer.Shared.csproj
dotnet sln add source/api/OlympiadQuizzer.Api.csproj
dotnet sln add source/client/OlympiadQuizzer.Client.csproj
dotnet sln add source/tests/OlympiadQuizzer.Tests.csproj

dotnet add source/api/OlympiadQuizzer.Api.csproj       reference source/shared/OlympiadQuizzer.Shared.csproj
dotnet add source/client/OlympiadQuizzer.Client.csproj reference source/shared/OlympiadQuizzer.Shared.csproj
dotnet add source/tests/OlympiadQuizzer.Tests.csproj   reference source/shared/OlympiadQuizzer.Shared.csproj
dotnet add source/tests/OlympiadQuizzer.Tests.csproj   reference source/api/OlympiadQuizzer.Api.csproj
dotnet add source/tests/OlympiadQuizzer.Tests.csproj   package Microsoft.AspNetCore.Mvc.Testing
```

`dotnet new web` (not `webapi`) — `webapi` scaffolds the weather-forecast sample and OpenAPI wiring the POC does not need. Delete any leftover sample code.

**`source/client/OlympiadQuizzer.Client.csproj`** — add inside the existing `<PropertyGroup>`:

```xml
<CompressionEnabled>false</CompressionEnabled>
```

Reason: GitHub Pages does no content negotiation for `.br`, and .NET 10 fingerprints published assets. Shipping only uncompressed files removes an entire class of "app is blank in production, 404 on `_framework/*`" failures. Costs transfer size; irrelevant for a POC. Revisit in Phase 2. See `assumptions.md` A-05.

Add `.gitattributes` at repo root (cheap guard against line-ending rewrites corrupting JS asset hashes):

```
* text=auto
*.js   -text
*.wasm binary
*.dat  binary
```

**Acceptance**: `dotnet build OlympiadQuizzer.sln` succeeds; `dotnet test` runs (0 tests) ; `source/` contains exactly `api`, `client`, `shared`, `tests`.

---

## T-02 — Shared models

All files in `source/shared/`. Namespace `OlympiadQuizzer.Shared`. Schema authority: ADR-011. Per-type field meaning: ADR-022.

**`QuestionType.cs`**
```csharp
[JsonConverter(typeof(JsonStringEnumConverter<QuestionType>))]
public enum QuestionType
{
    Unknown = 0,     // deserialization fallback — never written to questions.json
    MultiSelect,
    SingleAbcd,
    ShortAnswer,
    TrueFalse,
    Ordering,
    Matching
}
```
JSON values are camelCase (`"multiSelect"`). With `JsonNamingPolicy.CamelCase` set on the enum converter (see `JsonOptions`), PascalCase members map to camelCase strings automatically. An unrecognised string must land on `Unknown`, not throw — set `UnmappedMemberHandling`/fallback accordingly and cover it with test 26.

**`ContentBlock.cs`**
```csharp
public sealed class ContentBlock
{
    public string Type { get; set; } = "text";   // "text" | "code" | "image"
    public string? Text { get; set; }            // text + code blocks
    public string? File { get; set; }            // image blocks only (ADR-011)
}
```

**`Question.cs`** — property names PascalCase, serialized camelCase by policy. Do **not** hand-write `[JsonPropertyName]` attributes; the policy is the contract (ADR-011).
```csharp
public sealed class Question
{
    public string Id { get; set; } = "";
    public string Source { get; set; } = "other";     // "oij" | "vea" | "other"
    public string Competition { get; set; } = "";
    public string? Voivodeship { get; set; }
    public int? Stage { get; set; }
    public string Year { get; set; } = "";
    public QuestionType Type { get; set; }
    public List<ContentBlock> Content { get; set; } = new();
    public List<ContentBlock>? ContentCpp { get; set; }

    /// <summary>Per ADR-022: choices (multiSelect/singleAbcd), items as displayed
    /// (ordering), statements (trueFalse), left column (matching). Null only for shortAnswer.</summary>
    public List<string>? Options { get; set; }

    /// <summary>Right-hand pool. Non-null for matching only.</summary>
    public List<string>? MatchOptions { get; set; }

    /// <summary>Shape varies by type — see ADR-022. Read via the As* helpers.</summary>
    public JsonElement CorrectAnswer { get; set; }

    public int Points { get; set; } = 1;
    public bool PartialCredit { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> SourceUrls { get; set; } = new();
    public List<ContentBlock>? Explanation { get; set; }

    public int[]    CorrectIndices()  => Read<int[]>()    ?? Array.Empty<int>();
    public string[] CorrectStrings()  => Read<string[]>() ?? Array.Empty<string>();
    public bool[]   CorrectBooleans() => Read<bool[]>()   ?? Array.Empty<bool>();

    private T? Read<T>() =>
        CorrectAnswer.ValueKind == JsonValueKind.Undefined ? default
                                                           : CorrectAnswer.Deserialize<T>(JsonOptions.Default);
}
```
`JsonElement` rather than a custom converter: `correctAnswer` is `int[]`, `string[]` or `bool[]` depending on `type` (ADR-022). A converter is ~60 lines to save three helper methods. System.Text.Json gives a deserialized `JsonElement` property its own backing document, so it stays valid after the source string is gone — **cover this with test 33** (read `poc-5.correctAnswer` after the load method has returned).

**`QuizFilter.cs`** — the ADR-003 seam. POC passes `QuizFilter.None`.
```csharp
public sealed class QuizFilter
{
    public string? Source { get; set; }
    public string? Competition { get; set; }
    public QuestionType? Type { get; set; }
    public int? Limit { get; set; }
    public static QuizFilter None { get; } = new();
}
```

**`AnswerSubmission.cs`** — one shape per ADR-022; exactly one field populated per type.
```csharp
public sealed class AnswerSubmission
{
    public List<int> SelectedIndices { get; set; } = new();  // multiSelect, singleAbcd
    public string? Text { get; set; }                        // shortAnswer
    public List<bool?> Booleans { get; set; } = new();       // trueFalse
    public List<int> Order { get; set; } = new();            // ordering
    public List<int> Matches { get; set; } = new();          // matching (-1 = unanswered)
}
```

**`GradeResult.cs`**
```csharp
public sealed record GradeResult(bool IsCorrect, double PointsAwarded, double MaxPoints);
```

**`JsonOptions.cs`** — single instance used by API, client and tests. ADR-011's camelCase contract lives here and nowhere else.
```csharp
public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
```

**Acceptance**: `Shared` compiles with zero package references and no `Microsoft.AspNetCore.*` / WASM references (ADR-021 rule: no I/O, no HTTP, no DI, no UI).

---

## T-03 — Grader

`source/shared/Grader.cs`. This is the only real logic in the POC. Semantics are fixed by ADR-022 — read its two worked-example tables before writing this.

```csharp
public static class Grader
{
    public static string Normalize(string? s) =>
        (s ?? string.Empty).Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

    public static GradeResult Grade(Question q, AnswerSubmission? a)
    {
        double max = q.Points;
        if (a is null) return new GradeResult(false, 0, max);

        // returns (matched, total); total == 0 means "not a positional type"
        var (matched, total) = q.Type switch
        {
            QuestionType.MultiSelect => SetEqual(a.SelectedIndices, q.CorrectIndices()) ? (1, 1) : (0, 1),
            QuestionType.SingleAbcd  => SeqEqual(a.SelectedIndices, q.CorrectIndices()) ? (1, 1) : (0, 1),
            QuestionType.ShortAnswer => q.CorrectStrings().Any(e => Normalize(e) == Normalize(a.Text)) ? (1, 1) : (0, 1),
            QuestionType.TrueFalse   => PositionMatch(a.Booleans, q.CorrectBooleans()),
            QuestionType.Ordering    => PositionMatch(a.Order,    q.CorrectIndices()),
            QuestionType.Matching    => PositionMatch(a.Matches,  q.CorrectIndices()),
            _                        => (0, 1)          // Unknown — never throws
        };

        // Guard first: total == 0 means a malformed question (empty correctAnswer).
        // Without this, "matched == total" is 0 == 0 and an unanswerable question
        // grades as CORRECT. Test 27b exists for exactly this.
        if (total <= 0) return new GradeResult(false, 0, max);

        bool positional = q.Type is QuestionType.TrueFalse or QuestionType.Ordering or QuestionType.Matching;
        double awarded  = (positional && q.PartialCredit)
            ? max * matched / total
            : (matched == total ? max : 0);

        return new GradeResult(Math.Abs(awarded - max) < 1e-9 && max > 0, awarded, max);
    }
}
```

Rules the helpers must obey:

- `SetEqual` — order-insensitive, duplicate-insensitive, length must match after de-duplication. `[1,0]` equals `[0,1]`; `[0]` and `[0,1,2]` do not equal `[0,1]`.
- `SeqEqual` — exact order and length.
- `PositionMatch(submitted, expected)` — if lengths differ, return `(0, expected.Length)`; a wrong-length answer is never correct and never partially credited. Otherwise count element-wise equal positions. A `null` bool or a `-1` match index counts as not-matched.
- The `total <= 0` guard is load-bearing, not defensive noise. `PositionMatch` returns `(0, 0)` when `expected` is empty, and `0 == 0` would otherwise report a malformed question as answered correctly by anyone, including by an empty submission.
- `Math.Abs(... ) < 1e-9` rather than `==` — partial credit produces values like `1 * 2/3`.
- `max > 0` guard so a hypothetical zero-point question never reports `IsCorrect` for an empty answer.
- Never throws for any input, including null/empty/over-long arrays and `QuestionType.Unknown`. Tests 22, 26, 27 exist to enforce this.
- `multiSelect` / `singleAbcd` / `shortAnswer` ignore `partialCredit` even when true (ADR-022).

**Acceptance**: T-10 tests 1–27 green.

---

## T-04 — API

**`source/api/Data/questions.json`** — the six POC fixtures, verbatim. These must match ADR-022's worked examples exactly; test 33 asserts it.

```json
[
  {
    "id": "poc-1", "source": "other", "competition": "POC", "voivodeship": null, "stage": null, "year": "2026",
    "type": "multiSelect",
    "content": [{ "type": "text", "text": "Wielokrotny wybór — zaznacz A i B." }],
    "contentCpp": null,
    "options": ["A", "B", "C", "D"],
    "matchOptions": null,
    "correctAnswer": [0, 1],
    "points": 1, "partialCredit": false, "tags": [], "sourceUrls": [], "explanation": null
  },
  {
    "id": "poc-2", "source": "other", "competition": "POC", "voivodeship": null, "stage": null, "year": "2026",
    "type": "singleAbcd",
    "content": [
      { "type": "text", "text": "Jednokrotny wybór — poprawna odpowiedź to C. Poniższy blok sprawdza renderowanie kodu:" },
      { "type": "code", "text": "for i in range(3):\n    print('*')" }
    ],
    "contentCpp": null,
    "options": ["A", "B", "C", "D"],
    "matchOptions": null,
    "correctAnswer": [2],
    "points": 1, "partialCredit": false, "tags": [], "sourceUrls": [], "explanation": null
  },
  {
    "id": "poc-3", "source": "other", "competition": "POC", "voivodeship": null, "stage": null, "year": "2026",
    "type": "shortAnswer",
    "content": [{ "type": "text", "text": "Odpowiedź otwarta — wpisz: kajak" }],
    "contentCpp": null,
    "options": null,
    "matchOptions": null,
    "correctAnswer": ["kajak"],
    "points": 1, "partialCredit": false, "tags": [], "sourceUrls": [], "explanation": null
  },
  {
    "id": "poc-4", "source": "other", "competition": "POC", "voivodeship": null, "stage": null, "year": "2026",
    "type": "trueFalse",
    "content": [{ "type": "text", "text": "Prawda/fałsz — oceń trzy twierdzenia." }],
    "contentCpp": null,
    "options": ["2 + 2 = 4", "Słońce jest planetą", "Python jest językiem programowania"],
    "matchOptions": null,
    "correctAnswer": [true, false, true],
    "points": 1, "partialCredit": false, "tags": [], "sourceUrls": [], "explanation": null
  },
  {
    "id": "poc-5", "source": "other", "competition": "POC", "voivodeship": null, "stage": null, "year": "2026",
    "type": "ordering",
    "content": [{ "type": "text", "text": "Kolejność — ułóż elementy alfabetycznie: A, B, C, D." }],
    "contentCpp": null,
    "options": ["C", "A", "D", "B"],
    "matchOptions": null,
    "correctAnswer": [1, 3, 0, 2],
    "points": 1, "partialCredit": false, "tags": [], "sourceUrls": [], "explanation": null
  },
  {
    "id": "poc-6", "source": "other", "competition": "POC", "voivodeship": null, "stage": null, "year": "2026",
    "type": "matching",
    "content": [{ "type": "text", "text": "Dopasowanie — połącz: Kot→Mleko, Pies→Trawa, Ryba→Woda." }],
    "contentCpp": null,
    "options": ["Kot", "Pies", "Ryba"],
    "matchOptions": ["Woda", "Trawa", "Mleko"],
    "correctAnswer": [2, 1, 0],
    "points": 1, "partialCredit": false, "tags": [], "sourceUrls": [], "explanation": null
  }
]
```

Two deliberate deviations from the POC design spec's mock table, both flagged in `assumptions.md`:
- `poc-2` carries an extra `code` content block, so the code-block styling (dark bg + 3 px green left border) is actually exercised by the POC instead of shipping untested.
- `trueFalse` and `matching` have non-null `options` per ADR-022 — the spec table's `options: null` was internally inconsistent with its own statement/left-column lists.

**`source/api/OlympiadQuizzer.Api.csproj`** — ship the data file next to the binary:
```xml
<ItemGroup>
  <Content Include="Data\questions.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```
File copy, not embedded resource (the design spec said "embedded"): a copied file can be asserted against directly by test 28, and stays one artifact rather than two.

**`source/api/Program.cs`**
```csharp
using System.Text.Json;
using OlympiadQuizzer.Shared;

var builder = WebApplication.CreateBuilder(args);

// Render injects PORT (default 10000). Read it in code — an ENV line in the
// Dockerfile cannot expand $PORT at image-build time.
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    foreach (var c in JsonOptions.Default.Converters) o.SerializerOptions.Converters.Add(c);
});

const string CorsPolicy = "poc";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .WithOrigins("https://leafsoftwarepoland.github.io")
    // ASP.NET Core has no port wildcard — "http://localhost:*" is not a thing.
    .SetIsOriginAllowed(origin =>
        origin == "https://leafsoftwarepoland.github.io" ||
        (Uri.TryCreate(origin, UriKind.Absolute, out var u) &&
         (u.Host == "localhost" || u.Host == "127.0.0.1")))
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Fail fast: a bad data file must break the deploy, not serve an empty quiz.
var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "questions.json");
var questions = JsonSerializer.Deserialize<List<Question>>(File.ReadAllText(dataPath), JsonOptions.Default)
                ?? throw new InvalidOperationException($"questions.json empty or unreadable: {dataPath}");
if (questions.Count == 0) throw new InvalidOperationException("questions.json contains no questions.");

var app = builder.Build();

// No UseHttpsRedirection — Render terminates TLS at the edge; redirecting
// inside the container breaks the health check and the CORS preflight.
app.UseCors(CorsPolicy);

app.MapGet("/healthz", () => Results.Ok(new { ok = true }));
app.MapGet("/api/questions", () => Results.Ok(questions));

app.Run();

public partial class Program;   // required by WebApplicationFactory<Program> in T-11
```

**Acceptance**: `dotnet run --project source/api` → `/healthz` returns `{"ok":true}`; `/api/questions` returns 6 questions with camelCase keys; `PORT=8080 dotnet run` binds 8080.

---

## T-05 — Dockerfile

`source/api/Dockerfile`. **Build context is the repo root**, because the API references `source/shared` (ADR-021).

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY source/shared/ source/shared/
COPY source/api/    source/api/
RUN dotnet publish source/api/OlympiadQuizzer.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "OlympiadQuizzer.Api.dll"]
```

**Render service settings that must match this** (check in the Render dashboard, these are the classic first-deploy failures):

| Setting | Value |
|---|---|
| Language / Runtime | Docker |
| Dockerfile Path | `./source/api/Dockerfile` |
| Docker Build Context Directory | `.` (repo root — **not** `./source/api`) |
| Health Check Path | `/healthz` |
| Auto-Deploy | Off (deploys go through `deploy-backend.yml`, ADR-020) |
| Instance Type | Free |

`EXPOSE 10000` is documentation only; the actual bind comes from `PORT` in `Program.cs`. Do not add `ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT` — Docker will not expand `$PORT` at build time and the app would try to bind a literal `$PORT`.

**Acceptance**: from repo root, `docker build -f source/api/Dockerfile -t oq-api .` succeeds; `docker run -p 10000:10000 oq-api` serves `/healthz` on 10000. (If Docker Desktop is unavailable locally, skip to the Render deploy in T-13 and treat that as the test — record which path was taken.)

---

## T-06 — Client bootstrap

**`source/client/wwwroot/appsettings.json`** (dev value; CI overwrites it in T-12)
```json
{ "ApiBaseUrl": "http://localhost:5080" }
```
Set the API's launch profile port to 5080 so this matches out of the box, or edit both together.

**`source/client/Program.cs`**
```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"]
              ?? throw new InvalidOperationException("ApiBaseUrl missing from wwwroot/appsettings.json");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBase.TrimEnd('/') + "/"),
    Timeout = TimeSpan.FromSeconds(90)      // Render free-tier cold start
});
builder.Services.AddScoped<IQuestionRepository, ApiQuestionRepository>();
builder.Services.AddSingleton<QuizSession>();

await builder.Build().RunAsync();
```
`wwwroot/appsettings.json` is loaded automatically by standalone WASM — no extra fetch code. The trailing-slash normalisation matters: `new Uri(baseWithoutSlash, "api/questions")` silently drops the last path segment.

**`source/client/Services/ApiQuestionRepository.cs`** — ADR-003 names this `ApiQuestionRepository` (`JsonQuestionRepository` is reserved for the static-file implementation ADR-002 describes, which the POC does not build).
```csharp
public sealed class ApiQuestionRepository(HttpClient http) : IQuestionRepository
{
    public async Task<List<Question>> GetAsync(QuizFilter filter)
    {
        var all = await http.GetFromJsonAsync<List<Question>>("api/questions", JsonOptions.Default) ?? new();
        // POC: filter is the ADR-003 seam, not a feature. Only Limit is honoured.
        return filter.Limit is int n && n > 0 ? all.Take(n).ToList() : all;
    }
}
```
`IQuestionRepository` itself goes in `source/client/Services/IQuestionRepository.cs` — it is an I/O abstraction, so it stays out of `Shared` (ADR-021 rule).

**`source/client/Services/QuizSession.cs`** — in-memory quiz state; lost on refresh, which is acceptable for the POC.
```csharp
public sealed class QuizSession
{
    public List<Question> Questions { get; private set; } = new();
    public List<GradeResult?> Results { get; private set; } = new();
    public int CurrentIndex { get; set; }

    public void Start(List<Question> questions) { Questions = questions; Results = questions.Select(_ => (GradeResult?)null).ToList(); CurrentIndex = 0; }
    public void Record(int i, GradeResult r) => Results[i] = r;
    public double Score => Results.Where(r => r is not null).Sum(r => r!.PointsAwarded);
    public double MaxScore => Questions.Sum(q => q.Points);
    public bool IsStarted => Questions.Count > 0;
}
```

**Acceptance**: `dotnet run --project source/client` with the API running → no console errors, `ApiBaseUrl` resolves, `GET /api/questions` succeeds in DevTools Network.

---

## T-07 — CSS

`source/client/wwwroot/css/app.css`. Port `c:\Repositories\py-oij-quizzer\python\static\css\style.css` — copy the token block and component rules verbatim where they apply; drop the Flask-only sections (`.mode-card*`, `.landing-card*`, `.reject-list`, `nav`) unless a page actually uses them.

Must carry over exactly:
```css
:root {
  --bg: #0d0d0d;  --bg-code: #141414;  --accent: #00ff41;
  --text: #d8d8d8;  --text-dim: #888;  --verdict-red: #ff4444;
  --verdict-amber: #ffaa00;  --border: #2a2a2a;
  --font: ui-monospace, "Cascadia Code", "JetBrains Mono", Consolas, "DejaVu Sans Mono", monospace;
}
```
Plus: `pre` (dark bg, 3 px green left border, `overflow-x:auto`, `white-space:pre`), `.btn` (transparent bg, 1 px accent border, inverts on hover), `.verdict` / `.verdict-correct` / `.verdict-wrong`, `.open-input`, `.options-list`, `.breakdown-table`, `.score-heading`, `main { max-width: 800px; margin: 0 auto; }`.

Bootstrap 5 is grid-only (ADR-016). Load Bootstrap **before** `app.css` in `index.html` so the tokens win. Do not use Bootstrap colour utility classes (`btn-primary`, `bg-dark`, `text-muted`) anywhere — they reintroduce Bootstrap's palette.

Delete the template's `wwwroot/css/bootstrap/` local copy and the default `app.css` content; reference Bootstrap from CDN in `index.html` or keep the local copy — either is fine, but only one.

Responsive additions on top of the port (ADR-016):
```css
@media (max-width: 560px) {
  html, body { font-size: 16px; }              /* never below 16px on mobile */
  .options-list li { min-height: 44px; }        /* WCAG tap target */
  .options-list input[type="checkbox"],
  .options-list input[type="radio"] { width: 22px; height: 22px; }
  .btn { min-height: 44px; padding: 0.6rem 1.2rem; }
}
```

**Acceptance**: landing page is `#0d0d0d` with `#00ff41` headings and monospace text; a code block shows the green left border; at a 360 px viewport there is no horizontal page scroll.

---

## T-08 — Client shell + pages

**`source/client/wwwroot/index.html`** — keep `<base href="/" />` for local dev; T-12 rewrites it at publish time. Set `<html lang="pl">` (ADR-017/ADR-019). Title: `Olympiad Quizzer`.

**`App.razor`** — default template router is fine. Polish `NotFound` text: `Nie znaleziono strony.`

**`Layout/MainLayout.razor`** — minimal: `<main>@Body</main>`. No sidebar, no nav menu; delete `NavMenu.razor` and the template's `Layout` styling.

**`Pages/Home.razor`** — `@page "/"`
- `<h1>` app title, one-line Polish lead, `<button class="btn">Rozpocznij quiz</button>` → `NavigationManager.NavigateTo("quiz")` (relative — do **not** use a leading `/`, it breaks under the `/olympiad-quizzer-net/` base path).

**`Pages/Quiz.razor`** — `@page "/quiz"` — the core screen.

State machine per question: `Answering` → (Sprawdź) → `Revealed` → (Dalej) → next question, or navigate to `result` after the last one.

```razor
@inject IQuestionRepository Repo
@inject QuizSession Session
@inject NavigationManager Nav
```

- `OnInitializedAsync`: if `!Session.IsStarted`, `Session.Start(await Repo.GetAsync(QuizFilter.None))` inside try/catch.
- Loading state text: `Budzenie serwera… może potrwać do minuty.` (Render cold start, `solution-design.md` §5).
- Error state: Polish panel + `Spróbuj ponownie` button that re-runs the load. No auto-retry.
- Empty result (0 questions) → same error panel.
- Header: `<span class="progress">@(idx + 1) / @Session.Questions.Count</span>`.
- Renders `<QuestionRenderer Question="q" Submission="submission" ReadOnly="revealed" />` (T-09).
- `Sprawdź` → `Grader.Grade(q, submission)` → `Session.Record(idx, result)` → set `revealed = true`.
- Verdict block, `aria-live="polite"` (ADR-017): `POPRAWNIE` in `.verdict-correct`, `BŁĄD` in `.verdict-wrong`. When wrong, show the correct answer in `.correct-answer-block`, rendered in human terms (option letters/labels, not raw indices).
- `Dalej` (last question: `Zobacz wynik`) → advance or `Nav.NavigateTo("result")`.
- On question change call `FocusAsync()` on the first interactive element (ADR-017) — capture it with `@ref` and focus in `OnAfterRenderAsync` when the index changed.

**`Pages/Result.razor`** — `@page "/result"`
- If `!Session.IsStarted` → redirect to `/` (covers a direct deep-link / refresh).
- `<h1 class="score-heading">@Session.Score / @Session.MaxScore</h1>`
- `.breakdown-table`: one row per question — number, type label in Polish, `+`/`−` marker using `.hit` / `.miss`.
- `Jeszcze raz` button → clear session, navigate to `quiz`.

Polish type labels (single source, e.g. a `static Dictionary<QuestionType,string>` in the client): Wielokrotny wybór · Jednokrotny wybór · Odpowiedź otwarta · Prawda/fałsz · Kolejność · Dopasowanie.

**Acceptance**: full run of all 6 questions reaches the result page; all-correct gives `6 / 6`, all-wrong gives `0 / 6`; stopping the API mid-run shows the Polish error panel, not an unhandled exception.

---

## T-09 — Question components

`source/client/Components/`. `QuestionRenderer.razor` dispatches on `Question.Type`; six leaf components own their input widget. Every leaf takes `[Parameter] Question Question`, `[Parameter] AnswerSubmission Submission`, `[Parameter] bool ReadOnly`, and mutates `Submission` in place.

**`ContentRenderer.razor`** (used by all six): walks `Question.Content` and emits `<p>` for `text`, `<pre><code>` for `code`, and for `image` a Polish placeholder — image blocks are out of POC scope (ADR-010 deferred), so render `[obraz — poza zakresem POC]` rather than a broken `<img>`.

| Component | Widget | ARIA (ADR-017) |
|---|---|---|
| `MultiSelectQuestion` | checkbox per `Options[i]`, letter prefix A/B/C/D | `<fieldset>` + `<legend>`; `<label for>` linked to each input |
| `SingleAbcdQuestion` | radio group, one name per question id | `role="radiogroup"` on the container, `aria-checked` on options |
| `ShortAnswerQuestion` | single `<input class="open-input">` bound to `Submission.Text` | `aria-label="Odpowiedź"` |
| `TrueFalseQuestion` | one Prawda/Fałsz radio pair per `Options[i]` statement | each pair is its own `role="radiogroup"` with the statement as `aria-label` |
| `OrderingQuestion` | ordered list with `▲`/`▼` buttons per row | buttons need `aria-label="Przesuń w górę/dół: <item>"`; **no drag-and-drop in the POC** |
| `MatchingQuestion` | one `<select>` per `Options[i]` left item, populated from `MatchOptions` | `aria-label` = the left item text |

`OrderingQuestion` uses move-up/move-down buttons, not HTML5 drag-and-drop. ADR-016 asks for pointer-event drag support and ADR-017 asks for keyboard reorder; buttons satisfy both with one implementation and no JS interop. Drag-and-drop is a Phase 2 enhancement on top, not a prerequisite — recorded in `assumptions.md` A-07.

Initialisation, so the grader never sees a shape mismatch:
- `ordering` → `Submission.Order = [0, 1, .., n-1]` (the displayed order) on load.
- `matching` → `Submission.Matches = [-1] * Options.Count`.
- `trueFalse` → `Submission.Booleans = [null] * Options.Count`.

`ReadOnly=true` (after Sprawdź) disables every input so a revealed answer cannot be edited.

Unknown type → `<p>Nieobsługiwany typ pytania.</p>`, no throw.

**Acceptance**: manual checks M3/M4 in `test-strategy.md` pass; keyboard-only completion of all six questions is possible (M15).

---

## T-10 — L0 tests

`source/tests/GraderTests.cs`, `NormalizationTests.cs`, `QuestionLoadingTests.cs`. Tests 1–34 in `test-strategy.md`, with their exact names.

Fixture helper — build questions in code rather than reading JSON, except tests 28–34 which must read the shipped file:
```csharp
static Question Q(QuestionType type, object correct, string[]? options = null,
                  string[]? matchOptions = null, bool partial = false, int points = 1) => new()
{
    Id = "t", Source = "other", Competition = "POC", Year = "2026", Type = type,
    Options = options?.ToList(), MatchOptions = matchOptions?.ToList(),
    CorrectAnswer = JsonSerializer.SerializeToElement(correct, JsonOptions.Default),
    Points = points, PartialCredit = partial
};
```

Point `questions.json` at the real file (never a copy — a copy lets the shipped file rot while tests stay green). In `OlympiadQuizzer.Tests.csproj`:
```xml
<ItemGroup>
  <Content Include="..\api\Data\questions.json" Link="Data\questions.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Test 11 needs a genuinely decomposed string — build it explicitly, do not paste one into the source and hope the editor preserved it:
```csharp
var decomposed = "ko\u0301d";          // k + o + U+0301 combining acute  → "kód"
var precomposed = "k\u00F3d";          // k + U+00F3
Assert.Equal(Grader.Normalize(precomposed), Grader.Normalize(decomposed));
```

**Acceptance**: `dotnet test` green, 34 tests, zero skipped.

---

## T-11 — L1 API tests

`source/tests/ApiEndpointTests.cs`. Tests 35–42 in `test-strategy.md`.

```csharp
public class ApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApiEndpointTests(WebApplicationFactory<Program> f) => _client = f.CreateClient();
    // ...
}
```

Requires `public partial class Program;` at the end of the API's `Program.cs` (T-04). If `WebApplicationFactory<Program>` will not resolve, the cause is almost always that line missing or an `InternalsVisibleTo` gap — not a package version problem.

CORS tests (41–42) must issue a real preflight:
```csharp
var req = new HttpRequestMessage(HttpMethod.Options, "/api/questions");
req.Headers.Add("Origin", "https://leafsoftwarepoland.github.io");
req.Headers.Add("Access-Control-Request-Method", "GET");
```

**Acceptance**: `dotnet test` green, 42 tests total across T-10 + T-11.

---

## T-12 — `deploy-frontend.yml`

`.github/workflows/deploy-frontend.yml`. **Substitute the action tags resolved in T-00 before committing** — the tags below are the best current estimate and the two research passes disagreed.

```yaml
name: deploy-frontend

on:
  push:
    branches: [main]
    paths:
      - 'source/client/**'
      - 'source/shared/**'          # client compiles against Shared — a Shared change must redeploy
      - '.github/workflows/deploy-frontend.yml'
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: true

jobs:
  build:
    runs-on: self-hosted
    steps:
      - uses: actions/checkout@v7          # VERIFY IN T-00 — research sources disagreed
      - uses: actions/setup-dotnet@v6      # VERIFY IN T-00
        with:
          dotnet-version: '10.0.x'

      - name: Publish
        shell: powershell
        run: dotnet publish source/client/OlympiadQuizzer.Client.csproj -c Release -o publish-out

      - name: Rewrite base href, inject API URL, add Pages files
        shell: powershell
        env:
          RENDER_API_URL: ${{ secrets.RENDER_API_URL }}
        run: |
          $root = "publish-out/wwwroot"
          $utf8NoBom = New-Object System.Text.UTF8Encoding($false)

          # base href must end with a slash, or _framework/* resolves to the domain root
          $index = Join-Path $root "index.html"
          $html  = [System.IO.File]::ReadAllText($index)
          $html  = $html -replace '<base\s+href="[^"]*"\s*/?>', '<base href="/olympiad-quizzer-net/" />'
          [System.IO.File]::WriteAllText($index, $html, $utf8NoBom)

          # SPA deep-link fallback: GitHub Pages serves 404.html, Blazor's router takes over
          Copy-Item $index (Join-Path $root "404.html") -Force

          # _framework starts with an underscore — Jekyll would skip it
          [System.IO.File]::WriteAllText((Join-Path $root ".nojekyll"), "", $utf8NoBom)

          # WriteAllText with UTF8Encoding($false) — Set-Content -Encoding utf8 on
          # Windows PowerShell 5.1 emits a BOM, and a BOM here breaks JSON config parsing
          $cfg = '{ "ApiBaseUrl": "' + $env:RENDER_API_URL.TrimEnd('/') + '" }'
          [System.IO.File]::WriteAllText((Join-Path $root "appsettings.json"), $cfg, $utf8NoBom)

          Get-Content (Join-Path $root "appsettings.json")

      - uses: actions/upload-pages-artifact@v5    # VERIFY IN T-00 — reported as both v3 and v5
        with:
          path: publish-out/wwwroot

  deploy:
    needs: build
    runs-on: self-hosted
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - id: deployment
        uses: actions/deploy-pages@v5            # VERIFY IN T-00 — reported as both v4 and v5
```

Three traps this encodes:

1. **Trailing slash on `base href`.** `/olympiad-quizzer-net` without it makes the browser resolve `_framework/blazor.webassembly.js` against the domain root → 404 → blank page.
2. **No BOM.** Windows PowerShell 5.1's `Set-Content -Encoding utf8` writes a BOM; a BOM at the head of `appsettings.json` makes the WASM config load fail at startup with an opaque error.
3. **`source/shared/**` in the path filter.** Without it, a schema change in `Shared` deploys the backend but leaves a stale client compiled against the old model.

**Escape hatch** if T-00 found `bsdtar` and the PATH fix did not stick: change `runs-on: self-hosted` to `runs-on: ubuntu-latest` in **both** jobs (`setup-dotnet@v6` installs the .NET 10 SDK there, and the repo is public so the minutes are free). Then adjust the PowerShell step to `shell: pwsh` — the `[System.IO.File]` calls are portable as-is. Record the choice in the JOURNAL and in `assumptions.md` A-03.

**Acceptance**: push to `main` touching `source/client/**` produces a green run; `https://leafsoftwarepoland.github.io/olympiad-quizzer-net/` loads the app; DevTools shows zero 404s under `_framework/`.

---

## T-13 — `deploy-backend.yml`

`.github/workflows/deploy-backend.yml`. Manual only — ADR-020 keeps Render's Auto-Deploy off so a stray push can never burn free-tier instance hours.

```yaml
name: deploy-backend

on:
  workflow_dispatch:

jobs:
  deploy:
    runs-on: self-hosted
    steps:
      - name: Trigger Render deploy hook
        shell: powershell
        env:
          HOOK: ${{ secrets.RENDER_DEPLOY_HOOK }}
        run: |
          $r = Invoke-WebRequest -Uri $env:HOOK -Method POST -UseBasicParsing
          Write-Host "Deploy hook: $($r.StatusCode)"
          if ($r.StatusCode -ne 200) { exit 1 }

      - name: Wait for healthz
        shell: powershell
        run: |
          $url      = "https://olympiad-quizzer-net-api.onrender.com/healthz"
          $deadline = (Get-Date).AddMinutes(5)
          Start-Sleep -Seconds 30          # give Render time to start the new deploy
          while ((Get-Date) -lt $deadline) {
            try {
              $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 20
              if ($r.StatusCode -eq 200) { Write-Host "healthz OK: $($r.Content)"; exit 0 }
            } catch { Write-Host "waiting..." }
            Start-Sleep -Seconds 15
          }
          Write-Error "healthz did not return 200 within 5 minutes"
          exit 1
```

The `Start-Sleep -Seconds 30` before polling is not padding: the *previous* deploy is still serving 200 on `/healthz` for the first seconds after the hook fires, so polling immediately would report success against the old container.

Secrets used, by name only: `RENDER_DEPLOY_HOOK`, and `RENDER_API_URL` in T-12. Never echo either into logs.

**Acceptance**: `workflow_dispatch` run goes green; Render dashboard shows a new deploy triggered by the hook; `/healthz` answers `{"ok":true}` from the public URL.

---

## T-14 — Manual verification + record results

Run M1–M16 from `test-strategy.md`. Fill the results table at the bottom of `docs/specs/2026-08-08-olympiad-quizzer-poc-design.md` (that table is cited by ADR-006 and ADR-007 as deployment proof) and set the PASS/FAIL verdict.

Then:
- If M11 shows Render required a card or a card-verification hold → ADR-007's "no card" premise is broken. Do not silently accept it: raise it to the Architect as upstream feedback so ADR-007 gets amended and ADR-008 (Oracle Cloud) is reconsidered. See `assumptions.md` A-01.
- Update ADR-007's status line from `Accepted (test pending)` to the tested outcome, and ADR-006 likewise if anything surprised.

**Acceptance**: results table filled, verdict recorded, ADR-006/ADR-007 statuses updated.

---

## Out of scope — do not build

Real questions · timer / contest mode · SQLite / Dapper (ADR-004) · PWA / service worker (ADR-018) · user accounts (ADR-015) · image blocks (ADR-010) · OIJ Python/C++ toggle · `manifest.json` version indirection (ADR-009) · bUnit component tests · drag-and-drop ordering · retry/backoff policies · structured logging or metrics.
