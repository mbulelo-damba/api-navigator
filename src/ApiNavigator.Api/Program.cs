using ApiNavigator.Application.Catalogue;
using ApiNavigator.Application.Discovery;
using ApiNavigator.Infrastructure.Catalogue;
using ApiNavigator.Infrastructure.Discovery;

var builder = WebApplication.CreateBuilder(args);

// Register application use cases and their infrastructure implementation.
builder.Services.AddSingleton<IApiCatalogueRepository, InMemoryApiCatalogueRepository>();
builder.Services.AddSingleton<IApiCatalogueService, ApiCatalogueService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IApiDiscoverySource, ApisGuruDiscoverySource>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// A small health endpoint confirms that the API host is running.
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("GetHealth");

// Search the curated catalogue. Both filters are optional:
// /api/catalogue?search=weather&category=Weather
app.MapGet(
        "/api/catalogue",
        (string? search, string? category, IApiCatalogueService catalogueService) =>
            Results.Ok(catalogueService.Search(search, category)))
    .WithName("SearchApiCatalogue")
    .WithTags("API Catalogue");

// Retrieve one catalogue entry by its stable ID.
app.MapGet(
        "/api/catalogue/{id}",
        (string id, IApiCatalogueService catalogueService) =>
        {
            var entry = catalogueService.GetById(id);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        })
    .WithName("GetApiCatalogueEntry")
    .WithTags("API Catalogue");

// Search the external directory. Validate user input before calling the provider.
app.MapGet(
        "/api/discovery",
        async (string? search, string? category, IApiDiscoverySource discoverySource, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["search"] = ["A non-empty search term is required."]
                });
            }

            if (search.Length > 100)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["search"] = ["The search term cannot exceed 100 characters."]
                });
            }

            try
            {
                var query = new ApiDiscoveryQuery(search, category);
                var results = await discoverySource.SearchAsync(query, cancellationToken);
                return Results.Ok(results);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Results.Problem(
                    title: "External API directory timed out.",
                    statusCode: StatusCodes.Status504GatewayTimeout);
            }
            catch (HttpRequestException)
            {
                return Results.Problem(
                    title: "The external API directory is currently unavailable.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        })
    .WithName("SearchExternalApiDirectory")
    .WithTags("API Discovery");

app.Run();

// Expose Program to integration tests.
public partial class Program
{
}