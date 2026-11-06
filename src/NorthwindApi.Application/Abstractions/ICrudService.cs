using NorthwindApi.Application.Common;

namespace NorthwindApi.Application.Abstractions;

/// <summary>
/// Contrato CRUD genérico. Cada entidad concreta lo extiende fijando su clave,
/// sus DTOs y su objeto de filtro fuertemente tipado.
/// </summary>
public interface ICrudService<TKey, TDto, TCreate, TUpdate, TFilter>
{
    Task<PagedResult<TDto>> GetPagedAsync(PageQuery paging, TFilter filter, CancellationToken ct = default);
    Task<TDto?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<TDto> CreateAsync(TCreate dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(TKey id, TUpdate dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
}

// ----------------------- Entidades -----------------------
public interface ICategoryService : ICrudService<int, Dtos.CategoryDto, Dtos.CategoryCreateDto, Dtos.CategoryUpdateDto, Dtos.CategoryFilter> { }

public interface ICustomerService : ICrudService<string, Dtos.CustomerDto, Dtos.CustomerCreateDto, Dtos.CustomerUpdateDto, Dtos.CustomerFilter> { }

public interface IEmployeeService : ICrudService<int, Dtos.EmployeeDto, Dtos.EmployeeCreateDto, Dtos.EmployeeUpdateDto, Dtos.EmployeeFilter> { }

public interface IShipperService : ICrudService<int, Dtos.ShipperDto, Dtos.ShipperCreateDto, Dtos.ShipperUpdateDto, Dtos.ShipperFilter> { }

public interface ISupplierService : ICrudService<int, Dtos.SupplierDto, Dtos.SupplierCreateDto, Dtos.SupplierUpdateDto, Dtos.SupplierFilter> { }

public interface IProductService : ICrudService<int, Dtos.ProductDto, Dtos.ProductCreateDto, Dtos.ProductUpdateDto, Dtos.ProductFilter> { }

public interface IOrderService : ICrudService<int, Dtos.OrderDto, Dtos.OrderCreateDto, Dtos.OrderUpdateDto, Dtos.OrderFilter> { }

public interface IRegionService : ICrudService<int, Dtos.RegionDto, Dtos.RegionCreateDto, Dtos.RegionUpdateDto, Dtos.RegionFilter> { }

public interface ITerritoryService : ICrudService<string, Dtos.TerritoryDto, Dtos.TerritoryCreateDto, Dtos.TerritoryUpdateDto, Dtos.TerritoryFilter> { }

public interface ICustomerDemographicService : ICrudService<string, Dtos.CustomerDemographicDto, Dtos.CustomerDemographicCreateDto, Dtos.CustomerDemographicUpdateDto, Dtos.CustomerDemographicFilter> { }

/// <summary>Clave compuesta de "Order Details".</summary>
public readonly record struct OrderDetailKey(int OrderId, int ProductId);

/// <summary>
/// Servicio específico para "Order Details" por su clave compuesta y por la
/// validación explícita de las referencias a pedido y producto.
/// </summary>
public interface IOrderDetailService
{
    Task<PagedResult<Dtos.OrderDetailDto>> GetByOrderAsync(int orderId, PageQuery paging, CancellationToken ct = default);
    Task<Dtos.OrderDetailDto?> GetAsync(int orderId, int productId, CancellationToken ct = default);
    Task<Dtos.OrderDetailDto> CreateAsync(int orderId, Dtos.OrderDetailCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int orderId, int productId, Dtos.OrderDetailUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int orderId, int productId, CancellationToken ct = default);
}
