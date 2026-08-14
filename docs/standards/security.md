# Security rules

Non-negotiable. None of these four is a preference, and none may be relaxed by a pull request —
only by an ADR that addresses the attack surface directly.

## 1. Secrets

**Never write a secret value in any file** — code, doc, comment, config, test fixture, commit
message, log line.

For every secret, record: its name, its purpose, where it lives (repo secret / host dashboard /
runner machine), and how to rotate it. **Never the value.**

**Never write a full deploy hook URL.** A deploy hook is an unauthenticated trigger, so the URL
*is* the credential — and a partially redacted one still leaks the rest.

In code, secrets come from the environment or from injected configuration, with **no literal
fallback and no plausible-looking placeholder**. A placeholder that looks real is worse than no
value, because it survives review.

## 2. Browser-storage validation

**Everything read from browser storage must be sanitized and validated before use.**

- Parse inside a `catch` for the specific parse exception.
- Validate the result against an explicit predicate: schema version, non-empty collections,
  consistent counts, in-range indices, timestamps not in the future, sane limits, known enum
  values.
- On any failure, **discard and clear the key**, and return the user to a safe screen.
- **Never repair, never partially trust, never default a missing field.**
- Keys are version-suffixed, so a schema change is a discard, not a migration.

The user can edit this storage and that is accepted — this is a self-practice tool with no stakes,
and the worst outcome for someone who edits it is that they cheat themselves. What is **not**
accepted is a tampered value putting the app into a broken or exploitable state.

## 3. No settings import/export

Deliberate design decision, not a missing feature. **No settings export, no settings import, no
state import, no restore-from-file, no share link carrying state.**

Importing user-provided JSON is an attack surface — arbitrary shapes, sizes and nesting depth fed
into the one place the app trusts its own data — and it buys nothing for a tool with no accounts
and no sync.

Reject any feature request that asks for it and point at this rule. Reopening it requires an ADR
that addresses the attack surface, not a pull request that adds a file picker.

## 4. No XSS from question content

Question text, option text, explanation text and image alt text render as **text**, never as raw
HTML.

- Do not use `MarkupString` unless the content is explicitly sanitized first — and on question-bank
  or browser-storage content, **never**.
- Code blocks render inside `<pre><code>` as text.
- If syntax highlighting is ever added it must work on a parsed token model, never by building
  HTML from the question string.

This rule is also the **precondition** for the relaxed JSON escaping used on the wire. The two
decisions are coupled: relax the rendering rule and the escaping decision becomes unsafe
retroactively. They must not be separated.
