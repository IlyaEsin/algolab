using System.Diagnostics;
using AlgoLab.Core.Contracts;
using AlgoLab.Core.Json;
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

    public override RunResult RunCases(SolutionDescriptor solution)
    {
        var instance = (ISolution<TInput, TOutput>)Activator.CreateInstance(solution.SolutionType)!;
        var comparer = Problem.Comparer;
        var results = new List<CaseResult>();
        var notImplemented = false;
        var errored = false;

        foreach (var testCase in Problem.Cases)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var actual = instance.Solve(testCase.Input);
                stopwatch.Stop();
                var passed = comparer.Equals(testCase.Expected, actual);
                results.Add(new CaseResult(
                    testCase.Name,
                    AlgoLabJson.Describe(testCase.Input),
                    AlgoLabJson.Describe(testCase.Expected),
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
                    testCase.Name,
                    AlgoLabJson.Describe(testCase.Input),
                    AlgoLabJson.Describe(testCase.Expected),
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
                    testCase.Name,
                    AlgoLabJson.Describe(testCase.Input),
                    AlgoLabJson.Describe(testCase.Expected),
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
