namespace NorthwindApi.Application.Models;

// DTOs de resultado de procedimientos almacenados. Cada uno refleja la forma exacta
// del SELECT devuelto por el procedimiento correspondiente en instnwnd.sql.

/// <summary>Resultado de dbo.[Ten Most Expensive Products] (sin parámetros).</summary>
public sealed record TenMostExpensiveProductDto(string TenMostExpensiveProducts, decimal? UnitPrice);

/// <summary>Resultado de dbo.[Employee Sales by Country] (@Beginning_Date, @Ending_Date).</summary>
public sealed record EmployeeSalesByCountryDto(
    string? Country, string? LastName, string? FirstName,
    DateTime? ShippedDate, int OrderId, decimal? SaleAmount);

/// <summary>Resultado de dbo.[Sales by Year] (@Beginning_Date, @Ending_Date).</summary>
public sealed record SalesByYearProcDto(DateTime? ShippedDate, int OrderId, decimal? Subtotal, string? Year);

/// <summary>Resultado de dbo.CustOrdersDetail (@OrderID).</summary>
public sealed record CustOrdersDetailDto(
    string ProductName, decimal? UnitPrice, short? Quantity, int? Discount, decimal? ExtendedPrice);

/// <summary>Resultado de dbo.CustOrdersOrders (@CustomerID).</summary>
public sealed record CustOrdersOrdersDto(
    int OrderId, DateTime? OrderDate, DateTime? RequiredDate, DateTime? ShippedDate);

/// <summary>Resultado de dbo.CustOrderHist (@CustomerID).</summary>
public sealed record CustOrderHistDto(string ProductName, int? Total);

/// <summary>Resultado de dbo.SalesByCategory (@CategoryName, @OrdYear = '1998').</summary>
public sealed record SalesByCategoryProcDto(string ProductName, decimal? TotalPurchase);

/// <summary>Metadatos de un procedimiento almacenado expuesto por la API.</summary>
public sealed record StoredProcedureInfoDto(string Route, string ProcedureName, string Description, IReadOnlyList<string> Parameters);
