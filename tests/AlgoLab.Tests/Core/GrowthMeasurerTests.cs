using AlgoLab.Core.Measuring;
using AlgoLab.Core.Registry;
using AlgoLab.Tests.Fakes;

namespace AlgoLab.Tests.Core;

public sealed class GrowthMeasurerTests
{
    private static ProblemRegistry Registry => FakeRegistry.Instance;

    private static readonly MeasureOptions Fast = MeasureOptions.Default with
    {
        StartN = 256,
        MaxN = 4096,
        Repeats = 3,
        BudgetMs = 4000,
    };

    [Fact]
    public void Produces_a_point_per_ladder_step()
    {
        var problem = Registry.Find("fake-sum")!;

        var series = GrowthMeasurer.Measure(problem, problem.Solution(nameof(SumLoop)), Fast);

        Assert.Equal([256, 512, 1024, 2048, 4096], series.Points.Select(p => p.N));
        Assert.All(series.Points, p => Assert.True(p.MedianMs >= 0));
    }

    [Fact]
    public void Same_seed_gives_the_same_input_every_run()
    {
        var scaler = new SumScaler();

        Assert.Equal(
            scaler.Create(64, MeasureOptions.Seed).Values,
            scaler.Create(64, MeasureOptions.Seed).Values);
    }

    [Fact]
    public void Problem_without_a_scaler_reports_an_error_instead_of_throwing()
    {
        var problem = Registry.Find("fake-noise")!;

        var series = GrowthMeasurer.Measure(problem, problem.Solution(nameof(NoiseReverse)), Fast);

        Assert.Empty(series.Points);
        Assert.NotNull(series.Error);
    }

    [Fact]
    public void Unimplemented_attempt_reports_an_error_instead_of_throwing()
    {
        var problem = Registry.Find("fake-sum")!;

        var series = GrowthMeasurer.Measure(problem, problem.Solution(nameof(SumAttempt)), Fast);

        Assert.NotNull(series.Error);
        Assert.Empty(series.Points);
    }

    [Fact]
    public void Ladder_stops_on_the_time_budget()
    {
        var problem = Registry.Find("fake-slow")!;
        var options = Fast with { StartN = 512, MaxN = 1 << 20, BudgetMs = 300 };

        var series = GrowthMeasurer.Measure(problem, problem.Solution(nameof(SlowQuadratic)), options);

        Assert.True(series.Points.Count >= 1);
        Assert.True(series.Points[^1].N < 1 << 20);
    }

    [Fact]
    public void Allocating_solution_reports_nonzero_allocations()
    {
        var problem = Registry.Find("fake-slow")!;

        var series = GrowthMeasurer.Measure(problem, problem.Solution(nameof(SlowQuadratic)), Fast with { MaxN = 1024 });

        Assert.Contains(series.Points, p => p.AllocatedBytes > 0);
    }
}
