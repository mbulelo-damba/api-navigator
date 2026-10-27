using ApiNavigator.Application.Catalogue;
using Xunit;

namespace ApiNavigator.UnitTests.Catalogue;

public sealed class ApiCatalogueServiceTests
{
    private readonly ApiCatalogueService _service;

    public ApiCatalogueServiceTests()
    {
        IApiCatalogueRepository repository = new TestApiCatalogueRepository();
        _service = new ApiCatalogueService(repository);
    }

    [Fact]
    public void Search_WithNoFilters_ReturnsAllEntriesSortedByName()
    {
        var results = _service.Search(searchTerm: null, category: null);

        Assert.Equal(3, results.Count);
        Assert.Equal(
            results.Select(entry => entry.Name).OrderBy(name => name, StringComparer.OrdinalIgnoreCase),
            results.Select(entry => entry.Name));
    }

    [Fact]
    public void Search_WithTerm_FindsMatchingTagsCaseInsensitively()
    {
        var results = _service.Search(searchTerm: "WEATHER", category: null);

        var entry = Assert.Single(results);
        Assert.Equal("Open-Meteo", entry.Name);
    }

    [Fact]
    public void Search_WithCategory_ReturnsOnlyThatCategory()
    {
        var results = _service.Search(searchTerm: null, category: "development");

        var entry = Assert.Single(results);
        Assert.Equal("JSONPlaceholder", entry.Name);
    }

    private sealed class TestApiCatalogueRepository : IApiCatalogueRepository
    {
        public IReadOnlyList<ApiCatalogueEntry> GetAll() =>
        [
            new(
                "jsonplaceholder",
                "JSONPlaceholder",
                "A fake REST API for prototyping.",
                "Development",
                "https://jsonplaceholder.typicode.com",
                "https://jsonplaceholder.typicode.com/guide/",
                "None",
                ["testing", "mock data"]),

            new(
                "open-meteo",
                "Open-Meteo",
                "Weather forecasts.",
                "Weather",
                "https://api.open-meteo.com",
                "https://open-meteo.com/en/docs",
                "None",
                ["weather", "forecast"]),

            new(
                "dog-ceo",
                "Dog CEO",
                "Dog images and breed information.",
                "Animals",
                "https://dog.ceo/api",
                "https://dog.ceo/dog-api/documentation/",
                "None",
                ["dogs", "animals"])
        ];
    }
}
