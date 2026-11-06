using NorthwindApi.Application.Common;
using NorthwindApi.Application.Models;

namespace NorthwindApi.Application.Abstractions;

/// <summary>
/// Acceso de solo lectura a las 16 vistas de Northwind. Cada método corresponde
/// 1:1 con una vista y expone su DTO de columnas exacto.
/// </summary>
public interface IViewService
{
    IReadOnlyList<ViewInfoDto> GetCatalog();

    Task<PagedResult<CustomerSuppliersByCityDto>> GetCustomerSuppliersByCityAsync(string? city, string? relationship, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<AlphabeticalProductDto>> GetAlphabeticalProductsAsync(string? categoryName, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<CurrentProductDto>> GetCurrentProductsAsync(string? productName, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<OrdersQryDto>> GetOrdersQryAsync(string? customerId, string? country, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<ProductsAboveAveragePriceDto>> GetProductsAboveAveragePriceAsync(PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<ProductsByCategoryDto>> GetProductsByCategoryAsync(string? categoryName, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<QuarterlyOrderDto>> GetQuarterlyOrdersAsync(PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<InvoiceDto>> GetInvoicesAsync(string? customerId, string? country, string? shipperName, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<OrderDetailsExtendedDto>> GetOrderDetailsExtendedAsync(int? orderId, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<OrderSubtotalDto>> GetOrderSubtotalsAsync(int? orderId, decimal? minSubtotal, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<ProductSalesFor1997Dto>> GetProductSalesFor1997Async(string? categoryName, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<CategorySalesFor1997Dto>> GetCategorySalesFor1997Async(PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<SalesByCategoryViewDto>> GetSalesByCategoryAsync(string? categoryName, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<SalesTotalsByAmountDto>> GetSalesTotalsByAmountAsync(decimal? minAmount, PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<SummaryOfSalesByQuarterDto>> GetSummaryOfSalesByQuarterAsync(PageQuery paging, CancellationToken ct = default);
    Task<PagedResult<SummaryOfSalesByYearDto>> GetSummaryOfSalesByYearAsync(PageQuery paging, CancellationToken ct = default);
}

/// <summary>Invocación explícita y parametrizada de los 7 procedimientos almacenados.</summary>
public interface IStoredProcedureService
{
    IReadOnlyList<StoredProcedureInfoDto> GetCatalog();

    Task<IReadOnlyList<TenMostExpensiveProductDto>> GetTenMostExpensiveProductsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeSalesByCountryDto>> GetEmployeeSalesByCountryAsync(DateTime beginning, DateTime ending, CancellationToken ct = default);
    Task<IReadOnlyList<SalesByYearProcDto>> GetSalesByYearAsync(DateTime beginning, DateTime ending, CancellationToken ct = default);
    Task<IReadOnlyList<CustOrdersDetailDto>> GetCustomerOrderDetailAsync(int orderId, CancellationToken ct = default);
    Task<IReadOnlyList<CustOrdersOrdersDto>> GetCustomerOrdersAsync(string customerId, CancellationToken ct = default);
    Task<IReadOnlyList<CustOrderHistDto>> GetCustomerOrderHistoryAsync(string customerId, CancellationToken ct = default);
    Task<IReadOnlyList<SalesByCategoryProcDto>> GetSalesByCategoryAsync(string categoryName, string ordYear, CancellationToken ct = default);
}

/// <summary>Métricas agregadas para el dashboard.</summary>
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<SalesByMonthDto>> GetSalesByMonthAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<SalesByQuarterDto>> GetSalesByQuarterAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<SalesByYearDto>> GetSalesByYearAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int top, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<TopCategoryDto>> GetTopCategoriesAsync(int top, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<TopCustomerDto>> GetTopCustomersAsync(int top, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<RecentOrderDto>> GetRecentOrdersAsync(int top, CancellationToken ct = default);
    Task<IReadOnlyList<OrderStatusDto>> GetOrderStatusAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}

/// <summary>Construcción de tablas de reporte (usadas por los exportadores PDF/XLSX).</summary>
public interface IReportService
{
    Task<ReportTableDto> BuildSalesByCategoryAsync(string? categoryName, CancellationToken ct = default);
    Task<ReportTableDto> BuildSalesByYearAsync(int? year, CancellationToken ct = default);
    Task<ReportTableDto> BuildTopProductsAsync(int top, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<ReportTableDto> BuildTopCustomersAsync(int top, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<ReportTableDto> BuildInvoicesAsync(string? customerId, CancellationToken ct = default);
}

/// <summary>Serialización de <see cref="ReportTableDto"/> a PDF o XLSX.</summary>
public interface IReportExporter
{
    byte[] ToPdf(ReportTableDto table);
    byte[] ToXlsx(ReportTableDto table);
}
