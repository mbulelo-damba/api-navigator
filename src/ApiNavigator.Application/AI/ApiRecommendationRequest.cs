namespace ApiNavigator.Application.AI;

/// <summary>
/// Describes the developer's project and the API directory search to perform.
/// </summary>
public sealed record ApiRecommendationRequest(
    string ProjectDescription,
    string SearchTerm,
    string? Category = null);
