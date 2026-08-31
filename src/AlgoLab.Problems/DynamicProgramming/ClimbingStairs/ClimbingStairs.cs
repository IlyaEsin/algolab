using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.DynamicProgramming.ClimbingStairs;

public sealed record ClimbingStairsInput(int Steps);

public sealed class ClimbingStairs : Problem<ClimbingStairsInput, long>
{
    public override ProblemInfo Info => new(
        Slug: "climbing-stairs",
        Title: "Подъём по лестнице",
        Source: "https://leetcode.com/problems/climbing-stairs/",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.DynamicProgramming]);

    public override IEnumerable<TestCase<ClimbingStairsInput, long>> Cases =>
    [
        new(new ClimbingStairsInput(2), 2L, "две ступени"),
        new(new ClimbingStairsInput(3), 3L, "три ступени"),
        new(new ClimbingStairsInput(10), 89L, "десять ступеней"),
        new(new ClimbingStairsInput(1), 1L, "одна ступень"),
    ];
}
