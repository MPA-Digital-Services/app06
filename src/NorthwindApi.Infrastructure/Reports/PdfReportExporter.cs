using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using NorthwindApi.Application.Models;
using PdfSharp.Fonts;

namespace NorthwindApi.Infrastructure.Reports;

/// <summary>
/// Genera PDF a partir de un <see cref="ReportTableDto"/> con MigraDoc/PDFsharp
/// (licencia MIT). El layout es apaisado (A4 horizontal) para tablas anchas.
/// </summary>
public sealed class PdfReportExporter
{
    private const double PageUsableWidthCm = 24.0; // A4 horizontal (29.7) menos márgenes (2 x 2.85)

    static PdfReportExporter()
    {
        // PDFsharp 6 (build Core) no resuelve fuentes por sí solo.
        // En Windows se usan las fuentes del sistema; en Linux/macOS (contenedores)
        // se registra un resolvedor que lee fuentes TrueType instaladas en el sistema.
        if (OperatingSystem.IsWindows())
        {
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        }
        else
        {
            GlobalFontSettings.FontResolver = new FileSystemFontResolver();
        }
    }

    public byte[] ToPdf(ReportTableDto table)
    {
        var document = BuildDocument(table);

        var renderer = new PdfDocumentRenderer
        {
            Document = document
        };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private static Document BuildDocument(ReportTableDto table)
    {
        var document = new Document();
        document.Info.Title = table.Title;
        document.Info.Subject = table.Subtitle ?? string.Empty;
        document.Info.Author = "NorthwindApi";

        var style = document.Styles["Normal"]!;
        style.Font.Name = "Arial";
        style.Font.Size = 9;

        var section = document.AddSection();
        section.PageSetup.Orientation = Orientation.Landscape;
        section.PageSetup.LeftMargin = Unit.FromCentimeter(2.85);
        section.PageSetup.RightMargin = Unit.FromCentimeter(2.85);
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

        var title = section.AddParagraph(table.Title);
        title.Format.Font.Size = 16;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = Unit.FromCentimeter(0.2);

        if (!string.IsNullOrWhiteSpace(table.Subtitle))
        {
            var subtitle = section.AddParagraph(table.Subtitle);
            subtitle.Format.Font.Size = 11;
            subtitle.Format.Font.Color = Colors.DimGray;
        }

        if (!string.IsNullOrWhiteSpace(table.Period))
        {
            var period = section.AddParagraph(table.Period);
            period.Format.Font.Size = 9;
            period.Format.Font.Italic = true;
            period.Format.Font.Color = Colors.Gray;
        }

        var generated = section.AddParagraph($"Generado: {table.GeneratedAt:yyyy-MM-dd HH:mm} UTC");
        generated.Format.Font.Size = 8;
        generated.Format.Font.Color = Colors.Gray;
        generated.Format.SpaceAfter = Unit.FromCentimeter(0.4);

        var pdfTable = section.AddTable();
        pdfTable.Borders.Width = 0.5;
        pdfTable.Borders.Color = Colors.LightGray;
        pdfTable.Rows.LeftIndent = 0;

        AddColumns(pdfTable, table);

        var header = pdfTable.AddRow();
        header.HeadingFormat = true;
        header.Format.Font.Bold = true;
        header.Format.Font.Color = Colors.White;
        header.Shading.Color = Color.FromArgb(255, 31, 78, 120);
        header.TopPadding = Unit.FromCentimeter(0.1);
        header.BottomPadding = Unit.FromCentimeter(0.1);

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var paragraph = header.Cells[i].AddParagraph(Safe(table.Columns[i].Header));
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ToAlignment(table.Columns[i].Align);
        }

        foreach (var row in table.Rows)
        {
            var pdfRow = pdfTable.AddRow();
            pdfRow.TopPadding = Unit.FromCentimeter(0.06);
            pdfRow.BottomPadding = Unit.FromCentimeter(0.06);

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var value = i < row.Count ? row[i] : null;
                var paragraph = pdfRow.Cells[i].AddParagraph(Safe(value));
                paragraph.Format.Alignment = ToAlignment(table.Columns[i].Align);
            }
        }

        if (table.Totals is { Count: > 0 })
        {
            var totalRow = pdfTable.AddRow();
            totalRow.Format.Font.Bold = true;
            totalRow.Shading.Color = Color.FromArgb(255, 226, 235, 247);
            totalRow.TopPadding = Unit.FromCentimeter(0.1);
            totalRow.BottomPadding = Unit.FromCentimeter(0.1);

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var value = i < table.Totals.Count ? table.Totals[i] : null;
                var paragraph = totalRow.Cells[i].AddParagraph(Safe(value));
                paragraph.Format.Font.Bold = true;
                paragraph.Format.Alignment = ToAlignment(table.Columns[i].Align);
            }
        }

        var footer = section.Footers.Primary.AddParagraph();
        footer.AddText("NorthwindApi");
        footer.AddTab();
        footer.AddText("Página ");
        footer.AddPageField();
        footer.AddText(" de ");
        footer.AddNumPagesField();
        footer.Format.Font.Size = 8;
        footer.Format.Font.Color = Colors.Gray;

        return document;
    }

    private static void AddColumns(Table table, ReportTableDto source)
    {
        var totalWeight = source.Columns.Sum(c => c.Weight <= 0 ? 1 : c.Weight);

        for (var i = 0; i < source.Columns.Count; i++)
        {
            var weight = source.Columns[i].Weight <= 0 ? 1 : source.Columns[i].Weight;
            var width = PageUsableWidthCm * (weight / totalWeight);
            table.AddColumn(Unit.FromCentimeter(width));
        }
    }

    private static ParagraphAlignment ToAlignment(string align) => align?.ToLowerInvariant() switch
    {
        "right" => ParagraphAlignment.Right,
        "center" => ParagraphAlignment.Center,
        _ => ParagraphAlignment.Left
    };

    private static string Safe(string? value) => string.IsNullOrEmpty(value) ? string.Empty : value;
}
