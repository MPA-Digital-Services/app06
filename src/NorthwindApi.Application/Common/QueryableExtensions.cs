using System.Linq.Expressions;

namespace NorthwindApi.Application.Common;

/// <summary>
/// Ordenamiento seguro basado en una lista blanca de expresiones.
/// Nunca se construye SQL ni nombres de columna a partir de texto libre.
/// </summary>
public static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        PageQuery paging,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>> defaultSort)
    {
        Expression<Func<T, object>> chosen = defaultSort;

        if (!string.IsNullOrWhiteSpace(paging.Sort) &&
            sortMap.TryGetValue(paging.Sort.Trim(), out var mapped))
        {
            chosen = mapped;
        }

        return paging.Descending
            ? query.OrderByDescending(chosen)
            : query.OrderBy(chosen);
    }
}
