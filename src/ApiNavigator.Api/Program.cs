using ApiNavigator.Application.Catalogue;
using ApiNavigator.Infrastructure.Catalogue;

var builder = WebApplication.CreateBuilder(args);

// Register application use cases and their infrastructure implementation.
builder.Services.AddSingleton<IApiCatalogueRepository, InMemoryApiCatalogueRepository>();
builder.Services.AddSingleton<IApiCatalogueService, ApiCatalogueService>();

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

app.Run();

// Expose Program to integration tests.
public partial class Program
{
}
