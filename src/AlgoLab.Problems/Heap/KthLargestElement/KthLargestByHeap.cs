using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Heap.KthLargestElement;

[Solution("Куча размера k", Time = Complexity.ON, Space = Complexity.O1,
    Note = "Точнее O(n log k); при фиксированном k это линейный рост по n.")]
public sealed class KthLargestByHeap : ISolution<KthLargestInput, int>
{
    public int Solve(KthLargestInput input)
    {
        var heap = new PriorityQueue<int, int>();
        foreach (var value in input.Nums)
        {
            heap.Enqueue(value, value);
            if (heap.Count > input.K)
            {
                heap.Dequeue();
            }
        }

        return heap.Peek();
    }
}
