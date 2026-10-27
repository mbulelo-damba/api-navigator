namespace ApiNavigator.Application.Catalogue;

/// <summary>
/// Searches the available API catalogue without depending on a storage technology.
/// </summary>
public interface IApiCatalogueService
{
    IReadOnlyList<ApiCatalogueEntry> Search(string? searchTerm, string? category);
}
