namespace ApiNavigator.Application.Catalogue;

/// <summary>
/// Applies catalogue search and category filtering rules.
/// </summary>
public sealed class ApiCatalogueService : IApiCatalogueService
{
    private readonly IApiCatalogueRepository _repository;

    public ApiCatalogueService(IApiCatalogueRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<ApiCatalogueEntry> Search(string? searchTerm, string? category)
    {
        var entries = _repository.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();

            entries = entries.Where(entry =>
                entry.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                entry.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                entry.Category.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                entry.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var requestedCategory = category.Trim();

            entries = entries.Where(entry =>
                string.Equals(
                    entry.Category,
                    requestedCategory,
                    StringComparison.OrdinalIgnoreCase));
        }

        return entries
            .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
