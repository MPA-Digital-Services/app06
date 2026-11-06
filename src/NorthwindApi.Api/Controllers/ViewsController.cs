using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Api.Controllers;

/// <summary>
/// Acceso de solo lectura a las 16 vistas de Northwind (HasNoKey + ToView).
/// </summary>
[ApiController]
[Route("api/views")]
[Produces("application/json")]
public sealed class ViewsController : ControllerBase
{
    private readonly IViewService _service;

    public ViewsController(IViewService service) => _service = service;

    /// <summary>Catálogo de vistas disponibles con su descripción y columnas.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<ViewInfoDto>> GetCatalog() => Ok(_service.GetCatalog());

    /// <summary>Customer and Suppliers by City.</summary>
    [HttpGet("customer-suppliers-by-city")]
    public Task<ActionResult<PagedResult<CustomerSuppliersByCityDto>>> CustomerSuppliersByCity(
        [FromQuery] PageQuery paging, [FromQuery] string? city, [FromQuery] string? relationship, CancellationToken ct)
        => Wrap(_service.GetCustomerSuppliersByCityAsync(city, relationship, paging, ct));

    /// <summary>Alphabetical list of products.</summary>
    [HttpGet("alphabetical-products")]
    public Task<ActionResult<PagedResult<AlphabeticalProductDto>>> AlphabeticalProducts(
        [FromQuery] PageQuery paging, [FromQuery] string? categoryName, CancellationToken ct)
        => Wrap(_service.GetAlphabeticalProductsAsync(categoryName, paging, ct));

    /// <summary>Current Product List.</summary>
    [HttpGet("current-products")]
    public Task<ActionResult<PagedResult<CurrentProductDto>>> CurrentProducts(
        [FromQuery] PageQuery paging, [FromQuery] string? productName, CancellationToken ct)
        => Wrap(_service.GetCurrentProductsAsync(productName, paging, ct));

    /// <summary>Orders Qry.</summary>
    [HttpGet("orders-qry")]
    public Task<ActionResult<PagedResult<OrdersQryDto>>> OrdersQry(
        [FromQuery] PageQuery paging, [FromQuery] string? customerId, [FromQuery] string? country, CancellationToken ct)
        => Wrap(_service.GetOrdersQryAsync(customerId, country, paging, ct));

    /// <summary>Products Above Average Price.</summary>
    [HttpGet("products-above-average-price")]
    public Task<ActionResult<PagedResult<ProductsAboveAveragePriceDto>>> ProductsAboveAveragePrice(
        [FromQuery] PageQuery paging, CancellationToken ct)
        => Wrap(_service.GetProductsAboveAveragePriceAsync(paging, ct));

    /// <summary>Products by Category.</summary>
    [HttpGet("products-by-category")]
    public Task<ActionResult<PagedResult<ProductsByCategoryDto>>> ProductsByCategory(
        [FromQuery] PageQuery paging, [FromQuery] string? categoryName, CancellationToken ct)
        => Wrap(_service.GetProductsByCategoryAsync(categoryName, paging, ct));

    /// <summary>Quarterly Orders.</summary>
    [HttpGet("quarterly-orders")]
    public Task<ActionResult<PagedResult<QuarterlyOrderDto>>> QuarterlyOrders(
        [FromQuery] PageQuery paging, CancellationToken ct)
        => Wrap(_service.GetQuarterlyOrdersAsync(paging, ct));

    /// <summary>Invoices.</summary>
    [HttpGet("invoices")]
    public Task<ActionResult<PagedResult<InvoiceDto>>> Invoices(
        [FromQuery] PageQuery paging, [FromQuery] string? customerId, [FromQuery] string? country, [FromQuery] string? shipperName, CancellationToken ct)
        => Wrap(_service.GetInvoicesAsync(customerId, country, shipperName, paging, ct));

    /// <summary>Order Details Extended.</summary>
    [HttpGet("order-details-extended")]
    public Task<ActionResult<PagedResult<OrderDetailsExtendedDto>>> OrderDetailsExtended(
        [FromQuery] PageQuery paging, [FromQuery] int? orderId, CancellationToken ct)
        => Wrap(_service.GetOrderDetailsExtendedAsync(orderId, paging, ct));

    /// <summary>Order Subtotals.</summary>
    [HttpGet("order-subtotals")]
    public Task<ActionResult<PagedResult<OrderSubtotalDto>>> OrderSubtotals(
        [FromQuery] PageQuery paging, [FromQuery] int? orderId, [FromQuery] decimal? minSubtotal, CancellationToken ct)
        => Wrap(_service.GetOrderSubtotalsAsync(orderId, minSubtotal, paging, ct));

    /// <summary>Product Sales for 1997.</summary>
    [HttpGet("product-sales-1997")]
    public Task<ActionResult<PagedResult<ProductSalesFor1997Dto>>> ProductSales1997(
        [FromQuery] PageQuery paging, [FromQuery] string? categoryName, CancellationToken ct)
        => Wrap(_service.GetProductSalesFor1997Async(categoryName, paging, ct));

    /// <summary>Category Sales for 1997.</summary>
    [HttpGet("category-sales-1997")]
    public Task<ActionResult<PagedResult<CategorySalesFor1997Dto>>> CategorySales1997(
        [FromQuery] PageQuery paging, CancellationToken ct)
        => Wrap(_service.GetCategorySalesFor1997Async(paging, ct));

    /// <summary>Sales by Category.</summary>
    [HttpGet("sales-by-category")]
    public Task<ActionResult<PagedResult<SalesByCategoryViewDto>>> SalesByCategory(
        [FromQuery] PageQuery paging, [FromQuery] string? categoryName, CancellationToken ct)
        => Wrap(_service.GetSalesByCategoryAsync(categoryName, paging, ct));

    /// <summary>Sales Totals by Amount.</summary>
    [HttpGet("sales-totals-by-amount")]
    public Task<ActionResult<PagedResult<SalesTotalsByAmountDto>>> SalesTotalsByAmount(
        [FromQuery] PageQuery paging, [FromQuery] decimal? minAmount, CancellationToken ct)
        => Wrap(_service.GetSalesTotalsByAmountAsync(minAmount, paging, ct));

    /// <summary>Summary of Sales by Quarter.</summary>
    [HttpGet("summary-sales-by-quarter")]
    public Task<ActionResult<PagedResult<SummaryOfSalesByQuarterDto>>> SummarySalesByQuarter(
        [FromQuery] PageQuery paging, CancellationToken ct)
        => Wrap(_service.GetSummaryOfSalesByQuarterAsync(paging, ct));

    /// <summary>Summary of Sales by Year.</summary>
    [HttpGet("summary-sales-by-year")]
    public Task<ActionResult<PagedResult<SummaryOfSalesByYearDto>>> SummarySalesByYear(
        [FromQuery] PageQuery paging, CancellationToken ct)
        => Wrap(_service.GetSummaryOfSalesByYearAsync(paging, ct));

    private async Task<ActionResult<PagedResult<T>>> Wrap<T>(Task<PagedResult<T>> task)
        => Ok(await task);
}
