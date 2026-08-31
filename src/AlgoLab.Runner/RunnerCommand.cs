using AlgoLab.Core.Measuring;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Running;

namespace AlgoLab.Runner;

public static class RunnerCommand
{
    public static RunnerPayload Execute(string[] args, ProblemRegistry registry)
    {
        if (args.Length == 0)
        {
            return Fail("Использование: AlgoLab.Runner cases|measure --slug <slug> --solution <id> [--solution <id>...]");
        }

        var verb = args[0];
        string? slug = null;
        var solutionIds = new List<string>();

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--slug" when i + 1 < args.Length:
                    slug = args[++i];
                    break;
                case "--solution" when i + 1 < args.Length:
                    solutionIds.Add(args[++i]);
                    break;
                default:
                    return Fail($"Неизвестный или неполный аргумент: '{args[i]}'.");
            }
        }

        if (slug is null)
        {
            return Fail("Не указан --slug.");
        }

        if (solutionIds.Count == 0)
        {
            return Fail("Не указано ни одного --solution.");
        }

        var problem = registry.Find(slug);
        if (problem is null)
        {
            return Fail($"Задача '{slug}' не найдена.");
        }

        try
        {
            switch (verb)
            {
                case "cases":
                    var run = ProblemRunner.RunCases(problem, problem.Solution(solutionIds[0]));
                    return new RunnerPayload(run, Series: null, Error: null);

                case "measure":
                    var series = solutionIds
                        .Select(id => GrowthMeasurer.Measure(problem, problem.Solution(id)))
                        .ToArray();
                    return new RunnerPayload(Run: null, series, Error: null);

                default:
                    return Fail($"Неизвестная команда '{verb}'. Доступны: cases, measure.");
            }
        }
        catch (Exception exception)
        {
            return Fail($"{exception.GetType().Name}: {exception.Message}");
        }
    }

    private static RunnerPayload Fail(string error) => new(Run: null, Series: null, error);
}
