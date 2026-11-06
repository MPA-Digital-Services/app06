using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Api.Controllers;

/// <summary>
/// Métricas agregadas para el dashboard. El filtro opcional <c>from</c>/<c>to</c>
/// acota por la fecha del pedido (OrderDate).
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>Resumen general de ventas, pedidos, clientes y productos.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetSummaryAsync(from, to, ct));

    /// <summary>Ventas y número de pedidos agrupados por mes.</summary>
    [HttpGet("sales-by-month")]
    public async Task<ActionResult<IReadOnlyList<SalesByMonthDto>>> SalesByMonth(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetSalesByMonthAsync(from, to, ct));

    /// <summary>Ventas y número de pedidos agrupados por trimestre.</summary>
    [HttpGet("sales-by-quarter")]
    public async Task<ActionResult<IReadOnlyList<SalesByQuarterDto>>> SalesByQuarter(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetSalesByQuarterAsync(from, to, ct));

    /// <summary>Ventas y número de pedidos agrupados por año.</summary>
    [HttpGet("sales-by-year")]
    public async Task<ActionResult<IReadOnlyList<SalesByYearDto>>> SalesByYear(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetSalesByYearAsync(from, to, ct));

    /// <summary>Productos más vendidos por importe.</summary>
    [HttpGet("top-products")]
    public async Task<ActionResult<IReadOnlyList<TopProductDto>>> TopProducts(
        [FromQuery] int top = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
        => Ok(await _service.GetTopProductsAsync(top, from, to, ct));

    /// <summary>Categorías con mayores ventas.</summary>
    [HttpGet("top-categories")]
    public async Task<ActionResult<IReadOnlyList<TopCategoryDto>>> TopCategories(
        [FromQuery] int top = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
        => Ok(await _service.GetTopCategoriesAsync(top, from, to, ct));

    /// <summary>Clientes con mayores ventas.</summary>
    [HttpGet("top-customers")]
    public async Task<ActionResult<IReadOnlyList<TopCustomerDto>>> TopCustomers(
        [FromQuery] int top = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
        => Ok(await _service.GetTopCustomersAsync(top, from, to, ct));

    /// <summary>Pedidos más recientes.</summary>
    [HttpGet("recent-orders")]
    public async Task<ActionResult<IReadOnlyList<RecentOrderDto>>> RecentOrders(
        [FromQuery] int top = 10, CancellationToken ct = default)
        => Ok(await _service.GetRecentOrdersAsync(top, ct));

    /// <summary>Distribución de pedidos por estado derivado.</summary>
    [HttpGet("order-status")]
    public async Task<ActionResult<IReadOnlyList<OrderStatusDto>>> OrderStatus(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetOrderStatusAsync(from, to, ct));
}
