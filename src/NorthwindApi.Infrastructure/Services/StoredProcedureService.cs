using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;
using NorthwindApi.Application.Models;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Infrastructure.Services;

/// <summary>
/// Invocación explícita de los procedimientos almacenados de Northwind.
/// Se usa <c>Database.SqlQuery&lt;T&gt;</c> con FormattableString, de modo que todos
/// los valores viajan como parámetros SQL tipados (nunca concatenación de texto).
/// </summary>
public sealed class StoredProcedureService : IStoredProcedureService
{
    private readonly NorthwindContext _db;

    public StoredProcedureService(NorthwindContext db) => _db = db;

    public IReadOnlyList<StoredProcedureInfoDto> GetCatalog() => new List<StoredProcedureInfoDto>
    {
        new("ten-most-expensive-products", "Ten Most Expensive Products", "10 productos más caros. Sin parámetros.", Array.Empty<string>()),
        new("employee-sales-by-country", "Employee Sales by Country", "Ventas por empleado en un rango de fechas.", new[] { "Beginning_Date", "Ending_Date" }),
        new("sales-by-year", "Sales by Year", "Subtotales de pedidos enviados en un rango de fechas.", new[] { "Beginning_Date", "Ending_Date" }),
        new("customer-order-detail", "CustOrdersDetail", "Detalle de líneas de un pedido.", new[] { "OrderID" }),
        new("customer-orders", "CustOrdersOrders", "Pedidos de un cliente.", new[] { "CustomerID" }),
        new("customer-order-history", "CustOrderHist", "Historial de cantidades por producto de un cliente.", new[] { "CustomerID" }),
        new("sales-by-category", "SalesByCategory", "Ventas por producto de una categoría para 1996/1997/1998.", new[] { "CategoryName", "OrdYear" })
    };

    public async Task<IReadOnlyList<TenMostExpensiveProductDto>> GetTenMostExpensiveProductsAsync(CancellationToken ct = default)
    {
        return await _db.Database
            .SqlQuery<TenMostExpensiveProductDto>($"EXEC dbo.[Ten Most Expensive Products]")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EmployeeSalesByCountryDto>> GetEmployeeSalesByCountryAsync(DateTime beginning, DateTime ending, CancellationToken ct = default)
    {
        ValidateDateRange(beginning, ending);

        return await _db.Database
            .SqlQuery<EmployeeSalesByCountryDto>($"EXEC dbo.[Employee Sales by Country] @Beginning_Date = {beginning}, @Ending_Date = {ending}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SalesByYearProcDto>> GetSalesByYearAsync(DateTime beginning, DateTime ending, CancellationToken ct = default)
    {
        ValidateDateRange(beginning, ending);

        return await _db.Database
            .SqlQuery<SalesByYearProcDto>($"EXEC dbo.[Sales by Year] @Beginning_Date = {beginning}, @Ending_Date = {ending}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CustOrdersDetailDto>> GetCustomerOrderDetailAsync(int orderId, CancellationToken ct = default)
    {
        if (orderId <= 0)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["orderId"] = new[] { "El identificador de pedido debe ser mayor que 0." }
            });
        }

        return await _db.Database
            .SqlQuery<CustOrdersDetailDto>($"EXEC dbo.CustOrdersDetail @OrderID = {orderId}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CustOrdersOrdersDto>> GetCustomerOrdersAsync(string customerId, CancellationToken ct = default)
    {
        ValidateCustomerId(customerId);

        return await _db.Database
            .SqlQuery<CustOrdersOrdersDto>($"EXEC dbo.CustOrdersOrders @CustomerID = {customerId}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CustOrderHistDto>> GetCustomerOrderHistoryAsync(string customerId, CancellationToken ct = default)
    {
        ValidateCustomerId(customerId);

        return await _db.Database
            .SqlQuery<CustOrderHistDto>($"EXEC dbo.CustOrderHist @CustomerID = {customerId}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SalesByCategoryProcDto>> GetSalesByCategoryAsync(string categoryName, string ordYear, CancellationToken ct = default)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            errors["categoryName"] = new[] { "El nombre de categoría es obligatorio." };
        }

        if (ordYear is not ("1996" or "1997" or "1998"))
        {
            errors["ordYear"] = new[] { "El año debe ser 1996, 1997 o 1998." };
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException(errors);
        }

        return await _db.Database
            .SqlQuery<SalesByCategoryProcDto>($"EXEC dbo.SalesByCategory @CategoryName = {categoryName}, @OrdYear = {ordYear}")
            .ToListAsync(ct);
    }

    private static void ValidateDateRange(DateTime beginning, DateTime ending)
    {
        if (beginning > ending)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["dateRange"] = new[] { "La fecha inicial no puede ser posterior a la fecha final." }
            });
        }
    }

    private static void ValidateCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId) || customerId.Trim().Length > 5)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["customerId"] = new[] { "El identificador de cliente es obligatorio y tiene un máximo de 5 caracteres." }
            });
        }
    }
}
