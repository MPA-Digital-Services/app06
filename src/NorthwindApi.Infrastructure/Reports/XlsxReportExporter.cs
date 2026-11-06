using ClosedXML.Excel;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Infrastructure.Reports;

/// <summary>
/// Genera XLSX a partir de un <see cref="ReportTableDto"/> con ClosedXML (licencia MIT).
/// Las columnas "right" producen valores numéricos nativos de Excel cuando el texto
/// representa un número; el resto se escriben como texto.
/// </summary>
public sealed class XlsxReportExporter
{
    private static readonly XLColor HeaderFill = XLColor.FromArgb(31, 78, 120);
    private static readonly XLColor TotalFill = XLColor.FromArgb(226, 235, 247);

    public byte[] ToXlsx(ReportTableDto table)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(SanitizeSheetName(table.Title));

        var columnCount = Math.Max(1, table.Columns.Count);

        var titleCell = worksheet.Cell(1, 1);
        titleCell.Value = table.Title;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, columnCount).Merge();

        var row = 2;

        if (!string.IsNullOrWhiteSpace(table.Subtitle))
        {
            worksheet.Cell(row, 1).Value = table.Subtitle;
            worksheet.Range(row, 1, row, columnCount).Merge();
            row++;
        }

        if (!string.IsNullOrWhiteSpace(table.Period))
        {
            worksheet.Cell(row, 1).Value = table.Period;
            worksheet.Cell(row, 1).Style.Font.Italic = true;
            worksheet.Range(row, 1, row, columnCount).Merge();
            row++;
        }

        worksheet.Cell(row, 1).Value = $"Generado: {table.GeneratedAt:yyyy-MM-dd HH:mm} UTC";
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
        worksheet.Range(row, 1, row, columnCount).Merge();
        row += 2;

        var headerRow = row;
        for (var c = 0; c < table.Columns.Count; c++)
        {
            var cell = worksheet.Cell(headerRow, c + 1);
            cell.Value = table.Columns[c].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = HeaderFill;
            cell.Style.Alignment.Horizontal = ToHorizontal(table.Columns[c].Align);
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        row++;

        foreach (var dataRow in table.Rows)
        {
            for (var c = 0; c < table.Columns.Count; c++)
            {
                var value = c < dataRow.Count ? dataRow[c] : null;
                var cell = worksheet.Cell(row, c + 1);
                WriteCell(cell, value, table.Columns[c].Align);
            }

            row++;
        }

        if (table.Totals is { Count: > 0 })
        {
            for (var c = 0; c < table.Columns.Count; c++)
            {
                var value = c < table.Totals.Count ? table.Totals[c] : null;
                var cell = worksheet.Cell(row, c + 1);
                WriteCell(cell, value, table.Columns[c].Align);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = TotalFill;
                cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            }

            row++;
        }

        var dataHeader = worksheet.Range(headerRow, 1, Math.Max(headerRow, row - 1), columnCount);
        dataHeader.SetAutoFilter();

        worksheet.Columns(1, columnCount).AdjustToContents();
        worksheet.SheetView.FreezeRows(headerRow);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteCell(IXLCell cell, string? value, string align)
    {
        cell.Style.Alignment.Horizontal = ToHorizontal(align);

        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (IsRightAligned(align) && TryParseNumber(value, out var number))
        {
            cell.Value = number;
            cell.Style.NumberFormat.Format = "#,##0.00";
            return;
        }

        cell.SetValue(value);
    }

    private static bool IsRightAligned(string align) =>
        string.Equals(align, "right", StringComparison.OrdinalIgnoreCase);

    private static bool TryParseNumber(string value, out double number) =>
        double.TryParse(value, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out number);

    private static XLAlignmentHorizontalValues ToHorizontal(string align) => align?.ToLowerInvariant() switch
    {
        "right" => XLAlignmentHorizontalValues.Right,
        "center" => XLAlignmentHorizontalValues.Center,
        _ => XLAlignmentHorizontalValues.Left
    };

    private static string SanitizeSheetName(string name)
    {
        var invalid = new[] { '\\', '/', '?', '*', '[', ']', ':' };
        var cleaned = new string(name.Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
        return string.IsNullOrEmpty(cleaned) ? "Reporte" : cleaned[..Math.Min(31, cleaned.Length)];
    }
}
