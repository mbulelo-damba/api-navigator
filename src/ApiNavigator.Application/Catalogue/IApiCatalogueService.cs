namespace ApiNavigator.Application.Catalogue;

/// <summary>
/// Searches the available API catalogue and retrieves individual entries.
/// </summary>
public interface IApiCatalogueService
{
    IReadOnlyList<ApiCatalogueEntry> Search(string? searchTerm, string? category);

    ApiCatalogueEntry? GetById(string id);
}
