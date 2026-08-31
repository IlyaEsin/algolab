using System.Reflection;
using AlgoLab.Core.Contracts;

namespace AlgoLab.Core.Registry;

public sealed class ProblemRegistry
{
    private readonly Dictionary<string, ProblemDescriptor> _bySlug;

    private ProblemRegistry(IReadOnlyList<ProblemDescriptor> problems)
    {
        Problems = problems;
        _bySlug = problems.ToDictionary(p => p.Info.Slug, StringComparer.Ordinal);
    }

    public IReadOnlyList<ProblemDescriptor> Problems { get; }

    public ProblemDescriptor? Find(string slug) =>
        _bySlug.GetValueOrDefault(slug);

    public static ProblemRegistry Build(params Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false })
            .ToArray();

        var solutionsByShape = types
            .Select(t => new { Type = t, Shape = ClosedInterface(t, typeof(ISolution<,>)) })
            .Where(x => x.Shape is not null)
            .GroupBy(x => x.Shape!, ShapeComparer.Instance)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Type).ToArray(), ShapeComparer.Instance);

        var problems = new List<ProblemDescriptor>();
        var attached = new HashSet<Type>();

        foreach (var problemType in types.Where(t => ClosedBase(t, typeof(Problem<,>)) is not null))
        {
            var shape = ClosedBase(problemType, typeof(Problem<,>))!;
            var instance = Activator.CreateInstance(problemType)
                ?? throw new InvalidOperationException($"Задача '{problemType.Name}' не создаётся — нужен публичный конструктор без параметров.");

            var adapter = ProblemAdapter.Create(instance, shape[0], shape[1]);

            var solutions = solutionsByShape.GetValueOrDefault(shape, [])
                .Select(t => new SolutionDescriptor(t, Describe(t)))
                .OrderBy(s => s.Kind)
                .ThenBy(s => s.Id, StringComparer.Ordinal)
                .ToArray();

            foreach (var solution in solutions)
            {
                attached.Add(solution.SolutionType);
            }

            problems.Add(new ProblemDescriptor(problemType, shape[0], shape[1], adapter, solutions));
        }

        var orphans = solutionsByShape.Values.SelectMany(x => x).Where(t => !attached.Contains(t)).ToArray();
        if (orphans.Length > 0)
        {
            throw new InvalidOperationException(
                $"Решения без задачи: {string.Join(", ", orphans.Select(t => t.FullName))}. "
                + "Тип входа решения должен совпадать с типом входа задачи.");
        }

        var duplicates = problems.GroupBy(p => p.Info.Slug, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();
        if (duplicates.Length > 0)
        {
            throw new InvalidOperationException($"Повторяющиеся slug: {string.Join(", ", duplicates)}.");
        }

        return new ProblemRegistry(problems.OrderBy(p => p.Info.Slug, StringComparer.Ordinal).ToArray());
    }

    private static SolutionAttribute Describe(Type solutionType) =>
        solutionType.GetCustomAttribute<SolutionAttribute>()
        ?? throw new InvalidOperationException($"Решение '{solutionType.FullName}' без атрибута [Solution].");

    private static Type[]? ClosedInterface(Type type, Type openInterface) =>
        type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterface)
            ?.GetGenericArguments();

    private static Type[]? ClosedBase(Type type, Type openBase)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openBase)
            {
                return current.GetGenericArguments();
            }
        }

        return null;
    }

    private sealed class ShapeComparer : IEqualityComparer<Type[]>
    {
        public static ShapeComparer Instance { get; } = new();

        public bool Equals(Type[]? x, Type[]? y) =>
            x is not null && y is not null && x.SequenceEqual(y);

        public int GetHashCode(Type[] obj) =>
            HashCode.Combine(obj[0], obj[1]);
    }
}
