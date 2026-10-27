namespace ApiNavigator.Application.Catalogue;

/// <summary>
/// Describes a public API that developers can discover through API Navigator.
/// </summary>
public sealed record ApiCatalogueEntry(
    string Id,
    string Name,
    string Description,
    string Category,
    string BaseUrl,
    string DocumentationUrl,
    string Authentication,
    IReadOnlyList<string> Tags);
