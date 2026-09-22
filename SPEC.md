# ATM Product Specification

## Objective

Build a web-based ATM that demonstrates clear software design and architecture within a deliberately small scope.

## User and operating model

- One local user; authentication and authorization are out of scope.
- Exactly two accounts: **Checking** and **Savings**.
- Each account starts at **$1,000.00** on first database creation.
- Balances and transaction history persist across application restarts.

## Functional requirements

The user can:

1. View both account names and current balances.
2. Deposit a positive monetary amount into either account.
3. Withdraw a positive monetary amount from either account when sufficient funds exist.
4. Transfer a positive monetary amount from one account to the other when the source has sufficient funds.
5. View transaction history newest first, including time, type, amount, affected account or accounts, and resulting balance information sufficient to explain the ledger.

## Invariants and edge cases

- Money uses `decimal`; binary floating-point types are forbidden for balances or transaction amounts.
- Amounts must be greater than zero and have at most two fractional digits.
- An account balance may never become negative.
- A transfer must use different source and destination accounts.
- A transfer and its history records commit atomically or not at all.
- Failed validation or insufficient funds must not mutate a balance or append history.
- Duplicate form submissions must not be silently interpreted as two intentional transactions; the implementation must choose and document an idempotency or Post/Redirect/Get strategy.
- User-facing errors must be actionable and must not expose stack traces or database details.

### Resolved by the domain implementation

- Withdrawing or transferring an account's entire balance is allowed; the balance becomes exactly zero.
- Trailing fractional zeros are accepted (`1.100` is the same amount as `1.10`); a third significant fractional digit is rejected.
- Account identifiers are the stable, case-insensitive slugs `checking` and `savings`; display names are separate.
- An operation naming an account that does not exist is rejected without any state change.
- A concurrency conflict at commit time writes nothing and surfaces as a retryable error; the user is asked to review balances and try again.
- A transfer produces exactly one history record carrying both accounts and both post-transaction balances. Deposits record only the destination account and withdrawals only the source account, each with that account's post-transaction balance.

## Quality requirements

- Domain rules are independent of ASP.NET Core and Entity Framework Core.
- Application use cases orchestrate deposits, withdrawals, and transfers through abstractions.
- Infrastructure owns EF Core, SQLite, migrations, and transaction boundaries.
- Razor Pages remain thin adapters for input validation, use-case invocation, and presentation.
- Automated tests cover domain rules, application orchestration, persistence behavior, and the main HTTP path.
- The repository must build and test on macOS with the .NET 10 SDK.

## Explicitly out of scope

- Authentication, multiple users, cards, PINs, and sessions.
- Cash inventory, fees, interest, overdrafts, scheduled transactions, and external bank rails.
- Multiple currencies, foreign exchange, and fractional-cent accounting.
- Distributed services, queues, cloud infrastructure, and production deployment.
- A JavaScript SPA or public API unless a later ADR establishes a concrete need.

## Acceptance criteria

- A fresh setup produces exactly the two seeded accounts with the approved opening balances.
- Valid deposits, withdrawals, and transfers update balances and history correctly.
- Invalid amounts, same-account transfers, and overdrafts are rejected without state changes.
- Restarting the application preserves balances and history.
- `dotnet build Atm.sln` and `dotnet test Atm.sln` complete successfully.
- The README is sufficient for someone on macOS to run the project locally.
