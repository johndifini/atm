// Easter egg: the Konami code (↑ ↑ ↓ ↓ ← → ← → B A) toggles a Matrix theme.
// Presentation only — it never reads, changes, or submits form values, and the
// ATM stays fully usable underneath. The choice lasts for this browser tab's
// session; the early class in _Layout's <head> prevents a flash on reload.
(function () {
  "use strict";

  var STORAGE_KEY = "atm.matrix";
  var CLASS_NAME = "matrix";
  var SEQUENCE = ["arrowup", "arrowup", "arrowdown", "arrowdown",
    "arrowleft", "arrowright", "arrowleft", "arrowright", "b", "a"];
  var root = document.documentElement;
  var reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

  function remember(on) {
    try {
      if (on) { window.sessionStorage.setItem(STORAGE_KEY, "on"); }
      else { window.sessionStorage.removeItem(STORAGE_KEY); }
    } catch (e) { /* Storage can be blocked; the theme still works for this page. */ }
  }

  // Digital rain on a fixed canvas behind the content.
  var canvas = null;
  var context = null;
  var drops = [];
  var frame = 0;
  var lastDraw = 0;
  var GLYPH_SIZE = 16;
  var GLYPHS = "ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ0123456789$$$";

  function resize() {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    var columns = Math.ceil(canvas.width / GLYPH_SIZE);
    drops = [];
    for (var i = 0; i < columns; i++) {
      drops.push(Math.floor(Math.random() * -canvas.height / GLYPH_SIZE));
    }
    context.fillStyle = "#000";
    context.fillRect(0, 0, canvas.width, canvas.height);
  }

  function draw(time) {
    frame = window.requestAnimationFrame(draw);
    if (time - lastDraw < 50) { return; } // ~20 fps reads as "rain", not a blur.
    lastDraw = time;
    context.fillStyle = "rgba(0, 0, 0, 0.08)"; // Fading trails.
    context.fillRect(0, 0, canvas.width, canvas.height);
    context.font = GLYPH_SIZE + "px monospace";
    for (var i = 0; i < drops.length; i++) {
      var glyph = GLYPHS.charAt(Math.floor(Math.random() * GLYPHS.length));
      var y = drops[i] * GLYPH_SIZE;
      context.fillStyle = Math.random() < 0.04 ? "#D8FFD8" : "#00C83C";
      context.fillText(glyph, i * GLYPH_SIZE, y);
      if (y > canvas.height && Math.random() > 0.975) { drops[i] = 0; }
      drops[i]++;
    }
  }

  function startRain() {
    if (canvas || reducedMotion.matches) { return; }
    canvas = document.createElement("canvas");
    canvas.className = "matrix-rain";
    canvas.setAttribute("aria-hidden", "true");
    document.body.insertBefore(canvas, document.body.firstChild);
    context = canvas.getContext("2d");
    resize();
    window.addEventListener("resize", resize);
    frame = window.requestAnimationFrame(draw);
  }

  function stopRain() {
    if (!canvas) { return; }
    window.cancelAnimationFrame(frame);
    window.removeEventListener("resize", resize);
    canvas.remove();
    canvas = null;
    context = null;
  }

  // The red pill / blue pill control sits in the header, so it never covers the
  // forms or history; it is shown only while the theme is on.
  var pills = null;

  function showPills() {
    if (pills) { return; }
    pills = document.createElement("div");
    pills.className = "matrix-pills";
    pills.setAttribute("role", "region");
    pills.setAttribute("aria-label", "Matrix mode");
    pills.innerHTML =
      '<p class="matrix-pills-prompt" aria-live="polite">Stay in the Matrix?</p>' +
      '<div class="matrix-pills-actions">' +
      '<button type="button" class="matrix-pill matrix-pill-red">Red pill</button>' +
      '<button type="button" class="matrix-pill matrix-pill-blue">Blue pill</button>' +
      "</div>";
    pills.querySelector(".matrix-pill-red").addEventListener("click", function () {
      pills.querySelector(".matrix-pills-prompt").textContent =
        "Welcome to the real world. Balances here are decimal.";
    });
    pills.querySelector(".matrix-pill-blue").addEventListener("click", function () {
      setMatrix(false);
    });
    (document.querySelector(".site-header .container") || document.body).appendChild(pills);
  }

  function hidePills() {
    if (!pills) { return; }
    pills.remove();
    pills = null;
  }

  function setMatrix(on) {
    root.classList.toggle(CLASS_NAME, on);
    remember(on);
    if (on) { startRain(); showPills(); }
    else { stopRain(); hidePills(); }
  }

  function isOn() { return root.classList.contains(CLASS_NAME); }

  // Follow a live change to the OS reduced-motion setting.
  reducedMotion.addEventListener("change", function () {
    if (!isOn()) { return; }
    if (reducedMotion.matches) { stopRain(); } else { startRain(); }
  });

  // A rolling window of recent keys, so extra presses (↑ ↑ ↑ …) still match.
  var recent = [];
  var TARGET = SEQUENCE.join(" ");
  document.addEventListener("keydown", function (event) {
    if (event.key === "Escape" && isOn()) { setMatrix(false); return; }
    // Arrow keys have real jobs in fields and the tablist; never hijack them there.
    var target = event.target;
    if (target.closest && target.closest("input, select, textarea, [role='tab']")) {
      recent = [];
      return;
    }
    recent.push((event.key || "").toLowerCase());
    if (recent.length > SEQUENCE.length) { recent.shift(); }
    if (recent.join(" ") === TARGET) {
      recent = [];
      setMatrix(!isOn());
    }
  });

  // Restore a session that was already in the Matrix.
  if (isOn()) { setMatrix(true); }
})();
