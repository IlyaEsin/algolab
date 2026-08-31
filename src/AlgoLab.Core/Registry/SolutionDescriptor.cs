using AlgoLab.Core.Contracts;

namespace AlgoLab.Core.Registry;

public sealed class SolutionDescriptor
{
    internal SolutionDescriptor(Type solutionType, SolutionAttribute attribute)
    {
        SolutionType = solutionType;
        Id = solutionType.Name;
        Name = attribute.Name;
        Kind = attribute.Kind;
        Time = attribute.Time;
        Space = attribute.Space;
        Note = attribute.Note;
    }

    public Type SolutionType { get; }

    /// <summary>Стабильный идентификатор для API и UI — имя типа решения.</summary>
    public string Id { get; }

    public string Name { get; }

    public SolutionKind Kind { get; }

    public Complexity Time { get; }

    public Complexity Space { get; }

    public string? Note { get; }
}
