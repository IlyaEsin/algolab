using AlgoLab.Core.Registry;

namespace AlgoLab.Core.Measuring;

public static class GrowthMeasurer
{
    public static GrowthSeries Measure(
        ProblemDescriptor problem,
        SolutionDescriptor solution,
        MeasureOptions? options = null) =>
        problem.Adapter.Measure(solution, options ?? MeasureOptions.Default);
}
