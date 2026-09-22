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

- `atm-deck.pptx` — the seven-slide deck below, generated from real screenshots with the application's tokens (navy headings, one green accent, white cards on the pale-gray canvas). It is a build output: change `deck/build.js` and regenerate rather than editing the file by hand.
- `deck/build.js` — the generator, with `deck/package.json` pinning its only tooling dependency. `pptxgenjs` is not part of `Atm.sln` and adds nothing to the application at runtime.

```bash
cd design/deck && npm install && node build.js
```

Pass an output path (`node build.js /tmp/preview.pptx`) to preview a change without overwriting the committed deck.
- `screenshots/` — captures of the finished application taken with headless Chrome against a scratch database: `dashboard.png` (populated history), `receipt.png` (one-time success confirmation after Post/Redirect/Get), `overdraft.png` (inline insufficient-funds error with the typed value preserved), and `phone.png` (380px reflow with stacked cards and records).

## Seven-slide deck

1. **ATM Coding Exercise** — objective, scope, final stack, and finished dashboard screenshot.
2. **How I Framed the Problem** — requirements, non-goals, quality attributes, and why clarity drove the solution.
3. **Architecture at a Glance** — Razor Pages → application use cases → domain → EF Core/SQLite; show dependency direction.
4. **Key Decisions** — compact matrix for Razor Pages, modular monolith, SQLite, decimal money, and single-user scope; link to ADRs.
5. **Correctness and Failure Handling** — invariants, overdraft handling, validation, atomic transfers, concurrency, and history integrity.
6. **Testing and Delivery** — domain, application, SQLite integration, and HTTP test layers; macOS setup and handoff artifacts.
7. **Tradeoffs and More-Compute Roadmap** — deliberate omissions; then idempotency, richer auditing, authentication, accessibility automation, observability, load tests, deployment, and browser coverage.

Use a white or pale-gray canvas, navy headings, one green accent, sparse diagrams, and real screenshots. Keep one thesis and roughly three supporting points per slide.
