namespace NorthwindApi.Application.Models;

/// <summary>Archivo binario listo para descargar (PDF/XLSX).</summary>
public sealed record ReportFile(byte[] Content, string ContentType, string FileName);

/// <summary>Columna de una tabla de reporte.</summary>
public sealed record ReportColumnDto(string Header, string Align = "left", double Weight = 1);

/// <summary>
/// Representación neutral de una tabla tabular usada por los generadores PDF y XLSX.
/// El mismo objeto alimenta ambos formatos para garantizar consistencia.
/// </summary>
public sealed class ReportTableDto
{
    public string Title { get; init; } = "Reporte";
    public string? Subtitle { get; init; }
    public string? Period { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<ReportColumnDto> Columns { get; init; } = Array.Empty<ReportColumnDto>();
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; init; } = Array.Empty<IReadOnlyList<string?>>();

    /// <summary>Fila opcional de totales (mismos índices que Columnas).</summary>
    public IReadOnlyList<string?>? Totals { get; init; }
}

public static class ReportContentTypes
{
    public const string Pdf = "application/pdf";
    public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
