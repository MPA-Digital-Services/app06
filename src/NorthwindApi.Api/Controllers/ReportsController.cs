using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Api.Controllers;

/// <summary>
/// Exportación de reportes a PDF o XLSX. El sufijo de ruta indica el formato:
/// <c>/api/reports/{reporte}.pdf</c> o <c>/api/reports/{reporte}.xlsx</c>.
/// </summary>
[ApiController]
[Route("api/reports")]
[Produces("application/pdf", ReportContentTypes.Xlsx)]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly IReportExporter _exporter;

    public ReportsController(IReportService reports, IReportExporter exporter)
    {
        _reports = reports;
        _exporter = exporter;
    }

    /// <summary>Ventas por categoría (vista "Category Sales for 1997").</summary>
    [HttpGet("sales-by-category.{format}")]
    public async Task<IActionResult> SalesByCategory(string format, [FromQuery] string? categoryName, CancellationToken ct)
        => await Export(await _reports.BuildSalesByCategoryAsync(categoryName, ct), format, "ventas-por-categoria");

    /// <summary>Ventas por año.</summary>
    [HttpGet("sales-by-year.{format}")]
    public async Task<IActionResult> SalesByYear(string format, [FromQuery] int? year, CancellationToken ct)
        => await Export(await _reports.BuildSalesByYearAsync(year, ct), format, "ventas-por-anio");

    /// <summary>Productos más vendidos.</summary>
    [HttpGet("top-products.{format}")]
    public async Task<IActionResult> TopProducts(
        string format, [FromQuery] int top = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
        => await Export(await _reports.BuildTopProductsAsync(top, from, to, ct), format, "top-productos");

    /// <summary>Mejores clientes.</summary>
    [HttpGet("top-customers.{format}")]
    public async Task<IActionResult> TopCustomers(
        string format, [FromQuery] int top = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, CancellationToken ct = default)
        => await Export(await _reports.BuildTopCustomersAsync(top, from, to, ct), format, "top-clientes");

    /// <summary>Facturas (vista Invoices).</summary>
    [HttpGet("invoices.{format}")]
    public async Task<IActionResult> Invoices(string format, [FromQuery] string? customerId, CancellationToken ct)
        => await Export(await _reports.BuildInvoicesAsync(customerId, ct), format, "facturas");

    private Task<IActionResult> Export(ReportTableDto table, string format, string baseFileName)
    {
        var normalized = format?.Trim().ToLowerInvariant();

        switch (normalized)
        {
            case "pdf":
                return Task.FromResult<IActionResult>(
                    File(_exporter.ToPdf(table), ReportContentTypes.Pdf, $"{baseFileName}.pdf"));

            case "xlsx":
            case "excel":
                return Task.FromResult<IActionResult>(
                    File(_exporter.ToXlsx(table), ReportContentTypes.Xlsx, $"{baseFileName}.xlsx"));

            default:
                return Task.FromResult<IActionResult>(Problem(
                    title: "Formato no soportado",
                    detail: "Formatos permitidos: pdf, xlsx.",
                    statusCode: StatusCodes.Status400BadRequest));
        }
    }
}
