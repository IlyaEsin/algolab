using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.DynamicProgramming.ClimbingStairs;

[Solution("Рекурсия с мемоизацией", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class ClimbingStairsMemoized : ISolution<ClimbingStairsInput, long>
{
    public long Solve(ClimbingStairsInput input) =>
        Ways(input.Steps, new Dictionary<int, long>());

    private static long Ways(int steps, Dictionary<int, long> memo)
    {
        if (steps <= 2)
        {
            return Math.Max(steps, 1);
        }

        if (memo.TryGetValue(steps, out var cached))
        {
            return cached;
        }

        var result = Ways(steps - 1, memo) + Ways(steps - 2, memo);
        memo[steps] = result;
        return result;
    }
}
