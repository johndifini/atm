# ADR-0003: SQLite persistence through Entity Framework Core

**Status:** Accepted — 2026-09-21

## Context

Balances and history must survive restarts. The workload is local, single-user, and relational. An in-memory store would not meet persistence expectations; a server database would add setup and operational work without improving the exercise.

## Decision

Use SQLite through Entity Framework Core 10. Store the database locally, manage schema through migrations, and seed Checking and Savings at $1,000.00 each only when the database is first created.

## Consequences

- Anyone running it locally gets durable behavior with near-zero setup.
- EF Core supplies migrations, transactions, and optimistic-concurrency support.
- SQLite's concurrency and scale limits are accepted for this bounded workload.
- Integration tests must exercise SQLite rather than substituting EF Core's non-relational in-memory provider.
