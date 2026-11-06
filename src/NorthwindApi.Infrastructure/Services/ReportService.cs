using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Models;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Infrastructure.Services;

/// <summary>
/// Construye tablas neutrales (ReportTableDto) que alimentan tanto al exportador
/// PDF como al XLSX, garantizando que ambos formatos muestren los mismos datos.
/// </summary>
public sealed class ReportService : IReportService
{
    private const int MaxRows = 5000;

    private readonly NorthwindContext _db;

    public ReportService(NorthwindContext db) => _db = db;

    public async Task<ReportTableDto> BuildSalesByCategoryAsync(string? categoryName, CancellationToken ct = default)
    {
        var query = _db.Category_Sales_for_1997s.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            query = query.Where(x => x.CategoryName == categoryName);
        }

        var rows = await query.OrderBy(x => x.CategoryName).ToListAsync(ct);
        var total = rows.Sum(x => x.CategorySales ?? 0m);

        return new ReportTableDto
        {
            Title = "Ventas por categoría",
            Subtitle = "Categoría Sales for 1997",
            Period = categoryName is null ? "Todas las categorías" : $"Categoría: {categoryName}",
            Columns = new[]
            {
                new ReportColumnDto("Categoría", "left", 2),
                new ReportColumnDto("Ventas", "right", 1)
            },
            Rows = rows.Select(r => NewRow(r.CategoryName, Money(r.CategorySales))).ToList(),
            Totals = NewRow("Total", Money(total))
        };
    }

    public async Task<ReportTableDto> BuildSalesByYearAsync(int? year, CancellationToken ct = default)
    {
        var lines = await SalesLines(null, null).ToListAsync(ct);

        var grouped = lines
            .Where(x => x.OrderDate.HasValue)
            .Where(x => !year.HasValue || x.OrderDate!.Value.Year == year)
            .GroupBy(x => x.OrderDate!.Value.Year)
            .OrderBy(g => g.Key)
            .ToList();

        var totalSales = grouped.Sum(g => g.Sum(x => x.Amount));
        var totalOrders = grouped.Sum(g => g.Select(x => x.OrderId).Distinct().Count());

        return new ReportTableDto
        {
            Title = "Ventas por año",
            Subtitle = "Resumen anual de importes de línea",
            Period = year.HasValue ? $"Año {year}" : "Todos los años",
            Columns = new[]
            {
                new ReportColumnDto("Año", "left", 1),
                new ReportColumnDto("Pedidos", "right", 1),
                new ReportColumnDto("Ventas", "right", 2)
            },
            Rows = grouped
                .Select(g => NewRow(g.Key.ToString(CultureInfo.InvariantCulture),
                    g.Select(x => x.OrderId).Distinct().Count().ToString(CultureInfo.InvariantCulture),
                    Money(g.Sum(x => x.Amount))))
                .ToList(),
            Totals = NewRow("Total", totalOrders.ToString(CultureInfo.InvariantCulture), Money(totalSales))
        };
    }

    public async Task<ReportTableDto> BuildTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 100);

        var rows = await (
            from f in SalesLines(fromDate, toDate)
            join p in _db.Products.AsNoTracking() on f.ProductId equals p.ProductID
            join c in _db.Categories.AsNoTracking() on p.CategoryID equals c.CategoryID into cj
            from c in cj.DefaultIfEmpty()
            select new { f.ProductId, p.ProductName, CategoryName = c.CategoryName, f.Quantity, f.Amount })
            .ToListAsync(ct);

        var grouped = rows
            .GroupBy(x => new { x.ProductId, x.ProductName, x.CategoryName })
            .Select(g => new
            {
                g.Key.ProductName,
                CategoryName = g.Key.CategoryName ?? "Sin categoría",
                Units = g.Sum(x => (int)x.Quantity),
                Sales = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Sales)
            .Take(top)
            .ToList();

        return new ReportTableDto
        {
            Title = "Productos más vendidos",
            Subtitle = "Por importe de línea",
            Period = Period(fromDate, toDate),
            Columns = new[]
            {
                new ReportColumnDto("Producto", "left", 2),
                new ReportColumnDto("Categoría", "left", 2),
                new ReportColumnDto("Unidades", "right", 1),
                new ReportColumnDto("Ventas", "right", 2)
            },
            Rows = grouped.Select(r => NewRow(r.ProductName, r.CategoryName,
                r.Units.ToString(CultureInfo.InvariantCulture), Money(r.Sales))).ToList(),
            Totals = NewRow("Total", string.Empty,
                grouped.Sum(x => x.Units).ToString(CultureInfo.InvariantCulture),
                Money(grouped.Sum(x => x.Sales)))
        };
    }

    public async Task<ReportTableDto> BuildTopCustomersAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 100);

        var orders = await (
            from o in _db.Orders.AsNoTracking()
            where (fromDate == null || (o.OrderDate != null && o.OrderDate >= fromDate))
               && (toDate == null || (o.OrderDate != null && o.OrderDate <= toDate))
               && o.CustomerID != null
            select new { o.OrderID, CustomerId = o.CustomerID! })
            .ToListAsync(ct);

        var ids = orders.Select(o => o.OrderID).ToList();
        var amounts = await _db.Order_Details.AsNoTracking()
            .Where(d => ids.Contains(d.OrderID))
            .Select(d => new { d.OrderID, Amount = d.UnitPrice * d.Quantity * (decimal)(1 - d.Discount) })
            .ToListAsync(ct);

        var byOrder = amounts.GroupBy(a => a.OrderID).ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var names = await _db.Customers.AsNoTracking()
            .Select(c => new { c.CustomerID, c.CompanyName })
            .ToDictionaryAsync(c => c.CustomerID, c => c.CompanyName, ct);

        var grouped = orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Company = names.TryGetValue(g.Key, out var n) ? n : g.Key,
                Orders = g.Count(),
                Sales = g.Sum(o => byOrder.TryGetValue(o.OrderID, out var t) ? t : 0m)
            })
            .OrderByDescending(x => x.Sales)
            .Take(top)
            .ToList();

        return new ReportTableDto
        {
            Title = "Mejores clientes",
            Subtitle = "Por importe de línea",
            Period = Period(fromDate, toDate),
            Columns = new[]
            {
                new ReportColumnDto("Cliente", "left", 2),
                new ReportColumnDto("Empresa", "left", 2),
                new ReportColumnDto("Pedidos", "right", 1),
                new ReportColumnDto("Ventas", "right", 2)
            },
            Rows = grouped.Select(r => NewRow(r.CustomerId, r.Company,
                r.Orders.ToString(CultureInfo.InvariantCulture), Money(r.Sales))).ToList(),
            Totals = NewRow("Total", string.Empty,
                grouped.Sum(x => x.Orders).ToString(CultureInfo.InvariantCulture),
                Money(grouped.Sum(x => x.Sales)))
        };
    }

    public async Task<ReportTableDto> BuildInvoicesAsync(string? customerId, CancellationToken ct = default)
    {
        var query = _db.Invoices.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            query = query.Where(x => x.CustomerID == customerId);
        }

        var rows = await query
            .OrderBy(x => x.OrderID).ThenBy(x => x.ProductID)
            .Take(MaxRows)
            .ToListAsync(ct);

        return new ReportTableDto
        {
            Title = "Facturas",
            Subtitle = "Vista Invoices (importes de línea)",
            Period = customerId is null ? "Todos los clientes" : $"Cliente: {customerId}",
            Columns = new[]
            {
                new ReportColumnDto("Pedido", "right", 1),
                new ReportColumnDto("Cliente", "left", 2),
                new ReportColumnDto("Producto", "left", 2),
                new ReportColumnDto("Cant.", "right", 1),
                new ReportColumnDto("Precio", "right", 1),
                new ReportColumnDto("Desc.", "right", 1),
                new ReportColumnDto("Importe", "right", 2)
            },
            Rows = rows.Select(r => NewRow(
                r.OrderID.ToString(CultureInfo.InvariantCulture),
                r.CustomerName,
                r.ProductName,
                r.Quantity.ToString(CultureInfo.InvariantCulture),
                Money(r.UnitPrice),
                r.Discount.ToString("0.00", CultureInfo.InvariantCulture),
                Money(r.ExtendedPrice))).ToList(),
            Totals = NewRow("Total", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                Money(rows.Sum(r => r.ExtendedPrice ?? 0m)))
        };
    }

    private IQueryable<LineFact> SalesLines(DateTime? fromDate, DateTime? toDate) =>
        from o in _db.Orders.AsNoTracking()
        join d in _db.Order_Details.AsNoTracking() on o.OrderID equals d.OrderID
        where (fromDate == null || (o.OrderDate != null && o.OrderDate >= fromDate))
           && (toDate == null || (o.OrderDate != null && o.OrderDate <= toDate))
        select new LineFact
        {
            OrderId = o.OrderID,
            OrderDate = o.OrderDate,
            ProductId = d.ProductID,
            Quantity = d.Quantity,
            Amount = d.UnitPrice * d.Quantity * (decimal)(1 - d.Discount)
        };

    private static IReadOnlyList<string?> NewRow(params string?[] cells) => cells;

    private static string Money(decimal? value) =>
        (value ?? 0m).ToString("N2", CultureInfo.InvariantCulture);

    private static string Period(DateTime? fromDate, DateTime? toDate) => (fromDate, toDate) switch
    {
        (null, null) => "Todo el histórico",
        ({ } f, null) => $"Desde {f:yyyy-MM-dd}",
        (null, { } t) => $"Hasta {t:yyyy-MM-dd}",
        ({ } f, { } t) => $"{f:yyyy-MM-dd} a {t:yyyy-MM-dd}"
    };

    private sealed class LineFact
    {
        public int OrderId { get; init; }
        public DateTime? OrderDate { get; init; }
        public int ProductId { get; init; }
        public short Quantity { get; init; }
        public decimal Amount { get; init; }
    }
}
