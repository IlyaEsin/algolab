using AlgoLab.Core.Contracts;

namespace AlgoLab.Core.Measuring;

public enum VerdictKind
{
    /// <summary>Измерения не противоречат заявленному классу.</summary>
    Consistent,

    /// <summary>Измерения ложатся на заметно другой класс.</summary>
    Divergent,

    /// <summary>Данных не хватает для суждения.</summary>
    Inconclusive,
}

public readonly record struct FitPoint(double N, double Y);

public sealed record ComplexityVerdict(
    Complexity Declared,
    Complexity BestFit,
    double FitQuality,
    VerdictKind Kind);
