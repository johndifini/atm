# ADR-0006: Quiet banking UI and seven-slide narrative

**Status:** Accepted — 2026-09-21

## Context

UX is not the primary focus, but the interface and accompanying presentation must communicate trust, clarity, and deliberate choices. The workspace routes visual judgment through `jony-vibe`.

## Decision

Use the restrained, tokenized interface and seven-slide presentation direction recorded in `design/README.md`. The interface is one responsive dashboard with two account cards, a compact operation selector, one form at a time, and transaction history below. Avoid decorative branding, animation, and marketing chrome.

## Consequences

- Visual effort supports rather than distracts from the architecture demonstration.
- Shared tokens and components prevent one-off styling.
- Accessibility, inline validation, processing state, and non-color feedback remain required.
- The final deck can reuse the application's visual system and real screenshots.
