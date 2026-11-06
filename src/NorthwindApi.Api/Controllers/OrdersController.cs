using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Dtos;

namespace NorthwindApi.Api.Controllers;

/// <summary>Pedidos.</summary>
public sealed class OrdersController : CrudControllerBase<int, OrderDto, OrderCreateDto, OrderUpdateDto, OrderFilter>
{
    public OrdersController(IOrderService service) : base(service) { }
    protected override int GetKey(OrderDto dto) => dto.OrderId;
}
