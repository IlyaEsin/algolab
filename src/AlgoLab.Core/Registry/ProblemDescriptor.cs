using AlgoLab.Core.Contracts;

namespace AlgoLab.Core.Registry;

public sealed class ProblemDescriptor
{
    internal ProblemDescriptor(
        Type problemType,
        Type inputType,
        Type outputType,
        ProblemAdapter adapter,
        IReadOnlyList<SolutionDescriptor> solutions)
    {
        ProblemType = problemType;
        InputType = inputType;
        OutputType = outputType;
        Adapter = adapter;
        Solutions = solutions;
    }

    public Type ProblemType { get; }

    public Type InputType { get; }

    public Type OutputType { get; }

    public IReadOnlyList<SolutionDescriptor> Solutions { get; }

    internal ProblemAdapter Adapter { get; }

    public ProblemInfo Info => Adapter.Info;

    public bool HasScaler => Adapter.HasScaler;

    /// <summary>Namespace задачи. Совпадает с путём папки и служит префиксом embedded-ресурсов.</summary>
    public string Namespace => ProblemType.Namespace
        ?? throw new InvalidOperationException($"Задача '{ProblemType.Name}' объявлена без namespace.");

    public SolutionDescriptor Solution(string id) =>
        Solutions.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.Ordinal))
        ?? throw new InvalidOperationException(
            $"У задачи '{Info.Slug}' нет решения '{id}'. Доступны: {string.Join(", ", Solutions.Select(s => s.Id))}.");
}
