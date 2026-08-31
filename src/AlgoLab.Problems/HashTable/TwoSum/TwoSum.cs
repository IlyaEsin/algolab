using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.HashTable.TwoSum;

public sealed record TwoSumInput(int[] Nums, int Target);

public sealed class TwoSum : Problem<TwoSumInput, int[]>
{
    public override ProblemInfo Info => new(
        Slug: "two-sum",
        Title: "Сумма двух",
        Source: "https://leetcode.com/problems/two-sum/",
        Difficulty: Difficulty.Easy,
        Tags: [Tag.Array, Tag.HashTable]);

    public override IEnumerable<TestCase<TwoSumInput, int[]>> Cases =>
    [
        new(new TwoSumInput([2, 7, 11, 15], 9), [0, 1], "ответ в начале"),
        new(new TwoSumInput([3, 2, 4], 6), [1, 2], "ответ не в начале"),
        new(new TwoSumInput([3, 3], 6), [0, 1], "одинаковые значения"),
        new(new TwoSumInput([-3, 4, 3, 90], 0), [0, 2], "отрицательные числа"),
    ];

    public override IInputScaler<TwoSumInput> Scaler => new TwoSumScaler();
}

/// <summary>Строит массив, у которого единственная подходящая пара лежит в самом конце —
/// иначе перебор находил бы ответ за O(1) и кривая роста ничего бы не показала.</summary>
public sealed class TwoSumScaler : IInputScaler<TwoSumInput>
{
    public TwoSumInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var size = Math.Max(n, 2);
        var nums = new int[size];
        for (var i = 0; i < size; i++)
        {
            nums[i] = random.Next(2, 1_000_000) * 2;
        }

        nums[size - 2] = 1;
        nums[size - 1] = 2;
        return new TwoSumInput(nums, 3);
    }
}
