using System.Net.Http.Json;
using System.Text.Json;
using ApiNavigator.Application.Discovery;

namespace ApiNavigator.Infrastructure.Discovery;

/// <summary>
/// Searches the public APIs.guru OpenAPI directory.
/// </summary>
public sealed class ApisGuruDiscoverySource(HttpClient httpClient) : IApiDiscoverySource
{
    private const string CatalogueUrl = "https://api.apis.guru/v2/list.json";
    private const string DirectoryUrl = "https://apis.guru/";

    public async Task<IReadOnlyList<ApiDiscoveryResult>> SearchAsync(
        ApiDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var response = await httpClient.GetAsync(
            CatalogueUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength is > 20_000_000)
        {
            throw new HttpRequestException("The APIs.guru catalogue response exceeded the 20 MB limit.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            stream,
            new JsonDocumentOptions { MaxDepth = 32 },
            cancellationToken);

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