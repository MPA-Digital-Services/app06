using System.Net;
using System.Net.Http.Json;
using NorthwindApi.Application.Dtos;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Tests.Integration;

/// <summary>
/// Pruebas de integración extremo a extremo contra la base de datos Northwind real.
/// Requieren SQL Server / LocalDB con la base de datos Northwind cargada
/// (o la variable NORTHWIND_TEST_CONNECTION apuntando a una instancia accesible).
/// </summary>
public sealed class ApiEndpointsTests : IClassFixture<NorthwindApiFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointsTests(NorthwindApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_healthy()
    {
        using var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", payload);
    }

    [Fact]
    public async Task Dashboard_summary_returns_seeded_data()
    {
        var summary = await _client.GetFromJsonAsync<DashboardSummaryDto>("/api/dashboard/summary");

        Assert.NotNull(summary);
        Assert.True(summary!.TotalOrders > 0);
        Assert.True(summary.TotalSales > 0);
        Assert.True(summary.TotalCustomers > 0);
        Assert.True(summary.TotalProducts > 0);
    }

    [Fact]
    public async Task Views_catalog_lists_all_16_views()
    {
        var catalog = await _client.GetFromJsonAsync<List<ViewInfoDto>>("/api/views");

        Assert.NotNull(catalog);
        Assert.Equal(16, catalog!.Count);
        Assert.Contains(catalog, v => v.Route == "invoices");
    }

    [Fact]
    public async Task Invoices_view_returns_rows()
    {
        var page = await _client.GetFromJsonAsync<PagedResultWrapper<InvoiceDto>>("/api/views/invoices?page=1&pageSize=5");

        Assert.NotNull(page);
        Assert.NotEmpty(page!.Items);
        Assert.True(page.TotalCount > 0);
    }

    [Fact]
    public async Task Stored_procedure_ten_most_expensive_returns_10()
    {
        var rows = await _client.GetFromJsonAsync<List<TenMostExpensiveProductDto>>(
            "/api/stored-procedures/ten-most-expensive-products");

        Assert.NotNull(rows);
        Assert.Equal(10, rows!.Count);
    }

    [Fact]
    public async Task Stored_procedure_customer_orders_works_for_known_customer()
    {
        var rows = await _client.GetFromJsonAsync<List<CustOrdersOrdersDto>>(
            "/api/stored-procedures/customer-orders/ALFKI");

        Assert.NotNull(rows);
        Assert.NotEmpty(rows!);
    }

    [Fact]
    public async Task Stored_procedure_order_detail_returns_smallint_quantity()
    {
        var rows = await _client.GetFromJsonAsync<List<CustOrdersDetailDto>>(
            "/api/stored-procedures/customer-order-detail/10248");

        Assert.NotNull(rows);
        Assert.NotEmpty(rows!);
        Assert.All(rows!, r => Assert.True(r.Quantity > 0));
    }

    [Fact]
    public async Task Pdf_report_returns_pdf_bytes()
    {
        using var response = await _client.GetAsync("/api/reports/sales-by-category.pdf");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task Xlsx_report_returns_zip_bytes()
    {
        using var response = await _client.GetAsync("/api/reports/top-products.xlsx");

        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, bytes[..4]);
    }

    [Fact]
    public async Task Missing_entity_returns_404()
    {
        using var response = await _client.GetAsync("/api/employees/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_payload_returns_400_problem_details()
    {
        using var response = await _client.PostAsJsonAsync("/api/categories",
            new { categoryName = "", description = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", body);
    }

    [Fact]
    public async Task Duplicate_key_returns_409()
    {
        using var response = await _client.PostAsJsonAsync("/api/customers",
            new { customerId = "ALFKI", companyName = "Duplicado" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_referenced_category_returns_409()
    {
        using var response = await _client.DeleteAsync("/api/categories/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    /// <summary>Envoltura local para deserializar el resultado paginado.</summary>
    private sealed record PagedResultWrapper<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
