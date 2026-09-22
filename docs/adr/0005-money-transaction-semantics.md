# ADR-0005: Decimal money and atomic transaction semantics

**Status:** Accepted — 2026-09-21

## Context

Deposits, withdrawals, and transfers change financial state. Even in a coding exercise, rounding errors, partial transfers, or history that disagrees with balances would undermine the central domain.

## Decision

Represent money as `decimal` constrained to two fractional digits. Reject invalid amounts and overdrafts in the domain. Commit every balance mutation and its history atomically; a transfer debits, credits, and records its audit data within one database transaction. Use optimistic concurrency to prevent lost updates.

## Consequences

- Binary floating-point errors are excluded.
- Failed operations leave no partial state or misleading history.
- Infrastructure must map decimal precision and a concurrency token explicitly.
- Concurrency conflicts become a handled application outcome, not an unhandled exception or silent overwrite.
