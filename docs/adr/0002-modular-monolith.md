# ADR-0002: Modular monolith and inward dependencies

**Status:** Accepted — 2026-09-21

## Context

The system has a small scope but must demonstrate separation of concerns. One undifferentiated web project would obscure boundaries; microservices would introduce unjustified operational complexity.

## Decision

Use one solution and one web deployable split into Domain, Application, Infrastructure, and Web projects. Dependencies point inward: Application depends on Domain; Infrastructure implements Application ports; Web composes Application and Infrastructure.

## Consequences

- Business rules can be tested without web or database infrastructure.
- Persistence can change without rewriting use cases.
- The extra projects add modest ceremony, justified by the exercise's architecture focus.
- Cross-project abstractions are introduced only at real boundaries, not for every class.
