# ADR-0004: xUnit unit and integration testing

**Status:** Accepted — 2026-09-21

## Context

The project prioritizes correctness, error handling, and separation of concerns. Tests need to cover fast domain rules as well as real relational and HTTP behavior.

## Decision

Use xUnit. Keep separate Domain and Application unit-test projects, plus an IntegrationTests project using `WebApplicationFactory` and isolated SQLite databases. Use built-in xUnit assertions rather than adding an assertion library.

## Consequences

- Test boundaries mirror production boundaries.
- SQLite integration tests expose relational behavior that an in-memory fake would miss.
- Separate projects add small solution overhead but make intent and execution scope explicit.
- End-to-end browser automation is deferred; add it only if UI behavior grows beyond the small server-rendered surface.
