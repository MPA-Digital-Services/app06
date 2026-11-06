using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Application.Common;

/// <summary>
/// Parámetros de paginación y orden reutilizables en todos los endpoints de colección.
/// El tamaño de página se limita a <see cref="MaxPageSize"/> para evitar respuestas
/// desmedidas (riesgo de "exceso de datos").
/// </summary>
public class PageQuery
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>Número de página (1-based). Valores menores que 1 se normalizan a 1.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "'page' debe ser mayor o igual a 1.")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Tamaño de página. Se acota al rango [1, MaxPageSize].</summary>
    [Range(1, MaxPageSize, ErrorMessage = "'pageSize' debe estar entre 1 y 100.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>Nombre lógico del campo de orden (validado por cada servicio contra una lista permitida).</summary>
    public string? Sort { get; set; }

    /// <summary>Dirección del orden: "asc" (por defecto) o "desc".</summary>
    public string? Order { get; set; }

    public int Skip => (Page - 1) * PageSize;

    public bool Descending =>
        string.Equals(Order, "desc", StringComparison.OrdinalIgnoreCase);
}
