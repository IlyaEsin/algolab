using AlgoLab.Core.Contracts;

namespace AlgoLab.Core.Measuring;

/// <summary>Подгоняет измеренные точки под каждый класс сложности как y = a * f(n)
/// и выбирает лучший по R^2.
///
/// Ограничение, которое обязано доезжать до UI: соседние классы (n и n log n)
/// на достижимых n почти неразличимы, поэтому вердикт формулируется как
/// согласие/расхождение, а не как доказательство.</summary>
public static class ComplexityFit
{
    private const int MinimumPoints = 4;
    private const double MinimumQuality = 0.90;

    /// <summary>Насколько заявленный класс может уступать лучшему, оставаясь «согласующимся».</summary>
    private const double Tolerance = 0.02;

    private static readonly Complexity[] Candidates =
    [
        Complexity.O1,
        Complexity.OLogN,
        Complexity.ON,
        Complexity.ONLogN,
        Complexity.ON2,
        Complexity.ON3,
        Complexity.O2N,
        Complexity.ONFactorial,
    ];

    public static ComplexityVerdict Fit(IReadOnlyList<FitPoint> points, Complexity declared)
    {
        var scores = Candidates
            .Select(c => new { Complexity = c, Quality = Quality(points, c) })
            .Where(x => double.IsFinite(x.Quality))
            .ToArray();

        if (scores.Length == 0)
        {
            return new ComplexityVerdict(declared, Complexity.Unknown, 0d, VerdictKind.Inconclusive);
        }

        var best = scores.MaxBy(x => x.Quality)!;

        if (points.Count < MinimumPoints || best.Quality < MinimumQuality || declared == Complexity.Unknown)
        {
            return new ComplexityVerdict(declared, best.Complexity, best.Quality, VerdictKind.Inconclusive);
        }

        var declaredQuality = Quality(points, declared);
        var kind = declared == best.Complexity || declaredQuality >= best.Quality - Tolerance
            ? VerdictKind.Consistent
            : VerdictKind.Divergent;

        return new ComplexityVerdict(declared, best.Complexity, best.Quality, kind);
    }

    /// <summary>R^2 подгонки y = a * f(n) методом наименьших квадратов.
    /// Ряд из одинаковых значений (нулевая дисперсия) считается идеально константным.</summary>
    private static double Quality(IReadOnlyList<FitPoint> points, Complexity complexity)
    {
        if (complexity == Complexity.Unknown)
        {
            return double.NaN;
        }

        var mean = points.Average(p => p.Y);
        var totalVariance = points.Sum(p => (p.Y - mean) * (p.Y - mean));

        if (totalVariance == 0d)
        {
            return complexity == Complexity.O1 ? 1d : 0d;
        }

        double numerator = 0d, denominator = 0d;
        foreach (var point in points)
        {
            var f = complexity.Grow(point.N);
            if (!double.IsFinite(f))
            {
                return double.NaN;
            }

            numerator += point.Y * f;
            denominator += f * f;
        }

        if (denominator == 0d)
        {
            return double.NaN;
        }

        var scale = numerator / denominator;
        var residual = points.Sum(p =>
        {
            var error = p.Y - scale * complexity.Grow(p.N);
            return error * error;
        });

        return 1d - residual / totalVariance;
    }
}
