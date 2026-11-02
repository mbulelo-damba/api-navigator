namespace ApiNavigator.Application.Discovery;

/// <summary>
/// Represents an API found in an external directory.
/// Source attribution is retained so consumers can trace the original listing.
/// </summary>
public sealed record ApiDiscoveryResult(
    string Name,
    string? Description,
    string? BaseUrl,
    string? DocumentationUrl,
    string? OpenApiSpecificationUrl,
    string SourceName,
    string SourceUrl,
    IReadOnlyList<string> Tags);
