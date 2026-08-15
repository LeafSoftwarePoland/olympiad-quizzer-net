# Naming, identifier language, comments and JSON

## Identifier language

C# code, JSON keys, CSS classes, HTML attributes, test names, file names, branch names, commit
messages, ADRs: **English**.

User-facing UI (labels, buttons, errors, page titles), question text, explanations, and image
alt text: **Polish**.

**Route segments split by audience, and this is the one place it is not obvious:**

| Route | Language | Why |
|---|---|---|
| Client routes — the path a student reads in the address bar | **Polish** | user-facing text |
| API routes — `/v1/questions`, `/v1/filters` | **English** | read by a program, and a frozen wire contract |
| API error payloads | **machine codes**, never prose | read by a program; the client maps each code to Polish |

They are consistent with the audience rule, not with each other. Do not "make them match".

Error codes are constants in the Domain project, so the API emits them and the client maps them
against the same definitions — a mismatch is a compile error rather than a mystery on screen. See
[api.md](api.md) § Error handling.

Tag identifiers (`category[]`, `algorithms[]` values): **Polish snake_case, Latin letters only,
diacritics dropped** (ą→a, ę→e, ó→o, ś→s, ł→l, ź/ż→z, ć→c, ń→n) — `sledzenie_kodu`, `zlozonosc`.
Justified because tags are Polish domain concepts shown directly in the UI, and because they are
data values rather than code identifiers. Vocabulary lives in `docs/tags.md`.

## Naming

| Thing | Convention | Example |
|---|---|---|
| Types, methods, properties, constants, enum members | PascalCase | `QuestionQuery`, `MaxLimit` |
| Locals, parameters | camelCase | `matchedCount`, `cancellationToken` |
| Private fields | `_camelCase` | `_shuffler` |
| File-scoped private constants | `_camelCase` | `_corsPolicyName` |
| Interfaces | `I` + PascalCase | `IQuestionRepository` |
| Acronyms | two letters upper, three-plus PascalCase | `IO`, `ID`, but `Api`, `Json`, `Html`, `Url` |
| `.cs` / `.razor` files | match the single type they contain | one public type per file |
| Folders | PascalCase, matching the namespace segment | `Features/Quiz/Components/` |
| Docs, markdown, static assets, workflows, CSS | kebab-case | `testing-tiers.md`, `deploy-frontend.yml` |
| JSON keys | camelCase | `correctAnswer`, `sourceRaw` |
| Git branches | kebab-case, English, `type/` prefix | `feature/server-side-filtering` |
| Browser-storage keys | `oqn.<area>.v<n>` | `oqn.session.v1` |

`sourceRaw`, not `source_raw` — camelCase wins for JSON keys.

### Using directives

Do not use fully-qualified type names when a `using` directive covers it. Add the `using` and
shorten the reference. Fully-qualified names are load-bearing only when two namespaces export the
same simple name and a `using` alias would be less clear than the qualification.

## Comment policy

### Banned — PR-blocker

- ADR references in code: `// See ADR-123`, `// per ADR-456`
- Issue/PR numbers in code: `// #13`, `// issue #36`
- Task references: `// added in task-12`, `// TODO task-15`
- Foreign repo names in comments
- `// TODO` / `// FIXME` / `// HACK` not cleaned up before commit
- Commented-out code. Git remembers.
- XML doc comments. They rot, must be maintained, and add nothing for internal code.
- **Comment banners used as section dividers** — ASCII rules, box-drawing lines, `=====` headers.
  C# has `#region`; see [csharp.md](csharp.md) § Regions.

### Naming over comments

Code is a technical document. Name identifiers well — `sqliteDatabase` not `db`;
`matchedQuestionCount` not `count`. Accept longer names; reject vague ones. A well-named
identifier removes the need for a comment. When in doubt: rename, don't comment.

### Comment only the non-obvious WHY

Comment when there is a hidden constraint, a subtle invariant, a specific bug workaround, or
non-obvious API behaviour that code cannot express. Keep comments brief — one line is usually
enough. No essays.

If a comment could be deleted and replaced by reading the method name, delete it.

```csharp
// Bad — restates the next line.
// Set the limit to the default
limit = DefaultLimit;

// Good — states what the code cannot.
// Shuffle before capping: capping first would make the result deterministic by bank order.

// Good — pins a load-bearing constraint against a well-meaning cleanup.
// Compression is disabled to work around a .NET 10 WASM asset-fingerprinting defect on
// static hosting. Do not remove.
```

### Allowed

- Numbered orchestrator steps `// 1. ...`, `// 2. ...` — a spec for orchestration logic
- Interop / P-Invoke / JS-interop rationale
- Non-obvious serializer or framework configuration
- Non-obvious PowerShell in workflows — e.g. why a file is written with an explicit no-BOM UTF-8
  encoder rather than the shell's default cmdlet

## JSON

- One shared serializer options instance, owned by the Domain. Never construct a second one at a
  call site — both ends of the wire must use the same one, or camelCase gets configured on one
  side only.
- Keys are camelCase via the camelCase naming policy.
- **Never hand-write `[JsonPropertyName]`.** If a C# property name and its JSON key differ by more
  than casing, the C# name is wrong — rename the property.
- `[JsonConverter]` on a member is allowed where a shape genuinely varies. That is not the same
  thing as renaming a key.
- Enums serialise as camelCase strings, never as integers.
- **Read JSON files as text, not as a raw byte stream.** Reading as text strips a UTF-8 BOM;
  reading as bytes does not, and the resulting deserialisation error never mentions the BOM.
- **Write JSON files as UTF-8 without BOM.** In PowerShell: `New-Object System.Text.UTF8Encoding($false)`.
  A BOM in an `appsettings.json` fails startup in Production with an error that never mentions
  the BOM.
- Question text is Unicode by design — Polish diacritics, mathematical italics (𝑥), subscripts
  (₁₆), operators, middle dot. **Never strip, escape or "clean" it.** The relaxed JSON escaping
  this repo uses is safe **only** because rendered text never reaches a raw-HTML sink — see
  [security.md](security.md). The two decisions are coupled and must not be separated.
