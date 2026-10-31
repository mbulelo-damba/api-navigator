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

    [Fact]
    public void GetById_WithExistingId_ReturnsMatchingEntryCaseInsensitively()
    {
        var entry = _service.GetById("OPEN-METEO");

        Assert.NotNull(entry);
        Assert.Equal("Open-Meteo", entry.Name);
    }

    [Fact]
    public void GetById_WithUnknownId_ReturnsNull()
    {
        var entry = _service.GetById("unknown-api");

        Assert.Null(entry);
    }

    private sealed class TestApiCatalogueRepository : IApiCatalogueRepository
    {
        private static readonly IReadOnlyList<ApiCatalogueEntry> Entries =
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

        public IReadOnlyList<ApiCatalogueEntry> GetAll() => Entries;

        public ApiCatalogueEntry? GetById(string id) =>
            Entries.FirstOrDefault(entry =>
                string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
