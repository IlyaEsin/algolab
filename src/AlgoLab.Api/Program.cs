using AlgoLab.Api.Catalog;
using AlgoLab.Api.Contracts;
using AlgoLab.Api.Execution;
using AlgoLab.Core.Json;
using AlgoLab.Core.Registry;
using AlgoLab.Problems;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = AlgoLabJson.Options.PropertyNamingPolicy;
    options.SerializerOptions.Encoder = AlgoLabJson.Options.Encoder;
    foreach (var converter in AlgoLabJson.Options.Converters)
    {
        options.SerializerOptions.Converters.Add(converter);
    }
});

builder.Services.AddSingleton(ProblemRegistry.Build(ProblemsAssembly.Reference));
builder.Services.AddSingleton<ProblemCatalog>();
builder.Services.AddSingleton<RunnerHost>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/problems", (ProblemCatalog catalog) => catalog.List());

app.MapGet("/api/problems/{slug}", (string slug, ProblemCatalog catalog) =>
    catalog.Detail(slug) is { } detail ? Results.Ok(detail) : Results.NotFound());

app.MapPost("/api/run", async (RunRequest request, RunnerHost host, CancellationToken token) =>
    Results.Ok(await host.RunAsync(request.Slug, request.SolutionId, token)));

app.MapPost("/api/measure", async (MeasureRequest request, RunnerHost host, CancellationToken token) =>
    Results.Ok(await host.MeasureAsync(request.Slug, request.SolutionIds, token)));

app.MapFallbackToFile("index.html");

app.Run();

/// <summary>Открыт для WebApplicationFactory в тестах.</summary>
public partial class Program;
