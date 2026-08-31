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
    VerdictKind Kind)
{
    /// <summary>Человекочитаемое представление <see cref="Declared"/> для клиента —
    /// та же таблица, что уже используется для <c>SolutionView</c>, чтобы не дублировать
    /// её в TypeScript, где она разъехалась бы с этой. Get-only, поэтому не участвует
    /// в равенстве записи.</summary>
    public string DeclaredText => Declared.Display();

    /// <summary>Человекочитаемое представление <see cref="BestFit"/>, см. <see cref="DeclaredText"/>.</summary>
    public string BestFitText => BestFit.Display();
}
