"use strict";

const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

const fmtMoney = (n) =>
  new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n || 0);
const fmtInt = (n) => new Intl.NumberFormat("es-ES").format(n || 0);
const fmtDate = (v) => (v ? new Date(v).toLocaleDateString("es-ES") : "—");

function rangeQuery() {
  const from = $("#from").value ? `from=${$("#from").value}` : "";
  const to = $("#to").value ? `to=${$("#to").value}` : "";
  return [from, to].filter(Boolean).join("&");
}

async function api(path) {
  const res = await fetch(path, { headers: { Accept: "application/json" } });
  if (!res.ok) {
    let detail = res.statusText;
    try { const problem = await res.json(); detail = problem.detail || problem.title || detail; } catch { /* ignore */ }
    throw new Error(`${res.status}: ${detail}`);
  }
  return res.json();
}

function show(selector, html) { const el = $(selector); if (el) el.innerHTML = html; }
function showError(selector, err) { show(selector, `<div class="error">No se pudieron cargar los datos. ${escapeHtml(err.message)}</div>`); }
function escapeHtml(s) {
  return String(s ?? "").replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
}

// ------------------------------ Summary ------------------------------
async function loadSummary() {
  const q = rangeQuery();
  const data = await api(`/api/dashboard/summary${q ? "?" + q : ""}`);
  const cards = [
    { label: "Pedidos", value: fmtInt(data.totalOrders) },
    { label: "Ventas (líneas)", value: fmtMoney(data.totalSales) },
    { label: "Valor medio por pedido", value: fmtMoney(data.averageOrderValue) },
    { label: "Clientes", value: fmtInt(data.totalCustomers) },
    { label: "Productos", value: fmtInt(data.totalProducts) }
  ];
  show("#summary", cards.map((c) => `
    <div class="card"><div class="label">${c.label}</div><div class="value">${c.value}</div></div>`).join(""));
}

// ------------------------------ Monthly chart ------------------------------
async function loadMonthly() {
  const q = rangeQuery();
  const rows = await api(`/api/dashboard/sales-by-month${q ? "?" + q : ""}`);
  if (!rows.length) { show("#chart", `<div class="empty">Sin datos en el rango seleccionado.</div>`); return; }
  const max = Math.max(...rows.map((r) => r.sales), 1);
  const bars = rows.map((r) => {
    const h = Math.round((r.sales / max) * 100);
    return `<div class="bar-col" title="${escapeHtml(r.monthName)} ${r.year}: ${fmtMoney(r.sales)}">
      <div class="bar" style="height:${h}%"></div>
      <div class="cap">${escapeHtml(r.monthName.slice(0, 3))} ${String(r.year).slice(2)}</div>
    </div>`;
  }).join("");
  show("#chart", `<div class="bar-chart">${bars}</div>`);
}

// ------------------------------ Top products ------------------------------
async function loadTopProducts() {
  const q = rangeQuery();
  const rows = await api(`/api/dashboard/top-products?top=8${q ? "&" + q : ""}`);
  show("#topProducts", renderTable(rows, [
    { k: "productName", t: "Producto" },
    { k: "categoryName", t: "Categoría" },
    { k: "unitsSold", t: "Unidades", num: true },
    { k: "sales", t: "Ventas", num: true, money: true }
  ]));
}

// ------------------------------ Top customers ------------------------------
async function loadTopCustomers() {
  const q = rangeQuery();
  const rows = await api(`/api/dashboard/top-customers?top=8${q ? "&" + q : ""}`);
  show("#topCustomers", renderTable(rows, [
    { k: "customerId", t: "Cliente" },
    { k: "companyName", t: "Empresa" },
    { k: "orderCount", t: "Pedidos", num: true },
    { k: "sales", t: "Ventas", num: true, money: true }
  ]));
}

// ------------------------------ Recent orders ------------------------------
async function loadRecentOrders() {
  const rows = await api(`/api/dashboard/recent-orders?top=10`);
  show("#recentOrders", renderTable(rows, [
    { k: "orderId", t: "Pedido" },
    { k: "companyName", t: "Cliente" },
    { k: "orderDate", t: "Fecha", date: true },
    { k: "total", t: "Total", num: true, money: true },
    { k: "status", t: "Estado", status: true }
  ]));
}

function renderTable(rows, cols) {
  if (!rows.length) return `<div class="empty">Sin resultados.</div>`;
  const head = cols.map((c) => `<th class="${c.num ? "num" : ""}">${escapeHtml(c.t)}</th>`).join("");
  const body = rows.map((r) => `<tr>${cols.map((c) => {
    let v = r[c.k];
    if (c.money) v = fmtMoney(v);
    else if (c.date) v = fmtDate(v);
    else if (c.num) v = fmtInt(v);
    if (c.status) return `<td><span class="status ${escapeHtml(v)}">${escapeHtml(v)}</span></td>`;
    return `<td class="${c.num ? "num" : ""}">${escapeHtml(v)}</td>`;
  }).join("")}</tr>`).join("");
  return `<table><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table>`;
}

async function loadDashboard() {
  show("#summary", `<div class="spinner">Cargando…</div>`);
  const tasks = [
    [loadSummary, "#summary"],
    [loadMonthly, "#chart"],
    [loadTopProducts, "#topProducts"],
    [loadTopCustomers, "#topCustomers"],
    [loadRecentOrders, "#recentOrders"]
  ];
  for (const [fn, target] of tasks) {
    try { await fn(); } catch (e) { showError(target, e); }
  }
}

// ------------------------------ Views browser ------------------------------
let viewCatalog = [];

async function initViews() {
  try {
    viewCatalog = await api("/api/views");
    const sel = $("#viewSelect");
    sel.innerHTML = viewCatalog.map((v) => `<option value="${escapeHtml(v.route)}">${escapeHtml(v.viewName)}</option>`).join("");
    sel.addEventListener("change", loadView);
    await loadView();
  } catch (e) {
    show("#viewResult", `<div class="error">${escapeHtml(e.message)}</div>`);
  }
}

async function loadView() {
  const route = $("#viewSelect").value;
  if (!route) return;
  show("#viewResult", `<div class="spinner">Cargando vista…</div>`);
  try {
    const data = await api(`/api/views/${route}?page=1&pageSize=50`);
    const items = data.items || [];
    if (!items.length) { show("#viewResult", `<div class="empty">La vista no devolvió filas.</div>`); return; }
    const cols = Object.keys(items[0]);
    const head = cols.map((c) => `<th>${escapeHtml(c)}</th>`).join("");
    const body = items.map((r) => `<tr>${cols.map((c) => `<td>${escapeHtml(r[c])}</td>`).join("")}</tr>`).join("");
    const info = viewCatalog.find((v) => v.route === route);
    show("#viewResult", `
      <p class="muted">${escapeHtml(info ? info.description : "")} — ${fmtInt(data.totalCount)} filas (mostrando ${items.length}).</p>
      <div class="table-scroll"><table><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table></div>`);
  } catch (e) {
    show("#viewResult", `<div class="error">${escapeHtml(e.message)}</div>`);
  }
}

// ------------------------------ Stored procedures ------------------------------
async function loadTenMostExpensive() {
  show("#procResult", `<div class="spinner">Ejecutando…</div>`);
  try {
    const rows = await api("/api/stored-procedures/ten-most-expensive-products");
    show("#procResult", renderTable(rows, [
      { k: "tenMostExpensiveProducts", t: "Producto" },
      { k: "unitPrice", t: "Precio", num: true, money: true }
    ]));
  } catch (e) { show("#procResult", `<div class="error">${escapeHtml(e.message)}</div>`); }
}

async function runCustomerOrders() {
  const id = $("#procCustomerId").value.trim();
  if (!id) return;
  show("#procResult", `<div class="spinner">Ejecutando…</div>`);
  try {
    const rows = await api(`/api/stored-procedures/customer-orders/${encodeURIComponent(id)}`);
    show("#procResult", renderTable(rows, [
      { k: "orderId", t: "Pedido" },
      { k: "orderDate", t: "Fecha", date: true },
      { k: "requiredDate", t: "Requerido", date: true },
      { k: "shippedDate", t: "Enviado", date: true }
    ]));
  } catch (e) { show("#procResult", `<div class="error">${escapeHtml(e.message)}</div>`); }
}

// ------------------------------ Whois ---------------------------------------
async function whoami() {
  try { const h = await api("/health"); $("#health").textContent = `${h.status} · ${h.database}`; }
  catch { $("#health").textContent = "sin conexión"; }
}

document.addEventListener("DOMContentLoaded", () => {
  $("#apply").addEventListener("click", () => { loadDashboard(); });
  $("#viewSelect") && (async () => { await initViews(); })();
  $("#btnTenExpensive").addEventListener("click", loadTenMostExpensive);
  $("#btnCustomerOrders").addEventListener("click", runCustomerOrders);
  loadDashboard();
  whoami();
});
