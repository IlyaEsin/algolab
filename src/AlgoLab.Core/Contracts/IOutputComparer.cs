namespace AlgoLab.Core.Contracts;

/// <summary>Сравнение ожидаемого и полученного результата. Переопределяется задачей,
/// когда верных ответов несколько (перестановки, любая подходящая пара, float).</summary>
public interface IOutputComparer<in T>
{
    public bool Equals(T expected, T actual);
}
