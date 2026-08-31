using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.DynamicProgramming.ClimbingStairs;

[Solution("Два счётчика", Time = Complexity.ON, Space = Complexity.O1)]
public sealed class ClimbingStairsRolling : ISolution<ClimbingStairsInput, long>
{
    public long Solve(ClimbingStairsInput input)
    {
        long previous = 1, current = 1;
        for (var i = 2; i <= input.Steps; i++)
        {
            (previous, current) = (current, previous + current);
        }

        return current;
    }
}
