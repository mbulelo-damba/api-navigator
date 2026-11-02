namespace ApiNavigator.Application.AI;

/// <summary>
/// Finds API directory candidates and asks an AI model to assess their suitability.
/// </summary>
public interface IApiRecommendationService
{
    Task<ApiRecommendationResponse> RecommendAsync(
        ApiRecommendationRequest request,
        CancellationToken cancellationToken = default);
}
