namespace NorthwindApi.Application.Models;

// Métricas de negocio para el dashboard.
// IMPORTANTE: el esquema Northwind NO registra pagos/cobros. Por eso "ventas" (Sales)
// = suma de los importes de línea de "Order Details" (UnitPrice * Quantity * (1 - Discount)),
// nunca "ingresos cobrados". Los pedidos se filtran por OrderDate.

public sealed record DashboardSummaryDto(
    DateTime? From,
    DateTime? To,
    int TotalOrders,
    decimal TotalSales,
    decimal AverageOrderValue,
    int TotalCustomers,
    int TotalProducts)
{
    /// <summary>Fecha/hora de generación de la métrica (UTC).</summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public sealed record SalesByMonthDto(int Year, int Month, string MonthName, int OrderCount, decimal Sales);

public sealed record SalesByQuarterDto(int Year, int Quarter, int OrderCount, decimal Sales);

public sealed record SalesByYearDto(int Year, int OrderCount, decimal Sales);

public sealed record TopProductDto(int ProductId, string ProductName, string CategoryName, int UnitsSold, decimal Sales);

public sealed record TopCategoryDto(int CategoryId, string CategoryName, int UnitsSold, decimal Sales);

public sealed record TopCustomerDto(string CustomerId, string CompanyName, int OrderCount, decimal Sales);

public sealed record RecentOrderDto(
    int OrderId, string? CustomerId, string? CompanyName, DateTime? OrderDate,
    DateTime? ShippedDate, decimal Total, string Status);

/// <summary>
/// Estado derivado del pedido. No existe una columna de estado en el esquema:
/// el estado se INFIERE a partir de ShippedDate y RequiredDate.
/// </summary>
public sealed record OrderStatusDto(string Status, int Count);
