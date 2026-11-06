using NorthwindApi.Application.Common;
using NorthwindApi.Application.Dtos;
using NorthwindApi.Infrastructure.Services;

namespace NorthwindApi.Tests.Services;

public class EmployeeServiceTests
{
    [Fact]
    public async Task Create_rejects_future_birth_date()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new EmployeeService(db);

        var dto = new EmployeeCreateDto(
            LastName: "Pérez", FirstName: "Ana", Title: null, TitleOfCourtesy: null,
            BirthDate: DateTime.Now.AddDays(1), HireDate: null, Address: null, City: null,
            Region: null, PostalCode: null, Country: null, HomePhone: null, Extension: null,
            Notes: null, ReportsTo: null, PhotoPath: null);

        var ex = await Assert.ThrowsAsync<AppValidationException>(() => service.CreateAsync(dto));
        Assert.Contains("birthDate", ex.Errors.Keys);
    }

    [Fact]
    public async Task Create_accepts_past_birth_date()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new EmployeeService(db);

        var dto = new EmployeeCreateDto(
            LastName: "Gómez", FirstName: "Luis", Title: null, TitleOfCourtesy: null,
            BirthDate: new DateTime(1980, 1, 1), HireDate: null, Address: null, City: null,
            Region: null, PostalCode: null, Country: null, HomePhone: null, Extension: null,
            Notes: null, ReportsTo: null, PhotoPath: null);

        var created = await service.CreateAsync(dto);

        Assert.True(created.EmployeeId > 0);
        Assert.Equal("Gómez", created.LastName);
    }
}
