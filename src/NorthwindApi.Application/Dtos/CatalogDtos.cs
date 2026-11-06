using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Dtos;

// ----------------------- Categories -----------------------
public sealed record CategoryDto(int CategoryId, string CategoryName, string? Description);

public sealed record CategoryCreateDto(
    [Required, StringLength(15)] string CategoryName,
    string? Description);

public sealed record CategoryUpdateDto(
    [Required, StringLength(15)] string CategoryName,
    string? Description);

// ----------------------- Shippers -----------------------
public sealed record ShipperDto(int ShipperId, string CompanyName, string? Phone);

public sealed record ShipperCreateDto(
    [Required, StringLength(40)] string CompanyName,
    [StringLength(24)] string? Phone);

public sealed record ShipperUpdateDto(
    [Required, StringLength(40)] string CompanyName,
    [StringLength(24)] string? Phone);

// ----------------------- Suppliers -----------------------
public sealed record SupplierDto(
    int SupplierId,
    string CompanyName,
    string? ContactName,
    string? ContactTitle,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? Phone,
    string? Fax,
    string? HomePage);

public sealed record SupplierCreateDto(
    [Required, StringLength(40)] string CompanyName,
    [StringLength(30)] string? ContactName,
    [StringLength(30)] string? ContactTitle,
    [StringLength(60)] string? Address,
    [StringLength(15)] string? City,
    [StringLength(15)] string? Region,
    [StringLength(10)] string? PostalCode,
    [StringLength(15)] string? Country,
    [StringLength(24)] string? Phone,
    [StringLength(24)] string? Fax,
    string? HomePage);

public sealed record SupplierUpdateDto(
    [Required, StringLength(40)] string CompanyName,
    [StringLength(30)] string? ContactName,
    [StringLength(30)] string? ContactTitle,
    [StringLength(60)] string? Address,
    [StringLength(15)] string? City,
    [StringLength(15)] string? Region,
    [StringLength(10)] string? PostalCode,
    [StringLength(15)] string? Country,
    [StringLength(24)] string? Phone,
    [StringLength(24)] string? Fax,
    string? HomePage);

// ----------------------- Public filter objects -----------------------
public sealed class CategoryFilter
{
    [StringLength(15)]
    public string? CategoryName { get; set; }
}

public sealed class ShipperFilter
{
    [StringLength(40)]
    public string? CompanyName { get; set; }
}

public sealed class SupplierFilter
{
    [StringLength(40)] public string? CompanyName { get; set; }
    [StringLength(15)] public string? City { get; set; }
    [StringLength(15)] public string? Country { get; set; }
}
