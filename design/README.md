# ATM Design and Presentation Brief

This direction was produced through the workspace's `jony-vibe` design consultation. Keep design work for this repository under `design/`.

## Interface direction

Build a quiet banking utility: trustworthy, compact, and subordinate to the architecture demonstration.

- One responsive dashboard, approximately 960px maximum width.
- Two equal account cards at the top, with prominent tabular-numeral balances.
- Deposit, Withdraw, and Transfer as a segmented control or tabs; show one compact form at a time.
- One transaction-history table below, newest first; collapse to stacked records on narrow screens.
- Neutral system font at a 16px base, 8px spacing scale, 8–12px radii, and restrained borders or shadows.
- No marketing shell, illustration, logo exercise, gradients, or animation.

Suggested tokens:

| Token | Value |
|---|---|
| Page | `#F6F8FA` |
| Surface | `#FFFFFF` |
| Text | `#17212B` |
| Muted | `#667085` |
| Primary navy | `#174A7E` |
| Success | `#18794E` |
| Error | `#B42318` |
| Border | `#D9E0E7` |

Meet WCAG AA contrast and never communicate success or failure through color alone. Validate inline, preserve values after recoverable errors, disable submission while processing, and show clear success confirmation. Confirm withdrawals and transfers, but not deposits.

## Deliverables in this directory

- `atm-deck.pptx` — the seven-slide deck below, generated from real screenshots using the approved Jony Vibe presentation system: a `#121212` canvas, `#F5F5F5` type, and restrained `#00F604`, `#F67F00`, or `#0077F6` emphasis. Each slide has one dominant accent and targets no more than 36 authored words; detail belongs in speaker notes. It is a build output: change `deck/build.js` and regenerate rather than editing the file by hand.
- `deck/build.js` — the generator, with `deck/package.json` pinning its only tooling dependency. `pptxgenjs` is not part of `Atm.sln` and adds nothing to the application at runtime.

```bash
cd design/deck && npm install && node build.js
```

Pass an output path (`node build.js /tmp/preview.pptx`) to preview a change without overwriting the committed deck.
- `screenshots/` — captures of the finished application taken with headless Chrome against a scratch database: `dashboard.png` (populated history), `receipt.png` (one-time success confirmation after Post/Redirect/Get), `overdraft.png` (inline insufficient-funds error with the typed value preserved), and `phone.png` (380px reflow with stacked cards and records).

## Seven-slide deck

1. **ATM Coding Exercise** — objective, scope, final stack, and the finished dashboard as the visual anchor.
2. **Problem Framing** — the required product, deliberate boundaries, and clarity as the quality bar.
3. **Architecture and Dependency Direction** — Razor Pages to application use cases to domain, with EF Core/SQLite implementing application ports.
4. **Decisions and Their Costs** — a readable ledger for Razor Pages, modular monolith, SQLite, decimal money, and single-user scope.
5. **Financial Operations Fail Without Partial State** — invariants, atomic history, concurrency, and a real rejected-overdraft state.
6. **143 Tests Cover Each System Boundary** — domain, application, SQLite integration, HTTP behavior, a success receipt, and macOS commands.
7. **Tradeoffs and Roadmap** — deliberate omissions, an ordered roadmap, and the real 380 px layout.

Use a solid charcoal canvas, soft-white type, one dominant accent per slide, sparse diagrams, and each real screenshot once. Keep one focal element and no more than three supporting groups per slide. Aim for 36 authored words or fewer—title, labels, and body copy included—and move supporting detail to speaker notes before reducing type size. Use whitespace and hairlines instead of reproducing the application's card grid in the presentation.
