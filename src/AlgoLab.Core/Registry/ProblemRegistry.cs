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

    public static ProblemRegistry Build(params Assembly[] assemblies) =>
        Build(assemblies.SelectMany(a => a.GetTypes()));

    public static ProblemRegistry Build(IEnumerable<Type> types)
    {
        var candidates = types
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false })
            .ToArray();

        var solutionsByShape = candidates
            .Select(t => new { Type = t, Shape = ClosedInterface(t, typeof(ISolution<,>)) })
            .Where(x => x.Shape is not null)
            .GroupBy(x => x.Shape!, ShapeComparer.Instance)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Type).ToArray(), ShapeComparer.Instance);

        var problems = new List<ProblemDescriptor>();
        var attached = new HashSet<Type>();
        var problemTypeByShape = new Dictionary<Type[], Type>(ShapeComparer.Instance);

        foreach (var problemType in candidates.Where(t => ClosedBase(t, typeof(Problem<,>)) is not null))
        {
            var shape = ClosedBase(problemType, typeof(Problem<,>))!;

            if (problemTypeByShape.TryGetValue(shape, out var existingProblemType))
            {
                throw new InvalidOperationException(
                    $"Задачи '{existingProblemType.FullName}' и '{problemType.FullName}' закрывают одну и ту же пару "
                    + $"(вход, выход) — ({shape[0].FullName}, {shape[1].FullName}). "
                    + "Решения привязываются к задаче по этой паре, поэтому у пары типов может быть только одна задача.");
            }

            problemTypeByShape.Add(shape, problemType);

            var instance = CreateProblemInstance(problemType);

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

    private static object CreateProblemInstance(Type problemType)
    {
        try
        {
            return Activator.CreateInstance(problemType)!;
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException(
                $"Задача '{problemType.FullName}' не создаётся — нужен публичный конструктор без параметров.", ex);
        }
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
