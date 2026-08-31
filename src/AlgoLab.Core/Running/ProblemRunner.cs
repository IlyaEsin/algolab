using AlgoLab.Core.Registry;

namespace AlgoLab.Core.Running;

public static class ProblemRunner
{
    public static RunResult RunCases(ProblemDescriptor problem, SolutionDescriptor solution) =>
        problem.Adapter.RunCases(solution);

    public static IReadOnlyList<CaseDescription> DescribeCases(ProblemDescriptor problem) =>
        problem.Adapter.DescribeCases();

    /// <summary>Прогоняет решение на входе, сгенерированном Scaler'ом задачи — страхует
    /// от генератора, который сам не решается эталонным решением. Бросает, если у задачи
    /// нет Scaler'а; пробрасывает исключение решения без перехвата.</summary>
    public static void RunScaledInput(ProblemDescriptor problem, SolutionDescriptor solution, int n, int seed) =>
        problem.Adapter.RunScaledInput(solution, n, seed);
}
