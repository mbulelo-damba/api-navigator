var builder = WebApplication.CreateBuilder(args);

// Register application and infrastructure services here as the implementation grows.
var app = builder.Build();

// A small health endpoint confirms that the API host is running.
// This is infrastructure plumbing, not an API discovery feature.
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("GetHealth");

app.Run();

// Expose Program to integration tests when a test host is added.
public partial class Program;