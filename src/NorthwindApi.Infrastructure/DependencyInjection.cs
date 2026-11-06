using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Infrastructure.Data.Generated;
using NorthwindApi.Infrastructure.Reports;
using NorthwindApi.Infrastructure.Services;

namespace NorthwindApi.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra el DbContext, los servicios de datos (CRUD, vistas, procedimientos,
    /// dashboard, reportes) y los exportadores PDF/XLSX.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<NorthwindContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        RegisterCrudServices(services);

        services.AddScoped<IViewService, ViewService>();
        services.AddScoped<IStoredProcedureService, StoredProcedureService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddSingleton<PdfReportExporter>();
        services.AddSingleton<XlsxReportExporter>();
        services.AddSingleton<IReportExporter, ReportExporter>();

        return services;
    }

    private static void RegisterCrudServices(IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IShipperService, ShipperService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddScoped<IRegionService, RegionService>();
        services.AddScoped<ITerritoryService, TerritoryService>();
        services.AddScoped<ICustomerDemographicService, CustomerDemographicService>();
    }
}
