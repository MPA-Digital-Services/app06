using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Dtos;

public sealed record OrderDto(
    int OrderId,
    string? CustomerId,
    int? EmployeeId,
    DateTime? OrderDate,
    DateTime? RequiredDate,
    DateTime? ShippedDate,
    int? ShipVia,
    decimal? Freight,
    string? ShipName,
    string? ShipAddress,
    string? ShipCity,
    string? ShipRegion,
    string? ShipPostalCode,
    string? ShipCountry);

public sealed record OrderCreateDto(
    [StringLength(5)] string? CustomerId,
    int? EmployeeId,
    DateTime? OrderDate,
    DateTime? RequiredDate,
    DateTime? ShippedDate,
    int? ShipVia,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? Freight,
    [StringLength(40)] string? ShipName,
    [StringLength(60)] string? ShipAddress,
    [StringLength(15)] string? ShipCity,
    [StringLength(15)] string? ShipRegion,
    [StringLength(10)] string? ShipPostalCode,
    [StringLength(15)] string? ShipCountry);

public sealed record OrderUpdateDto(
    [StringLength(5)] string? CustomerId,
    int? EmployeeId,
    DateTime? OrderDate,
    DateTime? RequiredDate,
    DateTime? ShippedDate,
    int? ShipVia,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? Freight,
    [StringLength(40)] string? ShipName,
    [StringLength(60)] string? ShipAddress,
    [StringLength(15)] string? ShipCity,
    [StringLength(15)] string? ShipRegion,
    [StringLength(10)] string? ShipPostalCode,
    [StringLength(15)] string? ShipCountry);

public sealed class OrderFilter
{
    [StringLength(5)] public string? CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public int? ShipVia { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? Shipped { get; set; }
}

// ----------------------- Order Details (clave compuesta) -----------------------
public sealed record OrderDetailDto(
    int OrderId,
    int ProductId,
    decimal UnitPrice,
    short Quantity,
    float Discount);

public sealed record OrderDetailCreateDto(
    [Range(1, int.MaxValue)] int ProductId,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal UnitPrice,
    [Range(1, 32767)] short Quantity,
    [Range(0d, 1d)] float Discount);

public sealed record OrderDetailUpdateDto(
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal UnitPrice,
    [Range(1, 32767)] short Quantity,
    [Range(0d, 1d)] float Discount);
