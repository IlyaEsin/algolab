namespace AlgoLab.Core.Contracts;

/// <summary>Одно решение одной задачи. Тип входа задаёт привязку к задаче.</summary>
public interface ISolution<in TInput, out TOutput>
{
    public TOutput Solve(TInput input);
}
