using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiNavigator.IntegrationTests;

public sealed class DiscoveryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public DiscoveryEndpointTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/discovery")]
    [InlineData("/api/discovery?search=")]
    [InlineData("/api/discovery?search=%20%20")]
    public async Task GetDiscovery_RejectsMissingOrBlankSearch(string url)
    {
        using var response = await client.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDiscovery_RejectsSearchLongerThan100Characters()
    {
        var search = new string('a', 101);
        using var response = await client.GetAsync($"/api/discovery?search={search}");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
