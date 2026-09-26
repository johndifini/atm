# ATM Backlog

Running list of deferred work and known gaps for this exercise, so nothing slips. Scope boundaries and the working rules live in [`AGENTS.md`](AGENTS.md); foundational decisions live in [`docs/adr/`](docs/adr/README.md); the build order lives in [`PLAN.md`](PLAN.md). This is the lighter-weight follow-on tracker: add an item whenever work is deferred, and move it to **Done** with its commit when it ships.

## Open

- **Make Withdraw and Transfer reachable with Tab.** (atm, added 2026-09-22) — With JavaScript enabled, `src/Atm.Web/wwwroot/js/site.js` gives only the selected operation tab `tabindex="0"`; pressing Tab cannot move from Deposit to Withdraw or Transfer. Adjust the operation tab interaction so keyboard users can reach and activate all three operations with Tab, while preserving the no-JavaScript links in `src/Atm.Web/Pages/Index.cshtml`. Update the relevant accessibility checks and `docs/accessibility-review.md`. Defer implementation until the user approves this item.

- **Handle browser Back navigation gracefully.** (atm, added 2026-09-22) — Review how the operation tabs and URL stay in sync when a user presses the browser Back or Forward button. `src/Atm.Web/wwwroot/js/site.js` updates the `?op=` URL with `history.replaceState`; make navigation restore the matching visible form and selected tab, and cover the behavior with an appropriate UI test. Defer implementation until the user approves this item.

- **Add HTTPS setup instructions to the README.** (atm, added 2026-09-22) — Add a separate optional section to `README.md` explaining how to start the `https` launch profile in `src/Atm.Web/Properties/launchSettings.json`, open `https://localhost:7080`, and handle the local development certificate if needed. Keep the main setup instructions focused on HTTP. Do not implement until the user approves this item.

- **Plant a senior-engineer Easter egg in the ATM UI.** (atm, added 2026-09-22) — Leading idea is the float-precision gag: deposit `0.10`, then deposit `0.20`, and the balance reads exactly `0.30` alongside a subtle one-line note such as "0.1 + 0.2 = 0.3 here. That's what `decimal` is for." It lands as a joke for anyone who has been bitten by IEEE 754, and it doubles as a live demonstration of the repository's money invariant (`decimal`, never `double`/`float`, max two fractional digits — see [`AGENTS.md`](AGENTS.md) "Financial invariants"), which makes it worth a beat in the presentation deck described in [`design/README.md`](design/README.md). Alternates if that one does not fit the UI: reword the same-account transfer rejection as "Idempotent, but not free."; or add a `/teapot` route returning HTTP 418. **Constraints:** presentation layer only — `src/Atm.Web`, never `Atm.Domain` or `Atm.Application`, per the dependency direction in ADR-0002. No change to any financial invariant, transaction boundary, validation rule, or existing test expectation; the egg observes state, it does not alter it. Keep it discoverable but not intrusive for a first-time grader, and keep the UI aligned with [`design/README.md`](design/README.md). Trigger detection belongs in the PageModel or a view component, not in JavaScript that duplicates business rules. Add a test for whatever trigger condition ships, and note the egg in the deck's speaker notes so it is not mistaken for a defect during a demo.

## Done

_Nothing yet._
