namespace AlgoLab.Core.Contracts;

/// <summary>Строит вход заданного размера. Seed фиксирован измерителем,
/// поэтому один и тот же n всегда даёт один и тот же вход.</summary>
public interface IInputScaler<out TInput>
{
    public TInput Create(int n, int seed);
}
