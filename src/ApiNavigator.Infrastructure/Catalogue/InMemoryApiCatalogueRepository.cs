using ApiNavigator.Application.Catalogue;

namespace ApiNavigator.Infrastructure.Catalogue;

/// <summary>
/// Initial in-memory catalogue. Keeping this behind the repository interface
/// allows the storage implementation to change without changing application rules.
/// </summary>
public sealed class InMemoryApiCatalogueRepository : IApiCatalogueRepository
{
    private static readonly IReadOnlyList<ApiCatalogueEntry> Entries =
    [
        new(
            Id: "jsonplaceholder",
            Name: "JSONPlaceholder",
            Description: "A fake REST API for trying out requests and prototyping applications.",
            Category: "Development",
            BaseUrl: "https://jsonplaceholder.typicode.com",
            DocumentationUrl: "https://jsonplaceholder.typicode.com/guide/",
            Authentication: "None",
            Tags: ["testing", "mock data", "REST", "posts", "users"]),

        new(
            Id: "open-meteo",
            Name: "Open-Meteo",
            Description: "Weather forecasts and historical weather data for locations around the world.",
            Category: "Weather",
            BaseUrl: "https://api.open-meteo.com",
            DocumentationUrl: "https://open-meteo.com/en/docs",
            Authentication: "None for the free non-commercial API",
            Tags: ["weather", "forecast", "climate", "temperature"]),

        new(
            Id: "rest-countries",
            Name: "REST Countries",
            Description: "Country information including names, regions, populations, and flags.",
            Category: "Geography",
            BaseUrl: "https://restcountries.com",
            DocumentationUrl: "https://restcountries.com/",
            Authentication: "None",
            Tags: ["countries", "geography", "population", "flags"]),

        new(
            Id: "dog-ceo",
            Name: "Dog CEO",
            Description: "A public API for retrieving dog images and breed information.",
            Category: "Animals",
            BaseUrl: "https://dog.ceo/api",
            DocumentationUrl: "https://dog.ceo/dog-api/documentation/",
            Authentication: "None",
            Tags: ["dogs", "animals", "images", "breeds"]),

        new(
            Id: "open-library",
            Name: "Open Library",
            Description: "Book and author search data from the Open Library project.",
            Category: "Books",
            BaseUrl: "https://openlibrary.org",
            DocumentationUrl: "https://openlibrary.org/developers/api",
            Authentication: "None for public read endpoints",
            Tags: ["books", "authors", "reading", "search"])
    ];

    public IReadOnlyList<ApiCatalogueEntry> GetAll() => Entries;
}
