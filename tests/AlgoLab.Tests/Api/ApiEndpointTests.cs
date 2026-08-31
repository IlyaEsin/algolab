using System.Net;
using System.Net.Http.Json;
using AlgoLab.Api.Contracts;
using AlgoLab.Core.Json;
using AlgoLab.Core.Running;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AlgoLab.Tests.Api;

public sealed class ApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Problem_list_contains_two_sum()
    {
        var problems = await _client.GetFromJsonAsync<ProblemSummary[]>("/api/problems", AlgoLabJson.Options);

        Assert.Contains(problems!, p => p.Slug == "two-sum");
    }

    [Fact]
    public async Task Problem_detail_carries_readme_cases_and_sources()
    {
        var detail = await _client.GetFromJsonAsync<ProblemDetail>("/api/problems/two-sum", AlgoLabJson.Options);

        Assert.Contains("Сумма двух", detail!.Readme, StringComparison.Ordinal);
        Assert.NotEmpty(detail.Cases);
        Assert.Contains(detail.Solutions, s => s.Id == "TwoSumHashMap" && s.Source.Contains("Dictionary", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Unknown_problem_returns_404()
    {
        var response = await _client.GetAsync("/api/problems/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Run_executes_the_solution_in_a_child_process()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/run",
            new RunRequest("two-sum", "TwoSumHashMap"),
            AlgoLabJson.Options);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<RunnerPayload>(AlgoLabJson.Options);

        Assert.Equal(RunStatus.Passed, payload!.Run!.Status);
    }

    [Fact]
    public async Task Measure_returns_a_series_per_requested_solution()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/measure",
            new MeasureRequest("two-sum", ["TwoSumHashMap", "TwoSumBruteForce"]),
            AlgoLabJson.Options);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<RunnerPayload>(AlgoLabJson.Options);

        Assert.Equal(2, payload!.Series!.Count);
    }

    // Carried decision from the runner task: stdout is pinned to BOM-less UTF-8 because every
    // user-facing runner message is Russian. RunnerHost must decode the child process with the
    // matching encoding, or this text arrives mangled instead of intact. A nonexistent slug is
    // the natural way to make the runner emit one of these messages.
    [Fact]
    public async Task Run_with_unknown_slug_returns_the_runner_s_russian_error_intact()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/run",
            new RunRequest("no-such-problem", "TwoSumHashMap"),
            AlgoLabJson.Options);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<RunnerPayload>(AlgoLabJson.Options);

        Assert.Equal("Задача 'no-such-problem' не найдена.", payload!.Error);
    }
}
