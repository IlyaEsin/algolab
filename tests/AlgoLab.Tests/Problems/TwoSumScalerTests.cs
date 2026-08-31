using AlgoLab.Problems.HashTable.TwoSum;

namespace AlgoLab.Tests.Problems;

public sealed class TwoSumScalerTests
{
    [Theory]
    [InlineData(64, 1)]
    [InlineData(512, 2)]
    [InlineData(4096, 3)]
    public void Generated_input_has_exactly_one_matching_pair(int n, int seed)
    {
        var input = new TwoSumScaler().Create(n, seed);

        var matches = 0;
        for (var i = 0; i < input.Nums.Length; i++)
        {
            for (var j = i + 1; j < input.Nums.Length; j++)
            {
                if (input.Nums[i] + input.Nums[j] == input.Target)
                {
                    matches++;
                }
            }
        }

        Assert.Equal(1, matches);
    }
}
