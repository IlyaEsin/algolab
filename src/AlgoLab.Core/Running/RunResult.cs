namespace AlgoLab.Core.Running;

public enum RunStatus
{
    Passed,
    Failed,
    NotImplemented,
    Error,
}

public sealed record CaseResult(
    string Name,
    string Input,
    string Expected,
    string Actual,
    bool Passed,
    double ElapsedMs,
    string? Error);

public sealed record RunResult(
    string Slug,
    string SolutionId,
    RunStatus Status,
    IReadOnlyList<CaseResult> Cases);

/// <summary>Кейс без прогона: нужен каталогу, чтобы показать вход и ожидание,
/// не исполняя ничьего кода.</summary>
public sealed record CaseDescription(string Name, string Input, string Expected);
