namespace AlgoLab.Api.Contracts;

public sealed record ProblemSummary(
    string Slug,
    string Title,
    string Difficulty,
    IReadOnlyList<string> Tags,
    bool HasScaler);

public sealed record SolutionView(
    string Id,
    string Name,
    string Kind,
    string Time,
    string Space,
    string? Note,
    string Source);

public sealed record CaseView(string Name, string Input, string Expected);

public sealed record ProblemDetail(
    ProblemSummary Summary,
    string Source,
    string Readme,
    IReadOnlyList<CaseView> Cases,
    IReadOnlyList<SolutionView> Solutions);

public sealed record RunRequest(string Slug, string SolutionId);

public sealed record MeasureRequest(string Slug, IReadOnlyList<string> SolutionIds);
