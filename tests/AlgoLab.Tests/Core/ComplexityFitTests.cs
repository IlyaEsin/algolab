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

    [Fact]
    public void Empty_points_is_inconclusive_and_does_not_throw()
    {
        var verdict = ComplexityFit.Fit(Array.Empty<FitPoint>(), Complexity.ON);

        Assert.Equal(VerdictKind.Inconclusive, verdict.Kind);
    }

    [Fact]
    public void Declared_complexity_overflowing_at_measured_sizes_is_inconclusive_not_divergent()
    {
        // O(2^n) переполняется в double уже на n = 1024, так что заявленный класс
        // сам не подгоняется. Сравнение с ним неопределено — вердикт обязан уйти
        // в Inconclusive, а не упасть в Divergent из-за NaN >= x == false.
        var verdict = ComplexityFit.Fit(Series(n => n, 64, 128, 256, 512, 1024), Complexity.O2N);

        Assert.Equal(VerdictKind.Inconclusive, verdict.Kind);
        Assert.Equal(Complexity.ON, verdict.BestFit);
    }

    [Fact]
    public void Noisy_data_that_fits_no_class_well_is_inconclusive()
    {
        var points = new[]
        {
            new FitPoint(64, 1000d),
            new FitPoint(128, 1d),
            new FitPoint(256, 1000d),
            new FitPoint(512, 1d),
            new FitPoint(1024, 1000d),
        };

        var verdict = ComplexityFit.Fit(points, Complexity.ON);

        Assert.Equal(VerdictKind.Inconclusive, verdict.Kind);
    }
}
