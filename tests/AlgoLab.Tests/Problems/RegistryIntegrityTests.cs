using AlgoLab.Core.Contracts;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Resources;
using AlgoLab.Core.Running;

namespace AlgoLab.Tests.Problems;

public sealed class RegistryIntegrityTests
{
    private static ProblemRegistry Registry => ReferenceSolutionTests.Registry;

    public static TheoryData<string> Slugs()
    {
        var data = new TheoryData<string>();
        foreach (var problem in Registry.Problems)
        {
            data.Add(problem.Info.Slug);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Problem_has_a_readme(string slug)
    {
        var readme = SourceStore.ReadReadme(Registry.Find(slug)!);

        Assert.False(string.IsNullOrWhiteSpace(readme));
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Problem_has_at_least_one_reference_solution(string slug)
    {
        var problem = Registry.Find(slug)!;

        Assert.Contains(problem.Solutions, s => s.Kind == SolutionKind.Reference);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Reference_solutions_declare_their_complexity(string slug)
    {
        var problem = Registry.Find(slug)!;

        foreach (var solution in problem.Solutions.Where(s => s.Kind == SolutionKind.Reference))
        {
            Assert.NotEqual(Complexity.Unknown, solution.Time);
            Assert.NotEqual(Complexity.Unknown, solution.Space);
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Namespace_matches_the_slug_folder(string slug)
    {
        var problem = Registry.Find(slug)!;
        var folder = problem.Namespace.Split('.').Last();
        var expected = slug.Replace("-", string.Empty, StringComparison.Ordinal);

        Assert.Equal(expected, folder, ignoreCase: true);
    }

    [Fact]
    public void Every_problem_lives_in_its_own_namespace()
    {
        var duplicates = Registry.Problems
            .GroupBy(p => p.Namespace, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        Assert.Empty(duplicates);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Solution_sources_are_embedded(string slug)
    {
        var problem = Registry.Find(slug)!;

        foreach (var solution in problem.Solutions)
        {
            Assert.Contains("class", SourceStore.ReadSource(solution.SolutionType), StringComparison.Ordinal);
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Scaled_input_is_handled_by_every_reference_solution(string slug)
    {
        var problem = Registry.Find(slug)!;
        if (!problem.HasScaler)
        {
            return;
        }

        foreach (var solution in problem.Solutions.Where(s => s.Kind == SolutionKind.Reference))
        {
            ProblemRunner.RunScaledInput(problem, solution, n: 256, seed: 0);
        }
    }
}
