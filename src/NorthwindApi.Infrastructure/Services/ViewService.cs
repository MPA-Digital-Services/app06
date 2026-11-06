using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;
using NorthwindApi.Application.Models;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Infrastructure.Services;

/// <summary>
/// Acceso de solo lectura a las vistas. Todas las entidades de vista son
/// <c>HasNoKey() + ToView()</c>, por lo que aquí nunca se escriben datos.
/// </summary>
public sealed class ViewService : IViewService
{
    private readonly NorthwindContext _db;

    public ViewService(NorthwindContext db) => _db = db;

    private static async Task<PagedResult<TDto>> PageAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        PageQuery paging,
        Func<TEntity, TDto> map,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> orderBy,
        CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await orderBy(query).Skip(paging.Skip).Take(paging.PageSize).ToListAsync(ct);
        return new PagedResult<TDto>
        {
            Items = items.Select(map).ToList(),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        };
    }

    public IReadOnlyList<ViewInfoDto> GetCatalog() => new List<ViewInfoDto>
    {
        new("customer-suppliers-by-city", "Customer and Suppliers by City", "Clientes y proveedores agrupados por ciudad.", new[] { "City", "CompanyName", "ContactName", "Relationship" }),
        new("alphabetical-products", "Alphabetical list of products", "Productos no descontinuados con su categoría.", new[] { "ProductID", "ProductName", "CategoryName", "UnitPrice" }),
        new("current-products", "Current Product List", "Id y nombre de productos activos.", new[] { "ProductID", "ProductName" }),
        new("orders-qry", "Orders Qry", "Pedidos con datos del cliente.", new[] { "OrderID", "CustomerID", "OrderDate", "CompanyName" }),
        new("products-above-average-price", "Products Above Average Price", "Productos con precio superior al promedio.", new[] { "ProductName", "UnitPrice" }),
        new("products-by-category", "Products by Category", "Productos activos por categoría.", new[] { "CategoryName", "ProductName", "UnitsInStock" }),
        new("quarterly-orders", "Quarterly Orders", "Clientes con pedidos en 1997.", new[] { "CustomerID", "CompanyName", "City", "Country" }),
        new("invoices", "Invoices", "Detalle tipo factura por línea de pedido.", new[] { "OrderID", "CustomerName", "ProductName", "ExtendedPrice" }),
        new("order-details-extended", "Order Details Extended", "Líneas de pedido con precio extendido.", new[] { "OrderID", "ProductID", "ExtendedPrice" }),
        new("order-subtotals", "Order Subtotals", "Subtotal por pedido.", new[] { "OrderID", "Subtotal" }),
        new("product-sales-for-1997", "Product Sales for 1997", "Ventas por producto en 1997.", new[] { "CategoryName", "ProductName", "ProductSales" }),
        new("category-sales-for-1997", "Category Sales for 1997", "Ventas por categoría en 1997.", new[] { "CategoryName", "CategorySales" }),
        new("sales-by-category", "Sales by Category", "Ventas por categoría y producto (1997).", new[] { "CategoryID", "CategoryName", "ProductName", "ProductSales" }),
        new("sales-totals-by-amount", "Sales Totals by Amount", "Pedidos con subtotal superior a 2500 (1997).", new[] { "SaleAmount", "OrderID", "CompanyName", "ShippedDate" }),
        new("summary-sales-by-quarter", "Summary of Sales by Quarter", "Subtotales de pedidos enviados.", new[] { "ShippedDate", "OrderID", "Subtotal" }),
        new("summary-sales-by-year", "Summary of Sales by Year", "Subtotales de pedidos enviados.", new[] { "ShippedDate", "OrderID", "Subtotal" })
    };

    public Task<PagedResult<CustomerSuppliersByCityDto>> GetCustomerSuppliersByCityAsync(string? city, string? relationship, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Customer_and_Suppliers_by_Cities.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(city)) q = q.Where(e => e.City == city);
        if (!string.IsNullOrWhiteSpace(relationship)) q = q.Where(e => e.Relationship == relationship);
        return PageAsync(q, paging, e => new CustomerSuppliersByCityDto(e.City, e.CompanyName, e.ContactName, e.Relationship), x => x.OrderBy(e => e.City).ThenBy(e => e.CompanyName), ct);
    }

    public Task<PagedResult<AlphabeticalProductDto>> GetAlphabeticalProductsAsync(string? categoryName, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Alphabetical_list_of_products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(categoryName)) q = q.Where(e => e.CategoryName == categoryName);
        return PageAsync(q, paging, e => new AlphabeticalProductDto(e.ProductID, e.ProductName, e.SupplierID, e.CategoryID, e.QuantityPerUnit, e.UnitPrice, e.UnitsInStock, e.UnitsOnOrder, e.ReorderLevel, e.Discontinued, e.CategoryName), x => x.OrderBy(e => e.ProductName), ct);
    }

    public Task<PagedResult<CurrentProductDto>> GetCurrentProductsAsync(string? productName, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Current_Product_Lists.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(productName)) q = q.Where(e => e.ProductName.Contains(productName));
        return PageAsync(q, paging, e => new CurrentProductDto(e.ProductID, e.ProductName), x => x.OrderBy(e => e.ProductName), ct);
    }

    public Task<PagedResult<OrdersQryDto>> GetOrdersQryAsync(string? customerId, string? country, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Orders_Qries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(customerId)) q = q.Where(e => e.CustomerID == customerId);
        if (!string.IsNullOrWhiteSpace(country)) q = q.Where(e => e.Country == country);
        return PageAsync(q, paging, e => new OrdersQryDto(e.OrderID, e.CustomerID, e.EmployeeID, e.OrderDate, e.RequiredDate, e.ShippedDate, e.ShipVia, e.Freight, e.ShipName, e.ShipAddress, e.ShipCity, e.ShipRegion, e.ShipPostalCode, e.ShipCountry, e.CompanyName, e.Address, e.City, e.Region, e.PostalCode, e.Country), x => x.OrderByDescending(e => e.OrderDate), ct);
    }

    public Task<PagedResult<ProductsAboveAveragePriceDto>> GetProductsAboveAveragePriceAsync(PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Products_Above_Average_Prices.AsNoTracking();
        return PageAsync(q, paging, e => new ProductsAboveAveragePriceDto(e.ProductName, e.UnitPrice), x => x.OrderByDescending(e => e.UnitPrice), ct);
    }

    public Task<PagedResult<ProductsByCategoryDto>> GetProductsByCategoryAsync(string? categoryName, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Products_by_Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(categoryName)) q = q.Where(e => e.CategoryName == categoryName);
        return PageAsync(q, paging, e => new ProductsByCategoryDto(e.CategoryName, e.ProductName, e.QuantityPerUnit, e.UnitsInStock, e.Discontinued), x => x.OrderBy(e => e.CategoryName).ThenBy(e => e.ProductName), ct);
    }

    public Task<PagedResult<QuarterlyOrderDto>> GetQuarterlyOrdersAsync(PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Quarterly_Orders.AsNoTracking();
        return PageAsync(q, paging, e => new QuarterlyOrderDto(e.CustomerID, e.CompanyName, e.City, e.Country), x => x.OrderBy(e => e.CustomerID), ct);
    }

    public Task<PagedResult<InvoiceDto>> GetInvoicesAsync(string? customerId, string? country, string? shipperName, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Invoices.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(customerId)) q = q.Where(e => e.CustomerID == customerId);
        if (!string.IsNullOrWhiteSpace(country)) q = q.Where(e => e.Country == country);
        if (!string.IsNullOrWhiteSpace(shipperName)) q = q.Where(e => e.ShipperName == shipperName);
        return PageAsync(q, paging, e => new InvoiceDto(e.ShipName, e.ShipAddress, e.ShipCity, e.ShipRegion, e.ShipPostalCode, e.ShipCountry, e.CustomerID, e.CustomerName, e.Address, e.City, e.Region, e.PostalCode, e.Country, e.Salesperson, e.OrderID, e.OrderDate, e.RequiredDate, e.ShippedDate, e.ShipperName, e.ProductID, e.ProductName, e.UnitPrice, e.Quantity, e.Discount, e.ExtendedPrice, e.Freight), x => x.OrderBy(e => e.OrderID).ThenBy(e => e.ProductID), ct);
    }

    public Task<PagedResult<OrderDetailsExtendedDto>> GetOrderDetailsExtendedAsync(int? orderId, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Order_Details_Extendeds.AsNoTracking();
        if (orderId.HasValue) q = q.Where(e => e.OrderID == orderId);
        return PageAsync(q, paging, e => new OrderDetailsExtendedDto(e.OrderID, e.ProductID, e.ProductName, e.UnitPrice, e.Quantity, e.Discount, e.ExtendedPrice), x => x.OrderBy(e => e.OrderID).ThenBy(e => e.ProductID), ct);
    }

    public Task<PagedResult<OrderSubtotalDto>> GetOrderSubtotalsAsync(int? orderId, decimal? minSubtotal, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Order_Subtotals.AsNoTracking();
        if (orderId.HasValue) q = q.Where(e => e.OrderID == orderId);
        if (minSubtotal.HasValue) q = q.Where(e => e.Subtotal >= minSubtotal);
        return PageAsync(q, paging, e => new OrderSubtotalDto(e.OrderID, e.Subtotal), x => x.OrderByDescending(e => e.Subtotal), ct);
    }

    public Task<PagedResult<ProductSalesFor1997Dto>> GetProductSalesFor1997Async(string? categoryName, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Product_Sales_for_1997s.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(categoryName)) q = q.Where(e => e.CategoryName == categoryName);
        return PageAsync(q, paging, e => new ProductSalesFor1997Dto(e.CategoryName, e.ProductName, e.ProductSales), x => x.OrderBy(e => e.CategoryName).ThenBy(e => e.ProductName), ct);
    }

    public Task<PagedResult<CategorySalesFor1997Dto>> GetCategorySalesFor1997Async(PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Category_Sales_for_1997s.AsNoTracking();
        return PageAsync(q, paging, e => new CategorySalesFor1997Dto(e.CategoryName, e.CategorySales), x => x.OrderBy(e => e.CategoryName), ct);
    }

    public Task<PagedResult<SalesByCategoryViewDto>> GetSalesByCategoryAsync(string? categoryName, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Sales_by_Categories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(categoryName)) q = q.Where(e => e.CategoryName == categoryName);
        return PageAsync(q, paging, e => new SalesByCategoryViewDto(e.CategoryID, e.CategoryName, e.ProductName, e.ProductSales), x => x.OrderBy(e => e.CategoryName).ThenBy(e => e.ProductName), ct);
    }

    public Task<PagedResult<SalesTotalsByAmountDto>> GetSalesTotalsByAmountAsync(decimal? minAmount, PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Sales_Totals_by_Amounts.AsNoTracking();
        if (minAmount.HasValue) q = q.Where(e => e.SaleAmount >= minAmount);
        return PageAsync(q, paging, e => new SalesTotalsByAmountDto(e.SaleAmount, e.OrderID, e.CompanyName, e.ShippedDate), x => x.OrderByDescending(e => e.SaleAmount), ct);
    }

    public Task<PagedResult<SummaryOfSalesByQuarterDto>> GetSummaryOfSalesByQuarterAsync(PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Summary_of_Sales_by_Quarters.AsNoTracking();
        return PageAsync(q, paging, e => new SummaryOfSalesByQuarterDto(e.ShippedDate, e.OrderID, e.Subtotal), x => x.OrderBy(e => e.ShippedDate), ct);
    }

    public Task<PagedResult<SummaryOfSalesByYearDto>> GetSummaryOfSalesByYearAsync(PageQuery paging, CancellationToken ct = default)
    {
        var q = _db.Summary_of_Sales_by_Years.AsNoTracking();
        return PageAsync(q, paging, e => new SummaryOfSalesByYearDto(e.ShippedDate, e.OrderID, e.Subtotal), x => x.OrderBy(e => e.ShippedDate), ct);
    }
}
