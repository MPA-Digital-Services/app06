using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Dtos;

/// <summary>
/// DTO de empleado. Se excluye intencionalmente la columna binaria <c>Photo</c>
/// (tipo <c>image</c>) para evitar respuestas desmedidas.
/// </summary>
public sealed record EmployeeDto(
    int EmployeeId,
    string LastName,
    string FirstName,
    string? Title,
    string? TitleOfCourtesy,
    DateTime? BirthDate,
    DateTime? HireDate,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? HomePhone,
    string? Extension,
    string? Notes,
    int? ReportsTo,
    string? PhotoPath);

public sealed record EmployeeCreateDto(
    [Required, StringLength(20)] string LastName,
    [Required, StringLength(10)] string FirstName,
    [StringLength(30)] string? Title,
    [StringLength(25)] string? TitleOfCourtesy,
    DateTime? BirthDate,
    DateTime? HireDate,
    [StringLength(60)] string? Address,
    [StringLength(15)] string? City,
    [StringLength(15)] string? Region,
    [StringLength(10)] string? PostalCode,
    [StringLength(15)] string? Country,
    [StringLength(24)] string? HomePhone,
    [StringLength(4)] string? Extension,
    string? Notes,
    int? ReportsTo,
    [StringLength(255)] string? PhotoPath);

public sealed record EmployeeUpdateDto(
    [Required, StringLength(20)] string LastName,
    [Required, StringLength(10)] string FirstName,
    [StringLength(30)] string? Title,
    [StringLength(25)] string? TitleOfCourtesy,
    DateTime? BirthDate,
    DateTime? HireDate,
    [StringLength(60)] string? Address,
    [StringLength(15)] string? City,
    [StringLength(15)] string? Region,
    [StringLength(10)] string? PostalCode,
    [StringLength(15)] string? Country,
    [StringLength(24)] string? HomePhone,
    [StringLength(4)] string? Extension,
    string? Notes,
    int? ReportsTo,
    [StringLength(255)] string? PhotoPath);

public sealed class EmployeeFilter
{
    [StringLength(20)] public string? LastName { get; set; }
    [StringLength(10)] public string? FirstName { get; set; }
    [StringLength(30)] public string? Title { get; set; }
    [StringLength(15)] public string? Country { get; set; }
    public int? ReportsTo { get; set; }
}
