using AlgoLab.Core.Registry;

namespace AlgoLab.Core.Running;

public static class ProblemRunner
{
    public static RunResult RunCases(ProblemDescriptor problem, SolutionDescriptor solution) =>
        problem.Adapter.RunCases(solution);

    public static IReadOnlyList<CaseDescription> DescribeCases(ProblemDescriptor problem) =>
        problem.Adapter.DescribeCases();
}
