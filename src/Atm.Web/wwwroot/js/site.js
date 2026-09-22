// Progressive enhancement only: the page works fully without this script.
(function () {
  "use strict";

  // Switch operation panels without a round trip; the tab links still work as links.
  var tabs = Array.prototype.slice.call(document.querySelectorAll('[role="tab"]'));
  function select(tab) {
    tabs.forEach(function (t) {
      var selected = t === tab;
      t.setAttribute("aria-selected", selected ? "true" : "false");
      t.setAttribute("tabindex", selected ? "0" : "-1");
      var panel = document.getElementById(t.getAttribute("aria-controls"));
      if (panel) { panel.hidden = !selected; }
    });
    window.history.replaceState(null, "", tab.getAttribute("href"));
  }
  // Roving tabindex is applied only here: without this script every tab stays
  // a plain, focusable link.
  tabs.forEach(function (t) {
    t.setAttribute("tabindex", t.getAttribute("aria-selected") === "true" ? "0" : "-1");
  });
  tabs.forEach(function (tab, index) {
    tab.addEventListener("click", function (event) {
      event.preventDefault();
      select(tab);
    });
    tab.addEventListener("keydown", function (event) {
      var next = event.key === "ArrowRight" ? index + 1 : event.key === "ArrowLeft" ? index - 1 : -1;
      if (next < 0 || next >= tabs.length) { return; }
      event.preventDefault();
      select(tabs[next]);
      tabs[next].focus();
    });
  });

  // Confirm withdrawals and transfers, then block a second submission while processing.
  var currency = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" });
  function describe(form, template) {
    return template.replace(/\{(\w+)\}/g, function (_, name) {
      var field = form.querySelector('[data-field="' + name + '"]');
      if (!field) { return name; }
      if (field.tagName === "SELECT") { return field.options[field.selectedIndex].text; }
      var value = parseFloat(field.value);
      return isNaN(value) ? field.value : currency.format(value);
    });
  }
  Array.prototype.slice.call(document.querySelectorAll("form.op-form")).forEach(function (form) {
    form.addEventListener("submit", function (event) {
      if (form.dataset.submitting === "true") { event.preventDefault(); return; }
      var template = form.getAttribute("data-confirm");
      if (template && !window.confirm(describe(form, template))) { event.preventDefault(); return; }
      form.dataset.submitting = "true";
      var button = form.querySelector('button[type="submit"]');
      if (button) { button.disabled = true; button.textContent = "Processing…"; }
    });
  });
})();
