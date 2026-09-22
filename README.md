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

The SQLite database will be a local runtime artifact and is ignored by Git. The implementation should seed two accounts named **Checking** and **Savings**, each with an opening balance of **$1,000.00**.

## Read first

1. [`SPEC.md`](SPEC.md) — behavior, acceptance criteria, and explicit exclusions.
2. [`PLAN.md`](PLAN.md) — project boundaries, dependency direction, and implementation sequence.
3. [`AGENTS.md`](AGENTS.md) — exact working rules for humans and agents.
4. [`docs/adr/`](docs/adr/README.md) — why each foundation was selected.
5. [`design/README.md`](design/README.md) — UI direction and the presentation outline.

## Status

Phase 1 of [`PLAN.md`](PLAN.md) is complete: the framework-free domain model (`Money`, `Account`, `Transaction`, domain errors) and its unit tests. Application use cases, persistence, and the web UI are not yet implemented; no ATM feature should be inferred to exist until its acceptance tests and implementation are committed together.
