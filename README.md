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

Run these commands from the repository root. They work in PowerShell and Command Prompt on Windows, as well as in macOS and Linux shells:

```bash
dotnet restore Atm.sln
dotnet build Atm.sln --no-restore
dotnet run --project src/Atm.Web
```

The SQLite database is created as `src/Atm.Web/atm.db` on first start (configurable through the `ConnectionStrings:Atm` setting) and is ignored by Git. Startup applies migrations and, when the database is empty, seeds two accounts named **Checking** and **Savings**, each with an opening balance of **$1,000.00**.

Schema changes use the repo-local `dotnet-ef` tool (`dotnet tool restore` installs it):

```bash
dotnet ef migrations add YourMigrationName --project src/Atm.Infrastructure --output-dir Persistence/Migrations
```

Replace `YourMigrationName` with a name for the new migration. This command also works in PowerShell and Command Prompt.

## Verify

After building the solution, run all test projects with:

```bash
dotnet test Atm.sln --no-build
```

`--no-build` uses the binaries produced by the preceding build command. Omit it to build before testing.

## Read first

1. [`SPEC.md`](SPEC.md) — behavior, acceptance criteria, and explicit exclusions.
2. [`PLAN.md`](PLAN.md) — project boundaries, dependency direction, and implementation sequence.
3. [`AGENTS.md`](AGENTS.md) — exact working rules for humans and agents.
4. [`docs/adr/`](docs/adr/README.md) — why each foundation was selected.
5. [`design/README.md`](design/README.md) — UI direction and the presentation outline.

## Status

All six phases of [`PLAN.md`](PLAN.md) are complete: the framework-free domain model, the application ports and use cases, SQLite persistence with its first migration and startup seeding, the Razor Pages dashboard with Post/Redirect/Get and inline error mapping, HTTP tests over the real host, the accessibility review in [`docs/accessibility-review.md`](docs/accessibility-review.md), and the presentation deck with real screenshots under [`design/`](design/README.md).
