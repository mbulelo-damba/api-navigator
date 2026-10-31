using ApiNavigator.Application.Discovery;
using Xunit;

namespace ApiNavigator.UnitTests.Discovery;

public sealed class ApiDiscoveryQueryTests
{
    [Fact]
    public void Constructor_TrimsSearchTermAndCategory()
    {
        var query = new ApiDiscoveryQuery("  weather APIs  ", "  Climate  ");

        Assert.Equal("weather APIs", query.SearchTerm);
        Assert.Equal("Climate", query.Category);
    }

    [Fact]
    public void Constructor_ConvertsBlankCategoryToNull()
    {
        var query = new ApiDiscoveryQuery("weather", "   ");

        Assert.Null(query.Category);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsMissingSearchTerm(string? searchTerm)
    {
        Assert.Throws<ArgumentException>(() => new ApiDiscoveryQuery(searchTerm!));
    }
}
