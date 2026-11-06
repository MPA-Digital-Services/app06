using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Dtos;

public sealed record CustomerDto(
    string CustomerId,
    string CompanyName,
    string? ContactName,
    string? ContactTitle,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? Phone,
    string? Fax);

public sealed record CustomerCreateDto(
    [Required, StringLength(5, MinimumLength = 5)] string CustomerId,
    [Required, StringLength(40)] string CompanyName,
    [StringLength(30)] string? ContactName,
    [StringLength(30)] string? ContactTitle,
    [StringLength(60)] string? Address,
    [StringLength(15)] string? City,
    [StringLength(15)] string? Region,
    [StringLength(10)] string? PostalCode,
    [StringLength(15)] string? Country,
    [StringLength(24)] string? Phone,
    [StringLength(24)] string? Fax);

/// <summary>La clave primaria (CustomerID) es inmutable en actualizaciones.</summary>
public sealed record CustomerUpdateDto(
    [Required, StringLength(40)] string CompanyName,
    [StringLength(30)] string? ContactName,
    [StringLength(30)] string? ContactTitle,
    [StringLength(60)] string? Address,
    [StringLength(15)] string? City,
    [StringLength(15)] string? Region,
    [StringLength(10)] string? PostalCode,
    [StringLength(15)] string? Country,
    [StringLength(24)] string? Phone,
    [StringLength(24)] string? Fax);

public sealed class CustomerFilter
{
    [StringLength(40)] public string? CompanyName { get; set; }
    [StringLength(30)] public string? ContactName { get; set; }
    [StringLength(15)] public string? City { get; set; }
    [StringLength(15)] public string? Country { get; set; }
}

public sealed record CustomerDemographicDto(string CustomerTypeId, string? CustomerDesc);

public sealed record CustomerDemographicCreateDto(
    [Required, StringLength(10, MinimumLength = 10)] string CustomerTypeId,
    string? CustomerDesc);

public sealed record CustomerDemographicUpdateDto(string? CustomerDesc);

public sealed class CustomerDemographicFilter
{
    [StringLength(10)] public string? CustomerTypeId { get; set; }
}
