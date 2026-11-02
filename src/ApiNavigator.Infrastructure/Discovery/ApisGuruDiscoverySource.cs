using System.Text.Json;
using ApiNavigator.Application.Discovery;
using Microsoft.Extensions.Caching.Memory;

namespace ApiNavigator.Infrastructure.Discovery;

/// <summary>
/// Searches the public APIs.guru OpenAPI directory.
/// </summary>
public sealed class ApisGuruDiscoverySource(HttpClient httpClient, IMemoryCache cache) : IApiDiscoverySource
{
    private const string CatalogueUrl = "https://api.apis.guru/v2/list.json";
    private const string DirectoryUrl = "https://apis.guru/";
    private const string CacheKey = "apis-guru-catalogue-v1";
    private const int MaximumCatalogueBytes = 20_000_000;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
    private static readonly SemaphoreSlim CatalogueLock = new(1, 1);

    public async Task<IReadOnlyList<ApiDiscoveryResult>> SearchAsync(
        ApiDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var payload = await GetCataloguePayloadAsync(cancellationToken);
        using var document = JsonDocument.Parse(payload, new JsonDocumentOptions { MaxDepth = 32 });

        var results = new List<ApiDiscoveryResult>();
        var term = query.SearchTerm;

        foreach (var api in document.RootElement.EnumerateObject())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!api.Value.TryGetProperty("preferred", out var preferredElement) ||
                preferredElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var preferredVersion = preferredElement.GetString();
            if (string.IsNullOrWhiteSpace(preferredVersion) ||
                !api.Value.TryGetProperty("versions", out var versions) ||
                !versions.TryGetProperty(preferredVersion, out var version) ||
                !version.TryGetProperty("info", out var info))
            {
                continue;
            }

            var name = GetString(info, "title") ?? api.Name;
            var description = GetString(info, "description");
            var tags = GetStringArray(info, "x-apisguru-categories");
            var searchableText = $"{api.Name} {name} {description} {string.Join(' ', tags)}";

            if (!searchableText.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (query.Category is not null &&
                !tags.Any(tag => tag.Equals(query.Category, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var specificationUrl = GetString(version, "swaggerUrl");
            var documentationUrl = version.TryGetProperty("externalDocs", out var externalDocs)
                ? GetString(externalDocs, "url")
                : null;

            results.Add(new ApiDiscoveryResult(
                name,
                description,
                BaseUrl: null,
                documentationUrl,
                specificationUrl,
                "APIs.guru",
                DirectoryUrl,
                tags));
        }

        return results
            .OrderBy(result => result.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<byte[]> GetCataloguePayloadAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey, out byte[]? cachedPayload) && cachedPayload is not null)
        {
            return cachedPayload;
        }

        await CatalogueLock.WaitAsync(cancellationToken);
        try
        {
            if (cache.TryGetValue(CacheKey, out cachedPayload) && cachedPayload is not null)
            {
                return cachedPayload;
            }

            using var response = await httpClient.GetAsync(
                CatalogueUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength is > MaximumCatalogueBytes)
            {
                throw new HttpRequestException("The APIs.guru catalogue response exceeded the 20 MB limit.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(chunk, cancellationToken)) > 0)
            {
                if (buffer.Length + bytesRead > MaximumCatalogueBytes)
                {
                    throw new HttpRequestException("The APIs.guru catalogue response exceeded the 20 MB limit.");
                }

                await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
            }

            var payload = buffer.ToArray();
            cache.Set(CacheKey, payload, CacheDuration);
            return payload;
        }
        finally
        {
            CatalogueLock.Release();
        }
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static IReadOnlyList<string> GetStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}