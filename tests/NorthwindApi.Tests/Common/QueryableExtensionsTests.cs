using System.Linq.Expressions;
using NorthwindApi.Application.Common;

namespace NorthwindApi.Tests.Common;

public class QueryableExtensionsTests
{
    private sealed record Row(string Name, int Value);

    private static readonly Row[] Data =
    {
        new("Carlos", 30),
        new("Ana", 10),
        new("Beatriz", 20)
    };

    private static readonly Dictionary<string, Expression<Func<Row, object>>> SortMap = new()
    {
        ["name"] = r => r.Name,
        ["value"] = r => r.Value
    };

    [Fact]
    public void Uses_default_sort_when_no_sort_specified()
    {
        var result = Data.AsQueryable()
            .ApplySort(new PageQuery(), SortMap, r => r.Name)
            .Select(r => r.Name)
            .ToList();

        Assert.Equal(new[] { "Ana", "Beatriz", "Carlos" }, result);
    }

    [Fact]
    public void Applies_whitelisted_sort_field()
    {
        var result = Data.AsQueryable()
            .ApplySort(new PageQuery { Sort = "value" }, SortMap, r => r.Name)
            .Select(r => r.Value)
            .ToList();

        Assert.Equal(new[] { 10, 20, 30 }, result);
    }

    [Fact]
    public void Applies_descending_order()
    {
        var result = Data.AsQueryable()
            .ApplySort(new PageQuery { Sort = "value", Order = "desc" }, SortMap, r => r.Name)
            .Select(r => r.Value)
            .ToList();

        Assert.Equal(new[] { 30, 20, 10 }, result);
    }

    [Fact]
    public void Unknown_sort_field_falls_back_to_default()
    {
        var result = Data.AsQueryable()
            .ApplySort(new PageQuery { Sort = "DROP TABLE Users" }, SortMap, r => r.Name)
            .Select(r => r.Name)
            .ToList();

        Assert.Equal(new[] { "Ana", "Beatriz", "Carlos" }, result);
    }
}
