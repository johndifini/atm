# ATM Engineering Handoff

This repository is a single-user web ATM coding exercise. The architecture scaffold is approved; feature work begins from [`SPEC.md`](SPEC.md) and [`PLAN.md`](PLAN.md), not from assumptions.

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
