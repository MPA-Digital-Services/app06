namespace NorthwindApi.Application.Models;

// Los DTOs de vistas replican exactamente las columnas de salida de cada vista SQL.
// Todas las vistas son de SOLO LECTURA (HasNoKey + ToView en el mapeo de EF Core).

public sealed record CustomerSuppliersByCityDto(
    string? City, string CompanyName, string? ContactName, string Relationship);

public sealed record AlphabeticalProductDto(
    int ProductId, string ProductName, int? SupplierId, int? CategoryId, string? QuantityPerUnit,
    decimal? UnitPrice, short? UnitsInStock, short? UnitsOnOrder, short? ReorderLevel,
    bool Discontinued, string CategoryName);

public sealed record CurrentProductDto(int ProductId, string ProductName);

public sealed record OrdersQryDto(
    int OrderId, string? CustomerId, int? EmployeeId, DateTime? OrderDate, DateTime? RequiredDate,
    DateTime? ShippedDate, int? ShipVia, decimal? Freight, string? ShipName, string? ShipAddress,
    string? ShipCity, string? ShipRegion, string? ShipPostalCode, string? ShipCountry,
    string CompanyName, string? Address, string? City, string? Region, string? PostalCode, string? Country);

public sealed record ProductsAboveAveragePriceDto(string ProductName, decimal? UnitPrice);

public sealed record ProductsByCategoryDto(
    string CategoryName, string ProductName, string? QuantityPerUnit, short? UnitsInStock, bool Discontinued);

public sealed record QuarterlyOrderDto(string? CustomerId, string? CompanyName, string? City, string? Country);

public sealed record InvoiceDto(
    string? ShipName, string? ShipAddress, string? ShipCity, string? ShipRegion, string? ShipPostalCode,
    string? ShipCountry, string? CustomerId, string CustomerName, string? Address, string? City,
    string? Region, string? PostalCode, string? Country, string Salesperson, int OrderId,
    DateTime? OrderDate, DateTime? RequiredDate, DateTime? ShippedDate, string ShipperName,
    int ProductId, string ProductName, decimal UnitPrice, short Quantity, float Discount,
    decimal? ExtendedPrice, decimal? Freight);

public sealed record OrderDetailsExtendedDto(
    int OrderId, int ProductId, string ProductName, decimal UnitPrice, short Quantity,
    float Discount, decimal? ExtendedPrice);

public sealed record OrderSubtotalDto(int OrderId, decimal? Subtotal);

public sealed record ProductSalesFor1997Dto(string CategoryName, string ProductName, decimal? ProductSales);

public sealed record CategorySalesFor1997Dto(string CategoryName, decimal? CategorySales);

public sealed record SalesByCategoryViewDto(int CategoryId, string CategoryName, string ProductName, decimal? ProductSales);

public sealed record SalesTotalsByAmountDto(decimal? SaleAmount, int OrderId, string CompanyName, DateTime? ShippedDate);

public sealed record SummaryOfSalesByQuarterDto(DateTime? ShippedDate, int OrderId, decimal? Subtotal);

public sealed record SummaryOfSalesByYearDto(DateTime? ShippedDate, int OrderId, decimal? Subtotal);

/// <summary>Metadatos descriptivos de una vista, usados por el endpoint de catálogo.</summary>
public sealed record ViewInfoDto(string Route, string ViewName, string Description, IReadOnlyList<string> Columns);
