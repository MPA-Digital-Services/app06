using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Dtos;

public sealed record ProductDto(
    int ProductId,
    string ProductName,
    int? SupplierId,
    int? CategoryId,
    string? QuantityPerUnit,
    decimal? UnitPrice,
    short? UnitsInStock,
    short? UnitsOnOrder,
    short? ReorderLevel,
    bool Discontinued);

public sealed record ProductCreateDto(
    [Required, StringLength(40)] string ProductName,
    int? SupplierId,
    int? CategoryId,
    [StringLength(20)] string? QuantityPerUnit,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? UnitPrice,
    [Range(0, 32767)] short? UnitsInStock,
    [Range(0, 32767)] short? UnitsOnOrder,
    [Range(0, 32767)] short? ReorderLevel,
    bool Discontinued);

public sealed record ProductUpdateDto(
    [Required, StringLength(40)] string ProductName,
    int? SupplierId,
    int? CategoryId,
    [StringLength(20)] string? QuantityPerUnit,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? UnitPrice,
    [Range(0, 32767)] short? UnitsInStock,
    [Range(0, 32767)] short? UnitsOnOrder,
    [Range(0, 32767)] short? ReorderLevel,
    bool Discontinued);

public sealed class ProductFilter
{
    [StringLength(40)] public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public bool? Discontinued { get; set; }
    public decimal? MinUnitPrice { get; set; }
    public decimal? MaxUnitPrice { get; set; }
    public short? InStock { get; set; }
}
