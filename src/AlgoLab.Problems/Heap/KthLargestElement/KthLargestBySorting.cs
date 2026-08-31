using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Heap.KthLargestElement;

[Solution("Полная сортировка", Time = Complexity.ONLogN, Space = Complexity.ON)]
public sealed class KthLargestBySorting : ISolution<KthLargestInput, int>
{
    public int Solve(KthLargestInput input) =>
        input.Nums.OrderDescending().ElementAt(input.K - 1);
}
