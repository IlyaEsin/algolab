namespace AlgoLab.Core.Measuring;

public sealed record MeasureOptions(
    int StartN,
    int MaxN,
    int Repeats,
    int BudgetMs,
    int WarmupMs)
{
    /// <summary>Фиксированный seed: один и тот же n всегда даёт один и тот же вход,
    /// иначе кривая плясала бы от запуска к запуску.</summary>
    public const int Seed = 20260831;

    public static MeasureOptions Default { get; } = new(
        StartN: 64,
        MaxN: 1 << 16,
        Repeats: 5,
        BudgetMs: 2000,
        WarmupMs: 50);
}
