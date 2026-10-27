using System.Net;
using System.Net.Http.Json;
using ApiNavigator.Application.Catalogue;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiNavigator.IntegrationTests;

public sealed class CatalogueEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CatalogueEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCatalogue_ReturnsCuratedEntries()
    {
        using var response = await _client.GetAsync("/api/catalogue");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entries = await response.Content.ReadFromJsonAsync<List<ApiCatalogueEntry>>();

        Assert.NotNull(entries);
        Assert.Equal(5, entries.Count);
        Assert.Contains(entries, entry => entry.Name == "Open-Meteo");
    }

    [Fact]
    public async Task GetCatalogue_WithSearchTerm_ReturnsMatchingEntries()
    {
        var entries = await _client.GetFromJsonAsync<List<ApiCatalogueEntry>>(
            "/api/catalogue?search=weather");

        var entry = Assert.Single(entries!);
        Assert.Equal("Open-Meteo", entry.Name);
    }

    [Fact]
    public async Task GetCatalogue_WithCategory_ReturnsMatchingCategory()
    {
        var entries = await _client.GetFromJsonAsync<List<ApiCatalogueEntry>>(
            "/api/catalogue?category=Books");

        var entry = Assert.Single(entries!);
        Assert.Equal("Open Library", entry.Name);
    }
}
