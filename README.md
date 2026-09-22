# ATM

A focused, single-user web ATM exercise for managing two accounts: deposit, withdraw, transfer, inspect balances, and review transaction history.

This repository currently contains the approved **architecture scaffold**. Feature implementation is intentionally left to the downstream engineer; the durable requirements and decisions are captured in [`SPEC.md`](SPEC.md), [`PLAN.md`](PLAN.md), and [`docs/adr/`](docs/adr/README.md).

## Stack

- .NET 10 / C#
- ASP.NET Core Razor Pages
- Entity Framework Core with SQLite
- xUnit unit and integration tests
- Modular monolith with Domain, Application, Infrastructure, and Web projects

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Setup

```bash
dotnet restore Atm.sln
dotnet build Atm.sln --no-restore
dotnet test Atm.sln --no-build
dotnet run --project src/Atm.Web
```

The SQLite database is created as `src/Atm.Web/atm.db` on first start (configurable through the `ConnectionStrings:Atm` setting) and is ignored by Git. Startup applies migrations and, when the database is empty, seeds two accounts named **Checking** and **Savings**, each with an opening balance of **$1,000.00**.

Schema changes use the repo-local `dotnet-ef` tool (`dotnet tool restore` installs it):

```bash
dotnet ef migrations add <Name> --project src/Atm.Infrastructure --output-dir Persistence/Migrations
```

## Read first

1. [`SPEC.md`](SPEC.md) — behavior, acceptance criteria, and explicit exclusions.
2. [`PLAN.md`](PLAN.md) — project boundaries, dependency direction, and implementation sequence.
3. [`AGENTS.md`](AGENTS.md) — exact working rules for humans and agents.
4. [`docs/adr/`](docs/adr/README.md) — why each foundation was selected.
5. [`design/README.md`](design/README.md) — UI direction and the presentation outline.

## Status

Phases 1 to 3 of [`PLAN.md`](PLAN.md) are complete: the framework-free domain model, the application ports and use cases, and SQLite persistence with its first migration, startup seeding, and integration tests. The web UI is not yet implemented; no ATM feature should be inferred to exist until its acceptance tests and implementation are committed together.
