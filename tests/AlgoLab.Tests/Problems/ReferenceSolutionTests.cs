using AlgoLab.Core.Contracts;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;
using AlgoLab.Problems;

namespace AlgoLab.Tests.Problems;

public sealed class ReferenceSolutionTests
{
    public static ProblemRegistry Registry { get; } = ProblemRegistry.Build(ProblemsAssembly.Reference);

    public static TheoryData<string, string> ReferenceSolutions()
    {
        var data = new TheoryData<string, string>();
        foreach (var problem in Registry.Problems)
        {
            foreach (var solution in problem.Solutions.Where(s => s.Kind == SolutionKind.Reference))
            {
                data.Add(problem.Info.Slug, solution.Id);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ReferenceSolutions))]
    public void Reference_solution_passes_every_case(string slug, string solutionId)
    {
        var problem = Registry.Find(slug)!;
        var result = ProblemRunner.RunCases(problem, problem.Solution(solutionId));

        Assert.Equal(RunStatus.Passed, result.Status);
    }
}
