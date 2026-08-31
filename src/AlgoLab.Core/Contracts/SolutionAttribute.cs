namespace AlgoLab.Core.Contracts;

public enum SolutionKind
{
    Reference,
    Attempt,
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class SolutionAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public Complexity Time { get; init; } = Complexity.Unknown;

    /// <summary>Заявленная сложность по памяти. Сверяется с фактическими аллокациями кучи,
    /// снятыми вокруг одного вызова <c>Solve</c> — память стека (например, глубина рекурсии)
    /// этому измерению не видна вообще, поэтому декларация должна отвечать на вопрос
    /// «сколько кучи выделит Solve», а не на текстбучный вопрос о дополнительной памяти
    /// алгоритма.</summary>
    public Complexity Space { get; init; } = Complexity.Unknown;

    public SolutionKind Kind { get; init; } = SolutionKind.Reference;

    public string? Note { get; init; }
}
