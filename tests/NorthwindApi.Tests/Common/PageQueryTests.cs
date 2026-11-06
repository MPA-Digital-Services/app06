using NorthwindApi.Application.Common;

namespace NorthwindApi.Tests.Common;

public class PageQueryTests
{
    [Fact]
    public void Defaults_are_page_1_and_default_size()
    {
        var q = new PageQuery();

        Assert.Equal(1, q.Page);
        Assert.Equal(PageQuery.DefaultPageSize, q.PageSize);
        Assert.Equal(0, q.Skip);
        Assert.False(q.Descending);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void Page_is_normalized_to_at_least_1(int input, int expected)
    {
        var q = new PageQuery { Page = input };

        Assert.Equal(expected, q.Page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    [InlineData(5, 5)]
    [InlineData(1000, PageQuery.MaxPageSize)]
    public void PageSize_is_clamped(int input, int expected)
    {
        var q = new PageQuery { PageSize = input };

        Assert.Equal(expected, q.PageSize);
    }

    [Fact]
    public void Skip_is_computed_from_page_and_size()
    {
        var q = new PageQuery { Page = 3, PageSize = 10 };

        Assert.Equal(20, q.Skip);
    }

    [Theory]
    [InlineData("desc", true)]
    [InlineData("DESC", true)]
    [InlineData("asc", false)]
    [InlineData(null, false)]
    public void Descending_is_case_insensitive(string? order, bool expected)
    {
        var q = new PageQuery { Order = order };

        Assert.Equal(expected, q.Descending);
    }
}
