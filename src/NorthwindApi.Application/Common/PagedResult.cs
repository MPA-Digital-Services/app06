namespace NorthwindApi.Application.Common;

/// <summary>
/// Envoltura estándar para resultados paginados.
/// </summary>
public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public static PagedResult<T> Empty(int page, int pageSize) =>
        new() { Items = Array.Empty<T>(), Page = page, PageSize = pageSize, TotalCount = 0 };
}
