using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Api.Controllers;

/// <summary>
/// Invocación explícita de los 7 procedimientos almacenados de Northwind.
/// </summary>
[ApiController]
[Route("api/stored-procedures")]
[Produces("application/json")]
public sealed class StoredProceduresController : ControllerBase
{
    private readonly IStoredProcedureService _service;

    public StoredProceduresController(IStoredProcedureService service) => _service = service;

    /// <summary>Catálogo de procedimientos disponibles con sus parámetros.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<StoredProcedureInfoDto>> GetCatalog() => Ok(_service.GetCatalog());

    /// <summary>10 productos más caros.</summary>
    [HttpGet("ten-most-expensive-products")]
    public async Task<ActionResult<IReadOnlyList<TenMostExpensiveProductDto>>> TenMostExpensiveProducts(CancellationToken ct)
        => Ok(await _service.GetTenMostExpensiveProductsAsync(ct));

    /// <summary>Ventas por empleado en un rango de fechas.</summary>
    [HttpGet("employee-sales-by-country")]
    public async Task<ActionResult<IReadOnlyList<EmployeeSalesByCountryDto>>> EmployeeSalesByCountry(
        [FromQuery] DateTime beginningDate, [FromQuery] DateTime endingDate, CancellationToken ct)
        => Ok(await _service.GetEmployeeSalesByCountryAsync(beginningDate, endingDate, ct));

    /// <summary>Subtotales de pedidos enviados en un rango de fechas.</summary>
    [HttpGet("sales-by-year")]
    public async Task<ActionResult<IReadOnlyList<SalesByYearProcDto>>> SalesByYear(
        [FromQuery] DateTime beginningDate, [FromQuery] DateTime endingDate, CancellationToken ct)
        => Ok(await _service.GetSalesByYearAsync(beginningDate, endingDate, ct));

    /// <summary>Detalle de líneas de un pedido.</summary>
    [HttpGet("customer-order-detail/{orderId:int}")]
    public async Task<ActionResult<IReadOnlyList<CustOrdersDetailDto>>> CustomerOrderDetail(int orderId, CancellationToken ct)
        => Ok(await _service.GetCustomerOrderDetailAsync(orderId, ct));

    /// <summary>Pedidos de un cliente.</summary>
    [HttpGet("customer-orders/{customerId}")]
    public async Task<ActionResult<IReadOnlyList<CustOrdersOrdersDto>>> CustomerOrders(string customerId, CancellationToken ct)
        => Ok(await _service.GetCustomerOrdersAsync(customerId, ct));

    /// <summary>Historial de cantidades por producto de un cliente.</summary>
    [HttpGet("customer-order-history/{customerId}")]
    public async Task<ActionResult<IReadOnlyList<CustOrderHistDto>>> CustomerOrderHistory(string customerId, CancellationToken ct)
        => Ok(await _service.GetCustomerOrderHistoryAsync(customerId, ct));

    /// <summary>Ventas por producto de una categoría para 1996/1997/1998.</summary>
    [HttpGet("sales-by-category")]
    public async Task<ActionResult<IReadOnlyList<SalesByCategoryProcDto>>> SalesByCategory(
        [FromQuery] string categoryName, [FromQuery] string ordYear = "1998", CancellationToken ct = default)
        => Ok(await _service.GetSalesByCategoryAsync(categoryName, ordYear, ct));
}
