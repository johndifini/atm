# ADR-0001: .NET 10 and ASP.NET Core Razor Pages

**Status:** Accepted — 2026-09-21

## Context

The project targets .NET/C# and a web interface. It is single-user and locally run, and its priorities are organization, separation of concerns, error handling, and technical judgment. A separate JavaScript application would add a second build system and API surface without serving those priorities.

## Decision

Use .NET 10 and ASP.NET Core Razor Pages with server-rendered HTML and minimal JavaScript.

## Consequences

- The complete system builds and runs cross-platform on macOS.
- One deployable and one primary language keep the exercise focused.
- Razor Pages must stay a presentation adapter; domain behavior remains in inward projects.
- Rich client-side state and a public API are deferred unless later requirements justify them.
