using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.HashTable.TwoSum;

[Solution("Хеш-таблица", Time = Complexity.ON, Space = Complexity.ON)]
public sealed class TwoSumHashMap : ISolution<TwoSumInput, int[]>
{
    public int[] Solve(TwoSumInput input)
    {
        var seen = new Dictionary<int, int>(input.Nums.Length);
        for (var i = 0; i < input.Nums.Length; i++)
        {
            if (seen.TryGetValue(input.Target - input.Nums[i], out var j))
            {
                return [j, i];
            }

            seen[input.Nums[i]] = i;
        }

        return [];
    }
}
