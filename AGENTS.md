# ATM Engineering Handoff

This repository is a single-user web ATM coding exercise. The architecture scaffold is approved; feature work begins from [`SPEC.md`](SPEC.md) and [`PLAN.md`](PLAN.md), not from assumptions.

## Directory Structure

- `../` → The Borg workspace's `repos/` directory. This repo is an independent git repo inside it, bootstrapped by the workspace's Architetto agent.
- `../../` → The Borg workspace root, holding the workspace agents (including the `jony-vibe/` design agent) and the shared `cerebruh/` knowledge base. Neither parent exists in a standalone clone.
- [`SPEC.md`](SPEC.md) → Authoritative behavior. Keep it truthful when implementation settles an edge case.
- [`PLAN.md`](PLAN.md) → The approved build order.
- [`BACKLOG.md`](BACKLOG.md) → Deferred work and known gaps. Every item carries its own approval gate. Re-read it from disk immediately before writing it, and make the narrowest edit that does the job.
- [`README.md`](README.md) → Public overview, run instructions, and the deck callout.
- [`docs/adr/`](docs/adr/README.md) → Foundational decisions, ADR-0001 to ADR-0006.
- [`docs/accessibility-review.md`](docs/accessibility-review.md) → WCAG 2.2 AA review of the dashboard and its findings.
- `src/` → The four production projects (see Architecture contract). `tests/` → Domain, Application, and integration test projects.
- [`design/`](design/README.md) → The approved narrative and visual direction (`README.md`); the built deck (`atm-deck.pptx`, `atm-deck.pdf`); its generator (`deck/build.js`); and the app captures it uses (`screenshots/`).

## Architecture contract

- `src/Atm.Domain` contains framework-free business rules.
- `src/Atm.Application` contains use cases and ports; it depends only on Domain.
- `src/Atm.Infrastructure` contains EF Core/SQLite adapters and depends inward.
- `src/Atm.Web` is the Razor Pages composition root and presentation adapter.
- Foundational decisions live in [`docs/adr/`](docs/adr/README.md). Add an ADR before changing a foundation.

Business logic does not belong in PageModels, EF Core configurations, or JavaScript. Keep request models separate from domain entities, and never expose `IQueryable` outside Infrastructure.

## Financial invariants

- Use `decimal` for money and enforce no more than two fractional digits.
- Reject zero or negative amounts, overdrafts, and same-account transfers.
- Never permit a negative balance.
- Balance mutations and their history records are atomic.
- Store timestamps in UTC and format them only at the presentation boundary.
- Do not log sensitive form values or raw exception details.

## Build, run, and test

```bash
dotnet restore Atm.sln
dotnet build Atm.sln --no-restore
dotnet test Atm.sln --no-build
dotnet run --project src/Atm.Web
```

Run the affected test project while iterating, then run the whole solution before handoff. Integration tests must use an isolated temporary SQLite database and must not depend on execution order.

## Working rules

Always:

- Update tests with behavior.
- Keep the specification truthful when implementation resolves an unspecified edge case.
- Use EF Core migrations for schema changes.
- Preserve the dependency direction recorded in ADR-0002.
- Keep the UI aligned with [`design/README.md`](design/README.md).

Ask before:

- Adding a production dependency, external service, authentication, or another process/deployable.
- Changing the account count, opening balances, persistence engine, or public behavior.
- Relaxing a financial invariant or transaction boundary.

Never:

- Commit secrets, local databases, build output, or `.env` files.
- Use `double` or `float` for monetary values.
- replace migrations with ad hoc schema edits.
- Add distributed-system machinery to this focused exercise without an approved ADR.

## Presentation artifact

The finished project includes a slide deck explaining approach, decisions, tradeoffs, and what more compute would change. The approved narrative and visual direction are in [`design/README.md`](design/README.md); add screenshots only after the implementation is real.

## Workspace integration

The machine-local `.claude/commands` symlink inherits workspace commands and is intentionally ignored. `CLAUDE.md` is a compatibility wrapper and must contain exactly `@AGENTS.md`.

The bindings below restate workspace rules. Those rules live outside this repo and do not load here, so these lines are not duplicates: do not delete them.

- **Design routing** (workspace `AGENTS.md` → Design, taste, and UI). Visual, brand, layout, and presentation decisions go through the workspace's `../../jony-vibe/` design agent. That covers the slide deck and its screenshots, not just the web UI. A recorded consultation covers only what it covered, so a new artifact type needs a new consultation. A harness design skill's own visual rules do not replace it.
- **Closing sections** (workspace `AGENTS.md` → Communication style). End a substantial, multi-part, or decision-heavy response with a short `## Recap`. When a useful follow-up exists, follow the recap with this exact shape:

  ````markdown
  ## Suggested Next Prompt

  ```text
  <one directly reusable prompt>
  ```
  ````

  Write the prompt as one line of plain text, with no Markdown or backticks. Put nothing else inside the fence and nothing around it. Omit the section when no meaningful next step exists.
- **Deck QA in PowerPoint** (`../../.claude/rules/pptx-qa-uses-powerpoint.md`). Render and check the deck in Microsoft PowerPoint, never Keynote or Quick Look. Commit `28eab53` passed Keynote QA and still made PowerPoint offer a repair.
  - Export a copy to PDF via `osascript`, unsandboxed, with up to ten minutes allowed. Then rasterize the pages.
  - Close only the presentation reference captured at `open`. Never close by name, because that once closed the user's own deck.
  - A repair prompt is a QA failure: fix the generator.
  - If PowerPoint cannot run, report the visual check as blocked.
- **Deck hand edits** (`../../.claude/rules/generated-deck-hand-edits.md`). The user edits the built deck directly, so a rebuild in place can silently discard their work. Before any rebuild:
  1. Back up `design/atm-deck.pptx`.
  2. Build to a scratch path.
  3. Diff the two slide by slide: text and notes, shape geometry, and run properties, including typeface and letter spacing.
  4. Port every difference into `design/deck/build.js`, then rebuild and diff again until only re-save noise remains. Only then replace the original.

  Do the backup, check, and overwrite in one `set -euo pipefail` script with absolute paths. If an edit cannot be ported faithfully, stop and ask.
