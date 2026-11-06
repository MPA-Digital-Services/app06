using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;
using NorthwindApi.Application.Dtos;

namespace NorthwindApi.Api.Controllers;

/// <summary>
/// Líneas de pedido ("Order Details"). Con clave compuesta (OrderID, ProductID),
/// se exponen como subrecurso de un pedido: /api/orders/{orderId}/details.
/// </summary>
[ApiController]
[Route("api/orders/{orderId:int}/details")]
[Produces("application/json")]
public sealed class OrderDetailsController : ControllerBase
{
    private readonly IOrderDetailService _service;

    public OrderDetailsController(IOrderDetailService service) => _service = service;

    /// <summary>Lista las líneas de un pedido.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDetailDto>>> GetByOrder(
        int orderId, [FromQuery] PageQuery paging, CancellationToken ct)
        => Ok(await _service.GetByOrderAsync(orderId, paging, ct));

    /// <summary>Obtiene una línea concreta.</summary>
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<OrderDetailDto>> Get(int orderId, int productId, CancellationToken ct)
    {
        var dto = await _service.GetAsync(orderId, productId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Añade una línea al pedido.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDetailDto>> Create(
        int orderId, [FromBody] OrderDetailCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(orderId, dto, ct);
        return CreatedAtAction(nameof(Get), new { orderId, productId = created.ProductId }, created);
    }

    /// <summary>Actualiza una línea del pedido.</summary>
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> Update(
        int orderId, int productId, [FromBody] OrderDetailUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(orderId, productId, dto, ct);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Elimina una línea del pedido.</summary>
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Delete(int orderId, int productId, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(orderId, productId, ct);
        return deleted ? NoContent() : NotFound();
    }
}
