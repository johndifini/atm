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
- One deliberate exception: the Konami code (↑ ↑ ↓ ↓ ← → ← → B A, outside form fields) toggles a Matrix theme with digital rain for the rest of the tab's session. It is opt-in and presentation-only, keeps AA contrast, falls back to a static theme under reduced motion, and exits with Esc, the blue pill, or the code again.

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

- `atm-deck.pptx` — the seven-slide deck below, generated from real screenshots using the approved Jony Vibe presentation system: a `#121212` canvas, `#F5F5F5` type, and fixed accent roles: `#00F604` green is primary on every slide, `#F67F00` orange is secondary, and `#0077F6` blue is tertiary. Accents do not rotate between slides. Each slide targets no more than 36 authored words; detail belongs in speaker notes. It is a build output: change `deck/build.js` and regenerate rather than editing the file by hand.
- `deck/build.js` — the generator, with `deck/package.json` pinning its only tooling dependency. `pptxgenjs` is not part of `Atm.sln` and adds nothing to the application at runtime. The package override keeps its transitive `image-size` dependency above the vulnerable `<= 2.0.2` range reported in [GHSA-w3rx-r6r6-pgpr](https://github.com/advisories/GHSA-w3rx-r6r6-pgpr) and [GHSA-5p2g-fcmc-qvqq](https://github.com/advisories/GHSA-5p2g-fcmc-qvqq).

```bash
cd design/deck && npm install && node build.js
```

Pass an output path (`node build.js /tmp/preview.pptx`) to preview a change without overwriting the committed deck.

Visual QA: export each rebuild to PDF with Microsoft PowerPoint, the app the deck is for, and inspect every slide. Keynote and Quick Look are not a pass/fail check, because they open files PowerPoint repairs; `a5f576f` fixed table cells that passed Keynote and made PowerPoint offer repair. The export script and its pitfalls are in The Borg's `.claude/rules/pptx-qa-uses-powerpoint.md`.
- `screenshots/` — captures of the finished application taken with headless Chrome against a scratch database: `dashboard.png` (populated history), `receipt.png` (one-time success confirmation after Post/Redirect/Get), `overdraft.png` (inline insufficient-funds error with the typed value preserved), and `phone.png` (380px reflow with stacked cards and records).

## Seven-slide deck

1. **ATM Coding Exercise** — scope, test count, the finished dashboard as the visual anchor, and a hint at the Konami-code easter egg.
2. **Technology Stack** — Razor Pages with vanilla JavaScript and CSS, C# 14 on .NET 10, SQLite via EF Core 10, and xUnit with WebApplicationFactory; layering lives in the speaker notes.
3. **Decisions and Their Costs** — a readable ledger for Razor Pages, modular monolith, SQLite, and 2-decimal USD.
4. **Datastore Choice** — an in-memory store would have been enough; the SQLite file was chosen so balances survive restarts. SQLite in-memory, a hand-written store, EF Core InMemory, and Redis are weighed in the speaker notes.
5. **Financial Operations Fail Without Partial State** — invariants, atomic history, concurrency, and a real rejected-overdraft state.
6. **143 Tests by Layer** — domain, application, SQLite integration, HTTP behavior, a success receipt, and macOS commands.
7. **Tradeoffs and Roadmap** — deliberate omissions, an ordered roadmap, and the real 380 px layout.

No slide footers or slide numbers, and no slogan copy: every line states a fact about the application. Speaker notes keep their paragraph breaks, and the generator links ADR references in them to GitHub. Use a solid charcoal canvas, soft-white type, green as the dominant accent (orange only for a contrasting second group, blue only for a third), sparse diagrams, and each real screenshot once. Keep one focal element and no more than three supporting groups per slide. Aim for 36 authored words or fewer—title, labels, and body copy included—and move supporting detail to speaker notes before reducing type size. Use whitespace and hairlines instead of reproducing the application's card grid in the presentation.
