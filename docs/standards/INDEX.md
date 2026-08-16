---
project: olympiad-quizzer-net
language: C#
framework: .NET 10 / Blazor WebAssembly / ASP.NET Core
test_framework: xUnit 2.9
---

# Coding Standards — index

**Agent instruction — read this before you read anything else.**

**This index is a map, not a substitute.** Read every file listed below, in full, before writing or reviewing a single line of code. Not the headings. Not this page. Not the files you guess are relevant. All of them. An agent that reads this index and starts writing code has not read the standards.

Every rule in these files is enforced at every pull request. A violation is a blocking finding, not a suggestion — **including the rules whose value is not obvious on first reading**. Several exist because a specific failure already happened in this repository, and the reasoning is stated inline where it does. Read the reasoning before deciding a rule is arbitrary.

Where these files and an ADR disagree, **these files win**. ADRs record what was decided and why; this is how it is enforced.

## Files

| File | Read it for |
|---|---|
| [projects-and-solution.md](projects-and-solution.md) | `.csproj` settings, the project naming rule, program entry points, what is and is not committed |
| [api.md](api.md) | Controllers, route shape and versioning, startup composition, error-handling boundary |
| [testing-tiers.md](testing-tiers.md) | L0–L3 and Integrity, which project a test belongs in, the mirror rule, what may be mocked |
| [testing-conventions.md](testing-conventions.md) | Test naming pattern, AAA structure, tier traits |
| [csharp.md](csharp.md) | Null safety, `var`, initializers, error handling, logging, method length, regions |
| [naming-and-comments.md](naming-and-comments.md) | Identifier naming and language, comment policy, JSON rules |
| [blazor.md](blazor.md) | Component and frontend rules |
| [security.md](security.md) | Secrets, browser storage, import surface, XSS. **Non-negotiable.** |
| [process.md](process.md) | PR and commit format, ADR content rules |

## Scope

These files cover conventions specific to this repository. Where they are silent, follow the
[.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
and the [Framework Design Guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/).

**Test levels are defined in [testing-tiers.md](testing-tiers.md)** and nowhere else. `docs/architecture-guide.md` restates them for orientation; where the two disagree, this directory wins.

---

# How these rules compose

**Read this section twice. It is the most important page in the directory.**

These rules are a **system**, not a checklist. Most of them constrain each other, and that is deliberate: it means a situation nobody wrote a rule for usually has exactly one answer already, waiting to be derived from the rules that do exist. Your job when you hit an unwritten case is to **derive** it, not to invent one.

## Worked example — nobody wrote "how to test middleware"

Suppose the app gains exception-handling middleware. No rule in this directory mentions middleware. Read the rules together and all four answers fall out:

| Tier | What it can reach | What it therefore owns |
|---|---|---|
| **L0** | Middleware is a class; every collaborator substituted. | Does it catch, log, and shape the response correctly. |
| **L1** | **Structurally cannot reach it.** L1 constructs the subject by hand with no pipeline, no DI, no middleware — so middleware is unreachable *by definition*. | The other half: that the controller and repository **bubble** the exception instead of swallowing it. If they swallow, middleware never fires and L2 passes for the wrong reason. |
| **L2** | Whole app, real registrations, real pipeline, over HTTP. | The only tier that can prove the chain end to end: exception thrown deep, surfaces as a shaped response. |
| **L3** | Browser. | Nothing here — this behaviour is invisible above the wire. |

And the mocking question answers itself. A healthy SQLite file will not throw an arbitrary exception on command. § Mocking says substitute only when the real thing cannot produce the scenario under test — so mocking the store **at L2** is not a violation, it is the rule *authorising* the only way to test this. **The same rule that normally forbids mocking is the rule that licenses it here.**

Four tiers, four distinct obligations, one derived design, from rules that never mention middleware.

Note the consequence, because it is the point: this derivation **created work that did not previously exist**. L2 was not in scope until a rule-derived obligation landed in it. That is the system functioning, not the system failing.

## The rules also find defects

The mirror rule (one production file, one test file) exists as a **complexity meter**, not only for navigation. Watch what it produced here twice:

- One repository class had **four** test files → the meter says four responsibilities → split it → the split *reveals* a persistence seam → a mockable seam makes **Infrastructure L0 possible**, which the tier rules always permitted but nothing had triggered. A file-naming rule surfaced an architecture improvement.
- One grader class had **six** test files → split → six static classes → statics cannot carry a contract → strategy plus dependency injection → each grader independently mockable → **the browser-side session and summary gain testable seams**. A file-naming rule ended at a dependency-injection decision. (Grading is client-side, so the callers that benefit are components, not controllers — an earlier version of this line said "controller L0 tests" and was wrong.)

If a rule here feels like bureaucracy, you are probably about to ignore a signal.

## Rules that enforce each other

Some pairs interlock so tightly that obeying one produces the other for free. Recognise these, because trying to satisfy them separately means you have misread at least one.

**No-magic-values enforces AAA.** [testing-conventions.md](testing-conventions.md) exempts a genuine one-liner from AAA labelling — nothing to arrange, nothing to separate. It also forbids unexplained literals, so every value gets a named variable. But those declarations **are** an Arrange section, which ends one-liner status and pulls the labels back in. The exemption therefore all but eliminates itself: what survives is parameterised tests, where the values are named parameters supplied from outside, and calls that genuinely take no arguments.

Neither rule mentions the other. Read alone, the exemption looks like a loophole; read together, it closes itself.

**The same chain constrains where values may hide.** Because the point is named values rather than a tidy body, the obligation follows the data into `MemberData` sources and test-case types. A test-case class with `Value1` and `Item2` satisfies the letter of the one-liner exemption while defeating the rule that licensed it.

## Derive, or ask — never invent

**Derive** when the intersection of existing rules yields exactly one answer. The middleware case above is a derivation: nothing was invented, four existing rules were read together.

**Ask the user** when:
- two rules genuinely conflict and no precedence resolves it;
- the answer needs a product or business decision (what the user sees, what is in scope, what an error message says);
- deriving would require inventing a rule rather than combining existing ones.

**Never invent a rule and write it into these files.** This is not hypothetical. The AAA rule in this repository once read *"add labels when a section body reaches 6+ lines"* — a threshold no one ever asked for, added by an agent during an early scaffold task and inherited by every agent afterwards. Every agent that "ignored" AAA labelling was in fact **complying** with an invented rule, for weeks, while the user believed they were being disobeyed. A silently invented rule is worse than a missing one, because a missing rule prompts a question and an invented rule prompts obedience.

If you believe a rule is wrong or missing: **stop and say so.** Do not fix it in passing.

## Breaking a rule with sense

Rules exist to produce value. When one genuinely cannot serve a real situation, the answer is a **considered, decided, recorded exception with the minimum possible violation** — not silent deviation, and not abandoning the rule.

An exception is legitimate when all six hold:

1. **Real need.** The rules genuinely cannot serve it. Not inconvenience, not effort, not deadline.
2. **Proactive.** Decided before the work, not discovered after it as a justification for what already happened.
3. **Minimal violation.** Deviate on the fewest axes possible; comply with everything else.
4. **Self-announcing.** The deviation is visible in a folder name, a project name, a tier name — not buried in a document nobody reads.
5. **Recorded with its reasoning**, so the next reader inherits the *why*, not just the *what*.
6. **It makes the rest of the codebase more compliant.** This is the sharp test.

Worked example — the data-integrity suites. They validate the **artefact** the repo produces, not code under test. No tier from L0 to L3 covers that, and the one-production-counterpart rule excludes them from every existing test project, because they have no counterpart at all. So:

- deviation is confined to **two axes**: a non-ring folder (`source/Solution/`) and a tier outside L0–L3 (`Integrity`);
- everything else complies — naming form, project structure, tier trait, mirror rule where it applies;
- both names announce the deviation on sight;
- and moving those suites out **lightens `Infrastructure.L1` and `App.API.L1`**, giving each a single clean subject.

That last point is what makes it load-bearing rather than an escape hatch. **An exception that improves compliance elsewhere is sound. One that only relieves local pressure is an escape hatch wearing a justification** — and should be refused.
