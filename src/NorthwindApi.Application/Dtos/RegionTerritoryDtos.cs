using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Dtos;

// ----------------------- Region -----------------------
/// <summary>RegionID no es IDENTITY: el cliente debe proveerlo en la creación.</summary>
public sealed record RegionDto(int RegionId, string RegionDescription);

public sealed record RegionCreateDto(
    [Range(1, int.MaxValue)] int RegionId,
    [Required, StringLength(50)] string RegionDescription);

public sealed record RegionUpdateDto(
    [Required, StringLength(50)] string RegionDescription);

public sealed class RegionFilter
{
    [StringLength(50)] public string? RegionDescription { get; set; }
}

// ----------------------- Territories -----------------------
public sealed record TerritoryDto(string TerritoryId, string TerritoryDescription, int RegionId);

public sealed record TerritoryCreateDto(
    [Required, StringLength(20)] string TerritoryId,
    [Required, StringLength(50)] string TerritoryDescription,
    [Range(1, int.MaxValue)] int RegionId);

public sealed record TerritoryUpdateDto(
    [Required, StringLength(50)] string TerritoryDescription,
    [Range(1, int.MaxValue)] int RegionId);

public sealed class TerritoryFilter
{
    [StringLength(50)] public string? TerritoryDescription { get; set; }
    public int? RegionId { get; set; }
}
