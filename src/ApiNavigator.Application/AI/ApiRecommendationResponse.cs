using ApiNavigator.Application.Discovery;

namespace ApiNavigator.Application.AI;

/// <summary>
/// Combines real directory results with Meta AI's analysis of their suitability.
/// </summary>
public sealed record ApiRecommendationResponse(
    string Provider,
    string Model,
    string Analysis,
    IReadOnlyList<ApiDiscoveryResult> Candidates);
