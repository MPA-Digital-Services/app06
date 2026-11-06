using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Models;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Infrastructure.Services;

/// <summary>
/// Métricas de negocio. "Sales" siempre equivale a la suma de importes de línea de
/// "Order Details" (UnitPrice * Quantity * (1 - Discount)); el esquema no registra pagos.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly NorthwindContext _db;

    public DashboardService(NorthwindContext db) => _db = db;

    private IQueryable<Order> OrdersInRange(DateTime? fromDate, DateTime? toDate)
    {
        var q = _db.Orders.AsNoTracking();
        if (fromDate.HasValue) q = q.Where(o => o.OrderDate != null && o.OrderDate >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(o => o.OrderDate != null && o.OrderDate <= toDate.Value);
        return q;
    }

    private IQueryable<LineFact> SalesLines(DateTime? fromDate, DateTime? toDate)
    {
        var query =
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
        return query;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var totalOrders = await OrdersInRange(fromDate, toDate).CountAsync(ct);
        var totalSales = await SalesLines(fromDate, toDate).SumAsync(x => (decimal?)x.Amount, ct) ?? 0m;
        var totalCustomers = await _db.Customers.CountAsync(ct);
        var totalProducts = await _db.Products.CountAsync(ct);

        return new DashboardSummaryDto(
            fromDate,
            toDate,
            totalOrders,
            totalSales,
            totalOrders == 0 ? 0m : decimal.Round(totalSales / totalOrders, 2),
            totalCustomers,
            totalProducts);
    }

    public async Task<IReadOnlyList<SalesByMonthDto>> GetSalesByMonthAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var rows = await SalesLines(fromDate, toDate)
            .Where(x => x.OrderDate != null)
            .Select(x => new { x.OrderId, x.Amount, Year = x.OrderDate!.Value.Year, Month = x.OrderDate!.Value.Month })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => new { x.Year, x.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new SalesByMonthDto(
                g.Key.Year,
                g.Key.Month,
                CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                g.Select(x => x.OrderId).Distinct().Count(),
                g.Sum(x => x.Amount)))
            .ToList();
    }

    public async Task<IReadOnlyList<SalesByQuarterDto>> GetSalesByQuarterAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var rows = await SalesLines(fromDate, toDate)
            .Where(x => x.OrderDate != null)
            .Select(x => new { x.OrderId, x.Amount, Year = x.OrderDate!.Value.Year, Month = x.OrderDate!.Value.Month })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => new { x.Year, Quarter = ((x.Month - 1) / 3) + 1 })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Quarter)
            .Select(g => new SalesByQuarterDto(
                g.Key.Year,
                g.Key.Quarter,
                g.Select(x => x.OrderId).Distinct().Count(),
                g.Sum(x => x.Amount)))
            .ToList();
    }

    public async Task<IReadOnlyList<SalesByYearDto>> GetSalesByYearAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var rows = await SalesLines(fromDate, toDate)
            .Where(x => x.OrderDate != null)
            .Select(x => new { x.OrderId, x.Amount, Year = x.OrderDate!.Value.Year })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.Year)
            .OrderBy(g => g.Key)
            .Select(g => new SalesByYearDto(
                g.Key,
                g.Select(x => x.OrderId).Distinct().Count(),
                g.Sum(x => x.Amount)))
            .ToList();
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 100);

        var rows = await (
            from f in SalesLines(fromDate, toDate)
            join p in _db.Products.AsNoTracking() on f.ProductId equals p.ProductID
            join c in _db.Categories.AsNoTracking() on p.CategoryID equals c.CategoryID into cj
            from c in cj.DefaultIfEmpty()
            select new { f.ProductId, p.ProductName, CategoryName = c.CategoryName, f.Quantity, f.Amount })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => new { x.ProductId, x.ProductName, x.CategoryName })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName ?? "Sin categoría",
                g.Sum(x => (int)x.Quantity),
                g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Sales)
            .Take(top)
            .ToList();
    }

    public async Task<IReadOnlyList<TopCategoryDto>> GetTopCategoriesAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 100);

        var rows = await (
            from f in SalesLines(fromDate, toDate)
            join p in _db.Products.AsNoTracking() on f.ProductId equals p.ProductID
            join c in _db.Categories.AsNoTracking() on p.CategoryID equals c.CategoryID into cj
            from c in cj.DefaultIfEmpty()
            select new { CategoryId = (int?)c.CategoryID, CategoryName = c.CategoryName, f.Quantity, f.Amount })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => new { x.CategoryId, x.CategoryName })
            .Select(g => new TopCategoryDto(
                g.Key.CategoryId ?? 0,
                g.Key.CategoryName ?? "Sin categoría",
                g.Sum(x => (int)x.Quantity),
                g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Sales)
            .Take(top)
            .ToList();
    }

    public async Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 100);

        var ordered = await OrdersInRange(fromDate, toDate)
            .Where(o => o.CustomerID != null)
            .Select(o => new { o.OrderID, CustomerId = o.CustomerID! })
            .ToListAsync(ct);

        var orderIds = ordered.Select(o => o.OrderID).ToList();

        var amounts = await _db.Order_Details.AsNoTracking()
            .Where(d => orderIds.Contains(d.OrderID))
            .Select(d => new { d.OrderID, Amount = d.UnitPrice * d.Quantity * (decimal)(1 - d.Discount) })
            .ToListAsync(ct);

        var totalsByOrder = amounts
            .GroupBy(a => a.OrderID)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var customerNames = await _db.Customers.AsNoTracking()
            .Select(c => new { c.CustomerID, c.CompanyName })
            .ToDictionaryAsync(c => c.CustomerID, c => c.CompanyName, ct);

        return ordered
            .GroupBy(o => o.CustomerId)
            .Select(g => new TopCustomerDto(
                g.Key,
                customerNames.TryGetValue(g.Key, out var name) ? name : g.Key,
                g.Count(),
                g.Sum(o => totalsByOrder.TryGetValue(o.OrderID, out var t) ? t : 0m)))
            .OrderByDescending(x => x.Sales)
            .Take(top)
            .ToList();
    }

    public async Task<IReadOnlyList<RecentOrderDto>> GetRecentOrdersAsync(int top, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 100);

        var orders = await (
            from o in _db.Orders.AsNoTracking()
            join c in _db.Customers.AsNoTracking() on o.CustomerID equals c.CustomerID into cj
            from c in cj.DefaultIfEmpty()
            orderby o.OrderDate descending, o.OrderID descending
            select new { o.OrderID, o.CustomerID, Company = c.CompanyName, o.OrderDate, o.ShippedDate })
            .Take(top)
            .ToListAsync(ct);

        var ids = orders.Select(o => o.OrderID).ToList();
        var amounts = await _db.Order_Details.AsNoTracking()
            .Where(d => ids.Contains(d.OrderID))
            .Select(d => new { d.OrderID, Amount = d.UnitPrice * d.Quantity * (decimal)(1 - d.Discount) })
            .ToListAsync(ct);

        var totals = amounts.GroupBy(a => a.OrderID).ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        return orders.Select(o => new RecentOrderDto(
            o.OrderID,
            o.CustomerID,
            o.Company,
            o.OrderDate,
            o.ShippedDate,
            totals.TryGetValue(o.OrderID, out var t) ? t : 0m,
            DeriveStatus(o.ShippedDate, null))).ToList();
    }

    public async Task<IReadOnlyList<OrderStatusDto>> GetOrderStatusAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var orders = await OrdersInRange(fromDate, toDate)
            .Select(o => new { o.ShippedDate, o.RequiredDate })
            .ToListAsync(ct);

        return orders
            .GroupBy(o => DeriveStatus(o.ShippedDate, o.RequiredDate))
            .Select(g => new OrderStatusDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    /// <summary>
    /// El esquema no tiene columna de estado: se deriva de ShippedDate/RequiredDate.
    /// </summary>
    private static string DeriveStatus(DateTime? shippedDate, DateTime? requiredDate)
    {
        if (shippedDate.HasValue)
        {
            return "Enviado";
        }

        if (requiredDate.HasValue && requiredDate.Value.Date < DateTime.UtcNow.Date)
        {
            return "Vencido";
        }

        return "Pendiente";
    }

    private sealed class LineFact
    {
        public int OrderId { get; init; }
        public DateTime? OrderDate { get; init; }
        public int ProductId { get; init; }
        public short Quantity { get; init; }
        public decimal Amount { get; init; }
    }
}
