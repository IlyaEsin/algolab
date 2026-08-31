using AlgoLab.Core.Contracts;
using AlgoLab.Core.Json;
using AlgoLab.Core.Measuring;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Resources;
using AlgoLab.Core.Running;

namespace AlgoLab.Tests.Problems;

public sealed class RegistryIntegrityTests
{
    private static ProblemRegistry Registry => ReferenceSolutionTests.Registry;

    public static TheoryData<string> Slugs()
    {
        var data = new TheoryData<string>();
        foreach (var problem in Registry.Problems)
        {
            data.Add(problem.Info.Slug);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Problem_has_a_readme(string slug)
    {
        var readme = SourceStore.ReadReadme(Registry.Find(slug)!);

        Assert.False(string.IsNullOrWhiteSpace(readme));
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Problem_has_at_least_one_reference_solution(string slug)
    {
        var problem = Registry.Find(slug)!;

        Assert.Contains(problem.Solutions, s => s.Kind == SolutionKind.Reference);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Reference_solutions_declare_their_complexity(string slug)
    {
        var problem = Registry.Find(slug)!;

        foreach (var solution in problem.Solutions.Where(s => s.Kind == SolutionKind.Reference))
        {
            Assert.NotEqual(Complexity.Unknown, solution.Time);
            Assert.NotEqual(Complexity.Unknown, solution.Space);
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Namespace_matches_the_slug_folder(string slug)
    {
        var problem = Registry.Find(slug)!;
        var folder = problem.Namespace.Split('.').Last();
        var expected = slug.Replace("-", string.Empty, StringComparison.Ordinal);

        Assert.Equal(expected, folder, ignoreCase: true);
    }

    [Fact]
    public void Every_problem_lives_in_its_own_namespace()
    {
        var duplicates = Registry.Problems
            .GroupBy(p => p.Namespace, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        Assert.Empty(duplicates);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Solution_sources_are_embedded(string slug)
    {
        var problem = Registry.Find(slug)!;

        foreach (var solution in problem.Solutions)
        {
            Assert.Contains("class", SourceStore.ReadSource(solution.SolutionType), StringComparison.Ordinal);
        }
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Scaled_input_is_handled_by_every_reference_solution(string slug)
    {
        var problem = Registry.Find(slug)!;
        if (!problem.HasScaler)
        {
            return;
        }

        foreach (var solution in problem.Solutions.Where(s => s.Kind == SolutionKind.Reference))
        {
            ProblemRunner.RunScaledInput(problem, solution, n: 256, seed: 0);
        }
    }

    /// <summary>Ловит скейлер, который где-то по дороге насыщается (типично — зажимает
    /// значение в допустимый диапазон): если вход на двух верхних ступенях лестницы
    /// по умолчанию совпал, дальше он совпадает и подавно, кривая роста не может
    /// получиться, и любая заявленная сложность на этих данных обречена читаться как
    /// Divergent — не потому что решение неверно, а потому что измерять уже нечего.
    /// Проверяются именно верхние две ступени: насыщение — если оно есть — тем заметнее,
    /// чем крупнее n, а на маленьких n тот же скейлер вполне может честно отличаться.</summary>
    [Theory]
    [MemberData(nameof(Slugs))]
    public void Scaler_input_still_grows_at_the_top_of_the_default_ladder(string slug)
    {
        var problem = Registry.Find(slug)!;
        if (!problem.HasScaler)
        {
            return;
        }

        var high = MeasureOptions.Default.MaxN;
        var low = high / 2;

        var inputAtLow = DescribeScaledInput(problem, low, MeasureOptions.Seed);
        var inputAtHigh = DescribeScaledInput(problem, high, MeasureOptions.Seed);

        Assert.NotEqual(inputAtLow, inputAtHigh);
    }

    /// <summary>Строит вход скейлера напрямую, в обход ProblemRunner.RunScaledInput — тот
    /// прогоняет вход через решение и ничего не возвращает, а нам нужен сам вход для
    /// сравнения. ProblemAdapter, который его прячет, — internal и не виден тестам, поэтому
    /// вместо расширения публичной поверхности ядра задача поднимается заново через уже
    /// публичный ProblemType (тот же самый parameterless-конструктор, который уже требует
    /// реестр) и её собственное публичное свойство Scaler. IInputScaler{TInput} ковариантен
    /// по TInput, а все входы задач — record-классы (ссылочные типы), поэтому после одной
    /// рефлексивной выборки самого Scaler дальше можно работать через IInputScaler{object}
    /// без дальнейшей рефлексии.</summary>
    private static string DescribeScaledInput(ProblemDescriptor problem, int n, int seed)
    {
        var instance = Activator.CreateInstance(problem.ProblemType)!;
        var scaler = (IInputScaler<object>)problem.ProblemType
            .GetProperty(nameof(Problem<object, object>.Scaler))!
            .GetValue(instance)!;

        return AlgoLabJson.Describe(scaler.Create(n, seed));
    }
}
