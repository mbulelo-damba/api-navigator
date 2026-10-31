namespace ApiNavigator.Application.Discovery;

/// <summary>
/// Describes a keyword-based search against an external API directory.
/// </summary>
public sealed record ApiDiscoveryQuery
{
    public ApiDiscoveryQuery(string searchTerm, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ArgumentException("A search term is required.", nameof(searchTerm));
        }

        SearchTerm = searchTerm.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
    }

    public string SearchTerm { get; }

    public string? Category { get; }
}
