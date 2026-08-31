using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.HashTable.TwoSum;

[Solution("Перебор пар", Time = Complexity.ON2, Space = Complexity.O1)]
public sealed class TwoSumBruteForce : ISolution<TwoSumInput, int[]>
{
    public int[] Solve(TwoSumInput input)
    {
        for (var i = 0; i < input.Nums.Length; i++)
        {
            for (var j = i + 1; j < input.Nums.Length; j++)
            {
                if (input.Nums[i] + input.Nums[j] == input.Target)
                {
                    return [i, j];
                }
            }
        }

        return [];
    }
}
