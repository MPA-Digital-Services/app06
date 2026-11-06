using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Infrastructure.Reports;

/// <summary>
/// Exportador compuesto que delega en las implementaciones concretas de PDF y XLSX.
/// Permite que un único punto de entrada produzca cualquiera de los dos formatos.
/// </summary>
public sealed class ReportExporter : IReportExporter
{
    private readonly PdfReportExporter _pdf;
    private readonly XlsxReportExporter _xlsx;

    public ReportExporter(PdfReportExporter pdf, XlsxReportExporter xlsx)
    {
        _pdf = pdf;
        _xlsx = xlsx;
    }

    public byte[] ToPdf(ReportTableDto table) => _pdf.ToPdf(table);

    public byte[] ToXlsx(ReportTableDto table) => _xlsx.ToXlsx(table);
}
