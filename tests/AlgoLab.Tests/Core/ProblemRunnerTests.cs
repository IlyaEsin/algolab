using System.Reflection;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;
using AlgoLab.Tests.Fakes;

namespace AlgoLab.Tests.Core;

public sealed class ProblemRunnerTests
{
    // Filtered to the exact "AlgoLab.Tests.Fakes" namespace like ProblemRegistryTests.Registry:
    // an unfiltered Assembly.GetExecutingAssembly() scan also picks up the deliberately
    // colliding guard fixtures in "AlgoLab.Tests.Fakes.Isolated", which makes Build() throw.
    private static ProblemRegistry Registry => ProblemRegistry.Build(
        Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "AlgoLab.Tests.Fakes"));

    [Fact]
    public void Correct_solution_passes_all_cases()
    {
        var problem = Registry.Find("fake-sum")!;

        var result = ProblemRunner.RunCases(problem, problem.Solution(nameof(SumLoop)));

        Assert.Equal(RunStatus.Passed, result.Status);
        Assert.Equal(2, result.Cases.Count);
        Assert.All(result.Cases, c => Assert.True(c.Passed));
    }

    [Fact]
    public void Case_result_carries_readable_input_and_values()
    {
        var problem = Registry.Find("fake-sum")!;

        var first = ProblemRunner.RunCases(problem, problem.Solution(nameof(SumLoop))).Cases[0];

        Assert.Equal("базовый", first.Name);
        Assert.Contains("1", first.Input, StringComparison.Ordinal);
        Assert.Equal("6", first.Expected);
        Assert.Equal("6", first.Actual);
        Assert.Null(first.Error);
    }

    [Fact]
    public void Unimplemented_attempt_reports_not_implemented_rather_than_failure()
    {
        var problem = Registry.Find("fake-sum")!;

        var result = ProblemRunner.RunCases(problem, problem.Solution(nameof(SumAttempt)));

        Assert.Equal(RunStatus.NotImplemented, result.Status);
        Assert.All(result.Cases, c => Assert.False(c.Passed));
    }

    [Fact]
    public void Wrong_answer_is_reported_per_case()
    {
        var problem = Registry.Find("fake-wrong")!;

        var result = ProblemRunner.RunCases(problem, problem.Solution(nameof(WrongAlwaysZero)));

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.False(result.Cases[0].Passed);
        Assert.Equal("0", result.Cases[0].Actual);
    }

    [Fact]
    public void Thrown_exception_is_reported_as_error()
    {
        var problem = Registry.Find("fake-wrong")!;

        var result = ProblemRunner.RunCases(problem, problem.Solution(nameof(WrongThrows)));

        Assert.Equal(RunStatus.Error, result.Status);
        Assert.Contains("InvalidOperationException", result.Cases[0].Error!, StringComparison.Ordinal);
    }
}
