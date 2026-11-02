using System.Net;
using ApiNavigator.Application.Discovery;
using ApiNavigator.Infrastructure.Discovery;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ApiNavigator.UnitTests.Discovery;

public sealed class ApisGuruDiscoverySourceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchingPreferredVersionAndSourceAttribution()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var client = CreateClient();
        var source = new ApisGuruDiscoverySource(client, cache);

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
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var client = CreateClient();
        var source = new ApisGuruDiscoverySource(client, cache);

        var results = await source.SearchAsync(new ApiDiscoveryQuery("weather", "finance"));

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_CachesSuccessfulCatalogueResponse()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var requests = 0;
        using var client = new HttpClient(new StubHttpMessageHandler((_, _) =>
        {
            requests++;
            return Task.FromResult(JsonResponse(ValidPayload));
        }));
        var source = new ApisGuruDiscoverySource(client, cache);

        await source.SearchAsync(new ApiDiscoveryQuery("weather"));
        await source.SearchAsync(new ApiDiscoveryQuery("weather"));

        Assert.Equal(1, requests);
    }

    [Fact]
    public async Task SearchAsync_PropagatesHttpFailure()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var client = new HttpClient(new StubHttpMessageHandler(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))));
        var source = new ApisGuruDiscoverySource(client, cache);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => source.SearchAsync(new ApiDiscoveryQuery("weather")));
    }

    [Fact]
    public async Task SearchAsync_RejectsMalformedJson()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var client = new HttpClient(new StubHttpMessageHandler(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json")
            })));
        var source = new ApisGuruDiscoverySource(client, cache);

        await Assert.ThrowsAnyAsync<System.Text.Json.JsonException>(
            () => source.SearchAsync(new ApiDiscoveryQuery("weather")));
    }

    [Fact]
    public async Task SearchAsync_RejectsOversizedResponseWithoutContentLength()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var client = new HttpClient(new StubHttpMessageHandler(
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new OversizedStream())
            })));
        var source = new ApisGuruDiscoverySource(client, cache);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => source.SearchAsync(new ApiDiscoveryQuery("weather")));

        Assert.Contains("20 MB", exception.Message);
    }

    [Fact]
    public async Task SearchAsync_PropagatesCallerCancellation()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var client = new HttpClient(new StubHttpMessageHandler(
            async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return JsonResponse(ValidPayload);
            }));
        var source = new ApisGuruDiscoverySource(client, cache);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => source.SearchAsync(new ApiDiscoveryQuery("weather"), cancellation.Token));
    }

    private static HttpClient CreateClient() =>
        new(new StubHttpMessageHandler(
            (_, _) => Task.FromResult(JsonResponse(ValidPayload))));

    private static HttpResponseMessage JsonResponse(string payload) =>
        new(HttpStatusCode.OK) { Content = new StringContent(payload) };

    private const string ValidPayload = """
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

    private sealed class OversizedStream : Stream
    {
        private long position;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 20_000_001;
        public override long Position { get => position; set => throw new NotSupportedException(); }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = Length - position;
            var count = (int)Math.Min(buffer.Length, remaining);
            buffer.Span[..count].Fill((byte)' ');
            position += count;
            return ValueTask.FromResult(count);
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
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