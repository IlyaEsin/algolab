namespace AlgoLab.Core.Measuring;

public sealed record MeasurePoint(int N, double MedianMs, long AllocatedBytes);

public sealed record GrowthSeries(
    string Slug,
    string SolutionId,
    IReadOnlyList<MeasurePoint> Points,
    ComplexityVerdict Time,
    ComplexityVerdict Space,
    string? Error);
