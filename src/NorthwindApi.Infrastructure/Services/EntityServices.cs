using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Application.Abstractions;
using NorthwindApi.Application.Common;
using NorthwindApi.Application.Dtos;
using NorthwindApi.Infrastructure.Data.Generated;

namespace NorthwindApi.Infrastructure.Services;

// ============================================================================
//  Implementaciones CRUD de las entidades de Northwind.
//  Todas heredan de CrudServiceBase para reutilizar paginación/orden/validación.
// ============================================================================

public sealed class CategoryService : CrudServiceBase<Category, int, CategoryDto, CategoryCreateDto, CategoryUpdateDto, CategoryFilter>, ICategoryService
{
    private static readonly Dictionary<string, Expression<Func<Category, object>>> SortMap = new()
    {
        ["categoryId"] = e => e.CategoryID,
        ["categoryName"] = e => e.CategoryName
    };

    public CategoryService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Category> BuildQuery(CategoryFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.CategoryName))
        {
            q = q.Where(e => e.CategoryName.Contains(filter.CategoryName));
        }
        return q;
    }

    protected override IQueryable<Category> ApplySort(IQueryable<Category> q, PageQuery p) => Sort(q, p, SortMap, e => e.CategoryName);
    protected override Task<Category?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override CategoryDto Map(Category e) => new(e.CategoryID, e.CategoryName, e.Description);
    protected override Category ToEntity(CategoryCreateDto d) => new() { CategoryName = d.CategoryName, Description = d.Description };
    protected override void Apply(Category e, CategoryUpdateDto d) { e.CategoryName = d.CategoryName; e.Description = d.Description; }
}

public sealed class ShipperService : CrudServiceBase<Shipper, int, ShipperDto, ShipperCreateDto, ShipperUpdateDto, ShipperFilter>, IShipperService
{
    private static readonly Dictionary<string, Expression<Func<Shipper, object>>> SortMap = new()
    {
        ["shipperId"] = e => e.ShipperID,
        ["companyName"] = e => e.CompanyName
    };

    public ShipperService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Shipper> BuildQuery(ShipperFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.CompanyName))
        {
            q = q.Where(e => e.CompanyName.Contains(filter.CompanyName));
        }
        return q;
    }

    protected override IQueryable<Shipper> ApplySort(IQueryable<Shipper> q, PageQuery p) => Sort(q, p, SortMap, e => e.CompanyName);
    protected override Task<Shipper?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override ShipperDto Map(Shipper e) => new(e.ShipperID, e.CompanyName, e.Phone);
    protected override Shipper ToEntity(ShipperCreateDto d) => new() { CompanyName = d.CompanyName, Phone = d.Phone };
    protected override void Apply(Shipper e, ShipperUpdateDto d) { e.CompanyName = d.CompanyName; e.Phone = d.Phone; }
}

public sealed class SupplierService : CrudServiceBase<Supplier, int, SupplierDto, SupplierCreateDto, SupplierUpdateDto, SupplierFilter>, ISupplierService
{
    private static readonly Dictionary<string, Expression<Func<Supplier, object>>> SortMap = new()
    {
        ["supplierId"] = e => e.SupplierID,
        ["companyName"] = e => e.CompanyName,
        ["country"] = e => e.Country!,
        ["city"] = e => e.City!
    };

    public SupplierService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Supplier> BuildQuery(SupplierFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.CompanyName)) q = q.Where(e => e.CompanyName.Contains(filter.CompanyName));
        if (!string.IsNullOrWhiteSpace(filter.City)) q = q.Where(e => e.City == filter.City);
        if (!string.IsNullOrWhiteSpace(filter.Country)) q = q.Where(e => e.Country == filter.Country);
        return q;
    }

    protected override IQueryable<Supplier> ApplySort(IQueryable<Supplier> q, PageQuery p) => Sort(q, p, SortMap, e => e.CompanyName);
    protected override Task<Supplier?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override SupplierDto Map(Supplier e) => new(e.SupplierID, e.CompanyName, e.ContactName, e.ContactTitle, e.Address, e.City, e.Region, e.PostalCode, e.Country, e.Phone, e.Fax, e.HomePage);

    protected override Supplier ToEntity(SupplierCreateDto d) => new()
    {
        CompanyName = d.CompanyName, ContactName = d.ContactName, ContactTitle = d.ContactTitle,
        Address = d.Address, City = d.City, Region = d.Region, PostalCode = d.PostalCode,
        Country = d.Country, Phone = d.Phone, Fax = d.Fax, HomePage = d.HomePage
    };

    protected override void Apply(Supplier e, SupplierUpdateDto d)
    {
        e.CompanyName = d.CompanyName; e.ContactName = d.ContactName; e.ContactTitle = d.ContactTitle;
        e.Address = d.Address; e.City = d.City; e.Region = d.Region; e.PostalCode = d.PostalCode;
        e.Country = d.Country; e.Phone = d.Phone; e.Fax = d.Fax; e.HomePage = d.HomePage;
    }
}

public sealed class CustomerService : CrudServiceBase<Customer, string, CustomerDto, CustomerCreateDto, CustomerUpdateDto, CustomerFilter>, ICustomerService
{
    private static readonly Dictionary<string, Expression<Func<Customer, object>>> SortMap = new()
    {
        ["customerId"] = e => e.CustomerID,
        ["companyName"] = e => e.CompanyName,
        ["contactName"] = e => e.ContactName!,
        ["country"] = e => e.Country!,
        ["city"] = e => e.City!
    };

    public CustomerService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Customer> BuildQuery(CustomerFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.CompanyName)) q = q.Where(e => e.CompanyName.Contains(filter.CompanyName));
        if (!string.IsNullOrWhiteSpace(filter.ContactName)) q = q.Where(e => e.ContactName != null && e.ContactName.Contains(filter.ContactName));
        if (!string.IsNullOrWhiteSpace(filter.City)) q = q.Where(e => e.City == filter.City);
        if (!string.IsNullOrWhiteSpace(filter.Country)) q = q.Where(e => e.Country == filter.Country);
        return q;
    }

    protected override IQueryable<Customer> ApplySort(IQueryable<Customer> q, PageQuery p) => Sort(q, p, SortMap, e => e.CustomerID);
    protected override Task<Customer?> FindByIdAsync(string id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override CustomerDto Map(Customer e) => new(e.CustomerID, e.CompanyName, e.ContactName, e.ContactTitle, e.Address, e.City, e.Region, e.PostalCode, e.Country, e.Phone, e.Fax);

    protected override Customer ToEntity(CustomerCreateDto d) => new()
    {
        CustomerID = d.CustomerId.Trim(), CompanyName = d.CompanyName, ContactName = d.ContactName,
        ContactTitle = d.ContactTitle, Address = d.Address, City = d.City, Region = d.Region,
        PostalCode = d.PostalCode, Country = d.Country, Phone = d.Phone, Fax = d.Fax
    };

    protected override void Apply(Customer e, CustomerUpdateDto d)
    {
        e.CompanyName = d.CompanyName; e.ContactName = d.ContactName; e.ContactTitle = d.ContactTitle;
        e.Address = d.Address; e.City = d.City; e.Region = d.Region; e.PostalCode = d.PostalCode;
        e.Country = d.Country; e.Phone = d.Phone; e.Fax = d.Fax;
    }
}

public sealed class EmployeeService : CrudServiceBase<Employee, int, EmployeeDto, EmployeeCreateDto, EmployeeUpdateDto, EmployeeFilter>, IEmployeeService
{
    private static readonly Dictionary<string, Expression<Func<Employee, object>>> SortMap = new()
    {
        ["employeeId"] = e => e.EmployeeID,
        ["lastName"] = e => e.LastName,
        ["firstName"] = e => e.FirstName,
        ["hireDate"] = e => e.HireDate!
    };

    public EmployeeService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Employee> BuildQuery(EmployeeFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.LastName)) q = q.Where(e => e.LastName.Contains(filter.LastName));
        if (!string.IsNullOrWhiteSpace(filter.FirstName)) q = q.Where(e => e.FirstName.Contains(filter.FirstName));
        if (!string.IsNullOrWhiteSpace(filter.Title)) q = q.Where(e => e.Title == filter.Title);
        if (!string.IsNullOrWhiteSpace(filter.Country)) q = q.Where(e => e.Country == filter.Country);
        if (filter.ReportsTo.HasValue) q = q.Where(e => e.ReportsTo == filter.ReportsTo);
        return q;
    }

    protected override IQueryable<Employee> ApplySort(IQueryable<Employee> q, PageQuery p) => Sort(q, p, SortMap, e => e.LastName);
    protected override Task<Employee?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override EmployeeDto Map(Employee e) => new(e.EmployeeID, e.LastName, e.FirstName, e.Title, e.TitleOfCourtesy, e.BirthDate, e.HireDate, e.Address, e.City, e.Region, e.PostalCode, e.Country, e.HomePhone, e.Extension, e.Notes, e.ReportsTo, e.PhotoPath);

    protected override Employee ToEntity(EmployeeCreateDto d) => new()
    {
        LastName = d.LastName, FirstName = d.FirstName, Title = d.Title, TitleOfCourtesy = d.TitleOfCourtesy,
        BirthDate = d.BirthDate, HireDate = d.HireDate, Address = d.Address, City = d.City, Region = d.Region,
        PostalCode = d.PostalCode, Country = d.Country, HomePhone = d.HomePhone, Extension = d.Extension,
        Notes = d.Notes, ReportsTo = d.ReportsTo, PhotoPath = d.PhotoPath
    };

    protected override void Apply(Employee e, EmployeeUpdateDto d)
    {
        e.LastName = d.LastName; e.FirstName = d.FirstName; e.Title = d.Title; e.TitleOfCourtesy = d.TitleOfCourtesy;
        e.BirthDate = d.BirthDate; e.HireDate = d.HireDate; e.Address = d.Address; e.City = d.City; e.Region = d.Region;
        e.PostalCode = d.PostalCode; e.Country = d.Country; e.HomePhone = d.HomePhone; e.Extension = d.Extension;
        e.Notes = d.Notes; e.ReportsTo = d.ReportsTo; e.PhotoPath = d.PhotoPath;
    }

    protected override Task ValidateReferencesAsync(Employee entity, CancellationToken ct)
    {
        // CK_Birthdate exige BirthDate < getdate().
        if (entity.BirthDate.HasValue && entity.BirthDate.Value >= DateTime.Now)
        {
            throw new AppValidationException(new Dictionary<string, string[]>
            {
                ["birthDate"] = new[] { "La fecha de nacimiento debe ser anterior a la fecha actual." }
            });
        }
        return Task.CompletedTask;
    }
}

public sealed class ProductService : CrudServiceBase<Product, int, ProductDto, ProductCreateDto, ProductUpdateDto, ProductFilter>, IProductService
{
    private static readonly Dictionary<string, Expression<Func<Product, object>>> SortMap = new()
    {
        ["productId"] = e => e.ProductID,
        ["productName"] = e => e.ProductName,
        ["unitPrice"] = e => e.UnitPrice!,
        ["unitsInStock"] = e => e.UnitsInStock!
    };

    public ProductService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Product> BuildQuery(ProductFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.ProductName)) q = q.Where(e => e.ProductName.Contains(filter.ProductName));
        if (filter.CategoryId.HasValue) q = q.Where(e => e.CategoryID == filter.CategoryId);
        if (filter.SupplierId.HasValue) q = q.Where(e => e.SupplierID == filter.SupplierId);
        if (filter.Discontinued.HasValue) q = q.Where(e => e.Discontinued == filter.Discontinued);
        if (filter.MinUnitPrice.HasValue) q = q.Where(e => e.UnitPrice >= filter.MinUnitPrice);
        if (filter.MaxUnitPrice.HasValue) q = q.Where(e => e.UnitPrice <= filter.MaxUnitPrice);
        if (filter.InStock.HasValue) q = q.Where(e => e.UnitsInStock >= filter.InStock);
        return q;
    }

    protected override IQueryable<Product> ApplySort(IQueryable<Product> q, PageQuery p) => Sort(q, p, SortMap, e => e.ProductName);
    protected override Task<Product?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override ProductDto Map(Product e) => new(e.ProductID, e.ProductName, e.SupplierID, e.CategoryID, e.QuantityPerUnit, e.UnitPrice, e.UnitsInStock, e.UnitsOnOrder, e.ReorderLevel, e.Discontinued);

    protected override Product ToEntity(ProductCreateDto d) => new()
    {
        ProductName = d.ProductName, SupplierID = d.SupplierId, CategoryID = d.CategoryId,
        QuantityPerUnit = d.QuantityPerUnit, UnitPrice = d.UnitPrice, UnitsInStock = d.UnitsInStock,
        UnitsOnOrder = d.UnitsOnOrder, ReorderLevel = d.ReorderLevel, Discontinued = d.Discontinued
    };

    protected override void Apply(Product e, ProductUpdateDto d)
    {
        e.ProductName = d.ProductName; e.SupplierID = d.SupplierId; e.CategoryID = d.CategoryId;
        e.QuantityPerUnit = d.QuantityPerUnit; e.UnitPrice = d.UnitPrice; e.UnitsInStock = d.UnitsInStock;
        e.UnitsOnOrder = d.UnitsOnOrder; e.ReorderLevel = d.ReorderLevel; e.Discontinued = d.Discontinued;
    }
}

public sealed class OrderService : CrudServiceBase<Order, int, OrderDto, OrderCreateDto, OrderUpdateDto, OrderFilter>, IOrderService
{
    private static readonly Dictionary<string, Expression<Func<Order, object>>> SortMap = new()
    {
        ["orderId"] = e => e.OrderID,
        ["orderDate"] = e => e.OrderDate!,
        ["shippedDate"] = e => e.ShippedDate!,
        ["freight"] = e => e.Freight!
    };

    public OrderService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Order> BuildQuery(OrderFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.CustomerId)) q = q.Where(e => e.CustomerID == filter.CustomerId);
        if (filter.EmployeeId.HasValue) q = q.Where(e => e.EmployeeID == filter.EmployeeId);
        if (filter.ShipVia.HasValue) q = q.Where(e => e.ShipVia == filter.ShipVia);
        if (filter.FromDate.HasValue) q = q.Where(e => e.OrderDate >= filter.FromDate);
        if (filter.ToDate.HasValue) q = q.Where(e => e.OrderDate <= filter.ToDate);
        if (filter.Shipped.HasValue) q = filter.Shipped.Value ? q.Where(e => e.ShippedDate != null) : q.Where(e => e.ShippedDate == null);
        return q;
    }

    protected override IQueryable<Order> ApplySort(IQueryable<Order> q, PageQuery p) => Sort(q, p, SortMap, e => e.OrderDate!);
    protected override Task<Order?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override OrderDto Map(Order e) => new(e.OrderID, e.CustomerID, e.EmployeeID, e.OrderDate, e.RequiredDate, e.ShippedDate, e.ShipVia, e.Freight, e.ShipName, e.ShipAddress, e.ShipCity, e.ShipRegion, e.ShipPostalCode, e.ShipCountry);

    protected override Order ToEntity(OrderCreateDto d) => new()
    {
        CustomerID = d.CustomerId, EmployeeID = d.EmployeeId, OrderDate = d.OrderDate, RequiredDate = d.RequiredDate,
        ShippedDate = d.ShippedDate, ShipVia = d.ShipVia, Freight = d.Freight, ShipName = d.ShipName,
        ShipAddress = d.ShipAddress, ShipCity = d.ShipCity, ShipRegion = d.ShipRegion,
        ShipPostalCode = d.ShipPostalCode, ShipCountry = d.ShipCountry
    };

    protected override void Apply(Order e, OrderUpdateDto d)
    {
        e.CustomerID = d.CustomerId; e.EmployeeID = d.EmployeeId; e.OrderDate = d.OrderDate; e.RequiredDate = d.RequiredDate;
        e.ShippedDate = d.ShippedDate; e.ShipVia = d.ShipVia; e.Freight = d.Freight; e.ShipName = d.ShipName;
        e.ShipAddress = d.ShipAddress; e.ShipCity = d.ShipCity; e.ShipRegion = d.ShipRegion;
        e.ShipPostalCode = d.ShipPostalCode; e.ShipCountry = d.ShipCountry;
    }
}

public sealed class RegionService : CrudServiceBase<Region, int, RegionDto, RegionCreateDto, RegionUpdateDto, RegionFilter>, IRegionService
{
    private static readonly Dictionary<string, Expression<Func<Region, object>>> SortMap = new()
    {
        ["regionId"] = e => e.RegionID,
        ["regionDescription"] = e => e.RegionDescription
    };

    public RegionService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Region> BuildQuery(RegionFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.RegionDescription)) q = q.Where(e => e.RegionDescription.Contains(filter.RegionDescription));
        return q;
    }

    protected override IQueryable<Region> ApplySort(IQueryable<Region> q, PageQuery p) => Sort(q, p, SortMap, e => e.RegionID);
    protected override Task<Region?> FindByIdAsync(int id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override RegionDto Map(Region e) => new(e.RegionID, e.RegionDescription);
    protected override Region ToEntity(RegionCreateDto d) => new() { RegionID = d.RegionId, RegionDescription = d.RegionDescription };
    protected override void Apply(Region e, RegionUpdateDto d) { e.RegionDescription = d.RegionDescription; }
}

public sealed class TerritoryService : CrudServiceBase<Territory, string, TerritoryDto, TerritoryCreateDto, TerritoryUpdateDto, TerritoryFilter>, ITerritoryService
{
    private static readonly Dictionary<string, Expression<Func<Territory, object>>> SortMap = new()
    {
        ["territoryId"] = e => e.TerritoryID,
        ["territoryDescription"] = e => e.TerritoryDescription,
        ["regionId"] = e => e.RegionID
    };

    public TerritoryService(NorthwindContext db) : base(db) { }

    protected override IQueryable<Territory> BuildQuery(TerritoryFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.TerritoryDescription)) q = q.Where(e => e.TerritoryDescription.Contains(filter.TerritoryDescription));
        if (filter.RegionId.HasValue) q = q.Where(e => e.RegionID == filter.RegionId);
        return q;
    }

    protected override IQueryable<Territory> ApplySort(IQueryable<Territory> q, PageQuery p) => Sort(q, p, SortMap, e => e.TerritoryID);
    protected override Task<Territory?> FindByIdAsync(string id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override TerritoryDto Map(Territory e) => new(e.TerritoryID, e.TerritoryDescription, e.RegionID);
    protected override Territory ToEntity(TerritoryCreateDto d) => new() { TerritoryID = d.TerritoryId, TerritoryDescription = d.TerritoryDescription, RegionID = d.RegionId };
    protected override void Apply(Territory e, TerritoryUpdateDto d) { e.TerritoryDescription = d.TerritoryDescription; e.RegionID = d.RegionId; }
}

public sealed class CustomerDemographicService : CrudServiceBase<CustomerDemographic, string, CustomerDemographicDto, CustomerDemographicCreateDto, CustomerDemographicUpdateDto, CustomerDemographicFilter>, ICustomerDemographicService
{
    private static readonly Dictionary<string, Expression<Func<CustomerDemographic, object>>> SortMap = new()
    {
        ["customerTypeId"] = e => e.CustomerTypeID
    };

    public CustomerDemographicService(NorthwindContext db) : base(db) { }

    protected override IQueryable<CustomerDemographic> BuildQuery(CustomerDemographicFilter filter)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filter.CustomerTypeId)) q = q.Where(e => e.CustomerTypeID == filter.CustomerTypeId);
        return q;
    }

    protected override IQueryable<CustomerDemographic> ApplySort(IQueryable<CustomerDemographic> q, PageQuery p) => Sort(q, p, SortMap, e => e.CustomerTypeID);
    protected override Task<CustomerDemographic?> FindByIdAsync(string id, CancellationToken ct) => Set.FindAsync(new object?[] { id }, ct).AsTask();
    protected override CustomerDemographicDto Map(CustomerDemographic e) => new(e.CustomerTypeID, e.CustomerDesc);
    protected override CustomerDemographic ToEntity(CustomerDemographicCreateDto d) => new() { CustomerTypeID = d.CustomerTypeId.Trim(), CustomerDesc = d.CustomerDesc };
    protected override void Apply(CustomerDemographic e, CustomerDemographicUpdateDto d) { e.CustomerDesc = d.CustomerDesc; }
}

// ============================================================================
//  Order Details: servicio dedicado por su clave compuesta (OrderID, ProductID).
// ============================================================================
public sealed class OrderDetailService : IOrderDetailService
{
    private readonly NorthwindContext _db;

    public OrderDetailService(NorthwindContext db) => _db = db;

    public async Task<PagedResult<OrderDetailDto>> GetByOrderAsync(int orderId, PageQuery paging, CancellationToken ct = default)
    {
        var query = _db.Order_Details.AsNoTracking().Where(d => d.OrderID == orderId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(d => d.ProductID).Skip(paging.Skip).Take(paging.PageSize).ToListAsync(ct);
        return new PagedResult<OrderDetailDto>
        {
            Items = items.Select(Map).ToList(),
            Page = paging.Page,
            PageSize = paging.PageSize,
            TotalCount = total
        };
    }

    public async Task<OrderDetailDto?> GetAsync(int orderId, int productId, CancellationToken ct = default)
    {
        var entity = await _db.Order_Details.AsNoTracking().FirstOrDefaultAsync(d => d.OrderID == orderId && d.ProductID == productId, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<OrderDetailDto> CreateAsync(int orderId, OrderDetailCreateDto dto, CancellationToken ct = default)
    {
        var errors = new Dictionary<string, string[]>();

        if (!await _db.Orders.AnyAsync(o => o.OrderID == orderId, ct))
        {
            errors["orderId"] = new[] { $"El pedido {orderId} no existe." };
        }

        if (!await _db.Products.AnyAsync(p => p.ProductID == dto.ProductId, ct))
        {
            errors["productId"] = new[] { $"El producto {dto.ProductId} no existe." };
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException(errors);
        }

        if (await _db.Order_Details.AnyAsync(d => d.OrderID == orderId && d.ProductID == dto.ProductId, ct))
        {
            throw new ConflictException($"Ya existe una línea para el pedido {orderId} y producto {dto.ProductId}.");
        }

        var entity = new Order_Detail
        {
            OrderID = orderId,
            ProductID = dto.ProductId,
            UnitPrice = dto.UnitPrice,
            Quantity = dto.Quantity,
            Discount = dto.Discount
        };

        _db.Order_Details.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<bool> UpdateAsync(int orderId, int productId, OrderDetailUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _db.Order_Details.FirstOrDefaultAsync(d => d.OrderID == orderId && d.ProductID == productId, ct);
        if (entity is null)
        {
            return false;
        }

        entity.UnitPrice = dto.UnitPrice;
        entity.Quantity = dto.Quantity;
        entity.Discount = dto.Discount;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int orderId, int productId, CancellationToken ct = default)
    {
        var entity = await _db.Order_Details.FirstOrDefaultAsync(d => d.OrderID == orderId && d.ProductID == productId, ct);
        if (entity is null)
        {
            return false;
        }

        _db.Order_Details.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static OrderDetailDto Map(Order_Detail e) => new(e.OrderID, e.ProductID, e.UnitPrice, e.Quantity, e.Discount);
}
