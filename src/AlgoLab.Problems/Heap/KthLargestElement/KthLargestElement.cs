using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Heap.KthLargestElement;

public sealed record KthLargestInput(int[] Nums, int K);

public sealed class KthLargestElement : Problem<KthLargestInput, int>
{
    public override ProblemInfo Info => new(
        Slug: "kth-largest-element",
        Title: "K-й по величине элемент",
        Source: "https://leetcode.com/problems/kth-largest-element-in-an-array/",
        Difficulty: Difficulty.Medium,
        Tags: [Tag.Heap, Tag.Sorting, Tag.Array]);

    public override IEnumerable<TestCase<KthLargestInput, int>> Cases =>
    [
        new(new KthLargestInput([3, 2, 1, 5, 6, 4], 2), 5, "второй по величине"),
        new(new KthLargestInput([3, 2, 3, 1, 2, 4, 5, 5, 6], 4), 4, "с повторами"),
        new(new KthLargestInput([1], 1), 1, "один элемент"),
    ];

    public override IInputScaler<KthLargestInput> Scaler => new KthLargestScaler();
}

public sealed class KthLargestScaler : IInputScaler<KthLargestInput>
{
    public KthLargestInput Create(int n, int seed)
    {
        var random = new Random(seed);
        var nums = new int[Math.Max(n, 1)];
        for (var i = 0; i < nums.Length; i++)
        {
            nums[i] = random.Next();
        }

        return new KthLargestInput(nums, K: 10);
    }
}
