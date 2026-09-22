# Accessibility review — dashboard

**Scope:** the single dashboard page (`/`) and the error page, reviewed on 2026-09-21 against WCAG 2.2 AA after phase 4. No screen reader was run; this is a structural, keyboard, and contrast review of the rendered HTML and CSS, backed by the checks in `tests/Atm.IntegrationTests/Http/AccessibilityStructureTests.cs`.

## Findings and fixes

| # | Finding | Criterion | Fix |
|---|---|---|---|
| 1 | The page had no `h1`; the site title in the header was a plain link. | 1.3.1 Info and Relationships, 2.4.6 Headings | Added a visually hidden `h1` ("Accounts dashboard") above the content; sections remain `h2`. |
| 2 | Unselected tabs were rendered with `tabindex="-1"` on the server. Without JavaScript the arrow-key handler does not exist, so keyboard users could not reach two of the three operations. | 2.1.1 Keyboard | The server renders plain links. `site.js` applies the roving tabindex only when it loads, and manages it on click and arrow keys. |
| 3 | Invalid controls were styled red but never marked `aria-invalid`. | 4.1.2 Name, Role, Value; 3.3.1 Error Identification | Every control renders `aria-invalid="true"` when its ModelState entry has errors, and is `aria-describedby` its inline error element. |
| 4 | Input and select borders used the decorative card border (`#D9E0E7`, 1.33:1), below the 3:1 non-text minimum for form-control boundaries. | 1.4.11 Non-text Contrast | Introduced `--control-border: #7A8797` for form controls; the card border stays decorative. |

## Checks that passed

- **Language and landmarks:** `<html lang="en">`, a skip link to `<main id="main">`, `header` and `main` landmarks, `section` elements labelled by their headings.
- **Forms:** every `input` and `select` has a `<label for>`; required fields use the native `required` attribute; the amount input uses `type="number"`, `inputmode="decimal"`, `step="0.01"` and `min="0.01"` so browsers give inline guidance before submission, and the server remains authoritative.
- **Errors:** field errors render next to the control and are referenced by `aria-describedby`; form-level errors (concurrency conflicts) render in a `role="alert"` region with a text label "Error:". Submitted values are preserved after a recoverable error.
- **Success:** the one-time confirmation renders in a `role="status"` region with a text label "Success:". Outcome is never conveyed by colour alone; the coloured rule on each notice is reinforced by the label.
- **Tabs:** `role="tablist"`/`tab`/`tabpanel` with `aria-selected`, `aria-controls` and `aria-labelledby` wired to real ids; inactive panels use the `hidden` attribute, so they are removed from the accessibility tree and tab order. Arrow keys move between tabs when JavaScript is available.
- **History table:** a `caption` (visually hidden), `th scope="col"`, `<time datetime>` for timestamps, and stacked records on narrow screens that keep their labels through `data-label`.
- **Focus:** a 3px `:focus-visible` outline at 3.45:1 against the surface; the skip link becomes visible on focus.
- **Motion and timing:** no animation, no auto-refresh, no timeouts.
- **Reflow:** at a 380px viewport there is no horizontal scrolling; the two cards stack and the table collapses to records.
- **Confirmation dialogs:** native `confirm()` for withdrawals and transfers, which is keyboard and screen-reader accessible by default.

## Contrast

| Pair | Colours | Ratio | Required | Result |
|---|---|---|---|---|
| Body text | `#17212B` on `#FFFFFF` | 16.29:1 | 4.5:1 | pass |
| Muted text on surface | `#667085` on `#FFFFFF` | 4.97:1 | 4.5:1 | pass |
| Muted text on page | `#667085` on `#F6F8FA` | 4.67:1 | 4.5:1 | pass |
| Primary navy text (tabs, title) | `#174A7E` on `#FFFFFF` | 9.06:1 | 4.5:1 | pass |
| Button text on primary | `#FFFFFF` on `#174A7E` | 9.06:1 | 4.5:1 | pass |
| Success label | `#18794E` on `#FFFFFF` | 5.41:1 | 4.5:1 | pass |
| Error text | `#B42318` on `#FFFFFF` | 6.57:1 | 4.5:1 | pass |
| Focus ring on surface (non-text) | `#4C8DD6` on `#FFFFFF` | 3.45:1 | 3.0:1 | pass |
| Focus ring on page (non-text) | `#4C8DD6` on `#F6F8FA` | 3.24:1 | 3.0:1 | pass |
| Selected tab underline (non-text) | `#174A7E` on `#FFFFFF` | 9.06:1 | 3.0:1 | pass |
| Control border (non-text) | `#7A8797` on `#FFFFFF` | 3.66:1 | 3.0:1 | pass |

## Not covered

- Screen-reader behaviour (VoiceOver, NVDA) was not exercised; the ARIA pattern used for tabs is the standard one but should be confirmed with assistive technology before any wider release.
- Automated tooling (axe, Lighthouse) was not run in this environment; adding it to CI is listed on the roadmap in `design/README.md`.
