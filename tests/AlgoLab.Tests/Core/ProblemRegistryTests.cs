using System.Reflection;
using AlgoLab.Core.Contracts;
using AlgoLab.Core.Registry;
using AlgoLab.Tests.Fakes;

namespace AlgoLab.Tests.Core;

public sealed class ProblemRegistryTests
{
    private static ProblemRegistry Registry => ProblemRegistry.Build(Assembly.GetExecutingAssembly());

    [Fact]
    public void Finds_every_problem_in_the_assembly()
    {
        var slugs = Registry.Problems.Select(p => p.Info.Slug).ToArray();

        Assert.Contains("fake-sum", slugs);
        Assert.Contains("fake-noise", slugs);
    }

    [Fact]
    public void Attaches_solutions_by_generic_arguments()
    {
        var problem = Registry.Find("fake-sum");

        Assert.NotNull(problem);
        Assert.Equal(
            [nameof(SumAttempt), nameof(SumLoop)],
            problem.Solutions.Select(s => s.Id).OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public void Reads_declared_complexity_from_the_attribute()
    {
        var solution = Registry.Find("fake-sum")!.Solution(nameof(SumLoop));

        Assert.Equal("Цикл", solution.Name);
        Assert.Equal(Complexity.ON, solution.Time);
        Assert.Equal(Complexity.O1, solution.Space);
        Assert.Equal(SolutionKind.Reference, solution.Kind);
    }

    [Fact]
    public void Attempt_without_declared_complexity_is_unknown()
    {
        var attempt = Registry.Find("fake-sum")!.Solution(nameof(SumAttempt));

        Assert.Equal(SolutionKind.Attempt, attempt.Kind);
        Assert.Equal(Complexity.Unknown, attempt.Time);
    }

    [Fact]
    public void Reports_whether_the_problem_can_be_measured()
    {
        Assert.True(Registry.Find("fake-sum")!.HasScaler);
        Assert.False(Registry.Find("fake-noise")!.HasScaler);
    }

    [Fact]
    public void Unknown_slug_is_not_found()
    {
        Assert.Null(Registry.Find("no-such-problem"));
    }

    [Fact]
    public void Unknown_solution_id_throws_with_a_useful_message()
    {
        var problem = Registry.Find("fake-sum")!;

        var error = Assert.Throws<InvalidOperationException>(() => problem.Solution("Nope"));

        Assert.Contains("fake-sum", error.Message, StringComparison.Ordinal);
    }
}
