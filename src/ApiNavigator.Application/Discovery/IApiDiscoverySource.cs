namespace ApiNavigator.Application.Discovery;

/// <summary>
/// Defines an adapter for searching one external API directory.
/// Implementations belong in Infrastructure and must honor cancellation.
/// </summary>
public interface IApiDiscoverySource
{
    Task<IReadOnlyList<ApiDiscoveryResult>> SearchAsync(
        ApiDiscoveryQuery query,
        CancellationToken cancellationToken = default);
}
