namespace ApiNavigator.Application.Discovery;

public sealed record ApiDiscoveryQuery(string SearchTerm, string? Category = null);
