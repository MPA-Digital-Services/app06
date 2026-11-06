using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Dtos;

namespace NorthwindApi.Api.Controllers;

/// <summary>Categorías de productos.</summary>
public sealed class CategoriesController : CrudControllerBase<int, CategoryDto, CategoryCreateDto, CategoryUpdateDto, CategoryFilter>
{
    public CategoriesController(ICategoryService service) : base(service) { }
    protected override int GetKey(CategoryDto dto) => dto.CategoryId;
}

/// <summary>Transportistas (Shippers).</summary>
public sealed class ShippersController : CrudControllerBase<int, ShipperDto, ShipperCreateDto, ShipperUpdateDto, ShipperFilter>
{
    public ShippersController(IShipperService service) : base(service) { }
    protected override int GetKey(ShipperDto dto) => dto.ShipperId;
}

/// <summary>Proveedores.</summary>
public sealed class SuppliersController : CrudControllerBase<int, SupplierDto, SupplierCreateDto, SupplierUpdateDto, SupplierFilter>
{
    public SuppliersController(ISupplierService service) : base(service) { }
    protected override int GetKey(SupplierDto dto) => dto.SupplierId;
}

/// <summary>Clientes.</summary>
public sealed class CustomersController : CrudControllerBase<string, CustomerDto, CustomerCreateDto, CustomerUpdateDto, CustomerFilter>
{
    public CustomersController(ICustomerService service) : base(service) { }
    protected override string GetKey(CustomerDto dto) => dto.CustomerId;
}

/// <summary>Empleados.</summary>
public sealed class EmployeesController : CrudControllerBase<int, EmployeeDto, EmployeeCreateDto, EmployeeUpdateDto, EmployeeFilter>
{
    public EmployeesController(IEmployeeService service) : base(service) { }
    protected override int GetKey(EmployeeDto dto) => dto.EmployeeId;
}

/// <summary>Productos.</summary>
public sealed class ProductsController : CrudControllerBase<int, ProductDto, ProductCreateDto, ProductUpdateDto, ProductFilter>
{
    public ProductsController(IProductService service) : base(service) { }
    protected override int GetKey(ProductDto dto) => dto.ProductId;
}

/// <summary>Regiones.</summary>
public sealed class RegionsController : CrudControllerBase<int, RegionDto, RegionCreateDto, RegionUpdateDto, RegionFilter>
{
    public RegionsController(IRegionService service) : base(service) { }
    protected override int GetKey(RegionDto dto) => dto.RegionId;
}

/// <summary>Territorios.</summary>
public sealed class TerritoriesController : CrudControllerBase<string, TerritoryDto, TerritoryCreateDto, TerritoryUpdateDto, TerritoryFilter>
{
    public TerritoriesController(ITerritoryService service) : base(service) { }
    protected override string GetKey(TerritoryDto dto) => dto.TerritoryId;
}

/// <summary>Demográficos de cliente.</summary>
public sealed class CustomerDemographicsController : CrudControllerBase<string, CustomerDemographicDto, CustomerDemographicCreateDto, CustomerDemographicUpdateDto, CustomerDemographicFilter>
{
    public CustomerDemographicsController(ICustomerDemographicService service) : base(service) { }
    protected override string GetKey(CustomerDemographicDto dto) => dto.CustomerTypeId;
}
