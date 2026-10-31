namespace ApiNavigator.Application.Catalogue;

/// <summary>
/// Provides the catalogue entries available to the application.
/// Implementations may later read from a database or an external source.
/// </summary>
public interface IApiCatalogueRepository
{
    IReadOnlyList<ApiCatalogueEntry> GetAll();

    ApiCatalogueEntry? GetById(string id);
}
