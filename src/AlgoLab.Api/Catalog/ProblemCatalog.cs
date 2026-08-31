using AlgoLab.Api.Contracts;
using AlgoLab.Core.Contracts;
using AlgoLab.Core.Registry;
using AlgoLab.Core.Resources;
using AlgoLab.Core.Running;

namespace AlgoLab.Api.Catalog;

public sealed class ProblemCatalog(ProblemRegistry registry)
{
    public IReadOnlyList<ProblemSummary> List() =>
        registry.Problems.Select(Summarize).ToArray();

    public ProblemDetail? Detail(string slug)
    {
        var problem = registry.Find(slug);
        if (problem is null)
        {
            return null;
        }

        var cases = ProblemRunner.DescribeCases(problem)
            .Select(c => new CaseView(c.Name, c.Input, c.Expected))
            .ToArray();

        var solutions = problem.Solutions
            .Select(s => new SolutionView(
                s.Id,
                s.Name,
                s.Kind.ToString(),
                s.Time.Display(),
                s.Space.Display(),
                s.Note,
                SourceStore.ReadSource(s.SolutionType)))
            .ToArray();

        return new ProblemDetail(
            Summarize(problem),
            problem.Info.Source,
            SourceStore.ReadReadme(problem),
            cases,
            solutions);
    }

    /// <summary>Каталог не исполняет ничей код: он только описывает задачи.
    /// Всякое исполнение идёт через RunnerHost в отдельном процессе — иначе
    /// зациклившаяся попытка вешала бы сам список задач.</summary>
    private static ProblemSummary Summarize(ProblemDescriptor problem) => new(
        problem.Info.Slug,
        problem.Info.Title,
        problem.Info.Difficulty.ToString(),
        problem.Info.Tags.Select(t => t.ToString()).ToArray(),
        problem.HasScaler);
}
