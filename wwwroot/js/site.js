// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/* SkyPulse Airlines — interactions & micro-animations (part 1) */
(function () {
  "use strict";
  var $ = function (s, c) { return (c || document).querySelector(s); };
  var $$ = function (s, c) { return Array.prototype.slice.call((c || document).querySelectorAll(s)); };
  var reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* Navbar scroll state */
  var nav = $(".site-nav");
  function onScroll() { if (nav) nav.classList.toggle("is-scrolled", window.scrollY > 24); }
  onScroll();
  window.addEventListener("scroll", onScroll, { passive: true });

  /* Mobile nav */
  var toggle = $("[data-nav-toggle]"), menu = $("#navMenu");
  if (toggle && menu) {
    toggle.addEventListener("click", function () {
      var open = menu.classList.toggle("open");
      toggle.classList.toggle("open", open);
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
    });
    $$(".nav-links a", menu).forEach(function (a) {
      a.addEventListener("click", function () { menu.classList.remove("open"); toggle.classList.remove("open"); });
    });
  }

  /* Reveal on scroll (staggered) */
  var revealEls = $$(".reveal");
  if ("IntersectionObserver" in window && !reduced) {
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (e.isIntersecting) { e.target.classList.add("in"); io.unobserve(e.target); }
      });
    }, { threshold: 0.12, rootMargin: "0px 0px -30px 0px" });
    revealEls.forEach(function (el, i) {
      el.style.transitionDelay = (el.dataset.delay || (i % 6) * 60) + "ms";
      io.observe(el);
    });
  } else { revealEls.forEach(function (el) { el.classList.add("in"); }); }

  /* Animated counters */
  function fmt(el, v) {
    var d = parseInt(el.dataset.decimals || "0", 10);
    el.textContent = (el.dataset.prefix || "") + Number(v).toLocaleString(undefined, { minimumFractionDigits: d, maximumFractionDigits: d }) + (el.dataset.suffix || "");
  }
  function runCounter(el) {
    var target = parseFloat(el.dataset.count || "0");
    if (reduced) { fmt(el, target); return; }
    var start = performance.now(), dur = 1500;
    (function step(now) {
      var p = Math.min(1, (now - start) / dur), e = 1 - Math.pow(1 - p, 3);
      fmt(el, target * e);
      if (p < 1) requestAnimationFrame(step);
    })(start);
  }
  var counters = $$("[data-count]");
  if ("IntersectionObserver" in window) {
    var cio = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) { if (e.isIntersecting) { runCounter(e.target); cio.unobserve(e.target); } });
    }, { threshold: 0.4 });
    counters.forEach(function (el) { cio.observe(el); });
  } else { counters.forEach(runCounter); }

  /* Toasts */
  var stack = $("#toastStack");
  function toast(msg, type) {
    if (!stack || !msg) return;
    var t = document.createElement("div");
    t.className = "toast-item " + (type || "success");
    t.setAttribute("role", "status");
    var ico = document.createElement("span"); ico.className = "toast-ico";
    ico.innerHTML = type === "error" ? "!" : "&#10003;";
    var body = document.createElement("div"); body.textContent = msg;
    var x = document.createElement("button"); x.className = "toast-x"; x.type = "button"; x.setAttribute("aria-label", "Dismiss"); x.innerHTML = "&times;";
    x.addEventListener("click", kill);
    t.appendChild(ico); t.appendChild(body); t.appendChild(x);
    stack.appendChild(t);
    requestAnimationFrame(function () { requestAnimationFrame(function () { t.classList.add("show"); }); });
    var killed = false;
    function kill() { if (killed) return; killed = true; t.classList.remove("show"); setTimeout(function () { t.remove(); }, 400); }
    setTimeout(kill, 5200);
  }
  window.skyToast = toast;
  if (document.body.dataset.toastMessage) toast(document.body.dataset.toastMessage, "success");
  if (document.body.dataset.toastError) toast(document.body.dataset.toastError, "error");
  /* Hero parallax */
  var px = $("[data-parallax]");
  if (px && !reduced) {
    var ticking = false;
    window.addEventListener("scroll", function () {
      if (ticking) return; ticking = true;
      requestAnimationFrame(function () {
        var y = window.scrollY;
        if (y < 1000) px.style.transform = "translate3d(0," + (y * 0.22).toFixed(1) + "px,0) scale(1.05)";
        ticking = false;
      });
    }, { passive: true });
  }

  /* Trip-type tabs (round trip vs one way) */
  $$("[data-trip-toggle]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      $$("[data-trip-toggle]").forEach(function (x) { x.classList.remove("active"); x.setAttribute("aria-selected", "false"); });
      btn.classList.add("active"); btn.setAttribute("aria-selected", "true");
      var ret = document.getElementById(btn.dataset.returnField);
      if (ret) {
        ret.disabled = btn.dataset.trip === "oneway";
        var wrap = ret.closest(".field");
        if (wrap) wrap.classList.toggle("is-disabled", ret.disabled);
        if (ret.disabled) ret.value = "";
      }
    });
  });

  /* Date fields: not in the past */
  $$("input[data-min-today]").forEach(function (i) { i.min = new Date().toISOString().slice(0, 10); });

  /* Skeleton while searching */
  $$("form[data-skeleton]").forEach(function (f) {
    f.addEventListener("submit", function (e) {
      if (f.dataset.sent) return;
      if (!f.checkValidity()) return;
      e.preventDefault();
      var ov = $("#skeletonOverlay");
      if (ov && !reduced) {
        ov.classList.add("on");
        f.dataset.sent = "1";
        setTimeout(function () { f.submit(); }, 850);
      } else { f.dataset.sent = "1"; f.submit(); }
    });
  });

  /* Flight results sorting */
  var list = $("#flightList");
  $$("[data-sort]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      if (!list) return;
      $$("[data-sort]").forEach(function (x) { x.classList.remove("active"); x.setAttribute("aria-pressed", "false"); });
      btn.classList.add("active"); btn.setAttribute("aria-pressed", "true");
      var k = btn.dataset.sort;
      var cards = $$(".flight-card", list);
      cards.sort(function (a, c) {
        if (k === "price") return parseFloat(a.dataset.price) - parseFloat(c.dataset.price);
        if (k === "duration") return parseFloat(a.dataset.duration) - parseFloat(c.dataset.duration);
        return a.dataset.dep.localeCompare(c.dataset.dep);
      });
      cards.forEach(function (c) { list.appendChild(c); });
      if (!reduced) cards.forEach(function (c, i) {
        c.classList.remove("in"); c.style.transition = "none";
        void c.offsetHeight; c.style.transition = "";
        setTimeout(function () { c.classList.add("in"); }, 40 + i * 55);
      });
    });
  });

  /* Confirm modals */
  var pendingForm = null;
  document.addEventListener("submit", function (e) {
    var f = e.target;
    if (f.matches && f.matches("form[data-confirm]") && !f.dataset.confirmed) {
      e.preventDefault();
      pendingForm = f;
      var modalEl = document.getElementById("confirmModal");
      if (!modalEl || !window.bootstrap) { f.dataset.confirmed = "1"; f.submit(); return; }
      var bodyEl = modalEl.querySelector(".modal-body");
      if (bodyEl) bodyEl.textContent = f.dataset.confirmMessage || "Are you sure you want to continue?";
      window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }
  });
  var ok = $("#confirmOk");
  if (ok) ok.addEventListener("click", function () {
    if (pendingForm) { pendingForm.dataset.confirmed = "1"; pendingForm.submit(); }
  });

  /* Admin table search + filter */
  $$("[data-table-filter]").forEach(function (inp) {
    var t = $(inp.dataset.tableFilter); if (!t) return;
    inp.addEventListener("input", function () {
      var q = inp.value.toLowerCase();
      $$("tbody tr", t).forEach(function (tr) { tr.style.display = tr.textContent.toLowerCase().indexOf(q) > -1 ? "" : "none"; });
    });
  });
  $$("[data-table-select]").forEach(function (sel) {
    var t = $(sel.dataset.tableSelect); if (!t) return;
    sel.addEventListener("change", function () {
      var v = sel.value;
      $$("tbody tr", t).forEach(function (tr) { tr.style.display = (v === "all" || tr.dataset.status === v) ? "" : "none"; });
    });
  });

  /* Reschedule option selection */
  $$(".alt-option input[type=radio]").forEach(function (r) {
    r.addEventListener("change", function () {
      $$(".alt-option").forEach(function (o) { o.classList.remove("selected"); });
      r.closest(".alt-option").classList.add("selected");
      var s = $("#rescheduleSubmit"); if (s) s.disabled = false;
    });
  });

  /* Admin sidebar (mobile) */
  var sidebar = $(".admin-sidebar"), mbtn = $("[data-admin-menu]"), scrim = $("#adminScrim");
  function closeSidebar() { if (sidebar) sidebar.classList.remove("open"); if (scrim) scrim.classList.remove("on"); }
  if (mbtn) mbtn.addEventListener("click", function () {
    sidebar.classList.toggle("open"); if (scrim) scrim.classList.toggle("on", sidebar.classList.contains("open"));
  });
  if (scrim) scrim.addEventListener("click", closeSidebar);
  $$(".admin-nav a").forEach(function (a) { a.addEventListener("click", closeSidebar); });

  /* Cabin filter on search results */
  var cabinSel = $("[data-cabin-filter]");
  if (cabinSel) cabinSel.addEventListener("change", function () {
    var cabin = cabinSel.value;
    var list = $("#flightList");
    if (!list) { if (cabin !== "biz") return; }
    $$(".flight-card", list).forEach(function (c) {
      if (cabin === "biz") { c.style.display = parseFloat(c.dataset.priceBiz || "0") > 0 ? "" : "none"; }
      else if (cabin === "eco") { c.style.display = ""; }
      else { c.style.display = ""; }
    });
  });

  /* Animate chart bars when visible */
  var fills = $$(".hbar .fill");
  if ("IntersectionObserver" in window) {
    var fio = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (e.isIntersecting) { e.target.style.width = (e.target.dataset.width || 0) + "%"; fio.unobserve(e.target); }
      });
    }, { threshold: 0.3 });
    fills.forEach(function (f) { fio.observe(f); });
  } else { fills.forEach(function (f) { f.style.width = (f.dataset.width || 0) + "%"; }); }
})();
