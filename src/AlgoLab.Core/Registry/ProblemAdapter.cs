using AlgoLab.Core.Contracts;

namespace AlgoLab.Core.Registry;

/// <summary>Нетипизированный фасад над <see cref="Problem{TInput,TOutput}"/>: позволяет
/// реестру и API работать с задачами, не зная их generic-аргументов.</summary>
internal abstract class ProblemAdapter
{
    public abstract ProblemInfo Info { get; }

    public abstract bool HasScaler { get; }

    public static ProblemAdapter Create(object problemInstance, Type inputType, Type outputType)
    {
        var adapterType = typeof(ProblemAdapter<,>).MakeGenericType(inputType, outputType);
        return (ProblemAdapter)Activator.CreateInstance(adapterType, problemInstance)!;
    }
}

internal sealed class ProblemAdapter<TInput, TOutput>(Problem<TInput, TOutput> problem) : ProblemAdapter
{
    internal Problem<TInput, TOutput> Problem { get; } = problem;

    public override ProblemInfo Info => Problem.Info;

    public override bool HasScaler => Problem.Scaler is not null;
}
