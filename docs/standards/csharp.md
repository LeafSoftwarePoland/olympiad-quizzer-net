# C# style

## Null safety

`<Nullable>disable</Nullable>` in all `.csproj` files. Reference types are non-nullable by
convention and trust — not by compiler enforcement.

Why, since this fights the ecosystem default: the question schema has genuinely optional fields
throughout, so annotating honestly makes almost every property nullable, which carries no
information and turns warnings into background noise. Noise that trains a reader to ignore the
warning channel is worse than no warnings. With annotations off, `TreatWarningsAsErrors` becomes
usable. **Revisit if the schema ever becomes mostly-required.**

- No `?` on reference types. No `!` null-forgiving operator. No `#nullable enable` pragmas.
- `?` on **value** types stays — `int? Year` is correct and unrelated.
- Null checks at system boundaries only: API query parameters, JSON deserialisation results,
  browser-storage reads, configuration values. Internal code trusts its own invariants.
- Guard clauses over nested `if`. Return early.
- Collections a caller will iterate are initialised at declaration
  (`List<string> Category { get; set; } = new();`) so no downstream null guard is needed.

## Types and `var`

Prefer `var` where the type is obvious from context — the right-hand side makes the type
unambiguous without looking elsewhere:

```csharp
// Yes — type is explicit on the right-hand side.
var repository = new SqliteQuestionRepository(connection, shuffler, logger);
var builder = new StringBuilder(value.Length);
var port = Environment.GetEnvironmentVariable("PORT");

// No — type is not evident from the method name alone.
GradeResult result = Grade(question, answer);

// No — numeric literal where the exact type matters.
int matched = 0;
bool positional = question.Type == QuestionType.TrueFalse;
```

Keep the explicit type when:

- The literal is numeric or `bool` and the exact type matters (`int x = 0`, `bool flag = true`).
- The right-hand side is a method call and the return type is not self-evident from the name.
- The declaration widens to an interface deliberately (`IEnumerable<Question> candidates = ...`).

Never `var` for the result of a LINQ chain where the concrete type is opaque.
Target-typed `new()` requires the explicit type on the left — do not pair it with `var`.

## Initializers and expressions

- Use primary constructors where applicable.
- Prefer simplified initializers:
  - `new()` (target-typed new) when the type is on the left.
  - `[]` for an empty collection when the element type is evident from context.
  - `[.. existingCollection]` for spread / copy.
- Prefer collection expressions over `new List<T> { }` or `new T[] { }`.

```csharp
// Preferred
List<string> tags = [];
List<Question> copy = [.. original];

// Avoid
List<string> tags = new List<string>();
Question[] arr = new Question[0];
```

## Error handling

- **No empty `catch` blocks.** Extremely rare exceptions exist — e.g. a third-party endpoint that
  always performs its side-effect regardless of its error response, where propagating would crash
  the app for no benefit. Requires a comment explaining why.
- **No `catch (Exception)` that swallows.** Catch the specific type you can handle.
- General `catch (Exception e)` is allowed **only** when you catch, log and rethrow:

  ```csharp
  catch (Exception e)
  {
      _logger.LogError(e, "...");
      throw; // preserves the original stack trace
  }
  ```

  Use `throw;`, never `throw e;` — `throw e;` replaces the stack trace with the current call site,
  which destroys the only evidence of where the failure actually came from.
- A `catch` either handles, or logs and rethrows, or translates to a domain-meaningful result.
  Doing none of the three is a defect.
- **Fail fast at startup.** A missing or unreadable question bank throws and the process does not
  start. An API that boots and then serves an empty array forever is worse than one that refuses
  to boot, because a health check catches the second and a student catches the first.
- Untrusted input is validated at the boundary and **discarded** on failure, never repaired.
- Do not use exceptions for expected outcomes. "No questions matched these filters" is an empty
  collection returned with HTTP 200 — not an exception, not a 404, and not a 204, because the
  client deserialises an array and a bodyless response breaks it.

## Logging

- `ILogger<T>` only. No `Console.WriteLine`, no external sink.
- **Structured templates with named placeholders.** Never interpolate into the message:

```csharp
// Yes
_logger.LogInformation("Question query served: matched={MatchedCount} limit={Limit}", n, limit);

// No — destroys the structure and defeats log search
_logger.LogInformation($"Question query served: matched={n} limit={limit}");
```

- Levels: `Information` for lifecycle and served requests; `Warning` for a recoverable oddity
  (empty result, unknown tag, rejected parameter); `Error` for an unhandled failure. No `Debug`
  or `Trace` in committed code.
- Never log question text, answers, or a whole payload. The host keeps 7 days of logs; one
  30-question dump makes that window useless.

## Long method decomposition

Extract numbered private methods with a leading step comment when a method exceeds ~20 lines:

```csharp
private void BuildQuiz()
{
    // 1. Load questions
    // 2. Apply filters
    // 3. Shuffle
}
```

Prefer clear method names over inline comments. File length is a smell above ~300 lines; method
length above ~40. Not hard limits — prompts to look.

**No regions.**
