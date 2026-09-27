// Rebuilds the eight-slide ATM case-study deck from real application screenshots.
// Approved Jony Vibe system: charcoal canvas, soft-white type, one restrained
// accent per slide, and a 36-word target for authored on-slide copy.
const pptxgen = require("pptxgenjs");
const path = require("path");
const fs = require("fs");
const JSZip = require("jszip");

const DESIGN = path.resolve(__dirname, "..");
const SHOT = (name) => path.join(DESIGN, "screenshots", name);
const OUT = process.argv[2] ? path.resolve(process.argv[2]) : path.join(DESIGN, "atm-deck.pptx");
// Accent roles are fixed across the deck: green is primary, orange secondary, blue tertiary.
const C = { page: "121212", surface: "1B1B1B", text: "F5F5F5", muted: "A9ADB3", line: "333333", primary: "00F604", secondary: "F67F00", tertiary: "0077F6" };
const REPO = "https://github.com/johndifini/atm/blob/main";
const BORG = "https://github.com/johndifini/theborg/tree/main";
const ARCHITETTO = `${BORG}/architetto`;
// Speaker-note terms that become hyperlinks during post-processing.
const NOTE_LINKS = {
  ADRs: "https://github.com/johndifini/atm/tree/main/docs/adr",
  "ADR-0001": `${REPO}/docs/adr/0001-dotnet-razor-pages.md`,
  "ADR-0002": `${REPO}/docs/adr/0002-modular-monolith.md`,
  "ADR-0003": `${REPO}/docs/adr/0003-sqlite-ef-core.md`,
  "ADR-0005": `${REPO}/docs/adr/0005-money-transaction-semantics.md`,
  "SPEC.md": `${REPO}/SPEC.md`,
  "PLAN.md": `${REPO}/PLAN.md`,
  "AGENTS.md": `${REPO}/AGENTS.md`,
  "README.md": `${REPO}/README.md`,
  Architetto: ARCHITETTO,
  "jony-vibe": `${BORG}/jony-vibe`,
  C4PO: `${BORG}/c4po`,
};
const FONT = "Avenir Next";
const pptx = new pptxgen();
pptx.layout = "LAYOUT_16x9";
pptx.author = "John DiFini";
pptx.subject = "ATM coding exercise: architecture, correctness, and delivery";
pptx.title = "ATM Coding Exercise";
pptx.lang = "en-US";
pptx.theme = { headFontFace: FONT, bodyFontFace: FONT, lang: "en-US" };

function addText(slide, value, x, y, w, h, options = {}) {
  slide.addText(value, { x, y, w, h, fontFace: FONT, fontSize: 17, color: C.text, margin: 0, isTextBox: true, valign: "top", ...options });
}
function line(slide, x, y, w, color = C.line, width = 0.75) {
  slide.addShape(pptx.ShapeType.line, { x, y, w, h: 0, line: { color, width } });
}
function base(title, notes) {
  const accent = C.primary;
  const slide = pptx.addSlide(); slide.background = { color: C.page };
  slide.addShape(pptx.ShapeType.rect, { x: 0.64, y: 0.39, w: 0.10, h: 0.48, fill: { color: accent }, line: { color: accent, transparency: 100 } });
  addText(slide, title, 0.91, 0.39, 8.45, 0.56, { fontSize: title.length > 36 ? 29 : 32, bold: true, fit: "shrink" });
  if (notes) slide.addNotes(notes); return slide;
}
function kicker(slide, value, x, y, w, accent) {
  addText(slide, value.toUpperCase(), x, y, w, 0.25, { fontSize: 11, bold: true, color: accent, charSpacing: 1.6, fit: "shrink" });
}
function imageFrame(slide, imagePath, x, y, w, h) {
  slide.addShape(pptx.ShapeType.rect, { x, y, w, h, fill: { color: C.surface }, line: { color: C.line, width: 0.8 } });
  slide.addImage({ path: imagePath, x: x + 0.04, y: y + 0.04, w: w - 0.08, h: h - 0.08 });
}
function arrow(slide, x1, y1, x2, y2, accent, dashed = false) {
  slide.addShape(pptx.ShapeType.line, { x: Math.min(x1, x2), y: Math.min(y1, y2), w: Math.abs(x2 - x1) || 0.001, h: Math.abs(y2 - y1) || 0.001, flipH: x2 < x1, flipV: y2 < y1, line: { color: accent, width: 1.5, dashType: dashed ? "dash" : "solid", endArrowType: "triangle" } });
}
function wordCount(parts) { return parts.join(" ").trim().split(/\s+/).filter(Boolean).length; }
const WORD_BUDGETS = [
  ["ATM Coding Exercise", "Two accounts, persisted locally, covered by 143 tests.", "Hint: try the Konami code."],
  ["How I AI'ed", "Frame", "Architetto (GPT-5.6 Sol)", "I approved spec, plan, ADRs", "Build", "Fable 5.1", "Six phases, tests green per commit", "Chose to minimize rework over tokens", "Refine", "Opus 5.5, GPT-6 Sol", "Docs, deck, fixes"],
  ["Technology stack", "Frontend", "Razor Pages, vanilla JavaScript, CSS", "No client framework or packages", "Backend", "C# 14 on .NET 10", "ASP.NET Core", "Database", "SQLite via EF Core 10", "Code-first migrations", "Testing", "xUnit, WebApplicationFactory, coverlet"],
  ["Decisions and costs", "Decision", "Cost", "Razor Pages", "Full page reload per action", "Modular monolith", "One feature spans four projects", "SQLite + EF Core", "Single-writer ceiling", "2-decimal USD", "Single-currency support"],
  ["Datastore choice", "In-memory would suffice.", "Chosen", "SQLite file", "Survives restarts", "Considered", "SQLite in-memory", "Lost on restart", "Hand-written store", "Owns atomicity", "EF Core InMemory", "No transactions", "Redis", "Extra process"],
  ["Failure leaves no partial state", "Validate before mutation", "Commit balance and history together", "Reject stale writes", "Rejected overdraft. Persisted state stays unchanged."],
  ["143 tests by layer", "75 Domain", "33 Application", "35 Integration + HTTP", "Invariants", "Orchestration", "Persistence and HTTP", "Success receipt after Post/Redirect/Get."],
  ["Tradeoffs and roadmap", "Omitted", "Identity", "Banking breadth", "Distributed operations", "Next", "Double-submit protection, auditing, authentication", "Then", "Accessibility, observability, load tests", "Later", "Deployment, browser coverage", "Responsive at 380 px."],
];
// Slide 2 runs two words over by the author's choice: an explicit tradeoff line and the Architetto link.
const WORD_LIMIT_OVERRIDES = { 2: 38 };
WORD_BUDGETS.forEach((parts, index) => { const limit = WORD_LIMIT_OVERRIDES[index + 1] ?? 36; const count = wordCount(parts); if (count > limit) throw new Error(`Slide ${index + 1} has ${count} authored words; limit is ${limit}.`); });

// 1 — outcome first
{
  const slide = pptx.addSlide(); slide.background = { color: C.page };
  kicker(slide, "Case study", 0.64, 0.50, 2.8, C.primary);
  addText(slide, "ATM", 0.64, 0.93, 3.10, 0.65, { fontSize: 47, bold: true, color: C.primary });
  addText(slide, "Coding Exercise", 0.64, 1.60, 3.48, 0.54, { fontSize: 27, bold: true, fit: "shrink" });
  addText(slide, "Two accounts, persisted locally,\ncovered by 143 tests.", 0.64, 2.48, 3.18, 0.62, { fontSize: 15, color: C.muted, breakLine: true });
  imageFrame(slide, SHOT("dashboard.png"), 4.17, 0.50, 5.19, 4.04);
  addText(slide, "Hint: try the Konami code.", 4.17, 4.68, 5.19, 0.23, { fontSize: 11.5, color: C.primary });
  slide.addNotes("Open with the finished product, running against a scratch SQLite database. The intentionally narrow scope keeps the review on correctness, architectural boundaries, and evidence. The hint points at an opt-in easter egg: the Konami code (up, up, down, down, left, right, left, right, B, A) toggles a Matrix theme; Esc, the blue pill, or the code again exits it.");
}

// 2 — how the work was split across AI agents
{
  const slide = base("How I AI'ed", "Frame: Architetto (GPT-5.6 Sol in Codex)\n• Architetto is my open-source architect agent: it bootstraps new repositories and records every foundational decision.\n• Interviewed me on four open questions: time box, persistence, seed accounts, repo visibility.\n• The model proposed the stack; I approved it.\n• Scaffolded before any feature code: SPEC.md, PLAN.md, six ADRs, AGENTS.md.\n   – AGENTS.md lists the rules every coding agent must follow in this repo: money is decimal, balances never go negative, each balance change saves together with its history record, and the agent asks me before adding a dependency.\n\nBuild: Fable 5.1 in Claude Code\n• Six phases in order: domain, application, persistence, web, HTTP tests and accessibility, deck.\n• Each phase committed only with the build and every test green; domain tests written first.\n• Why Fable 5.1: I know the best approach is to plan with the strongest model and execute with cheaper ones, but when I reached the implementation phase, I was running short on time. Therefore, I chose the strongest end-to-end model at the time (a whopping week ago, before Opus 5.5 shipped).\n   – This build had traps that fail quietly: money rounding, half-finished transfers, lost updates, migrations. A bug there costs a phase of rework.\n   – Quick questions went to Sonnet 5.\n• With more time: Fable 5.1 plans each phase, Sonnet 5 implements it, the same tests gate it.\n\nRefine: Opus 5.5 and GPT-6 Sol in Codex\n• Windows setup steps in README.md, a Dependabot fix, the deck redesign, slide wording.\n• The design direction came from jony-vibe, my design-consultation agent; the first deck skipped that consultation and was redone.\n• C4PO, my workspace-admin agent, moved the deck's visual checks from Keynote to PowerPoint after Keynote passed a deck that PowerPoint had to repair.\n• I sent back what didn't hold up.");
  const rows = [["Frame", [{ text: "Architetto", options: { hyperlink: { url: ARCHITETTO, tooltip: "Architetto on GitHub" }, color: C.primary, underline: { style: "sng" } } }, { text: " (GPT-5.6 Sol)" }], "I approved spec, plan, ADRs"], ["Build", "Fable 5.1", "Six phases, tests green per commit", "Chose to minimize rework over tokens"], ["Refine", "Opus 5.5, GPT-6 Sol", "Docs, deck, fixes"]];
  rows.forEach(([label, value, detail, tradeoff], i) => {
    const y = 1.38 + i * 1.10 + (i > 1 ? 0.20 : 0);
    if (i > 0) line(slide, 0.64, y - 0.22, 8.72);
    kicker(slide, label, 0.64, y + 0.06, 1.90, C.primary);
    addText(slide, value, 2.70, y, 6.66, 0.36, { fontSize: 20, bold: true, fit: "shrink" });
    addText(slide, detail, 2.70, y + 0.38, 6.66, 0.24, { fontSize: 12.5, color: C.muted, fit: "shrink" });
    if (tradeoff) addText(slide, tradeoff, 2.70, y + 0.68, 6.66, 0.24, { fontSize: 12.5, bold: true, color: C.secondary, fit: "shrink" });
  });
}

// 3 — technology stack
{
  const slide = base("Technology stack", "No client-side framework or npm packages ship with the app.\nPages are server-rendered Razor Pages with a small amount of vanilla JavaScript and one “hand-written” stylesheet.\n\nThe solution is a modular monolith of four projects (Domain, Application, Infrastructure, Web) with dependencies pointing inward, recorded in ADR-0002.\nEF Core and SQLite live only in the Infrastructure project.\n\nTests use xUnit, WebApplicationFactory for in-process HTTP, and coverlet for coverage.");
  const rows = [["Frontend", "Razor Pages, vanilla JavaScript, CSS", "No client framework or packages"], ["Backend", "C# 14 on .NET 10", "ASP.NET Core"], ["Database", "SQLite via EF Core 10", "Code-first migrations"], ["Testing", "xUnit, WebApplicationFactory, coverlet", ""]];
  rows.forEach(([label, value, detail], i) => {
    const y = 1.38 + i * 1.00;
    if (i > 0) line(slide, 0.64, y - 0.20, 8.72);
    kicker(slide, label, 0.64, y + 0.06, 1.90, C.primary);
    addText(slide, value, 2.70, y, 6.66, 0.36, { fontSize: 20, bold: true, fit: "shrink" });
    if (detail) addText(slide, detail, 2.70, y + 0.38, 6.66, 0.24, { fontSize: 12.5, color: C.muted, fit: "shrink" });
  });
}

// 4 — decision ledger
{
  const slide = base("Decisions and costs", "The ADRs explain why each choice fits the exercise.\n\nRazor Pages (ADR-0001). Cost: a full page reload per action. The server renders HTML with minimal JavaScript, so the whole app is written in C# and ships as a single unit. A separate JavaScript front end (e.g., a React single-page app) would add its own npm build and a JSON API for the browser to call, because the browser would fetch data instead of receiving finished pages.\n\nModular monolith (ADR-0002). Cost: one feature spans four projects, so adding a transaction type touches Domain, Application, Infrastructure, and Web. The payoff is that business rules can be tested without the web host or the database. Microservices would add operational complexity with no payoff at this scope.\n\nSQLite + EF Core (ADR-0003). Cost: single-writer ceiling. A server database is the upgrade path if concurrent load ever matters. The next slide covers the datastore choice.\n\n2-decimal USD (ADR-0005). Cost: single-currency support. Amounts are decimal with at most two fractional digits, which rules out floating-point error. The two-digit rule fits USD only, and there is no currency field, so supporting another currency (e.g., JPY with 0 places or KWD with 3) would change the Money type.");
  const header = (value, color) => ({ text: value, options: { bold: true, color, fill: { color: C.page }, fontFace: FONT, fontSize: 12.5 } });
  const cell = (value, options = {}) => ({ text: value, options: { fontFace: FONT, fontSize: 17, color: C.text, valign: "middle", ...options } });
  const rows = [[header("DECISION", C.primary), header("COST", C.secondary)], [cell("Razor Pages", { bold: true }), cell("Full page reload per action", { color: C.muted })], [cell("Modular monolith", { bold: true }), cell("One feature spans four projects", { color: C.muted })], [cell("SQLite + EF Core", { bold: true }), cell("Single-writer ceiling", { color: C.muted })], [cell("2-decimal USD", { bold: true }), cell("Single-currency support", { color: C.muted })]];
  slide.addTable(rows, { x: 0.64, y: 1.27, w: 8.72, h: 3.66, colW: [4.35, 4.37], rowH: [0.42, 0.81, 0.81, 0.81, 0.81], border: { type: "solid", color: C.line, pt: 0.75 }, fill: { color: C.page }, margin: 0.10 });
}

// 5 — datastore choice and the in-memory alternatives
{
  const slide = base("Datastore choice", "An in-memory store would have been enough. I chose a persistent one so balances and history survive restarts (ADR-0003).\n• SQLite – single-file DB with near-zero setup.\n• EF Core – the ORM (the .NET counterpart to Hibernate).\n   – Transactions: saves a transfer's debit, credit, and history rows in a single DB transaction, so they either commit or roll back as a unit.\n   – Optimistic concurrency: rejects a write based on an outdated balance, e.g., two tabs withdrawing against the same $1,000 and overdrawing it. Doesn't lock account rows; assumes conflicts are rare.\n\nEvery option sits behind the same Application ports, so switching is an Infrastructure-only change.\n• SQLite in-memory – an in-memory connection string with one connection held open. Keeps EF Core, migrations, transactions, and constraints, but data is lost on restart. The best in-memory choice, e.g., for a demo mode.\n• Hand-written store – dictionaries behind a single lock. No dependencies, but the code itself must commit balance and history together instead of relying on a database transaction.\n• EF Core InMemory provider – discouraged by Microsoft. It isn't relational, ignores transactions, and enforces no constraints, so it can't prove the invariants.\n• Redis – a separate server process to install and run, and atomic transfers need MULTI/EXEC or Lua scripts. More operational machinery than this exercise needs, and still volatile unless its persistence is configured.");
  addText(slide, "In-memory would suffice.", 0.91, 0.98, 8.45, 0.26, { fontSize: 13, color: C.muted });
  const rows = [["Chosen", C.primary, "SQLite file", "Survives restarts"], ["Considered", C.secondary, "SQLite in-memory", "Lost on restart"], ["", null, "Hand-written store", "Owns atomicity"], ["", null, "EF Core InMemory", "No transactions"], ["", null, "Redis", "Extra process"]];
  rows.forEach(([label, accent, option, reason], i) => {
    const y = 1.62 + i * 0.70;
    if (i > 0) line(slide, 0.64, y - 0.17, 8.72);
    if (label) kicker(slide, label, 0.64, y + 0.06, 1.90, accent);
    addText(slide, option, 2.70, y, 3.30, 0.36, { fontSize: 19, bold: true, fit: "shrink" });
    addText(slide, reason, 6.10, y + 0.04, 3.26, 0.32, { fontSize: 15, color: i === 0 ? C.text : C.muted, fit: "shrink" });
  });
}

// 6 — correctness in one failure state
{
  const slide = base("Failure leaves no partial state", "The screenshot is a real rejected overdraft. The typed value stays visible, the error names the available balance, and persisted state is unchanged. Account versions turn stale writes into a safe retry instead of lost data.");
  ["Validate before mutation", "Commit balance and history together", "Reject stale writes"].forEach((value, i) => { addText(slide, String(i + 1).padStart(2, "0"), 0.64, 1.43 + i * 1.10, 0.45, 0.30, { fontSize: 13, bold: true, color: C.primary }); addText(slide, value, 1.22, 1.38 + i * 1.10, 3.00, 0.56, { fontSize: 20, bold: true, fit: "shrink" }); });
  imageFrame(slide, SHOT("overdraft.png"), 4.63, 1.24, 4.73, 3.59);
  addText(slide, "Rejected overdraft. Persisted state stays unchanged.", 4.63, 4.91, 4.73, 0.22, { fontSize: 11.5, bold: true, color: C.primary, align: "right", fit: "shrink" });
}

// 7 — evidence over commands
{
  const slide = base("143 tests by layer", "Domain tests are pure. Application tests use hand-written fakes. Integration and HTTP tests exercise the real host with an isolated SQLite file per test, including concurrency conflicts, atomic rollback, Post/Redirect/Get, security, and accessibility structure. Commands: dotnet restore Atm.sln; dotnet build Atm.sln --no-restore; dotnet test Atm.sln --no-build.");
  [["75", "Domain", "Invariants"], ["33", "Application", "Orchestration"], ["35", "Integration + HTTP", "Persistence and HTTP"]].forEach(([n, label, scope], i) => { const y = 1.34 + i * 1.07; addText(slide, n, 0.64, y, 0.92, 0.52, { fontSize: 34, bold: true, color: C.primary }); addText(slide, label, 1.65, y + 0.02, 2.27, 0.28, { fontSize: 16, bold: true, fit: "shrink" }); addText(slide, scope, 1.65, y + 0.38, 2.27, 0.24, { fontSize: 12, color: C.muted, fit: "shrink" }); });
  imageFrame(slide, SHOT("receipt.png"), 5.25, 1.22, 4.11, 3.20);
  addText(slide, "Success receipt after Post/Redirect/Get.", 5.25, 4.58, 4.11, 0.24, { fontSize: 12, color: C.primary, align: "right" });
}

// 8 — omissions and ordered next steps
{
  const slide = base("Tradeoffs and roadmap", "The omissions are conscious. The roadmap begins with the correctness gap that Post/Redirect/Get does not solve: a retried network request. The phone capture proves the existing presentation adapter already reflows at 380 pixels.\n\nIn a real bank, the Domain and Application layers would become a backend service behind an API that every channel (ATM, mobile, web, branch) shares, so the rules live in one place. This ATM would be one thin client. Splitting that backend into microservices becomes worthwhile once separate teams own accounts, transfers, and fraud and need to deploy independently. The inward dependencies here are what make that extraction straightforward.");
  kicker(slide, "Omitted", 0.64, 1.30, 2.45, C.secondary);
  ["Identity", "Banking breadth", "Distributed operations"].forEach((value, i) => addText(slide, value, 0.64, 1.78 + i * 0.72, 2.45, 0.35, { fontSize: 19, bold: true, fit: "shrink" }));
  slide.addShape(pptx.ShapeType.line, { x: 3.34, y: 1.29, w: 0, h: 3.34, line: { color: C.line, width: 1 } });
  [["Next", "Double-submit protection, auditing, authentication"], ["Then", "Accessibility, observability, load tests"], ["Later", "Deployment, browser coverage"]].forEach(([stage, items], i) => { const y = 1.30 + i * 1.07; kicker(slide, stage, 3.70, y, 0.80, C.primary); addText(slide, items, 3.70, y + 0.38, 3.00, 0.52, { fontSize: 16, bold: true, fit: "shrink" }); });
  imageFrame(slide, SHOT("phone.png"), 7.64, 1.18, 1.10, 3.62);
  addText(slide, "Responsive at 380 px.", 6.90, 4.91, 2.58, 0.18, { fontSize: 10.5, color: C.primary, align: "center", fit: "shrink" });
}

// pptxgenjs writes each slide's notes as one run with embedded line breaks and cannot
// hyperlink notes text. Split the run into real paragraphs and link NOTE_LINKS terms.
async function linkNotes(zip, notesPath) {
  const relsPath = notesPath.replace("notesSlides/", "notesSlides/_rels/") + ".rels";
  let xml = await zip.file(notesPath).async("string");
  let rels = await zip.file(relsPath).async("string");
  const rPr = '<a:rPr lang="en-US" dirty="0"/>';
  const single = /<a:p><a:r><a:rPr lang="en-US" dirty="0"\/><a:t>([\s\S]*?)<\/a:t><\/a:r><a:endParaRPr lang="en-US" dirty="0"\/><\/a:p>/;
  const match = xml.match(single);
  if (!match) return;
  let nextId = (rels.match(/Id="rId\d+"/g) || []).length + 1;
  const terms = Object.keys(NOTE_LINKS);
  const pattern = new RegExp(`(${terms.map((t) => t.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")).join("|")})`);
  const paragraphs = match[1].split(/\r?\n/).map((line) => {
    const runs = line.split(pattern).filter(Boolean).map((part) => {
      if (!NOTE_LINKS[part]) return `<a:r>${rPr}<a:t>${part}</a:t></a:r>`;
      const id = `rId${nextId++}`;
      rels = rels.replace("</Relationships>", `<Relationship Id="${id}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink" Target="${NOTE_LINKS[part]}" TargetMode="External"/></Relationships>`);
      return `<a:r><a:rPr lang="en-US" dirty="0"><a:hlinkClick r:id="${id}"/></a:rPr><a:t>${part}</a:t></a:r>`;
    });
    return `<a:p>${runs.join("")}<a:endParaRPr lang="en-US" dirty="0"/></a:p>`;
  });
  zip.file(notesPath, xml.replace(single, () => paragraphs.join("")));
  zip.file(relsPath, rels);
}

// pptxgenjs sizes the notes page as the slide rotated (5.625 x 10 in) but keeps a notes
// master laid out for 7.5 in, so printed notes pages clip on the right. Use US Letter
// with 0.5 in margins and re-center every notes placeholder. Values are EMU.
const NOTES_PAGE = { cx: 7772400, cy: 10058400 };
const NOTES_GEOMETRY = {
  "0,0,2971800,458788": [457200, 228600, 3429000, 458788], // header
  "3884613,0,2971800,458788": [3886200, 228600, 3429000, 458788], // date
  "685800,1143000,5486400,3086100": [457200, 685800, 6858000, 3857625], // slide image
  "685800,4400550,5486400,3600450": [457200, 4800600, 6858000, 4480560], // notes body
  "0,8685213,2971800,458787": [457200, 9371013, 3429000, 458787], // footer
  "3884613,8685213,2971800,458787": [3886200, 9371013, 3429000, 458787], // slide number
};
function fixNotesGeometry(xml) {
  return xml.replace(/<a:off x="(\d+)" y="(\d+)"\/><a:ext cx="(\d+)" cy="(\d+)"\/>/g, (all, x, y, cx, cy) => {
    const next = NOTES_GEOMETRY[[x, y, cx, cy].join(",")];
    return next ? `<a:off x="${next[0]}" y="${next[1]}"/><a:ext cx="${next[2]}" cy="${next[3]}"/>` : all;
  });
}

async function writeDeck() {
  await pptx.writeFile({ fileName: OUT, compression: true });
  const zip = await JSZip.loadAsync(fs.readFileSync(OUT));
  const contentTypesPath = "[Content_Types].xml";
  let contentTypes = await zip.file(contentTypesPath).async("string");
  contentTypes = contentTypes.replace(/<Override PartName="\/ppt\/slideMasters\/slideMaster(?:[2-9]|[1-9][0-9]+)\.xml" ContentType="application\/vnd\.openxmlformats-officedocument\.presentationml\.slideMaster\+xml"\/>/g, "");
  zip.file(contentTypesPath, contentTypes);
  for (const notesPath of Object.keys(zip.files).filter((name) => /^ppt\/notesSlides\/notesSlide\d+\.xml$/.test(name))) await linkNotes(zip, notesPath);
  const presentationPath = "ppt/presentation.xml";
  const presentation = await zip.file(presentationPath).async("string");
  zip.file(presentationPath, presentation.replace(/<p:notesSz[^>]*\/>/, `<p:notesSz cx="${NOTES_PAGE.cx}" cy="${NOTES_PAGE.cy}"/>`));
  for (const notesPath of Object.keys(zip.files).filter((name) => /^ppt\/(notesMasters\/notesMaster|notesSlides\/notesSlide)\d+\.xml$/.test(name))) {
    zip.file(notesPath, fixNotesGeometry(await zip.file(notesPath).async("string")));
  }
  fs.writeFileSync(OUT, await zip.generateAsync({ type: "nodebuffer", compression: "DEFLATE" }));
  console.log("wrote", OUT);
}
writeDeck();
