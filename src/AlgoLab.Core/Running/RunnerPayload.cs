using AlgoLab.Core.Measuring;

namespace AlgoLab.Core.Running;

/// <summary>То, что исполнитель печатает в stdout. Ровно одно из полей заполнено.</summary>
public sealed record RunnerPayload(
    RunResult? Run,
    IReadOnlyList<GrowthSeries>? Series,
    string? Error);
