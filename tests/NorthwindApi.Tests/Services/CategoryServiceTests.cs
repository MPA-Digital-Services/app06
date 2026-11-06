using NorthwindApi.Application.Common;
using NorthwindApi.Application.Dtos;
using NorthwindApi.Infrastructure.Services;

namespace NorthwindApi.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task Create_then_get_by_id_roundtrips()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);

        var created = await service.CreateAsync(new CategoryCreateDto("Bebidas", "Refrescos"));

        Assert.True(created.CategoryId > 0);
        var fetched = await service.GetByIdAsync(created.CategoryId);
        Assert.NotNull(fetched);
        Assert.Equal("Bebidas", fetched!.CategoryName);
        Assert.Equal("Refrescos", fetched.Description);
    }

    [Fact]
    public async Task Get_by_id_returns_null_when_missing()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);

        Assert.Null(await service.GetByIdAsync(12345));
    }

    [Fact]
    public async Task Update_returns_false_when_missing()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);

        Assert.False(await service.UpdateAsync(999, new CategoryUpdateDto("X", null)));
    }

    [Fact]
    public async Task Delete_removes_entity()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);
        var created = await service.CreateAsync(new CategoryCreateDto("Temporal", null));

        Assert.True(await service.DeleteAsync(created.CategoryId));
        Assert.Null(await service.GetByIdAsync(created.CategoryId));
    }

    [Fact]
    public async Task Get_paged_applies_filter_and_pagination()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);
        for (var i = 1; i <= 5; i++)
        {
            await service.CreateAsync(new CategoryCreateDto($"Cat{i:00}", null));
        }

        var page = await service.GetPagedAsync(
            new PageQuery { Page = 2, PageSize = 2 },
            new CategoryFilter(),
            CancellationToken.None);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasNext);
        Assert.True(page.HasPrevious);
    }

    [Fact]
    public async Task Get_paged_filters_by_name()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);
        await service.CreateAsync(new CategoryCreateDto("Bebidas", null));
        await service.CreateAsync(new CategoryCreateDto("Carnes", null));

        var page = await service.GetPagedAsync(
            new PageQuery(),
            new CategoryFilter { CategoryName = "Beb" },
            CancellationToken.None);

        Assert.Single(page.Items);
        Assert.Equal("Bebidas", page.Items[0].CategoryName);
    }

    [Fact]
    public async Task Get_paged_sorts_descending_by_name()
    {
        using var db = InMemoryContextFactory.Create();
        var service = new CategoryService(db);
        await service.CreateAsync(new CategoryCreateDto("Alfa", null));
        await service.CreateAsync(new CategoryCreateDto("Zeta", null));

        var page = await service.GetPagedAsync(
            new PageQuery { Sort = "categoryName", Order = "desc" },
            new CategoryFilter(),
            CancellationToken.None);

        Assert.Equal("Zeta", page.Items[0].CategoryName);
    }
}
