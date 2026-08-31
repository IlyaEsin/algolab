using System.Diagnostics;
using AlgoLab.Core.Contracts;
using AlgoLab.Core.Json;
using AlgoLab.Core.Measuring;
using AlgoLab.Core.Running;

namespace AlgoLab.Core.Registry;

/// <summary>Нетипизированный фасад над <see cref="Problem{TInput,TOutput}"/>: позволяет
/// реестру и API работать с задачами, не зная их generic-аргументов.</summary>
internal abstract class ProblemAdapter
{
    public abstract ProblemInfo Info { get; }

    public abstract bool HasScaler { get; }

    public abstract RunResult RunCases(SolutionDescriptor solution);

    public abstract IReadOnlyList<CaseDescription> DescribeCases();

    public abstract void RunScaledInput(SolutionDescriptor solution, int n, int seed);

    public abstract GrowthSeries Measure(SolutionDescriptor solution, MeasureOptions options);

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

    public override IReadOnlyList<CaseDescription> DescribeCases() =>
        Problem.Cases
            .Select(c => new CaseDescription(
                c.Name,
                AlgoLabJson.Describe(c.Input),
                AlgoLabJson.Describe(c.Expected)))
            .ToArray();

    public override void RunScaledInput(SolutionDescriptor solution, int n, int seed)
    {
        var scaler = Problem.Scaler
            ?? throw new InvalidOperationException($"Задача '{Problem.Info.Slug}' не задаёт Scaler.");

        var input = scaler.Create(n, seed);
        var instance = (ISolution<TInput, TOutput>)Activator.CreateInstance(solution.SolutionType)!;
        instance.Solve(input);
    }

    /// <summary>Приёмник результата: без него JIT вправе выбросить вызов целиком,
    /// а типизированное поле, в отличие от object, не боксит значимые типы
    /// и не портит замер аллокаций.</summary>
    private static TOutput? _sink;

    public override GrowthSeries Measure(SolutionDescriptor solution, MeasureOptions options)
    {
        var scaler = Problem.Scaler;
        if (scaler is null)
        {
            return Failed(solution, $"У задачи '{Problem.Info.Slug}' нет Scaler — кривая роста не снимается.");
        }

        var instance = (ISolution<TInput, TOutput>)Activator.CreateInstance(solution.SolutionType)!;
        var points = new List<MeasurePoint>();
        var total = Stopwatch.StartNew();

        try
        {
            for (var n = options.StartN; n <= options.MaxN; n *= 2)
            {
                var input = scaler.Create(n, MeasureOptions.Seed);

                var warmup = Stopwatch.StartNew();
                for (var i = 0; i < 3 && warmup.ElapsedMilliseconds < options.WarmupMs; i++)
                {
                    _sink = instance.Solve(input);
                }

                var samples = new double[options.Repeats];
                for (var i = 0; i < options.Repeats; i++)
                {
                    var sample = Stopwatch.StartNew();
                    _sink = instance.Solve(input);
                    sample.Stop();
                    samples[i] = sample.Elapsed.TotalMilliseconds;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                var before = GC.GetAllocatedBytesForCurrentThread();
                _sink = instance.Solve(input);
                var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

                points.Add(new MeasurePoint(n, Median(samples), allocated));

                if (total.ElapsedMilliseconds > options.BudgetMs)
                {
                    break;
                }
            }
        }
        catch (NotImplementedException)
        {
            return Failed(solution, "Решение не реализовано.");
        }
        catch (Exception exception)
        {
            return Failed(solution, $"{exception.GetType().Name}: {exception.Message}");
        }

        var timeVerdict = ComplexityFit.Fit(
            points.Select(p => new FitPoint(p.N, p.MedianMs)).ToArray(),
            solution.Time);

        var spaceVerdict = ComplexityFit.Fit(
            points.Select(p => new FitPoint(p.N, p.AllocatedBytes)).ToArray(),
            solution.Space);

        return new GrowthSeries(Problem.Info.Slug, solution.Id, points, timeVerdict, spaceVerdict, Error: null);
    }

    private GrowthSeries Failed(SolutionDescriptor solution, string error) => new(
        Problem.Info.Slug,
        solution.Id,
        [],
        new ComplexityVerdict(solution.Time, Complexity.Unknown, 0d, VerdictKind.Inconclusive),
        new ComplexityVerdict(solution.Space, Complexity.Unknown, 0d, VerdictKind.Inconclusive),
        error);

    private static double Median(double[] samples)
    {
        var sorted = samples.Order().ToArray();
        return sorted.Length % 2 == 1
            ? sorted[sorted.Length / 2]
            : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2d;
    }

    public override RunResult RunCases(SolutionDescriptor solution)
    {
        var instance = (ISolution<TInput, TOutput>)Activator.CreateInstance(solution.SolutionType)!;
        var comparer = Problem.Comparer;
        var results = new List<CaseResult>();
        var notImplemented = false;
        var errored = false;

        foreach (var testCase in Problem.Cases)
        {
            var name = testCase.Name;
            var input = AlgoLabJson.Describe(testCase.Input);
            var expected = AlgoLabJson.Describe(testCase.Expected);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var actual = instance.Solve(testCase.Input);
                stopwatch.Stop();
                var passed = comparer.Equals(testCase.Expected, actual);
                results.Add(new CaseResult(
                    name,
                    input,
                    expected,
                    AlgoLabJson.Describe(actual),
                    passed,
                    stopwatch.Elapsed.TotalMilliseconds,
                    Error: null));
            }
            catch (NotImplementedException)
            {
                stopwatch.Stop();
                notImplemented = true;
                results.Add(new CaseResult(
                    name,
                    input,
                    expected,
                    Actual: string.Empty,
                    Passed: false,
                    stopwatch.Elapsed.TotalMilliseconds,
                    Error: null));
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                errored = true;
                results.Add(new CaseResult(
                    name,
                    input,
                    expected,
                    Actual: string.Empty,
                    Passed: false,
                    stopwatch.Elapsed.TotalMilliseconds,
                    Error: $"{exception.GetType().Name}: {exception.Message}"));
            }
        }

        var status = (notImplemented, errored, results.All(r => r.Passed)) switch
        {
            (true, _, _) => RunStatus.NotImplemented,
            (_, true, _) => RunStatus.Error,
            (_, _, true) => RunStatus.Passed,
            _ => RunStatus.Failed,
        };

        return new RunResult(Problem.Info.Slug, solution.Id, status, results);
    }
}
