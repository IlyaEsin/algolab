using AlgoLab.Core.Contracts;
using AlgoLab.Core.Measuring;

namespace AlgoLab.Tests.Core;

public sealed class ComplexityFitTests
{
    private static FitPoint[] Series(Func<double, double> shape, params int[] sizes) =>
        sizes.Select(n => new FitPoint(n, shape(n))).ToArray();

    [Fact]
    public void Linear_data_fits_linear_class()
    {
        var verdict = ComplexityFit.Fit(Series(n => 0.5 * n, 64, 128, 256, 512, 1024), Complexity.ON);

        Assert.Equal(Complexity.ON, verdict.BestFit);
        Assert.Equal(VerdictKind.Consistent, verdict.Kind);
    }

    [Fact]
    public void Quadratic_data_declared_linear_diverges()
    {
        var verdict = ComplexityFit.Fit(Series(n => 0.001 * n * n, 64, 128, 256, 512, 1024), Complexity.ON);

        Assert.Equal(Complexity.ON2, verdict.BestFit);
        Assert.Equal(VerdictKind.Divergent, verdict.Kind);
    }

    [Fact]
    public void Constant_data_fits_constant_class()
    {
        var verdict = ComplexityFit.Fit(Series(_ => 3d, 64, 128, 256, 512, 1024), Complexity.O1);

        Assert.Equal(Complexity.O1, verdict.BestFit);
        Assert.Equal(VerdictKind.Consistent, verdict.Kind);
    }

    [Fact]
    public void All_zero_data_is_constant_not_a_division_by_zero()
    {
        var verdict = ComplexityFit.Fit(Series(_ => 0d, 64, 128, 256, 512), Complexity.O1);

        Assert.Equal(Complexity.O1, verdict.BestFit);
        Assert.Equal(VerdictKind.Consistent, verdict.Kind);
    }

    [Fact]
    public void Too_few_points_is_inconclusive()
    {
        var verdict = ComplexityFit.Fit(Series(n => n, 64, 128), Complexity.ON);

        Assert.Equal(VerdictKind.Inconclusive, verdict.Kind);
    }

    [Fact]
    public void Nearby_classes_do_not_produce_a_false_divergence()
    {
        // n log n объявлено, данные линейные: на достижимых n эти классы неразличимы,
        // и вердикт обязан остаться "согласуется", а не "расходится".
        var verdict = ComplexityFit.Fit(Series(n => n, 64, 128, 256, 512, 1024), Complexity.ONLogN);

        Assert.Equal(VerdictKind.Consistent, verdict.Kind);
    }

    [Fact]
    public void Unknown_declared_complexity_is_inconclusive_but_still_reports_best_fit()
    {
        var verdict = ComplexityFit.Fit(Series(n => n, 64, 128, 256, 512, 1024), Complexity.Unknown);

        Assert.Equal(VerdictKind.Inconclusive, verdict.Kind);
        Assert.Equal(Complexity.ON, verdict.BestFit);
    }
}
