// Generates design/atm-deck.pptx — the seven-slide deck outlined in
// design/README.md — from the real screenshots in design/screenshots.
//
//   cd design/deck && npm install && node build.js
//
// Pass an output path to write elsewhere (used to verify a change without
// overwriting the committed deck):
//
//   node build.js /tmp/preview.pptx
//
// pptxgenjs is a tooling dependency of this script only. It is deliberately
// not part of Atm.sln and adds no runtime dependency to the application.
const pptxgen = require("pptxgenjs");
const path = require("path");

const DESIGN = path.resolve(__dirname, "..");
const SHOT = (n) => path.join(DESIGN, "screenshots", n);
const OUT = process.argv[2]
  ? path.resolve(process.argv[2])
  : path.join(DESIGN, "atm-deck.pptx");

// Tokens from design/README.md
const NAVY = "174A7E", TEXT = "17212B", MUTED = "667085", GREEN = "18794E", ERROR = "B42318";
const PAGE = "F6F8FA", SURFACE = "FFFFFF", BORDER = "D9E0E7";
const FONT = "Calibri", MONO = "Courier New";

const pres = new pptxgen();
pres.layout = "LAYOUT_16x9"; // 10 x 5.625 in
pres.author = "John DiFini";
pres.title = "ATM Coding Exercise";

let slideNo = 0;
function base(title, notes) {
  const s = pres.addSlide();
  s.background = { color: PAGE };
  slideNo += 1;
  s.addText(title, { x: 0.6, y: 0.35, w: 8.8, h: 0.6, fontFace: FONT, fontSize: 28, bold: true, color: NAVY, margin: 0, isTextBox: true });
  s.addText(String(slideNo), { x: 9.0, y: 5.15, w: 0.5, h: 0.3, fontFace: FONT, fontSize: 9, color: MUTED, align: "right", margin: 0, isTextBox: true });
  if (notes) s.addNotes(notes);
  return s;
}
function card(s, x, y, w, h) {
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y, w, h, rectRadius: 0.08, fill: { color: SURFACE }, line: { color: BORDER, width: 0.75 } });
}
function heading(s, text, x, y, w, color = NAVY) {
  s.addText(text, { x, y, w, h: 0.32, fontFace: FONT, fontSize: 14, bold: true, color, margin: 0, isTextBox: true });
}
function bullets(s, items, x, y, w, h, size = 12) {
  s.addText(items.map((t, i) => ({ text: t, options: { bullet: true, breakLine: i < items.length - 1 } })),
    { x, y, w, h, fontFace: FONT, fontSize: size, color: TEXT, valign: "top", margin: 0, paraSpaceAfter: 4, isTextBox: true });
}
function numberBadge(s, n, x, y) {
  s.addShape(pres.shapes.OVAL, { x, y, w: 0.34, h: 0.34, fill: { color: NAVY }, line: { color: NAVY } });
  s.addText(String(n), { x, y, w: 0.34, h: 0.34, fontFace: FONT, fontSize: 11, bold: true, color: SURFACE, align: "center", valign: "middle", margin: 0, isTextBox: true });
}

// ---------- 1. ATM Coding Exercise ----------
{
  const s = pres.addSlide();
  s.background = { color: PAGE };
  slideNo += 1;
  s.addText("CODING EXERCISE", { x: 0.6, y: 0.55, w: 4.2, h: 0.3, fontFace: FONT, fontSize: 11, bold: true, color: MUTED, charSpacing: 2, margin: 0, isTextBox: true });
  s.addText("ATM", { x: 0.6, y: 0.85, w: 4.2, h: 0.9, fontFace: FONT, fontSize: 48, bold: true, color: NAVY, margin: 0, isTextBox: true });
  s.addText("A single-user web ATM built to show clear architecture within a deliberately small scope.",
    { x: 0.6, y: 1.75, w: 4.2, h: 0.75, fontFace: FONT, fontSize: 14, color: TEXT, margin: 0, isTextBox: true });
  const rows = [
    ["Objective", "Deposit, withdraw, transfer, view balances and history, with correct money handling."],
    ["Scope", "One local user, two accounts (Checking and Savings) seeded at $1,000.00, persisted across restarts."],
    ["Stack", ".NET 10, ASP.NET Core Razor Pages, EF Core with SQLite, xUnit. One deployable, no JavaScript framework."],
  ];
  rows.forEach(([k, v], i) => {
    const y = 2.7 + i * 0.75;
    s.addText(k, { x: 0.6, y, w: 1.0, h: 0.3, fontFace: FONT, fontSize: 12, bold: true, color: NAVY, margin: 0, isTextBox: true });
    s.addText(v, { x: 1.6, y, w: 3.2, h: 0.7, fontFace: FONT, fontSize: 11.5, color: TEXT, valign: "top", margin: 0, isTextBox: true });
  });
  card(s, 5.15, 0.6, 4.35, 3.55);
  s.addImage({ path: SHOT("dashboard.png"), x: 5.3, y: 0.75, w: 4.05, h: 3.16 });
  s.addText("The finished dashboard: two accounts, one operation at a time, history newest first.",
    { x: 5.15, y: 4.25, w: 4.35, h: 0.5, fontFace: FONT, fontSize: 10, italic: true, color: MUTED, margin: 0, isTextBox: true });
  s.addText(String(slideNo), { x: 9.0, y: 5.15, w: 0.5, h: 0.3, fontFace: FONT, fontSize: 9, color: MUTED, align: "right", margin: 0, isTextBox: true });
  s.addNotes("Frame the exercise in one breath: a web ATM, one user, two accounts, real persistence. The point is not features; it is showing how a small system is organised so that correctness is easy to see and verify. The screenshot is the real app, not a mock.");
}

// ---------- 2. How I Framed the Problem ----------
{
  const s = base("How I framed the problem", "I prioritised organisation, separation of concerns and error handling over UI polish. So every choice was made to keep the interesting rules in one obvious place and to make the rest thin. Non-goals were written down first so scope could not creep.");
  const cells = [
    ["Requirements", ["Two seeded accounts, balances and history that survive restarts", "Deposit, withdraw, transfer with positive two-decimal amounts", "Newest-first history that explains the ledger"]],
    ["Non-goals", ["Authentication, cards, PINs, multiple users", "Fees, interest, overdrafts, cash inventory, currencies", "SPA, public API, queues, cloud deployment"]],
    ["Quality attributes", ["Business rules independent of ASP.NET Core and EF Core", "No partial writes: balance and history commit together", "Tests at every boundary, runnable on macOS with one SDK"]],
    ["Why clarity drove it", ["A reader should find any rule in under a minute", "Thin adapters make the domain the only place bugs can hide", "Small, explicit scope beats speculative generality"]],
  ];
  cells.forEach(([h, items], i) => {
    const x = i % 2 === 0 ? 0.6 : 5.1, y = i < 2 ? 1.2 : 3.2;
    card(s, x, y, 4.3, 1.8);
    heading(s, h, x + 0.25, y + 0.2, 3.8, i === 3 ? GREEN : NAVY);
    bullets(s, items, x + 0.25, y + 0.58, 3.85, 1.15, 11.5);
  });
}

// ---------- 3. Architecture at a Glance ----------
{
  const s = base("Architecture at a glance", "Modular monolith, four projects, one deployable. Read the arrows: everything points inward. The web project references Infrastructure only to register adapters in the composition root; no business behaviour lives there. ADR-0002 records the decision.");
  const box = (x, y, w, h, title, sub, opts = {}) => {
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y, w, h, rectRadius: 0.08, fill: { color: opts.fill || SURFACE }, line: { color: opts.line || NAVY, width: 1.25, dashType: opts.dash || "solid" } });
    s.addText(title, { x, y: y + 0.1, w, h: 0.32, fontFace: FONT, fontSize: 13, bold: true, color: opts.color || NAVY, align: "center", margin: 0, isTextBox: true });
    s.addText(sub, { x: x + 0.1, y: y + 0.42, w: w - 0.2, h: h - 0.5, fontFace: FONT, fontSize: 10, color: MUTED, align: "center", valign: "top", margin: 0, isTextBox: true });
  };
  const arrow = (x1, y1, x2, y2, opts = {}) => {
    s.addShape(pres.shapes.LINE, { x: Math.min(x1, x2), y: Math.min(y1, y2), w: Math.abs(x2 - x1) || 0.001, h: Math.abs(y2 - y1) || 0.001,
      flipH: x2 < x1, flipV: y2 < y1, line: { color: opts.color || NAVY, width: 1.5, endArrowType: "triangle", dashType: opts.dash || "solid" } });
  };
  const Y = 1.55, H = 1.05;
  box(0.6, Y, 1.5, H, "Browser", "server-rendered HTML", { line: BORDER, color: MUTED });
  box(2.55, Y, 1.9, H, "Atm.Web", "Razor Pages, input validation, error mapping, DI");
  box(4.9, Y, 1.9, H, "Atm.Application", "use cases, ports, orchestration");
  box(7.25, Y, 2.15, H, "Atm.Domain", "Money, Account, Transaction, invariants");
  arrow(2.1, Y + H / 2, 2.55, Y + H / 2);
  arrow(4.45, Y + H / 2, 4.9, Y + H / 2);
  arrow(6.8, Y + H / 2, 7.25, Y + H / 2);
  const Y2 = 3.35;
  box(4.9, Y2, 1.9, H, "Atm.Infrastructure", "EF Core, SQLite, migrations, unit of work");
  arrow(5.85, Y2, 5.85, Y + H);
  s.addText("implements ports", { x: 5.95, y: Y + H + 0.25, w: 1.6, h: 0.25, fontFace: FONT, fontSize: 9.5, italic: true, color: MUTED, margin: 0, isTextBox: true });
  // composition-root reference: Web -> Infrastructure (dotted, elbow)
  s.addShape(pres.shapes.LINE, { x: 3.5, y: Y + H, w: 0.001, h: Y2 + H / 2 - (Y + H), line: { color: MUTED, width: 1, dashType: "dash" } });
  arrow(3.5, Y2 + H / 2, 4.9, Y2 + H / 2, { color: MUTED, dash: "dash" });
  s.addText("composition root registers adapters", { x: 2.55, y: Y2 + H / 2 + 0.08, w: 2.3, h: 0.4, fontFace: FONT, fontSize: 9.5, italic: true, color: MUTED, margin: 0, isTextBox: true });
  s.addText([
    { text: "Every solid arrow points inward. ", options: { bold: true, color: NAVY } },
    { text: "Domain has no package references; Application depends only on Domain; Infrastructure implements Application's ports; Web composes them. Persistence can change without touching a use case, and every business rule is testable without a database or a web server." },
  ], { x: 0.6, y: 4.6, w: 8.8, h: 0.75, fontFace: FONT, fontSize: 11.5, color: TEXT, valign: "top", margin: 0, isTextBox: true });
}

// ---------- 4. Key Decisions ----------
{
  const s = base("Key decisions", "Each row is an ADR in docs/adr. The tradeoff column is the honest cost; none of these are free. The through-line is: choose the smallest thing that makes the architecture legible.");
  const hdr = (t) => ({ text: t, options: { bold: true, color: SURFACE, fill: { color: NAVY }, fontFace: FONT, fontSize: 11 } });
  const cell = (t, o = {}) => ({ text: t, options: { fontFace: FONT, fontSize: 10.5, color: TEXT, valign: "top", ...o } });
  const rows = [
    [hdr("Decision"), hdr("Why"), hdr("Tradeoff"), hdr("ADR")],
    [cell("Razor Pages, server-rendered", { bold: true }), cell("One language, one build, one deployable; Post/Redirect/Get and validation come built in"), cell("No rich client interactivity; a real API would need adding later"), cell("0001")],
    [cell("Modular monolith", { bold: true }), cell("Four projects with inward dependencies show separation without distributed complexity"), cell("Extra project ceremony for a small codebase"), cell("0002")],
    [cell("SQLite through EF Core", { bold: true }), cell("Durable, zero-setup, relational; migrations, transactions and concurrency tokens for free"), cell("Single-writer limits; decimal stored as text, so no arithmetic in SQL"), cell("0003")],
    [cell("Decimal money, two digits", { bold: true }), cell("A Money value type rejects negatives and sub-cent precision at construction; no float anywhere"), cell("Conversions at every boundary; currency is implicitly USD"), cell("0005")],
    [cell("Single-user scope", { bold: true }), cell("Removes auth, sessions and identity so the financial rules stay the centre of the exercise"), cell("Concurrency is handled, but there is no per-user isolation"), cell("Spec")],
  ];
  s.addTable(rows, { x: 0.6, y: 1.2, w: 8.8, colW: [1.9, 3.5, 2.7, 0.7], border: { type: "solid", color: BORDER, pt: 0.75 }, fill: { color: SURFACE }, rowH: [0.35, 0.62, 0.62, 0.62, 0.62, 0.62], margin: 0.07 });
}

// ---------- 5. Correctness and Failure Handling ----------
{
  const s = base("Correctness and failure handling", "Walk the five rules top to bottom. The screenshot is a real overdraft attempt: the error is inline, the typed value is preserved, and the database is untouched. Concurrency: the Version column is the EF Core token; a stale write updates zero rows and the user sees a retry message, never a silent overwrite.");
  const items = [
    ["Invariants live in the domain", "Money is a value type: non-negative, at most two decimals. Operations need a positive amount. Balance can never go below zero."],
    ["Rejections happen before any mutation", "Overdrafts, zero amounts, same-account transfers and unknown accounts throw first; nothing is touched and nothing is written."],
    ["One commit, all or nothing", "Each operation returns the history record it creates. Infrastructure saves balance and record in one SQLite transaction."],
    ["Optimistic concurrency", "Account.Version increments per mutation and is the EF Core concurrency token. Conflicts map to a safe retry message."],
    ["History that explains the ledger", "One immutable record per operation carrying the account or accounts touched and each resulting balance; a transfer is one row, not two."],
  ];
  items.forEach(([h, t], i) => {
    const y = 1.2 + i * 0.8;
    numberBadge(s, i + 1, 0.6, y + 0.02);
    s.addText(h, { x: 1.05, y, w: 4.0, h: 0.3, fontFace: FONT, fontSize: 12.5, bold: true, color: NAVY, margin: 0, isTextBox: true });
    s.addText(t, { x: 1.05, y: y + 0.3, w: 4.0, h: 0.5, fontFace: FONT, fontSize: 10.5, color: TEXT, valign: "top", margin: 0, isTextBox: true });
  });
  // Cropped overdraft state (image is 1280x1000; render at 8in wide => 160 px/in)
  card(s, 5.45, 1.2, 4.05, 2.35);
  s.addImage({ path: SHOT("overdraft.png"), x: 5.6, y: 1.35, w: 8, h: 6.25, sizing: { type: "crop", x: 1.1, y: 1.48, w: 3.75, h: 2.05 } });
  s.addText([
    { text: "Overdraft attempt. ", options: { bold: true, color: ERROR } },
    { text: "The message names the available balance, the typed amount stays in the field, and the account row is unchanged." },
  ], { x: 5.45, y: 3.7, w: 4.05, h: 0.6, fontFace: FONT, fontSize: 10.5, color: TEXT, valign: "top", margin: 0, isTextBox: true });
  card(s, 5.45, 4.35, 4.05, 0.85);
  s.addText([
    { text: "Duplicate submissions: ", options: { bold: true, color: NAVY } },
    { text: "Post/Redirect/Get with a one-time receipt, submit disabled while processing, confirmation for withdrawals and transfers." },
  ], { x: 5.65, y: 4.45, w: 3.7, h: 0.65, fontFace: FONT, fontSize: 10.5, color: TEXT, valign: "top", margin: 0, isTextBox: true });
}

// ---------- 6. Testing and Delivery ----------
{
  const s = base("Testing and delivery", "Test boundaries mirror production boundaries. Domain tests are pure. Application tests use a hand-written fake that records write order, which is how we know history is appended before commit. Integration tests run the real host over a throwaway SQLite file per test, including a genuine concurrency conflict and a forced primary-key violation to prove atomicity. Setup is four commands on macOS.");
  const stats = [["75", "domain"], ["33", "application"], ["35", "integration"], ["143", "total, all green"]];
  stats.forEach(([n, l], i) => {
    const x = 0.6 + i * 2.25;
    card(s, x, 1.15, 2.05, 1.05);
    s.addText(n, { x: x + 0.15, y: 1.22, w: 1.8, h: 0.6, fontFace: FONT, fontSize: 30, bold: true, color: i === 3 ? GREEN : NAVY, margin: 0, isTextBox: true });
    s.addText(l, { x: x + 0.15, y: 1.8, w: 1.8, h: 0.3, fontFace: FONT, fontSize: 10.5, color: MUTED, margin: 0, isTextBox: true });
  });
  card(s, 0.6, 2.45, 4.3, 2.75);
  heading(s, "Four test layers", 0.85, 2.62, 3.8);
  bullets(s, [
    "Domain: money precision, overdraft prevention, mutations return their history record",
    "Application: fakes prove failed operations never reach commit, and history is staged before commit",
    "SQLite: seeding once, restart persistence, newest-first order, concurrency conflict, all-or-nothing commit",
    "HTTP: Post/Redirect/Get, inline error mapping, antiforgery, hidden error details, accessibility structure",
  ], 0.85, 3.0, 3.85, 2.1, 10.5);
  card(s, 5.15, 2.45, 4.35, 2.75);
  heading(s, "Delivery on macOS", 5.4, 2.62, 3.8);
  s.addText(["dotnet restore Atm.sln", "dotnet build Atm.sln --no-restore", "dotnet test Atm.sln --no-build", "dotnet run --project src/Atm.Web"].map((t, i) => ({ text: t, options: { breakLine: i < 3 } })),
    { x: 5.4, y: 3.0, w: 3.9, h: 0.95, fontFace: MONO, fontSize: 10, color: TEXT, valign: "top", margin: 0, isTextBox: true });
  s.addText([
    { text: "Handoff artifacts: ", options: { bold: true, color: NAVY } },
    { text: "SPEC.md, PLAN.md, six ADRs, the accessibility review, this deck, and a README anyone can follow. SQLite is created and seeded on first run; schema changes go through EF Core migrations." },
  ], { x: 5.4, y: 4.05, w: 3.9, h: 1.05, fontFace: FONT, fontSize: 10.5, color: TEXT, valign: "top", margin: 0, isTextBox: true });
}

// ---------- 7. Tradeoffs and More-Compute Roadmap ----------
{
  const s = base("Tradeoffs and what more compute would change", "Left is what was left out on purpose, and why that was the right call for this exercise. Right is the order I would add things with more time, starting with idempotency keys because Post/Redirect/Get handles refreshes but not a retried network request. The phone view shows the responsive layout already holds up.");
  card(s, 0.6, 1.15, 3.6, 4.05);
  heading(s, "Deliberately left out", 0.85, 1.32, 3.2);
  bullets(s, [
    "Authentication, users, cards and PINs",
    "Fees, interest, overdraft facilities, cash inventory",
    "Multiple currencies and sub-cent accounting",
    "A JavaScript SPA or public API",
    "Queues, services, cloud deployment",
    "Idempotency keys (PRG covers refreshes, not retried requests)",
  ], 0.85, 1.72, 3.15, 3.4, 11);
  card(s, 4.4, 1.15, 3.5, 4.05);
  heading(s, "With more compute, in order", 4.65, 1.32, 3.1, GREEN);
  s.addText([
    "Idempotency keys per submission", "Richer auditing: actor, request id, reversals", "Authentication and per-user accounts",
    "Automated accessibility checks in CI", "Observability: structured logs, metrics, tracing", "Load tests against SQLite's write limits",
    "Packaged deployment and configuration", "Browser-level end-to-end coverage",
  ].map((t, i, a) => ({ text: t, options: { bullet: { type: "number" }, breakLine: i < a.length - 1 } })),
    { x: 4.65, y: 1.72, w: 3.1, h: 3.4, fontFace: FONT, fontSize: 11, color: TEXT, valign: "top", margin: 0, paraSpaceAfter: 4, isTextBox: true });
  // phone view (390x1400 at 1.2in wide => 325 px/in; show the top 3.4in)
  card(s, 8.1, 1.15, 1.4, 4.05);
  s.addImage({ path: SHOT("phone.png"), x: 8.2, y: 1.25, w: 1.2, h: 4.31, sizing: { type: "crop", x: 0, y: 0, w: 1.2, h: 3.85 } });
}

pres.writeFile({ fileName: OUT }).then((f) => console.log("wrote", f));
