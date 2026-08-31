namespace AlgoLab.Core.Contracts;

public abstract class Problem<TInput, TOutput>
{
    public abstract ProblemInfo Info { get; }

    public abstract IEnumerable<TestCase<TInput, TOutput>> Cases { get; }

    public virtual IOutputComparer<TOutput> Comparer => StructuralComparer<TOutput>.Instance;

    /// <summary>Генератор входа для кривой роста. <c>null</c> — задача не измеряется.</summary>
    public virtual IInputScaler<TInput>? Scaler => null;

    /// <summary>Бюджет на прогон всех тест-кейсов. Поднимается задачами с тяжёлыми кейсами.</summary>
    public virtual TimeSpan CaseTimeout => TimeSpan.FromSeconds(5);
}
