namespace AlgoLab.Core.Contracts;

/// <summary>Класс сложности. <see cref="Unknown"/> — значение по умолчанию для попыток,
/// у которых сложность не заявлена.</summary>
public enum Complexity
{
    Unknown,
    O1,
    OLogN,
    ON,
    ONLogN,
    ON2,
    ON3,
    O2N,
    ONFactorial,
}

public static class ComplexityExtensions
{
    public static string Display(this Complexity complexity) => complexity switch
    {
        Complexity.Unknown => "не заявлена",
        Complexity.O1 => "O(1)",
        Complexity.OLogN => "O(log n)",
        Complexity.ON => "O(n)",
        Complexity.ONLogN => "O(n log n)",
        Complexity.ON2 => "O(n^2)",
        Complexity.ON3 => "O(n^3)",
        Complexity.O2N => "O(2^n)",
        Complexity.ONFactorial => "O(n!)",
        _ => throw new ArgumentOutOfRangeException(nameof(complexity)),
    };

    /// <summary>Функция роста, по которой измеренные точки подгоняются под класс сложности.
    /// Может вернуть бесконечность для экспоненциальных классов на больших n —
    /// вызывающая сторона обязана это проверять.</summary>
    public static double Grow(this Complexity complexity, double n) => complexity switch
    {
        Complexity.O1 => 1d,
        Complexity.OLogN => Math.Log2(n + 1d),
        Complexity.ON => n,
        Complexity.ONLogN => n * Math.Log2(n + 1d),
        Complexity.ON2 => n * n,
        Complexity.ON3 => n * n * n,
        Complexity.O2N => Math.Pow(2d, n),
        Complexity.ONFactorial => Math.Exp(LogFactorial(n)),
        Complexity.Unknown => double.NaN,
        _ => throw new ArgumentOutOfRangeException(nameof(complexity)),
    };

    private static double LogFactorial(double n)
    {
        // Приближение Стирлинга: точного факториала хватает только до n = 170,
        // а подгонка должна работать и там, где значения нужны лишь в сравнении.
        var x = n + 1d;
        return (x - 0.5) * Math.Log(x) - x + 0.5 * Math.Log(2 * Math.PI) + 1d / (12 * x);
    }
}
