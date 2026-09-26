// Rebuilds the seven-slide ATM case-study deck from real application screenshots.
// Approved Jony Vibe system: charcoal canvas, soft-white type, one restrained
// accent per slide, and a 36-word target for authored on-slide copy.
const pptxgen = require("pptxgenjs");
const path = require("path");
const fs = require("fs");
const JSZip = require("jszip");

const DESIGN = path.resolve(__dirname, "..");
const SHOT = (name) => path.join(DESIGN, "screenshots", name);
const OUT = process.argv[2] ? path.resolve(process.argv[2]) : path.join(DESIGN, "atm-deck.pptx");
const C = { page: "121212", surface: "1B1B1B", text: "F5F5F5", muted: "A9ADB3", line: "333333", green: "00F604", orange: "F67F00", blue: "0077F6" };
const FONT = "Avenir Next";
const pptx = new pptxgen();
pptx.layout = "LAYOUT_16x9";
pptx.author = "John DiFini";
pptx.subject = "ATM coding exercise: architecture, correctness, and delivery";
pptx.title = "ATM Coding Exercise";
pptx.lang = "en-US";
pptx.theme = { headFontFace: FONT, bodyFontFace: FONT, lang: "en-US" };
let slideNumber = 0;

function addText(slide, value, x, y, w, h, options = {}) {
  slide.addText(value, { x, y, w, h, fontFace: FONT, fontSize: 17, color: C.text, margin: 0, isTextBox: true, valign: "top", ...options });
}
function line(slide, x, y, w, color = C.line, width = 0.75) {
  slide.addShape(pptx.ShapeType.line, { x, y, w, h: 0, line: { color, width } });
}
function footer(slide) {
  line(slide, 0.64, 5.19, 8.72);
  addText(slide, String(slideNumber).padStart(2, "0"), 8.92, 5.24, 0.44, 0.18, { fontSize: 10, color: C.muted, align: "right" });
}
function base(title, accent, notes) {
  const slide = pptx.addSlide(); slideNumber += 1; slide.background = { color: C.page };
  slide.addShape(pptx.ShapeType.rect, { x: 0.64, y: 0.39, w: 0.10, h: 0.48, fill: { color: accent }, line: { color: accent, transparency: 100 } });
  addText(slide, title, 0.91, 0.39, 8.45, 0.56, { fontSize: title.length > 36 ? 29 : 32, bold: true, fit: "shrink" });
  footer(slide); if (notes) slide.addNotes(notes); return slide;
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
  ["ATM Coding Exercise", "Financial correctness, made inspectable.", "Two accounts, persisted locally, covered by 143 tests.", "Real application. Real transactions."],
  ["Problem framing", "Goals", "Two persistent accounts", "Deposit, withdraw, transfer", "Explain every balance", "Boundaries", "One local user", "No speculative infrastructure", "Clarity is the quality bar."],
  ["Architecture and dependency direction", "Browser", "Web", "Application", "Domain", "Infrastructure", "composition only", "implements ports", "Business rules depend on nothing external."],
  ["Decisions and costs", "Decision", "Cost", "Razor Pages", "Less client interactivity", "Modular monolith", "Extra project structure", "SQLite + EF Core", "Single-writer ceiling", "decimal Money", "USD remains implicit", "Single user", "No user isolation"],
  ["Failure leaves no partial state", "Validate before mutation", "Commit balance and history together", "Reject stale writes", "Rejected overdraft. Persisted state stays unchanged."],
  ["143 tests. Every boundary.", "75 Domain", "33 Application", "35 Integration + HTTP", "Invariants", "Orchestration", "Persistence and HTTP", "A real receipt after redirect."],
  ["Tradeoffs and roadmap", "Omitted", "Identity", "Banking breadth", "Distributed operations", "Next", "Idempotency, auditing, authentication", "Then", "Accessibility, observability, load tests", "Later", "Deployment, browser coverage", "Responsive at 380 px."],
];
WORD_BUDGETS.forEach((parts, index) => { const count = wordCount(parts); if (count > 36) throw new Error(`Slide ${index + 1} has ${count} authored words; limit is 36.`); });

// 1 — outcome first
{
  const slide = pptx.addSlide(); slideNumber += 1; slide.background = { color: C.page };
  kicker(slide, "Case study", 0.64, 0.50, 2.8, C.green);
  addText(slide, "ATM", 0.64, 0.93, 3.10, 0.65, { fontSize: 47, bold: true, color: C.green });
  addText(slide, "Coding Exercise", 0.64, 1.60, 3.48, 0.54, { fontSize: 27, bold: true, fit: "shrink" });
  addText(slide, "Financial correctness,\nmade inspectable.", 0.64, 2.48, 3.20, 1.02, { fontSize: 24, bold: true, breakLine: true, fit: "shrink" });
  addText(slide, "Two accounts, persisted locally,\ncovered by 143 tests.", 0.64, 3.82, 3.18, 0.62, { fontSize: 15, color: C.muted, breakLine: true });
  imageFrame(slide, SHOT("dashboard.png"), 4.17, 0.50, 5.19, 4.04);
  addText(slide, "Real application. Real transactions.", 4.17, 4.68, 5.19, 0.23, { fontSize: 11.5, color: C.green });
  footer(slide);
  slide.addNotes("Open with the finished product. This is the real application using a scratch SQLite database, not a mock. The intentionally narrow scope keeps the review on correctness, architectural boundaries, and evidence.");
}

// 2 — scope as contrast
{
  const slide = base("Problem framing", C.orange, "The exercise is deliberately smaller than a retail bank. Two accounts and three operations are enough to expose the financial rules. Everything else is excluded until a real requirement earns the complexity.");
  kicker(slide, "Goals", 0.64, 1.34, 3.75, C.orange); kicker(slide, "Boundaries", 5.08, 1.34, 3.75, C.orange);
  slide.addShape(pptx.ShapeType.line, { x: 4.72, y: 1.31, w: 0, h: 2.80, line: { color: C.line, width: 1 } });
  ["Two persistent accounts", "Deposit, withdraw, transfer", "Explain every balance"].forEach((value, i) => addText(slide, value, 0.64, 1.93 + i * 0.70, 3.82, 0.36, { fontSize: 18.5, bold: true, fit: "shrink" }));
  ["One local user", "No speculative infrastructure"].forEach((value, i) => addText(slide, value, 5.08, 1.93 + i * 0.94, 4.04, 0.48, { fontSize: 20, bold: true, fit: "shrink" }));
  line(slide, 0.64, 4.28, 8.72, C.orange, 2.2);
  addText(slide, "Clarity is the quality bar.", 0.64, 4.48, 8.72, 0.37, { fontSize: 24, bold: true, color: C.orange });
}

// 3 — editable dependency diagram
{
  const slide = base("Architecture and dependency direction", C.blue, "Solid arrows show dependencies pointing inward. Infrastructure implements Application ports. Web references Infrastructure only in the composition root. ADR-0002 records this modular-monolith boundary.");
  const nodes = [[0.64, 1.40, "Browser"], [2.34, 1.56, "Web"], [4.30, 1.78, "Application"], [6.49, 2.87, "Domain"]];
  nodes.forEach(([x, w, title], i) => { const strong = i === 3; slide.addShape(pptx.ShapeType.rect, { x, y: 1.72, w, h: 1.10, fill: { color: strong ? C.blue : C.surface }, line: { color: strong ? C.blue : C.line, width: strong ? 1.3 : 0.8 } }); addText(slide, title, x + 0.10, 2.08, w - 0.20, 0.30, { fontSize: 16, bold: true, align: "center", color: strong ? C.text : C.muted, fit: "shrink" }); });
  arrow(slide, 2.04, 2.27, 2.34, 2.27, C.blue); arrow(slide, 3.90, 2.27, 4.30, 2.27, C.blue); arrow(slide, 6.08, 2.27, 6.49, 2.27, C.blue);
  slide.addShape(pptx.ShapeType.rect, { x: 4.30, y: 3.45, w: 1.78, h: 0.80, fill: { color: C.surface }, line: { color: C.line, width: 0.8 } });
  addText(slide, "Infrastructure", 4.42, 3.70, 1.54, 0.26, { fontSize: 14, bold: true, align: "center", color: C.muted, fit: "shrink" });
  arrow(slide, 5.19, 3.45, 5.19, 2.82, C.blue); addText(slide, "implements ports", 5.38, 3.04, 1.20, 0.20, { fontSize: 10.5, color: C.muted });
  arrow(slide, 3.14, 2.82, 4.30, 3.84, C.muted, true); addText(slide, "composition only", 2.88, 3.42, 1.24, 0.20, { fontSize: 10.5, color: C.muted });
  addText(slide, "Business rules depend on nothing external.", 0.64, 4.64, 8.72, 0.31, { fontSize: 20, bold: true, color: C.blue, align: "center" });
}

// 4 — decision ledger
{
  const slide = base("Decisions and costs", C.orange, "The ADRs explain why each choice fits the exercise. This slide keeps the tradeoff visible: every simplification accepts a constraint. The common choice is the smallest architecture that keeps rules and boundaries legible.");
  const header = (value) => ({ text: value, options: { bold: true, color: C.orange, fill: { color: C.page }, fontFace: FONT, fontSize: 12.5 } });
  const cell = (value, options = {}) => ({ text: value, options: { fontFace: FONT, fontSize: 17, color: C.text, valign: "middle", ...options } });
  const rows = [[header("DECISION"), header("COST")], [cell("Razor Pages", { bold: true }), cell("Less client interactivity", { color: C.muted })], [cell("Modular monolith", { bold: true }), cell("Extra project structure", { color: C.muted })], [cell("SQLite + EF Core", { bold: true }), cell("Single-writer ceiling", { color: C.muted })], [cell("decimal Money", { bold: true }), cell("USD remains implicit", { color: C.muted })], [cell("Single user", { bold: true }), cell("No user isolation", { color: C.muted })]];
  slide.addTable(rows, { x: 0.64, y: 1.27, w: 8.72, h: 3.66, colW: [4.35, 4.37], rowH: [0.42, 0.65, 0.65, 0.65, 0.65, 0.64], border: { type: "solid", color: C.line, pt: 0.75 }, fill: { color: C.page }, margin: 0.10 });
}

// 5 — correctness in one failure state
{
  const slide = base("Failure leaves no partial state", C.orange, "The screenshot is a real rejected overdraft. The typed value stays visible, the error names the available balance, and persisted state is unchanged. Account versions turn stale writes into a safe retry instead of lost data.");
  ["Validate before mutation", "Commit balance and history together", "Reject stale writes"].forEach((value, i) => { addText(slide, String(i + 1).padStart(2, "0"), 0.64, 1.43 + i * 1.10, 0.45, 0.30, { fontSize: 13, bold: true, color: C.orange }); addText(slide, value, 1.22, 1.38 + i * 1.10, 3.00, 0.56, { fontSize: 20, bold: true, fit: "shrink" }); });
  imageFrame(slide, SHOT("overdraft.png"), 4.63, 1.24, 4.73, 3.59);
  addText(slide, "Rejected overdraft. Persisted state stays unchanged.", 4.63, 4.91, 4.73, 0.22, { fontSize: 11.5, bold: true, color: C.orange, align: "right", fit: "shrink" });
}

// 6 — evidence over commands
{
  const slide = base("143 tests. Every boundary.", C.green, "Domain tests are pure. Application tests use hand-written fakes. Integration and HTTP tests exercise the real host with an isolated SQLite file per test, including concurrency conflicts, atomic rollback, Post/Redirect/Get, security, and accessibility structure. Commands: dotnet restore Atm.sln; dotnet build Atm.sln --no-restore; dotnet test Atm.sln --no-build.");
  [["75", "Domain", "Invariants"], ["33", "Application", "Orchestration"], ["35", "Integration + HTTP", "Persistence and HTTP"]].forEach(([n, label, scope], i) => { const y = 1.34 + i * 1.07; addText(slide, n, 0.64, y, 0.92, 0.52, { fontSize: 34, bold: true, color: C.green }); addText(slide, label, 1.65, y + 0.02, 2.27, 0.28, { fontSize: 16, bold: true, fit: "shrink" }); addText(slide, scope, 1.65, y + 0.38, 2.27, 0.24, { fontSize: 12, color: C.muted, fit: "shrink" }); });
  imageFrame(slide, SHOT("receipt.png"), 5.25, 1.22, 4.11, 3.20);
  addText(slide, "A real receipt after redirect.", 5.25, 4.58, 4.11, 0.24, { fontSize: 12, color: C.green, align: "right" });
}

// 7 — omissions and ordered next steps
{
  const slide = base("Tradeoffs and roadmap", C.blue, "The omissions are conscious. The roadmap begins with the correctness gap that Post/Redirect/Get does not solve: a retried network request. The phone capture proves the existing presentation adapter already reflows at 380 pixels.");
  kicker(slide, "Omitted", 0.64, 1.30, 2.45, C.blue);
  ["Identity", "Banking breadth", "Distributed operations"].forEach((value, i) => addText(slide, value, 0.64, 1.78 + i * 0.72, 2.45, 0.35, { fontSize: 19, bold: true, fit: "shrink" }));
  slide.addShape(pptx.ShapeType.line, { x: 3.34, y: 1.29, w: 0, h: 3.34, line: { color: C.line, width: 1 } });
  [["Next", "Idempotency, auditing, authentication"], ["Then", "Accessibility, observability, load tests"], ["Later", "Deployment, browser coverage"]].forEach(([stage, items], i) => { const y = 1.30 + i * 1.07; kicker(slide, stage, 3.70, y, 0.80, C.blue); addText(slide, items, 3.70, y + 0.38, 2.72, 0.52, { fontSize: 16, bold: true, fit: "shrink" }); });
  imageFrame(slide, SHOT("phone.png"), 7.64, 1.18, 1.10, 3.62);
  addText(slide, "Responsive at 380 px.", 6.90, 4.91, 2.58, 0.18, { fontSize: 10.5, color: C.blue, align: "center", fit: "shrink" });
}

async function writeDeck() {
  await pptx.writeFile({ fileName: OUT, compression: true });
  const zip = await JSZip.loadAsync(fs.readFileSync(OUT));
  const contentTypesPath = "[Content_Types].xml";
  let contentTypes = await zip.file(contentTypesPath).async("string");
  contentTypes = contentTypes.replace(/<Override PartName="\/ppt\/slideMasters\/slideMaster(?:[2-9]|[1-9][0-9]+)\.xml" ContentType="application\/vnd\.openxmlformats-officedocument\.presentationml\.slideMaster\+xml"\/>/g, "");
  zip.file(contentTypesPath, contentTypes);
  fs.writeFileSync(OUT, await zip.generateAsync({ type: "nodebuffer", compression: "DEFLATE" }));
  console.log("wrote", OUT);
}
writeDeck();
