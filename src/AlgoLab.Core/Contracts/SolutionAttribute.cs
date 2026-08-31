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

    public Complexity Space { get; init; } = Complexity.Unknown;

    public SolutionKind Kind { get; init; } = SolutionKind.Reference;

    public string? Note { get; init; }
}
