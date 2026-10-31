using System.Net;
using ApiNavigator.Application.Discovery;
using ApiNavigator.Infrastructure.Discovery;
using Xunit;

namespace ApiNavigator.UnitTests.Discovery;

public sealed class ApisGuruDiscoverySourceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingPreferredVersionAndSourceAttribution()
    {
        using var client = CreateClient();
        var source = new ApisGuruDiscoverySource(client);

        var results = await source.SearchAsync(new ApiDiscoveryQuery("weather"));

        var result = Assert.Single(results);
        Assert.Equal("Weather API", result.Name);
        Assert.Equal("Forecast information.", result.Description);
        Assert.Equal("https://example.com/openapi.json", result.OpenApiSpecificationUrl);
        Assert.Equal("https://example.com/docs", result.DocumentationUrl);
        Assert.Equal("APIs.guru", result.SourceName);
        Assert.Equal("https://apis.guru/", result.SourceUrl);
    }

    [Fact]
    public async Task SearchAsync_AppliesCategoryFilter()
    {
        using var client = CreateClient();
        var source = new ApisGuruDiscoverySource(client);

        var results = await source.SearchAsync(new ApiDiscoveryQuery("weather", "finance"));

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_PropagatesHttpFailure()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))));
        var source = new ApisGuruDiscoverySource(client);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => source.SearchAsync(new ApiDiscoveryQuery("weather")));
    }

    private static HttpClient CreateClient()
    {
        const string payload = """
        {
          "weather.example": {
            "preferred": "v2",
            "versions": {
              "v1": {
                "info": { "title": "Old Weather API" },
                "swaggerUrl": "https://example.com/v1.json"
              },
              "v2": {
                "info": {
                  "title": "Weather API",
                  "description": "Forecast information.",
                  "x-apisguru-categories": ["science"]
                },
                "externalDocs": { "url": "https://example.com/docs" },
                "swaggerUrl": "https://example.com/openapi.json"
              }
            }
          }
        }
        """;

        return new HttpClient(new StubHttpMessageHandler(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload)
            })));
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => send(request, cancellationToken);
    }
}