using AlgoLab.Core.Contracts;

namespace AlgoLab.Problems.Lists.ReverseLinkedList;

[Solution("Три указателя", Time = Complexity.ON, Space = Complexity.ON,
    Note = "Сам разворот перекладывает указатели на месте и не требует дополнительной "
        + "памяти — O(1). Измеренная здесь O(n) — это построение списка из входного "
        + "массива и обратно внутри Solve, а не работа алгоритма.")]
public sealed class ReverseLinkedListIterative : ISolution<ReverseLinkedListInput, int[]>
{
    public int[] Solve(ReverseLinkedListInput input)
    {
        ListNode? previous = null;
        var current = ListNode.FromArray(input.Values);

        while (current is not null)
        {
            var next = current.Next;
            current.Next = previous;
            previous = current;
            current = next;
        }

        return ListNode.ToArray(previous);
    }
}
