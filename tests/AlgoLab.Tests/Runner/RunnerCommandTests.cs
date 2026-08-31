using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;
using AlgoLab.Problems;
using AlgoLab.Runner;

namespace AlgoLab.Tests.Runner;

public sealed class RunnerCommandTests
{
    private static ProblemRegistry Registry { get; } = ProblemRegistry.Build(ProblemsAssembly.Reference);

    [Fact]
    public void Cases_command_runs_the_named_solution()
    {
        var payload = RunnerCommand.Execute(
            ["cases", "--slug", "two-sum", "--solution", "TwoSumHashMap"],
            Registry);

        Assert.Null(payload.Error);
        Assert.NotNull(payload.Run);
        Assert.Equal(RunStatus.Passed, payload.Run.Status);
    }

    [Fact]
    public void Measure_command_accepts_several_solutions()
    {
        var payload = RunnerCommand.Execute(
            ["measure", "--slug", "two-sum", "--solution", "TwoSumHashMap", "--solution", "TwoSumBruteForce"],
            Registry);

        Assert.Null(payload.Error);
        Assert.NotNull(payload.Series);
        Assert.Equal(2, payload.Series.Count);
    }

    [Fact]
    public void Unknown_verb_is_reported_as_an_error()
    {
        var payload = RunnerCommand.Execute(["dance", "--slug", "two-sum"], Registry);

        Assert.NotNull(payload.Error);
    }

    [Fact]
    public void Unknown_slug_is_reported_as_an_error()
    {
        var payload = RunnerCommand.Execute(["cases", "--slug", "nope", "--solution", "X"], Registry);

        Assert.NotNull(payload.Error);
        Assert.Contains("nope", payload.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_solution_argument_is_reported_as_an_error()
    {
        var payload = RunnerCommand.Execute(["cases", "--slug", "two-sum"], Registry);

        Assert.NotNull(payload.Error);
    }
}
